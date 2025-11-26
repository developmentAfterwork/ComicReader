using ComicReader.Converter;
using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.Services.Queue;
using ComicReader.ViewModels.Model;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PopupService = ComicReader.Services.PopupService;

namespace ComicReader.ViewModels
{
	public partial class MangaDetailsViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;
		private readonly SettingsService settingsService;
		private readonly MangaQueue mangaQueue;
		private readonly SimpleNotificationService simpleNotificationService;
		private readonly FileSaverService fileSaverService;
		private readonly Factory factory;
		private readonly RequestHelper requestHelper;
		private readonly PopupService popupService;

		[ObservableProperty]
		private ObservableCollection<IChapterModel> _Chapters = new ObservableCollection<IChapterModel>();

		[ObservableProperty]
		private ObservableCollection<string> _Genres = new ObservableCollection<string>();

		[ObservableProperty]
		private ICommand _ItemSelectedCommand;

		[ObservableProperty]
		private IChapterModel? _SelectedItem;

		[ObservableProperty]
		private bool _IsSearching = true;

		[ObservableProperty]
		private string _CoverUrl = "";

		[ObservableProperty]
		private string _Title = "";

		[ObservableProperty]
		private string _Autor = "";

		[ObservableProperty]
		private string _Status = "";

		[ObservableProperty]
		private string _LanguageFlagUrl = "";

		[ObservableProperty]
		private string _Description = "";

		[ObservableProperty]
		private string _TotalChapersCount = "";

		public ICommand BookmarkManga { get; set; }

		public ICommand DownloadMissingManga { get; set; }

		public ICommand Refesh { get; set; }

		public ICommand DeleteManga { get; set; }

		public ICommand Open { get; set; }

		private IManga? _lastManga;

		private Task _initTask;

		[ObservableProperty]
		public ImageSource _coverUrlImageSource = default!;

		public MangaDetailsViewModel(InMemoryDatabase database, Navigation navigation, SettingsService settingsService, MangaQueue mangaQueue, SimpleNotificationService simpleNotificationService, FileSaverService fileSaverService, Factory factory, RequestHelper requestHelper, PopupService popupService)
		{
			inMemoryDatabase = database;
			this.navigation = navigation;
			this.settingsService = settingsService;
			this.mangaQueue = mangaQueue;
			this.simpleNotificationService = simpleNotificationService;
			this.fileSaverService = fileSaverService;
			this.factory = factory;
			this.requestHelper = requestHelper;
			this.popupService = popupService;

			ItemSelectedCommand = new AsyncRelayCommand<object>(ChapterSelected);
			BookmarkManga = new AsyncRelayCommand(AddBookmarkManga);
			DownloadMissingManga = new AsyncRelayCommand(OnDownloadMissingManga);
			Refesh = new AsyncRelayCommand(OnRefresh);
			DeleteManga = new AsyncRelayCommand(OnDeleteManga);
			Open = new AsyncRelayCommand(OnOpen);

			_initTask = new Task(async () => await Init());
		}

		public void OnAppearing()
		{
			IsSearching = true;

			_initTask = new Task(async () => await Init());
			_initTask.Start();
		}

		private async Task Init()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			bool showDownloadPages = settingsService.GetShowDownloadedPagesNumbers();

			if (_lastManga != null && manga == _lastManga) {
				var cToShow = Chapters.Where(c => !settingsService.GetChapterReaded(c.Chapter)).ToList();
				Chapters.Clear();

				List<IChapterModel> cModels = new List<IChapterModel>();
				foreach (var chapter in cToShow) {
					var m = await IChapterModel.Create(chapter.Chapter, fileSaverService, showDownloadPages);
					cModels.Add(m);
				}
				Chapters.AddRange(cModels);

				IsSearching = false;
				return;
			}

			await Task.Delay(300);

			_lastManga = manga;

			CoverUrl = manga.CoverUrl;
			Title = manga.Name;
			Autor = manga.Autor;
			Status = manga.Status;
			LanguageFlagUrl = manga.LanguageFlagUrl;
			Description = manga.Description;

			var chaptersList = await manga.GetBooks().ConfigureAwait(false);

			TotalChapersCount = $"{chaptersList.Count} Chapters";

			var chaptersToShow = chaptersList.Where(c => !settingsService.GetChapterReaded(c)).ToList();
			Chapters.Clear();

			List<IChapterModel> chapterModels = new List<IChapterModel>();
			foreach (var chapter in chaptersToShow) {
				var m = await IChapterModel.Create(chapter, fileSaverService, showDownloadPages);
				chapterModels.Add(m);
			}
			Chapters.AddRange(chapterModels);

			Genres.Clear();
			Genres.AddRange(manga.Genres);

			string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
			if (File.Exists(pathWithFile)) {
				CoverUrlImageSource = ImageSource.FromFile(pathWithFile);
			} else {
				var mem = await (new RequestHelper(TimeSpan.FromSeconds(30))).DoGetRequestStream(manga.CoverUrl, manga.RequestHeaders);
				if (mem != null) {
					CoverUrlImageSource = ImageSource.FromStream(() => mem);
				}
			}

			IsSearching = false;
		}

		public async Task ChapterSelected(object? chapterObj)
		{
			if (SelectedItem != null) {
				inMemoryDatabase.Set<IChapter>("selectedChapter", SelectedItem.Chapter);
				SelectedItem = null;

				await navigation.GoToReadChapter();
			}
		}

		public async Task AddBookmarkManga()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			try {
				settingsService.BookmarkManga(manga);

				await manga.Save();

				string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
				if (!File.Exists(pathWithFile)) {
					await requestHelper.DownloadFile(manga.CoverUrl, pathWithFile, 3, settingsService.GetRequestTimeout(), manga.RequestHeaders);
				}

				if (settingsService.GetAutoAddChaptersToQueue()) {
					var chapters = await manga.GetBooks();

					int i = 0;
					foreach (var chapter in chapters) {
						await simpleNotificationService.ShowProgress("Add chapters", $"{++i}/{chapters.Count}", i, chapters.Count);
						await mangaQueue.AddChapter(chapter);
					}
					simpleNotificationService.Close();
				}
			} catch (Exception ex) {
				await simpleNotificationService.ShowError($"Error", $"{manga.Name} - {ex.Message}");
			}
		}

		public async Task OnDeleteManga()
		{
			var result = await popupService.ShowPopupAsync("Delete Manga", "Do you want to delete that manga?", "Yes", "No");
			if (!result) {
				return;
			}

			await Task.Run(async () => {
				IsSearching = true;

				IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");

				try {
					var chapters = await manga.GetBooks();

					settingsService.RemoveManga(manga);
					fileSaverService.DeleteManga(manga);

					int i = 0;
					foreach (var chapter in chapters) {
						await simpleNotificationService.ShowProgress("Remove chapters", $"{++i}/{chapters.Count}", i, chapters.Count);
						await mangaQueue.RemoveChapter(chapter);
					}
					simpleNotificationService.Close();

					await navigation.CloseCurrent();
				} catch (Exception ex) {
					await simpleNotificationService.ShowError($"Error", $"{manga.Name} - {ex.Message}");
				}

				IsSearching = false;
			});
		}

		public async Task OnDownloadMissingManga()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");

			IsSearching = true;
			await mangaQueue.AddMissingChaptersFromManga(manga, simpleNotificationService);
			IsSearching = false;
		}

		public async Task OnRefresh()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");

			IsSearching = true;

			await manga.Refresh(factory, fileSaverService, simpleNotificationService);

			IManga? reloadedManga = await factory.GetMangaFromBookmarkId(manga.GetUniqIdentifier());
			if (reloadedManga == null) {
				await simpleNotificationService.ShowError("Error", "Can't reload manga after refresh.");
				IsSearching = false;
				return;
			}

			inMemoryDatabase.Set<IManga>("selectedManga", reloadedManga);
			await Init();

			IsSearching = false;
		}

		private async Task OnOpen()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");

			await Browser.OpenAsync(manga.HomeUrl, BrowserLaunchMode.SystemPreferred);
		}
	}
}
