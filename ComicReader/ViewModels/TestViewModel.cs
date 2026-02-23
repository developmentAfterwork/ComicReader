using ComicReader.Interpreter;
using ComicReader.Reader;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ComicReader.ViewModels
{
	public partial class TestViewModel : ObservableObject
	{
		private readonly Factory factory;

		private Task? testTask;

		[ObservableProperty]
		private ObservableCollection<string> _testResults = new();

		public TestViewModel(Factory factory)
		{
			this.factory = factory;
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

			IManga? mangaWithChapters = null;
			foreach (var manga in mangas) {
				try {
					var chp = await manga.GetBooks();
					if (chp != null && chp.Count > 0) {
						mangaWithChapters = manga;
						break;
					}
				} catch {
					// Ignore errors and continue searching for a manga with chapters
				}
			}

			if (mangaWithChapters == null) {
				TestResults.Add("Update: no manga");
				return;
			}

			var chapters = await mangaWithChapters.GetBooks();
			if (chapters == null || chapters.Count == 0) {
				TestResults.Add("Update: no chapters");
				return;
			}

			var chapter = chapters.First();
			var pages = await chapter.GetPageUrls(false, factory);
			if (pages == null || pages.Count == 0) {
				TestResults.Add("Update: no pages");
				return;
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

			IManga? mangaWithChapters = null;
			foreach (var manga in mangas) {
				try {
					var chp = await manga.GetBooks();
					if (chp != null && chp.Count > 0) {
						mangaWithChapters = manga;
						break;
					}
				} catch {
					// Ignore errors and continue searching for a manga with chapters
				}
			}

			if (mangaWithChapters == null) {
				TestResults.Add("Search: no manga");
				return;
			}

			var chapters = await mangaWithChapters.GetBooks();
			if (chapters == null || chapters.Count == 0) {
				TestResults.Add("Search: no chapters");
				return;
			}

			var chapter = chapters.First();
			var pages = await chapter.GetPageUrls(false, factory);
			if (pages == null || pages.Count == 0) {
				TestResults.Add("Search: no pages");
				return;
			}

			TestResults.Add("Search: completed");
		}
	}
}
