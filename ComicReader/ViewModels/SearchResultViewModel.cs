using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
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
		private ObservableCollection<IManga> _SearchResult = new ObservableCollection<IManga>();

		[ObservableProperty]
		private bool _IsSearching = true;

		public ICommand ItemSelectedCommand { get; set; }

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

				SearchResult.Clear();
				SearchResult.AddRange(searchResults.Values.SelectMany(t => t));

				IsSearching = false;
			});
		}

		public async Task MangaSelected(object? mangaObj)
		{
			IManga? manga = mangaObj as IManga;

			if (manga != null) {
				inMemoryDatabase.Set<IManga>("selectedManga", manga);

				await navigation.GoToMangaDetails();
			}
		}
	}
}
