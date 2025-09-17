using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.ViewModels.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;

namespace ComicReader.ViewModels
{
	public partial class AllReaderNewsViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;

		[ObservableProperty]
		private ObservableCollection<IMangaModelGroup> _SearchResultGroup = [];

		[ObservableProperty]
		private bool _IsSearching = true;

		public AllReaderNewsViewModel(InMemoryDatabase inMemoryDatabase, Navigation navigation)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;
		}

		public void OnAppearing()
		{
			if (IsSearching == false) {
				return;
			}

			List<IReader> activeReaders = inMemoryDatabase.Get<List<IReader>>("activeReaders");

			SearchResultGroup = new ObservableCollection<IMangaModelGroup>();
			List<IMangaModelGroup> mangaModelsGroup = new List<IMangaModelGroup>();
			foreach (IReader reader in activeReaders) {
				mangaModelsGroup.Add(new IMangaModelGroup() {
					Source = reader.Title,
					IsSearching = true,
					Mangas = new()
				});
			}
			SearchResultGroup.AddRange(mangaModelsGroup);
			IsSearching = false;

			_ = Task.Run(async () => {
				foreach (IReader reader in activeReaders) {
					await reader.LoadUpdatesAndNewMangs().ContinueWith(async a => {
						List<IManga> r = a.Result;
						List<IMangaModel> mList = new();
						foreach (var manga in r) {
							var mangaModel = await IMangaModel.Create(manga, manga.RequestHeaders);
							mList.Add(mangaModel);
						}

						var group = SearchResultGroup.First(m => m.Source == (mList.FirstOrDefault()?.Source ?? ""));
						if (group != null) {
							group.Mangas.AddRange(mList);
							group.IsSearching = false;
						}
					});
				}
			});
		}

		public async Task MangaSelected(object? mangaObj)
		{
			if (mangaObj is IMangaModel model) {
				inMemoryDatabase.Set<IManga>("selectedManga", model.Manga);

				await navigation.GoToMangaDetails();
			}
		}
	}
}
