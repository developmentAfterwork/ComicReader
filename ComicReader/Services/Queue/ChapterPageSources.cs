using ComicReader.Interpreter;

namespace ComicReader.Services.Queue
{
	public record ChapterPageSources
	{
		public string Source { get; set; } = string.Empty;

		public string MangaName { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;

		public Dictionary<string, string> UrlToLocalFileMapper { get; set; } = new();

		public Dictionary<string, string>? RequestHeaders { get; set; } = null;

		public ChapterPageSources()
		{

		}

		public ChapterPageSources(IChapter chapter, Factory factory)
		{
			Source = chapter.Source;
			MangaName = chapter.MangaName;
			Title = chapter.Title;
			UrlToLocalFileMapper = chapter.UrlToLocalFileMapper;
			RequestHeaders = chapter.RequestHeaders;

			if (RequestHeaders is null || RequestHeaders.Count == 0) {
				var header = chapter.RequestHeaders;
				var sc = chapter as SaveableChapter;
				if (header is null && sc is not null) {
					header = factory.GetOriginChapter(sc).RequestHeaders;
				}
			}
		}
	}
}
