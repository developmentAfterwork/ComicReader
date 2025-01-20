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
			Routing.RegisterRoute("ReaderNews", typeof(ReaderNewsView));
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

		public async Task GotoReaderNews()
		{
			await Shell.Current.GoToAsync("/ReaderNews");
		}
	}
}
