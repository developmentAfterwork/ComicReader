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
					IManga? saveableManga = await mangaFactory.GetMangaFromBookmarkId(bookmarkId).ConfigureAwait(false);
					if (saveableManga == null) {
						continue;
					}

					var originManga = mangaFactory.GetOriginManga((SaveableManga)saveableManga);

					Dictionary<string, List<IChapter>> savedChapters = new Dictionary<string, List<IChapter>>();
					foreach (var chapter in (await saveableManga.GetBooks()).Take(1)) {
						var key = chapter.GetUniqIdentifier();
						if (savedChapters.ContainsKey(key) == false) {
							savedChapters.Add(key, new());
						}

						savedChapters[chapter.GetUniqIdentifier()].Add(chapter);
					}

					Dictionary<string, List<IChapter>> newChapters = new Dictionary<string, List<IChapter>>();
					foreach (var chapter in await originManga.GetBooks()) {
						var key = chapter.GetUniqIdentifier();
						if (newChapters.ContainsKey(key) == false) {
							newChapters.Add(key, new());
						}

						newChapters[chapter.GetUniqIdentifier()].Add(chapter);
					}

					var diff = newChapters.Keys.Except(savedChapters.Keys).ToList();

					if (diff.Any()) {
						foreach (var key in diff) {
							foreach (var c in newChapters[key]) {
								chaptersForUpdate.Add(new MangaChapterViewModel(saveableManga, c));
							}
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

			if (settingsService.GetAutoAddChaptersToQueue()) {
				foreach (var chapter in NewChapters) {
					await mangaQueue.AddChapter(chapter.Chapter);
				}
			}

			IsSearching = false;
		}
	}
}
