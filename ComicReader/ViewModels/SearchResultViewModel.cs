using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.ViewModels.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;

namespace ComicReader.ViewModels
{
	public partial class SearchResultViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;

		[ObservableProperty]
		private ObservableCollection<IMangaModelGroup> _SearchResultGroup = new ObservableCollection<IMangaModelGroup>();

		[ObservableProperty]
		private bool _IsSearching = true;

		private string lastSearchWord = "";

		public SearchResultViewModel(InMemoryDatabase inMemoryDatabase, Navigation navigation)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;
		}

		public void OnAppearing()
		{
			string searchWords = inMemoryDatabase.Get<string>("searchText");
			IsSearching = !(lastSearchWord == searchWords);

			if (IsSearching == false) {
				return;
			}

			lastSearchWord = searchWords;
			List<IReader> activeReaders = inMemoryDatabase.Get<List<IReader>>("activeReaders");

			var searchTasks = new Dictionary<string, Task<List<IManga>>>();

			_ = Task.Run(async () => {
				foreach (IReader reader in activeReaders) {
					searchTasks.Add(reader.Title, reader.Search(searchWords));
				}

				await Task.WhenAll(searchTasks.Values.ToArray());

				var searchResults = searchTasks.ToDictionary(a => a.Key, a => a.Value.Result);

				List<IMangaModelGroup> mangaModelsGroup = new List<IMangaModelGroup>();
				foreach (var manga in searchResults.Values.SelectMany(s => s)) {
					var mangaModel = await IMangaModel.Create(manga, manga.RequestHeaders);

					if (!mangaModelsGroup.Any(m => m.Source == mangaModel.Source)) {
						mangaModelsGroup.Add(new IMangaModelGroup() {
							Source = manga.Source,
							Mangas = new()
						});
					}

					mangaModelsGroup.First(m => m.Source == mangaModel.Source).Mangas.Add(mangaModel);
				}

				SearchResultGroup.Clear();
				SearchResultGroup.AddRange(mangaModelsGroup);

				IsSearching = false;
			});
		}

		public async Task MangaSelected(object? mangaObj)
		{
			var model = mangaObj as IMangaModel;
			if (model != null) {
				inMemoryDatabase.Set<IManga>("selectedManga", model.Manga);

				await navigation.GoToMangaDetails();
			}
		}
	}
}
