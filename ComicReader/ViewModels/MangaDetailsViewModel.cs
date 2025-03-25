﻿using ComicReader.Converter;
using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.Services.Queue;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static Android.Graphics.ColorSpace;

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
		[ObservableProperty]
		private ObservableCollection<IChapter> _Chapters = new ObservableCollection<IChapter>();

		[ObservableProperty]
		private ObservableCollection<string> _Genres = new ObservableCollection<string>();

		[ObservableProperty]
		private ICommand _ItemSelectedCommand;

		[ObservableProperty]
		private IChapter _SelectedItem;

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

		private IManga? _lastManga;

		private Task _initTask;

		[ObservableProperty]
		public ImageSource _coverUrlImageSource;

		public MangaDetailsViewModel(InMemoryDatabase database, Navigation navigation, SettingsService settingsService, MangaQueue mangaQueue, SimpleNotificationService simpleNotificationService, FileSaverService fileSaverService, Factory factory, RequestHelper requestHelper)
		{
			inMemoryDatabase = database;
			this.navigation = navigation;
			this.settingsService = settingsService;
			this.mangaQueue = mangaQueue;
			this.simpleNotificationService = simpleNotificationService;
			this.fileSaverService = fileSaverService;
			this.factory = factory;
			this.requestHelper = requestHelper;

			ItemSelectedCommand = new AsyncRelayCommand<object>(ChapterSelected);
			BookmarkManga = new AsyncRelayCommand(AddBookmarkManga);
			DownloadMissingManga = new AsyncRelayCommand(OnDownloadMissingManga);
			Refesh = new AsyncRelayCommand(OnRefresh);
			DeleteManga = new AsyncRelayCommand(OnDeleteManga);

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

			if (_lastManga != null && manga == _lastManga) {
				var cToShow = Chapters.Where(c => !settingsService.GetChapterReaded(c)).ToList();
				Chapters.Clear();
				Chapters.AddRange(cToShow);

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
			Chapters.AddRange(chaptersToShow);

			Genres.Clear();
			Genres.AddRange(manga.Genres);

			string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
			if (File.Exists(pathWithFile)) {
				CoverUrlImageSource = ImageSource.FromFile(pathWithFile);
			} else {
				var mem = await (new RequestHelper()).DoGetRequestStream(manga.CoverUrl, manga.RequestHeaders);
				if (mem != null) {
					CoverUrlImageSource = ImageSource.FromStream(() => mem);
				}
			}

			IsSearching = false;
		}

		public async Task ChapterSelected(object? chapterObj)
		{
			if (SelectedItem != null) {
				inMemoryDatabase.Set<IChapter>("selectedChapter", SelectedItem);
				SelectedItem = null;

				await navigation.GoToReadChapter();
			}
		}

		public async Task AddBookmarkManga()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			settingsService.BookmarkManga(manga);

			await manga.Save();

			string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
			if (!File.Exists(pathWithFile)) {
				await requestHelper.DownloadFile(manga.CoverUrl, pathWithFile, 3, manga.RequestHeaders);
			}
		}

		public Task OnDeleteManga()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			settingsService.RemoveManga(manga);

			fileSaverService.DeleteManga(manga);
			return navigation.CloseCurrent();
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

			var allChapters = await manga.GetBooks();

			foreach (var chapter in allChapters) {
				if (fileSaverService.FileExists(chapter)) {
					IChapter chapterObj = chapter;
					if (chapter is SaveableChapter) {
						var allPageUrls = await chapter.GetPageUrls(false, factory);
						var mappedUrls = chapter.UrlToLocalFileMapper;

						// check that all images are downloaded
						var allImagesExists = mappedUrls.Values.All(f => File.Exists(f));
						if (!allImagesExists) {
							fileSaverService.DeleteChapterFile(chapterObj);
							await fileSaverService.DeleteImagesFromChapter(chapterObj, factory);
						}
					}
				}
			}

			IsSearching = false;
		}
	}
}
