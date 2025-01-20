using ComicReader.Interpreter;

namespace ComicReader.Services.Queue
{
	public record ChapterPageSources
	{
		public string Source { get; set; } = string.Empty;

		public string MangaName { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;

		public Dictionary<string, string> UrlToLocalFileMapper { get; set; } = new();

		public virtual Dictionary<string, string>? RequestHeaders { get; } = null;

		public ChapterPageSources()
		{

		}

		public ChapterPageSources(IChapter chapter)
		{
			Source = chapter.Source;
			MangaName = chapter.MangaName;
			Title = chapter.Title;
			UrlToLocalFileMapper = chapter.UrlToLocalFileMapper;
			RequestHeaders = chapter.RequestHeaders;
		}
	}
}
