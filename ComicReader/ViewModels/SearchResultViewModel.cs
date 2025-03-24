using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.ViewModels.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class SearchResultViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;

		[ObservableProperty]
		private ObservableCollection<IMangaModel> _SearchResult = new ObservableCollection<IMangaModel>();

		[ObservableProperty]
		private bool _IsSearching = true;

		[ObservableProperty]
		private ICommand _ItemSelectedCommand;

		[ObservableProperty]
		private IMangaModel _SelectedItem;

		private string lastSearchWord = "";

		public SearchResultViewModel(InMemoryDatabase inMemoryDatabase, Navigation navigation)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;

			ItemSelectedCommand = new AsyncRelayCommand<object>(MangaSelected);
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

				List<IMangaModel> mangaModels = new List<IMangaModel>();
				foreach (var manga in searchResults.Values.SelectMany(s => s)) {
					mangaModels.Add(await IMangaModel.Create(manga, manga.RequestHeaders));
				}

				SearchResult.Clear();
				SearchResult.AddRange(mangaModels);

				IsSearching = false;
			});
		}

		public async Task MangaSelected(object? mangaObj)
		{
			if (SelectedItem != null) {
				inMemoryDatabase.Set<IManga>("selectedManga", SelectedItem.Manga);
				SelectedItem = null;

				await navigation.GoToMangaDetails();
			}
		}
	}
}
