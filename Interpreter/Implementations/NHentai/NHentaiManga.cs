using ComicReader.Helper;

namespace ComicReader.Interpreter.Implementations.NHentai
{
	internal class NHentaiManga : IManga
	{
		private readonly RequestHelper _requestHelper;
		private readonly HtmlHelper _htmlHelper;

		public string? ID { get; }

		public string Name { get; }

		public string HomeUrl { get; }

		public string CoverUrl { get; }

		public string Autor { get; }

		public string Status { get; }

		public string LanguageFlagUrl { get; }

		public string Description { get; }

		public List<string> Genres { get; }

		public Dictionary<string, string>? RequestHeaders => null;

		public string Source { get; }

		public NHentaiManga(string name,
			string homeUrl,
			string coverUrl,
			string autor,
			string status,
			string languageFlagUrl,
			string description,
			List<string> genres,
			string source,
			RequestHelper requestHelper,
			HtmlHelper htmlHelper)
		{
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;

			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;

			Autor = autor;
			Status = status;
			LanguageFlagUrl = languageFlagUrl;
			Description = description;
			Genres = genres;
			Source = source;
		}

		public Task<List<IChapter>> GetBooks()
		{
			List<IChapter> r = new List<IChapter>() { new NHentaiChapter(ID, Source, Name, "Part 1", HomeUrl, "unknown", _requestHelper, _htmlHelper) };
			return Task.FromResult(r);
		}
	}
}
