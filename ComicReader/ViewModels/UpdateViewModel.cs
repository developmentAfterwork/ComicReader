using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ComicReader.Helper;
using System.Collections.ObjectModel;
using CsQuery.ExtensionMethods.Internal;
using ComicReader.Views.Models;
using ComicReader.Services.Queue;

namespace ComicReader.ViewModels
{
	public partial class UpdateViewModel : ObservableObject
	{
		public ICommand SearchForUpdatesCommand { get; set; }

		[ObservableProperty]
		private bool _IsSearching = false;

		private readonly SettingsService settingsService;
		private readonly Factory mangaFactory;
		private readonly SimpleNotificationService simpleNotificationService;
		private readonly MangaQueue mangaQueue;

		[ObservableProperty]
		private ObservableCollection<MangaChapterViewModel> _newChapters = new ObservableCollection<MangaChapterViewModel>();

		[ObservableProperty]
		private bool _HasChapters = false;

		public UpdateViewModel(SettingsService settingsService, Factory mangaFactory, SimpleNotificationService simpleNotificationService, MangaQueue mangaQueue)
		{
			SearchForUpdatesCommand = new AsyncRelayCommand(OnSearchingForUpdates);
			this.settingsService = settingsService;
			this.mangaFactory = mangaFactory;
			this.simpleNotificationService = simpleNotificationService;
			this.mangaQueue = mangaQueue;
		}

		private async Task OnSearchingForUpdates()
		{
			IsSearching = true;
			HasChapters = false;

			var allBookmarkedMangas = settingsService.GetBookmarkedMangaUniqIdentifiers();

			List<MangaChapterViewModel> chaptersForUpdate = new List<MangaChapterViewModel>();

			var bookmarked = allBookmarkedMangas.Where(s => s.Contains("|")).ToList();
			foreach (var bookmarkId in bookmarked) {
				try {
					// TODO: check if bookmarked ID mangaFile exists
					IManga saveableManga = await mangaFactory.GetMangaFromBookmarkId(bookmarkId).ConfigureAwait(false);
					var originManga = mangaFactory.GetOriginManga((SaveableManga)saveableManga);

					var savedChapters = (await saveableManga.GetBooks()).ToDictionary(d => d.GetUniqIdentifier(), d => d);
					var newChapters = (await originManga.GetBooks()).ToDictionary(d => d.GetUniqIdentifier(), d => d);
					var diff = newChapters.Keys.Except(savedChapters.Keys).ToList();

					if (diff.Any()) {
						foreach (var key in diff) {
							chaptersForUpdate.Add(new MangaChapterViewModel(saveableManga, newChapters[key]));
						}

						await originManga.Save();
					}
				} catch (Exception ex) {
					await simpleNotificationService.ShowError("Error", ex.Message);
				}
			}

			NewChapters.Clear();
			NewChapters.AddRange(chaptersForUpdate);
			HasChapters = NewChapters.Any();

			foreach (var chapter in NewChapters) {
				await mangaQueue.AddChapter(chapter.Chapter);
			}

			IsSearching = false;
		}
	}
}
