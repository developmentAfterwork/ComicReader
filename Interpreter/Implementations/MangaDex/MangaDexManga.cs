using ComicReader.Helper;
using Interpreter.Interface;
using Newtonsoft.Json;

namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexManga : IManga
	{
		private readonly IRequest _requestHelper;
		private readonly HtmlHelper _htmlHelper;
		private readonly TimeSpan _timeout;

		public string ID { get; }

		public string Source { get; }

		public string Name { get; }

		public string HomeUrl { get; }

		public string CoverUrl { get; }

		public string Autor { get; }

		public string Status { get; }

		public string LanguageFlagUrl { get; }

		public string Description { get; }

		public List<string> Genres { get; }

		public Dictionary<string, string>? RequestHeaders => null;

		public MangaDexManga(string? id, string source, string name, string homeUrl, string coverUrl, string autor, string status, string languageFlagUrl, string description, List<string> genres, IRequest requestHelper, HtmlHelper htmlHelper, TimeSpan timeout)
		{
			ID = id ?? throw new Exception("ID is null");
			Source = source;
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;
			Autor = autor;
			Status = status;
			LanguageFlagUrl = languageFlagUrl;
			Description = description;
			Genres = genres;
			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
			_timeout = timeout;
		}

		public async Task<List<IChapter>> GetBooks()
		{
			int offset = 0;
			int total = 0;

			List<IChapter> chapters = new List<IChapter>();

			try {
				do {
					var url = $"https://api.mangadex.org/chapter?manga={ID}&limit=20&offset={offset}";
					var chaptersResult = await _requestHelper.DoGetRequest(url, 3, false, _timeout).ConfigureAwait(false);
					var data = JsonConvert.DeserializeObject<MangaDexResult<List<ChapterResult>>>(chaptersResult);

					if (data != null && data.Result == "ok") {
						offset += 20;
						total = data.Total;

						foreach (var d in data.Data.Where(d => d.Attributes.TranslatedLanguage == "en")) {
							var updatedAt = DateTimeOffset.TryParse(d.Attributes.UpdatedAt, out DateTimeOffset date);
							var c = new MangaDexChapter(d.Id, $"{d.Attributes.Volume} - {d.Attributes.Chapter}", HomeUrl, updatedAt ? date.ToString("yyyy-mm-dd HH:mm:ss") : d.Attributes.UpdatedAt, Name, Source, _timeout, _requestHelper, _htmlHelper);

							chapters.Add(c);
						}
					}
				} while (offset + 20 < total);
			} catch {
				throw;
			}

			return chapters;
		}
	}
}
