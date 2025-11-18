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
		private readonly BackgroundService backgroundService;
		private readonly SimpleNotificationService simpleNotificationService;

		[ObservableProperty]
		private ObservableCollection<ChapterPageSources> _ChaptersToDownload = new ObservableCollection<ChapterPageSources>();

		[ObservableProperty]
		private bool _IsDownloading = false;

		[ObservableProperty]
		private bool _HasNoEntries = false;

		private int _numberOfErrors = 0;

		public ICommand StartQueueCommand { get; set; }

		public DownloadsViewModel(MangaQueue mangaQueue, SettingsService settingsService, FileSaverService fileSaverService, Factory factory, BackgroundService backgroundService, SimpleNotificationService simpleNotificationService)
		{
			this.mangaQueue = mangaQueue;
			this.settingsService = settingsService;
			this.fileSaverService = fileSaverService;
			this.factory = factory;
			this.backgroundService = backgroundService;
			this.simpleNotificationService = simpleNotificationService;

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

		private async void OnError(object? sender, ChapterPageSources e)
		{
			await simpleNotificationService.ShowError("Queue Error", $"Failed downloads {++_numberOfErrors}");
		}

		private void OnStartQueueCommand()
		{
			_numberOfErrors = 0;

			var id = Guid.NewGuid().ToString();
			backgroundService.Register(id, (t) => mangaQueue.Download(settingsService.GetRequestTimeout()));
			backgroundService.Start(id, "Download all chapters");
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
