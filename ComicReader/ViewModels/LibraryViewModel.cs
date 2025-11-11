using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CsQuery.ExtensionMethods;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ComicReader.ViewModels {
	public partial class LibraryViewModel : ObservableObject {
		private readonly SettingsService settingsService;
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;
		private readonly Factory factory;

		[ObservableProperty]
		private bool _isBusy = false;

		[ObservableProperty]
		private ObservableCollection<MangaViewModel> _BookmarkedMangas = new ObservableCollection<MangaViewModel>();

		public LibraryViewModel(SettingsService settingsService, InMemoryDatabase inMemoryDatabase, Navigation navigation, Factory factory) {
			this.settingsService = settingsService;
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;
			this.factory = factory;
		}

		public async Task OnAppearing() {
			IsBusy = true;

			List<string> bookmarkedUniqIds = new();

			BookmarkedMangas.ForEach(b => b.Selected -= OnMangeSelected);
			BookmarkedMangas.Clear();

			bookmarkedUniqIds = settingsService.GetBookmarkedMangaUniqIdentifiers();

			ObservableCollection<MangaViewModel> m = new();
			BookmarkedMangas = m;

			IsBusy = false;

			foreach (var bookmarkId in bookmarkedUniqIds.Where(s => s.Contains("|"))) {
				Stopwatch sw = Stopwatch.StartNew();
				try {
					IManga manga = await factory.GetMangaFromBookmarkId(bookmarkId);

					MangaViewModel model = new MangaViewModel(manga);
					model.Selected += OnMangeSelected;

					if (settingsService.GetHideEmptyManga()) {
						try {
							var chapters = await manga.GetBooks().ConfigureAwait(false);
							var toGo = chapters.Where(c => !settingsService.GetChapterReaded(c)).ToList();

							if (toGo.Any()) {
								m.Add(model);
							}
						} catch { }
					} else {
						m.Add(model);
					}
				} catch (Exception ex) {
					var e = ex.ToString();
				}
				sw.Stop();
				Console.WriteLine($"Loaded manga {bookmarkId} in {sw.ElapsedMilliseconds} ms");
			}
		}

		private async void OnMangeSelected(object? sender, IManga e) {
			inMemoryDatabase.Set<IManga>("selectedManga", e);

			await navigation.GoToMangaDetails();
		}
	}
}
