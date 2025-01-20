namespace ComicReader.Interpreter
{
	public interface IChapter
	{
		string Source { get; }

		string MangaName { get; }

		string Title { get; }

		string HomeUrl { get; }

		string LastUpdate { get; }

		Task<List<string>> GetPageUrls(bool preDownloadChapters, Factory factory);

		Dictionary<string, string> UrlToLocalFileMapper { get; }

		Dictionary<string, string>? RequestHeaders { get; }
	}
}
