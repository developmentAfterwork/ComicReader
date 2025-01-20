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
	public partial class ReaderNewsViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;

		[ObservableProperty]
		private ObservableCollection<IManga> _LoadResult = new ObservableCollection<IManga>();

		[ObservableProperty]
		private bool _IsLoading = true;

		public ICommand ItemSelectedCommand { get; set; }

		public ReaderNewsViewModel(InMemoryDatabase inMemoryDatabase, Navigation navigation)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;

			ItemSelectedCommand = new AsyncRelayCommand<object>(MangaSelected);
		}

		public void OnAppearing()
		{
			if (IsLoading) {
				IsLoading = true;

				IReader activeReaders = inMemoryDatabase.Get<IReader>("selectedReader");

				_ = Task.Run(async () => {
					var mangas = await activeReaders.LoadUpdatesAndNewMangs();

					LoadResult.Clear();
					LoadResult.AddRange(mangas);

					IsLoading = false;
				});
			}
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
