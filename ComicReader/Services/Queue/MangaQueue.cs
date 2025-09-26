using ComicReader.Helper;
using ComicReader.Interpreter;
using Newtonsoft.Json;

namespace ComicReader.Services.Queue
{
	public class MangaQueue
	{
		private readonly FileSaverService fileSaverService;
		private readonly RequestHelper requestHelper;
		private readonly SimpleNotificationService simpleNotificationService;
		private readonly Factory factory;
		private Dictionary<string, ChapterPageSources> _chaptersToDownload = new Dictionary<string, ChapterPageSources>();

		public IReadOnlyList<ChapterPageSources> ChaptersToDownload => _chaptersToDownload.Values.ToList();

		public event EventHandler? Start;
		public event EventHandler? End;
		public event EventHandler<ChapterPageSources>? ChapterFinished;
		public event EventHandler<ChapterPageSources>? Error;

		private bool _wasInit = false;

		public MangaQueue(FileSaverService fileSaverService, RequestHelper requestHelper, SimpleNotificationService simpleNotificationService, Factory factory)
		{
			this.fileSaverService = fileSaverService;
			this.requestHelper = requestHelper;
			this.simpleNotificationService = simpleNotificationService;
			this.factory = factory;
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
			_chaptersToDownload[$"{chapter.Source}{chapter.MangaName}{chapter.Title}"] = new ChapterPageSources(chapter);
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
			if (!_wasInit) {
				await Init();
			}

			var key = $"{source.Source}{source.MangaName}{source.Title}";
			if (_chaptersToDownload.ContainsKey(key)) {
				_chaptersToDownload.Remove(key);
			}
			await SaveQueue();
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

		public void StartDownload()
		{
			Start?.Invoke(this, EventArgs.Empty);

			var t = new Thread(async () => { await DownloadAllChapters(); });
			t.Start();
		}

		public Task Download()
		{
			Start?.Invoke(this, EventArgs.Empty);

			return DownloadAllChapters();
		}

		private async Task DownloadAllChapters()
		{
			try {
				var copy = _chaptersToDownload.ToDictionary(c => c.Key, c => c.Value);

				await simpleNotificationService.ShowProgress("Download chapters", $"0/{copy.Count}", 0, copy.Count);
				int current = 0;
				foreach (var chapter in copy) {
					try {
						await simpleNotificationService.ShowProgress("Download chapters", $"{++current}/{copy.Count}", current, copy.Count);

						int currentPage = 0;
						foreach (var urlPair in chapter.Value.UrlToLocalFileMapper) {
							await simpleNotificationService.ShowProgress(SimpleNotificationService.ProgressId + 1, chapter.Value.MangaName, $"{++currentPage}/{chapter.Value.UrlToLocalFileMapper.Count}", currentPage, chapter.Value.UrlToLocalFileMapper.Count);
							try {
								if (!fileSaverService.FileExists(urlPair.Value)) {
									await requestHelper.DownloadFile(urlPair.Key, urlPair.Value, 3, chapter.Value.RequestHeaders);
								}
							} catch (Exception) {
								await Task.Delay(500);
								if (!fileSaverService.FileExists(urlPair.Value)) {
									await requestHelper.DownloadFile(urlPair.Key, urlPair.Value, 3, chapter.Value.RequestHeaders);
								}
							}
						}

						simpleNotificationService.Close(SimpleNotificationService.ProgressId + 1);

						await RemoveEntry(chapter.Value);
						ChapterFinished?.Invoke(this, chapter.Value);
					} catch (Exception) {
						Error?.Invoke(this, chapter.Value);
					}
				}

				simpleNotificationService.Close();
			} catch (Exception) { }

			End?.Invoke(this, EventArgs.Empty);
		}
	}
}
