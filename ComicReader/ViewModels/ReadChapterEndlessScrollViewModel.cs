using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using FFImageLoading;
using FFImageLoading.Helpers;
using Interpreter.Interface;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class ReadChapterEndlessScrollViewModel : ObservableObject
	{
		private readonly object syncObject = new object();

		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly SettingsService settingsService;
		private readonly Factory factory;
		private readonly FileSaverService fileSaverService;
		private readonly IRequest requestHelper;

		[ObservableProperty]
		private ObservableCollection<string> _pages = [];

		[ObservableProperty]
		private IChapter _Chapter = new SaveableChapter();

		public ICommand Changed = new RelayCommand(() => { });

		[ObservableProperty]
		private ICommand _OpenInBrowser;

		[ObservableProperty]
		private string _position = "0/0";

		[ObservableProperty]
		private bool _allowSwipe = true;

		[ObservableProperty]
		private string _selectedItem = string.Empty;

		[ObservableProperty]
		private string? _currentPage = null;

		[ObservableProperty]
		private bool _isLoading = false;

		private readonly bool automaticSwitchToNextChapter = true;

		public event EventHandler<(int index, int targetAmount)>? PageIndexChanged;

		public ReadChapterEndlessScrollViewModel(InMemoryDatabase inMemoryDatabase, SettingsService settingsService, Factory factory, FileSaverService fileSaverService, IRequest requestHelper)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.settingsService = settingsService;
			this.factory = factory;
			this.fileSaverService = fileSaverService;
			this.requestHelper = requestHelper;

			OpenInBrowser = new RelayCommand(() => {
				var chapter = Chapter;
				Browser.OpenAsync(chapter.HomeUrl, BrowserLaunchMode.SystemPreferred);
			});

			StaticClassHolder<Singleton<InMemoryDatabase>>.Value = new Singleton<InMemoryDatabase>(inMemoryDatabase);
		}

		public void OnAppearing()
		{
			_ = InitChapter(inMemoryDatabase.Get<IChapter>("selectedChapter"));
		}

		private async Task InitChapter(IChapter chapter)
		{
			IsLoading = true;

			try {
				var requestTimeout = settingsService.GetRequestTimeout();

				var predownloadFiles = settingsService.GetPreDownloadImages();

				var ffConfig = new FFImageLoading.Config.Configuration();
				if (chapter.RequestHeaders != null) {
					foreach (var header in chapter.RequestHeaders) {
						ffConfig.HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
					}
				}

				var imageService = ServiceHelper.GetService<IImageService>();
				imageService.Initialize(ffConfig);

				List<string> pages = [];
				await Task.Run(async () => {
					var p = await chapter.GetPageUrls(predownloadFiles, factory);
					fileSaverService.CheckFiles(chapter.UrlToLocalFileMapper.Values.ToList());

					if (predownloadFiles) {
						await chapter.DownloadChapterPages(
							requestTimeout,
							factory,
							() => {
								return Task.CompletedTask;
							},
							(ex) => { },
							settingsService,
							requestHelper,
							fileSaverService
						);

						fileSaverService.CheckFiles(chapter.UrlToLocalFileMapper.Values.ToList());
					}

					await MainThread.InvokeOnMainThreadAsync(() => {
						pages = p;
					});
				});

				inMemoryDatabase.Set<IChapter>("selectedChapter", chapter);
				inMemoryDatabase.Set<IChapter>("ichapterParameter", chapter);

				Chapter = chapter;

				Pages.Clear();

				IsLoading = false;
				Pages.AddRange(pages);
				Position = $"1/{Pages.Count}";

				if (automaticSwitchToNextChapter) {
					await Task.Delay(50);
					Pages.Add(pages.Last());
					Position = $"1/{Pages.Count}";
				}

				var saved = settingsService.GetSaveChapterPosition(Chapter);
				if (saved > 0 && settingsService.GetHideEmptyManga()) {
					var c = pages.Count;
					if (automaticSwitchToNextChapter) {
						c++;
					}

					RestorePosition(saved, c);
				} else {
					SelectedItem = Pages[0];
					CurrentPage = Pages[0];
				}
			} finally {
				IsLoading = false;
			}
		}

		private int position = -1;
		public async Task Scrolled(int position)
		{
			lock (syncObject) {
				if (this.position == position) {
					return;
				}

				this.position = position;
			}

			if (Pages.Count == 0) {
				return;
			}

			CurrentPage = Pages[Math.Clamp(position, 0, Pages.Count)];

			int currentPosition = position + 1;

			Position = $"{currentPosition}/{Pages.Count}";

			var saved = settingsService.GetSaveChapterPosition(Chapter);
			if (currentPosition > saved) {
				settingsService.SetSaveChapterPosition(Chapter, currentPosition);
			}

			if (IsReadedToEnd(currentPosition)) {
				settingsService.SetChapterAsReaded(Chapter);
				if (automaticSwitchToNextChapter) {
					await SelectNextChapter();
				}
			}
		}

		private bool IsReadedToEnd(int currentPosition)
		{
			return currentPosition >= Pages.Count && currentPosition >= Chapter.UrlToLocalFileMapper.Count && Chapter.UrlToLocalFileMapper.Count > 0;
		}

		private async Task SelectNextChapter()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			var chapters = await manga.GetBooks();
			var currentIndex = chapters.IndexOf(Chapter);

			if (currentIndex >= 0 && currentIndex + 1 <= chapters.Count) {
				if (settingsService.GetDeleteChaptersAfterReading()) {
					await fileSaverService.DeleteImagesFromChapter(chapters[currentIndex], factory);
				}

				Pages.Clear();
				await InitChapter(chapters[Math.Clamp(currentIndex + 1, 0, chapters.Count)]);
			}
		}

		internal void OnDisappearing()
		{
			var predownloadFiles = settingsService.GetPreDownloadImages();

			if (!predownloadFiles) {
				fileSaverService.DeleteAllEmptyFolders();
			}
		}

		internal void RestorePosition(int saved, int targetPageAmount)
		{
			if (Pages.Count > 0) {
				if (saved > 0) {
					_ = Task.Run(async () => {
						await Task.Delay(500);
						SelectedItem = Pages[saved - 1];
						PageIndexChanged?.Invoke(this, (saved - 1, targetPageAmount));
					});
				}
			}
		}
	}
}
