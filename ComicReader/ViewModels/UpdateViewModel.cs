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
using Newtonsoft.Json;

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
			int currentChapter = 0;

			foreach (var bookmarkId in bookmarked) {
				var messageTxt = $"Mangas: {++currentChapter}/{bookmarked.Count}";
				await simpleNotificationService.ShowProgress("Check mangas", messageTxt, currentChapter, bookmarked.Count);

				try {
					// TODO: check if bookmarked ID mangaFile exists
					IManga? manga = null;
					try {
						manga = await mangaFactory.GetMangaFromBookmarkId(bookmarkId).ConfigureAwait(false);
					} catch { }

					if (manga == null) {
						continue;
					}

					SaveableManga? saveableManga = manga as SaveableManga;
					if (saveableManga == null) {
						continue;
					}

					var originManga = mangaFactory.GetOriginManga(saveableManga);

					Dictionary<string, List<IChapter>> savedChapters = new Dictionary<string, List<IChapter>>();
					foreach (var chapter in await manga.GetBooks()) {
						var key = chapter.GetUniqIdentifier();
						if (!savedChapters.ContainsKey(key)) {
							savedChapters.Add(key, new());
						}

						savedChapters[key].Add(chapter);
					}

					Dictionary<string, List<IChapter>> newChapters = new Dictionary<string, List<IChapter>>();
					foreach (var chapter in await originManga.GetBooks()) {
						var key = chapter.GetUniqIdentifier();
						if (!newChapters.ContainsKey(key)) {
							newChapters.Add(key, new());
						}

						newChapters[key].Add(chapter);
					}

					var diff = newChapters.Keys.Except(savedChapters.Keys).ToList();
					if (diff.Any()) {
						foreach (var key in diff) {
							if (newChapters.ContainsKey(key)) {
								foreach (var c in newChapters[key]) {
									chaptersForUpdate.Add(new MangaChapterViewModel(manga, c));
								}
							} else {
								await simpleNotificationService.ShowNotification("Chapter not found in new chapters", $"{key} - {bookmarkId} - {JsonConvert.SerializeObject(newChapters.Keys)}");
							}
						}

						await originManga.Save();
					}
				} catch (Exception ex) {
					await simpleNotificationService.ShowError("Error Diff", $"{bookmarkId} - {ex.Message}");
				}
			}

			NewChapters.Clear();
			NewChapters.AddRange(chaptersForUpdate);
			HasChapters = NewChapters.Any();

			if (settingsService.GetAutoAddChaptersToQueue()) {
				foreach (var chapter in NewChapters) {
					try {
						await mangaQueue.AddChapter(chapter.Chapter);
					} catch (Exception ex) {
						await simpleNotificationService.ShowError("Error Queue", $"{chapter.Chapter.MangaName} - {chapter.Chapter.Title} - {ex.Message}");
					}
				}
			}

			IsSearching = false;
		}
	}
}
