using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using FFImageLoading;
using FFImageLoading.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels {
	public partial class ReadChapterViewModel : ObservableObject {
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly SettingsService settingsService;
		private readonly Factory factory;
		private readonly FileSaverService fileSaverService;

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

		public ReadChapterViewModel(InMemoryDatabase inMemoryDatabase, SettingsService settingsService, Factory factory, FileSaverService fileSaverService) {
			this.inMemoryDatabase = inMemoryDatabase;
			this.settingsService = settingsService;
			this.factory = factory;
			this.fileSaverService = fileSaverService;

			OpenInBrowser = new RelayCommand(() => {
				var chapter = Chapter;
				Browser.OpenAsync(chapter.HomeUrl, BrowserLaunchMode.SystemPreferred);
			});

			StaticClassHolder<Singleton<InMemoryDatabase>>.Value = new Singleton<InMemoryDatabase>(inMemoryDatabase);
		}

		public void OnAppearing() {
			_ = InitChapter(inMemoryDatabase.Get<IChapter>("selectedChapter"));
		}

		private async Task InitChapter(IChapter chapter) {
			IsLoading = true;

			try {
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
				await MainThread.InvokeOnMainThreadAsync(async () => {
					var p = await chapter.GetPageUrls(predownloadFiles, factory);
					pages = p;
				});

				inMemoryDatabase.Set<IChapter>("selectedChapter", chapter);
				inMemoryDatabase.Set<IChapter>("ichapterParameter", chapter);

				Chapter = chapter;

				Pages.Clear();
				Pages.AddRange(pages);
				if (automaticSwitchToNextChapter) {
					Pages.Add(pages.Last());
				}

				Position = $"1/{Pages.Count}";

				var saved = settingsService.GetSaveChapterPosition(Chapter);
				if (saved > 0) {
					_ = Task.Run(async () => {
						await Task.Delay(500);
						SelectedItem = Pages[saved - 1];
					});
				} else {
					SelectedItem = Pages[0];
				}
			} finally {
				IsLoading = false;
			}
		}

		public async Task Scrolled(int position) {
			CurrentPage = Pages[position];

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

		private bool IsReadedToEnd(int currentPosition) {
			return currentPosition >= Pages.Count && currentPosition >= Chapter.UrlToLocalFileMapper.Count && Chapter.UrlToLocalFileMapper.Count > 0;
		}

		private async Task SelectNextChapter() {
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			var chapters = await manga.GetBooks();
			var currentIndex = chapters.IndexOf(Chapter);

			if (currentIndex > 0 && currentIndex + 1 <= chapters.Count) {
				if (settingsService.GetDeleteChaptersAfterReading()) {
					await fileSaverService.DeleteImagesFromChapter(chapters[currentIndex], factory);
				}

				Pages.Clear();
				await InitChapter(chapters[currentIndex + 1]);
			}
		}

		internal void OnDisappearing() {
			var predownloadFiles = settingsService.GetPreDownloadImages();

			if (!predownloadFiles) {
				fileSaverService.DeleteAllEmptyFolders();
			}
		}
	}
}
