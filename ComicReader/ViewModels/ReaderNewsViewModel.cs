using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using ComicReader.ViewModels.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using Interpreter.Interface;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class ReaderNewsViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;
		private readonly IRequest request;
		private readonly SettingsService settingsService;

		[ObservableProperty]
		private ObservableCollection<IMangaModel> _LoadResult = new ObservableCollection<IMangaModel>();

		[ObservableProperty]
		private bool _IsLoading = false;

		[ObservableProperty]
		private ICommand _ItemSelectedCommand;

		[ObservableProperty]
		private IMangaModel? _SelectedItem;

		public ReaderNewsViewModel(InMemoryDatabase inMemoryDatabase, Navigation navigation, IRequest request, SettingsService settingsService)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;
			this.request = request;
			this.settingsService = settingsService;
			ItemSelectedCommand = new AsyncRelayCommand<object>(MangaSelected);
		}

		public void OnAppearing()
		{
			if (!IsLoading) {
				IsLoading = true;

				IReader activeReaders = inMemoryDatabase.Get<IReader>("selectedReader");

				_ = Task.Run(async () => {
					var mangas = await activeReaders.LoadUpdatesAndNewMangs();

					List<IMangaModel> mangaModels = new List<IMangaModel>();
					foreach (var manga in mangas) {
						mangaModels.Add(IMangaModel.Create(manga, request, settingsService, manga.RequestHeaders));
					}

					LoadResult.Clear();
					LoadResult.AddRange(mangaModels);

					IsLoading = false;
				});
			}
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
