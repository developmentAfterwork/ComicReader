using ComicReader.Views;

namespace ComicReader.Services
{
	public class Navigation
	{
		public static void Init()
		{
			Routing.RegisterRoute("SearchResult", typeof(SearchResultView));
			Routing.RegisterRoute("MangaDetails", typeof(MangaDetailsView));
			Routing.RegisterRoute("ReadChapter", typeof(ReadChapterView));
			Routing.RegisterRoute("ReadEndlessChapter", typeof(ReadChapterEndlessScrollView));
			Routing.RegisterRoute("ReaderNews", typeof(ReaderNewsView));
			Routing.RegisterRoute("AllReaderNews", typeof(AllReaderNewsView));
		}

		public async Task GoToSearchResult()
		{
			await Shell.Current.GoToAsync("/SearchResult");
		}

		public async Task GoToMangaDetails()
		{
			await Shell.Current.GoToAsync("/MangaDetails");
		}

		public async Task GoToReadChapter()
		{
			await Shell.Current.GoToAsync("/ReadChapter");
		}

		public async Task GoToReadEndlessScrollChapter()
		{
			await Shell.Current.GoToAsync("/ReadEndlessChapter");
		}

		public async Task GotoReaderNews()
		{
			await Shell.Current.GoToAsync("/ReaderNews");
		}

		public async Task GotoAllReaderNews()
		{
			await Shell.Current.GoToAsync("/AllReaderNews");
		}

		internal async Task CloseCurrent()
		{
			await Shell.Current.GoToAsync("..");
		}
	}
}
