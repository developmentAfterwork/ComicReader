using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CsQuery.ExtensionMethods;
using Interpreter.Interface;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ComicReader.ViewModels
{
	public partial class LibraryViewModel : ObservableObject
	{
		private readonly SettingsService settingsService;
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;
		private readonly Factory factory;
		private readonly IRequest request;

		[ObservableProperty]
		private bool _isBusy = false;

		[ObservableProperty]
		private ObservableCollection<MangaViewModel> _BookmarkedMangas = new ObservableCollection<MangaViewModel>();

		public LibraryViewModel(SettingsService settingsService, InMemoryDatabase inMemoryDatabase, Navigation navigation, Factory factory, IRequest request)
		{
			this.settingsService = settingsService;
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;
			this.factory = factory;
			this.request = request;
		}

		public Task OnAppearing()
		{
			IsBusy = true;

			return Task.Run(async () => {
				List<string> bookmarkedUniqueIds = new();

				BookmarkedMangas.ForEach(b => b.Selected -= OnMangeSelected);
				BookmarkedMangas.Clear();

				bookmarkedUniqueIds = settingsService.GetBookmarkedMangaUniqIdentifiers();

				ObservableCollection<MangaViewModel> m = new();
				BookmarkedMangas = m;

				IsBusy = false;

				List<IManga> cachedMangas = new();
				foreach (var bookmarkId in bookmarkedUniqueIds.Where(s => s.Contains("|"))) {
					try {
						IManga? manga = await factory.GetMangaFromBookmarkId(bookmarkId);
						if (manga == null)
							continue;

						cachedMangas.Add(manga);
					} catch { }
				}

				foreach (var manga in cachedMangas.OrderBy(c => c.IsFavorite ? 1 : 2).ThenBy(c => c.Name)) {
					try {
						MangaViewModel model = new MangaViewModel(manga, request, settingsService);
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
					} catch { }
				}
			});
		}

		private async void OnMangeSelected(object? sender, IManga e)
		{
			inMemoryDatabase.Set<IManga>("selectedManga", e);

			await navigation.GoToMangaDetails();
		}
	}
}
