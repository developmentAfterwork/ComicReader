using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using ComicReader.Services.Queue;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class DownloadsViewModel : ObservableObject
	{
		private readonly MangaQueue mangaQueue;
		private readonly SettingsService settingsService;
		private readonly FileSaverService fileSaverService;
		private readonly Factory factory;

		[ObservableProperty]
		private ObservableCollection<ChapterPageSources> _ChaptersToDownload = new ObservableCollection<ChapterPageSources>();

		[ObservableProperty]
		private bool _IsDownloading = false;

		[ObservableProperty]
		private bool _HasNoEntries = false;

		public ICommand StartQueueCommand { get; set; }

		public DownloadsViewModel(MangaQueue mangaQueue, SettingsService settingsService, FileSaverService fileSaverService, Factory factory)
		{
			this.mangaQueue = mangaQueue;
			this.settingsService = settingsService;
			this.fileSaverService = fileSaverService;
			this.factory = factory;
			this.mangaQueue.Start += OnStart;
			this.mangaQueue.End += OnEnd;
			this.mangaQueue.ChapterFinished += OnChapterFinished;
			this.mangaQueue.Error += OnError;

			StartQueueCommand = new RelayCommand(OnStartQueueCommand);
		}

		private void OnStart(object? sender, EventArgs e)
		{
			IsDownloading = true;
			HasNoEntries = false;
		}

		private void OnEnd(object? sender, EventArgs e)
		{
			IsDownloading = false;
			HasNoEntries = !ChaptersToDownload.Any();
		}

		private void OnChapterFinished(object? sender, ChapterPageSources e)
		{
			ChaptersToDownload.Clear();
			ChaptersToDownload.AddRange(mangaQueue.ChaptersToDownload);
		}

		private void OnError(object? sender, ChapterPageSources e)
		{
			_ = Task.Run(async () => {
				var bookmarkedUniqIds = settingsService.GetBookmarkedMangaUniqIdentifiers();

				foreach (var bookmarkId in bookmarkedUniqIds.Where(s => s.Contains("|"))) {
					try {
						IManga manga = await factory.GetMangaFromBookmarkId(bookmarkId);

						if (e.Source == manga.Source && e.MangaName == manga.Name) {
							var chapters = await manga.GetBooks();
							var chapter = chapters.SingleOrDefault(c => c.Title == e.Title);

							if (chapter != null) {
								fileSaverService.DeleteChapterFile(chapter);
								await fileSaverService.DeleteImagesFromChapter(chapter, factory);
								await chapter.Save(false, factory);
								await mangaQueue.AddChapter(chapter);

								ChaptersToDownload.Clear();
								ChaptersToDownload.AddRange(mangaQueue.ChaptersToDownload);
							}

							break;
						}
					} catch { }
				}
			});
		}

		private void OnStartQueueCommand()
		{
			mangaQueue.StartDownload();
		}

		public async Task OnAppearing()
		{
			try {
				await mangaQueue.Init();
			} catch { }

			ChaptersToDownload.Clear();
			ChaptersToDownload.AddRange(mangaQueue.ChaptersToDownload);

			HasNoEntries = !ChaptersToDownload.Any();
		}
	}
}
