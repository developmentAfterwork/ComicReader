using ComicReader.Helper;

namespace ComicReader.Interpreter
{
	public record MangaKatanaManga : IManga
	{
		public static readonly string SourceKey = "MangaKatana";

		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string Name { get; }

		public string HomeUrl { get; }

		public string CoverUrl { get; }

		public string Autor { get; }

		public string Status { get; }

		public string LanguageFlagUrl { get; }

		public string Description { get; }

		public List<string> Genres { get; }

		public string Source => MangaKatanaManga.SourceKey;

		public MangaKatanaManga(
			string name,
			string homeUrl,
			string coverUrl,
			string autor,
			string status,
			string languageFlagUrl,
			string description,
			List<string> genres,
			RequestHelper requestHelper,
			HtmlHelper htmlHelper)
		{
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;

			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;

			Autor = autor;
			Status = status;
			LanguageFlagUrl = languageFlagUrl;
			Description = description;
			Genres = genres;
		}

		public async Task<List<IChapter>> GetBooks()
		{
			var response = await requestHelper.DoGetRequest(HomeUrl, 3);
			var chaptersHtml = htmlHelper.ElementsByClass(response, "chapters").First();
			var allChapterHtmls = htmlHelper.ElementsByClass(chaptersHtml, "chapter");
			var allChapterUpdateTimes = htmlHelper.ElementsByClass(chaptersHtml, "update_time");

			var chapters = new List<IChapter>();
			for (int i = 0; i < allChapterHtmls.Count; i++) {
				var chapter = allChapterHtmls[i];
				var update = allChapterUpdateTimes[i];

				chapters.Add(ParseChapter(chapters, chapter, update));
			}

			chapters.Reverse();
			return chapters;
		}

		private MangaKatanaChapter ParseChapter(List<IChapter> chapters, string chapterHtml, string updateTime)
		{
			var url = htmlHelper.GetAttribute(chapterHtml, "href");
			var title = htmlHelper.ElementByType(chapterHtml, "a");

			return new MangaKatanaChapter(title, url, updateTime, Name, Source, requestHelper, htmlHelper);
		}
	}
}
