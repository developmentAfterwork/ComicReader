using ComicReader.Helper;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations
{
	internal record MangaKakalotManga : IManga
	{
		public static readonly string SourceKey = "MangaKakalot";

		private readonly IRequest requestHelper;
		private readonly HtmlHelper htmlHelper;
		private readonly TimeSpan timeout;

		public string? ID { get; }

		public string Name { get; }

		public string HomeUrl { get; }

		public string CoverUrl { get; }

		public string Autor { get; }

		public string Status { get; }

		public string LanguageFlagUrl { get; }

		public string Description { get; }

		public List<string> Genres { get; }

		public string Source => MangaKakalotManga.SourceKey;

		public Dictionary<string, string>? RequestHeaders => new() {
			{ "referer", "https://www.mangakakalot.gg/" }
		};

		public MangaKakalotManga(
			string name,
			string homeUrl,
			string coverUrl,
			string autor,
			string status,
			string languageFlagUrl,
			string description,
			List<string> genres,
			IRequest requestHelper,
			HtmlHelper htmlHelper,
			TimeSpan timeout)
		{
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;

			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.timeout = timeout;

			Autor = autor;
			Status = status;
			LanguageFlagUrl = languageFlagUrl;
			Description = description;
			Genres = genres;
		}

		public async Task<List<IChapter>> GetBooks()
		{
			try {
				var response = await requestHelper.DoGetRequest(HomeUrl, 3, true, timeout);

				var chaptersHtml = htmlHelper.ElementsByClass(response, "row-content-chapter").FirstOrDefault();

				if (chaptersHtml is null) {
					return GetAlternative(response);
				}

				var allChapterHtmls = htmlHelper.ElementsByClass(chaptersHtml, "a-h");

				var chapters = new List<IChapter>();
				for (int i = 0; i < allChapterHtmls.Count; i++) {
					var chapterHtml = allChapterHtmls[i];

					chapters.Add(ParseChapter(chapterHtml));
				}

				chapters.Reverse();

				return chapters;
			} catch {
				return new List<IChapter>();
			}
		}

		private MangaKakalotChapter ParseChapter(string html)
		{
			var url = htmlHelper.GetAttribute(html, "href");
			var time = htmlHelper.ElementsByClass(html, "chapter-time").First();
			var title = htmlHelper.ElementsByClass(html, "chapter-name").First();

			return new MangaKakalotChapter(title, url, time, Name, Source, timeout, requestHelper, htmlHelper);
		}

		private List<IChapter> GetAlternative(string? response)
		{
			var chaptersHtml = htmlHelper.ElementsByClass(response ?? string.Empty, "chapter-list").FirstOrDefault();

			if (chaptersHtml is null) {
				return GetAlternative2(response);
			}

			var allChapterHtmls = htmlHelper.ElementsByClass(chaptersHtml, "row");

			var chapters = new List<IChapter>();
			for (int i = 0; i < allChapterHtmls.Count; i++) {
				var chapterHtml = allChapterHtmls[i];
				var a = htmlHelper.ElementByType(chapterHtml, "span");
				var url = htmlHelper.GetAttribute(a, "href");
				var title = htmlHelper.ElementByType(chapterHtml, "a");

				chapters.Add(new MangaKakalotChapter(title, url, "...", Name, Source, timeout, requestHelper, htmlHelper));
			}

			chapters.Reverse();

			return chapters;
		}

		private List<IChapter> GetAlternative2(string? response)
		{
			var chaptersHtml = htmlHelper.ElementsByClass(response ?? string.Empty, "read-chapter").FirstOrDefault();

			if (chaptersHtml is null) {
				return new List<IChapter>();
			}

			var allChapterHtmls = htmlHelper.ElementsByTypeOuter(chaptersHtml, "a");

			var chapters = new List<IChapter>();

			if (allChapterHtmls.Count == 2) {
				var chapterHtmlStart = allChapterHtmls[0];
				var chapterHtmlEnd = allChapterHtmls[1];

				var startNumberStr = htmlHelper.GetAttribute(chapterHtmlStart ?? "", "href").Substring(htmlHelper.GetAttribute(chapterHtmlStart ?? "", "href").LastIndexOf("-") + 1);
				var endNumberStr = htmlHelper.GetAttribute(chapterHtmlEnd ?? "", "href").Substring(htmlHelper.GetAttribute(chapterHtmlEnd ?? "", "href").LastIndexOf("-") + 1);

				var start = int.Parse(startNumberStr);
				var end = int.Parse(endNumberStr);

				var url = htmlHelper.GetAttribute(chapterHtmlStart ?? "", "href").Substring(0, htmlHelper.GetAttribute(chapterHtmlStart ?? "", "href").LastIndexOf("-") + 1);
				for (int i = start; i <= end; i++) {
					var u = url + i.ToString();
					var title = u.Replace(HomeUrl, "").Substring(1);
					chapters.Add(new MangaKakalotChapter(title, u, "...", Name, Source, timeout, requestHelper, htmlHelper));
				}
			} else {
				return new List<IChapter>();
			}

			return chapters;
		}
	}
}
