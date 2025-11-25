using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Interpreter.Interface;
using Interpreter.Interface;
using Newtonsoft.Json;

namespace ComicReader.Services.Queue
{
	public class MangaQueue
	{
		private readonly FileSaverService fileSaverService;
		private readonly IRequest requestHelper;
		private readonly SimpleNotificationService simpleNotificationService;
		private readonly Factory factory;
		private readonly SettingsService settingsService;

		private Dictionary<string, ChapterPageSources> _chaptersToDownload = new Dictionary<string, ChapterPageSources>();

		public IReadOnlyList<ChapterPageSources> ChaptersToDownload => _chaptersToDownload.Values.ToList();

		public event EventHandler? Start;
		public event EventHandler? End;
		public event EventHandler<ChapterPageSources>? ChapterFinished;
		public event EventHandler<(string lastError, ChapterPageSources chapter)>? Error;

		private bool _wasInit = false;

		public MangaQueue(FileSaverService fileSaverService, IRequest requestHelper, SimpleNotificationService simpleNotificationService, Factory factory, SettingsService settingsService)
		{
			this.fileSaverService = fileSaverService;
			this.requestHelper = requestHelper;
			this.simpleNotificationService = simpleNotificationService;
			this.factory = factory;
			this.settingsService = settingsService;
		}

		public async Task Init()
		{
			await LoadQueue();
			_wasInit = true;
		}

		public async Task AddManga(IManga manga, SimpleNotificationService simpleNotificationService)
		{
			if (!_wasInit) {
				await Init().ConfigureAwait(false);
			}

			List<IChapter> chapters = await GetChapters(manga).ConfigureAwait(false);

			foreach (var chapter in chapters) {
				await TryAddChapter(chapter, simpleNotificationService).ConfigureAwait(false);
			}
		}

		private static async Task<List<IChapter>> GetChapters(IManga manga)
		{
			List<IChapter> c = new();

			try {
				c = await manga.GetBooks().ConfigureAwait(false);
			} catch (Exception) { }

			return c;
		}

		private async Task TryAddChapter(IChapter chapter, SimpleNotificationService simpleNotificationService)
		{
			try {
				await AddChapter(chapter).ConfigureAwait(false);
			} catch (Exception) {
				try {
					await Task.Delay(500).ConfigureAwait(false);
					await AddChapter(chapter).ConfigureAwait(false);
				} catch (Exception) {
					await simpleNotificationService.ShowError("Error At", $"{chapter.MangaName} - {chapter.Title}");
				}
			}
		}

		public async Task AddMissingChaptersFromManga(IManga manga, SimpleNotificationService simpleNotificationService)
		{
			if (!_wasInit) {
				await Init().ConfigureAwait(false);
			}

			List<IChapter> chapters = await GetChapters(manga).ConfigureAwait(false);

			await simpleNotificationService.ShowProgress("Add missing chapters", $"0/{chapters.Count}", 0, chapters.Count);
			int current = 0;
			foreach (var chapter in chapters) {
				await simpleNotificationService.ShowProgress("Add missing chapters", $"{++current}/{chapters.Count}", current, chapters.Count);

				var key = $"{chapter.Source}{chapter.MangaName}{chapter.Title}";
				if (!fileSaverService.FileExists(chapter)) {
					await TryAddChapter(chapter, simpleNotificationService).ConfigureAwait(false);
				}
			}
		}

		public async Task AddChapter(IChapter chapter)
		{
			if (!_wasInit) {
				await Init();
			}

			await chapter.Save(false, factory);
			_chaptersToDownload[$"{chapter.Source}{chapter.MangaName}{chapter.Title}"] = new ChapterPageSources(chapter, factory);
			await SaveQueue();
		}

		public async Task RemoveChapter(IChapter chapter)
		{
			if (!_wasInit) {
				await Init();
			}

			var key = $"{chapter.Source}{chapter.MangaName}{chapter.Title}";
			if (_chaptersToDownload.ContainsKey(key)) {
				_chaptersToDownload.Remove(key);
			}
			await SaveQueue();
		}

		public async Task RemoveEntry(ChapterPageSources source)
		{
			try {
				if (!_wasInit) {
					await Init();
				}

				var key = $"{source.Source}{source.MangaName}{source.Title}";
				if (_chaptersToDownload.ContainsKey(key)) {
					_chaptersToDownload.Remove(key);
				}

				await SaveQueue();
			} catch (Exception ex) {
				await simpleNotificationService.ShowError("Error At", $"RemoveEntry - {ex.Message}");
			}
		}

		private async Task SaveQueue()
		{
			var json = JsonConvert.SerializeObject(_chaptersToDownload);

			var path = fileSaverService.GetSecurePathToDocuments("queue.json");

			await fileSaverService.SaveFile(path, json);
		}

		private async Task LoadQueue()
		{
			var path = fileSaverService.GetSecurePathToDocuments("queue.json");

			if (fileSaverService.FileExists(path)) {
				try {
					string json = await fileSaverService.LoadFile(path);

					_chaptersToDownload = JsonConvert.DeserializeObject<Dictionary<string, ChapterPageSources>>(json)!;
				} catch (Exception) {
					fileSaverService.DeleteFile(path);
					_chaptersToDownload = new();
				}
			}
		}

		public void StartDownload(TimeSpan timeout)
		{
			Start?.Invoke(this, EventArgs.Empty);

			var t = new Thread(async () => { await DownloadAllChapters(timeout); });
			t.Start();
		}

		public Task Download(TimeSpan timeout)
		{
			Start?.Invoke(this, EventArgs.Empty);

			return DownloadAllChapters(timeout);
		}

		private async Task DownloadAllChapters(TimeSpan timeout)
		{
			try {
				var copy = _chaptersToDownload.ToDictionary(c => c.Key, c => c.Value);

				int chapterCount = copy.Count;
				int currentChapter = 0;

				foreach (var chapter in copy) {
					try {
						int pagesCount = chapter.Value.UrlToLocalFileMapper.Count;
						int currentPage = 0;

						var messageTxt = $"Mangas: {++currentChapter}/{chapterCount} - Chapters: {currentPage}/{pagesCount}";
						await simpleNotificationService.ShowProgress("Download mangas", messageTxt, currentPage, pagesCount);

						bool hasError = false;
						string lastError = string.Empty;

						await chapter.Value.UrlToLocalFileMapper.DownloadPages(
							chapter.Value.RequestHeaders,
							chapter.Value.Source,
							chapter.Value.MangaName,
							chapter.Value.Title,
							timeout,
							factory,
							async () => {
								messageTxt = $"Mangas: {currentChapter}/{chapterCount} - Chapters: {++currentPage}/{pagesCount}";
								await simpleNotificationService.ShowProgress("Download mangas", messageTxt, currentPage, pagesCount);
							},
							(ex) => {
								lastError = ex.Message;
								hasError = true;
							},
							settingsService,
							requestHelper,
							fileSaverService);

						await RemoveEntry(chapter.Value);
						if (hasError) {
							Error?.Invoke(this, (lastError, chapter.Value));
						} else {
							ChapterFinished?.Invoke(this, chapter.Value);
						}
					} catch (Exception ex) {
						await RemoveEntry(chapter.Value);
						Error?.Invoke(this, (ex.Message, chapter.Value));
					}
				}

				simpleNotificationService.Close();
			} catch (Exception ex) {
				await simpleNotificationService.ShowError("Error At", $"DownloadAllChapters - {ex.Message}");
			}

			End?.Invoke(this, EventArgs.Empty);
		}
	}
}
