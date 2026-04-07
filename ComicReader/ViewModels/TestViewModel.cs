using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using static Android.Graphics.Drawables.ShapeDrawable;

namespace ComicReader.ViewModels
{
	public partial class TestViewModel : ObservableObject
	{
		private readonly Factory factory;
		private readonly SettingsService settingsService;

		private Task? testTask;

		[ObservableProperty]
		private ObservableCollection<string> _testResults = new();

		public TestViewModel(Factory factory, SettingsService settingsService)
		{
			this.factory = factory;
			this.settingsService = settingsService;
		}

		public void OnAppearing()
		{
			if (testTask == null) {
				TestResults.Clear();
				testTask = Task.Run(RunTests);
			}
		}

		private async Task RunTests()
		{
			TestResults.Add("Begin tests");

			foreach (var readerFactory in factory.CreateAllReaders()) {
				try {
					TestResults.Add(readerFactory.Title);
					await RunUpdateAndNewMangasTest(readerFactory);
					await RunSearchTest(readerFactory, "Demon");
				} catch (Exception ex) {
					TestResults.Add(ex.Message);
				}
			}

			try {
				await TestCurrentMangas();
			} catch (Exception ex) {
				TestResults.Add(ex.Message);
			}

			TestResults.Add("Tests completed");

			testTask = null;
		}

		private async Task RunUpdateAndNewMangasTest(IReader reader)
		{
			var mangas = await reader.LoadUpdatesAndNewMangs();
			if (mangas == null || mangas.Count == 0) {
				TestResults.Add("Update: no mangas");
				return;
			}

			bool foundMangaWithPages = false;
			foreach (var manga in mangas) {
				try {
					var chapters = await manga.GetBooks();
					if (chapters != null && chapters.Count > 0) {
						foreach (var chapter in chapters) {
							var pages = await chapter.GetPageUrls(false, factory);
							if (pages != null && pages.Count > 0) {
								foundMangaWithPages = true;
								break;
							}
						}

						if (foundMangaWithPages) {
							break;
						}
					}
				} catch { }
			}

			if (!foundMangaWithPages) {
				TestResults.Add("failed");
			}

			TestResults.Add("Update: completed");
		}

		private async Task RunSearchTest(IReader reader, string keyword)
		{
			var mangas = await reader.Search(keyword);
			if (mangas == null || mangas.Count == 0) {
				TestResults.Add("Search: no mangas");
				return;
			}

			bool foundMangaWithPages = false;
			foreach (var manga in mangas) {
				try {
					var chapters = await manga.GetBooks();
					if (chapters != null && chapters.Count > 0) {
						foreach (var chapter in chapters) {
							var pages = await chapter.GetPageUrls(false, factory);
							if (pages != null && pages.Count > 0) {
								foundMangaWithPages = true;
								break;
							}
						}

						if (foundMangaWithPages) {
							break;
						}
					}
				} catch { }
			}

			if (!foundMangaWithPages) {
				TestResults.Add("failed");
			}

			TestResults.Add("Search: completed");
		}

		private async Task TestCurrentMangas()
		{
			List<string> bookmarkedUniqueIds = settingsService.GetBookmarkedMangaUniqIdentifiers();

			List<IManga> cachedMangas = new();
			foreach (var bookmarkId in bookmarkedUniqueIds.Where(s => s.Contains("|"))) {
				try {
					IManga? manga = await factory.GetMangaFromBookmarkId(bookmarkId);
					if (manga == null)
						continue;

					cachedMangas.Add(manga);
				} catch (Exception ex) {
					TestResults.Add($"Error loading manga: " + $"bookmark id is {bookmarkId}: {ex.Message}");
				}
			}

			foreach (var manga in cachedMangas) {
				try {
					var save = manga as SaveableManga;
					if (save == null)
						continue;

					var orgManga = factory.GetOriginManga(save);
					var books = await orgManga.GetBooks();

					var book = books.First();
					var pages = book.GetPageUrls(false, factory);
					TestResults.Add($"{manga.Name} completed");

				} catch (Exception ex) {
					TestResults.Add($"Error loading manga: " + $"bookmark id is {manga.Name}: {ex.Message}");
				}
			}
		}
	}
}
