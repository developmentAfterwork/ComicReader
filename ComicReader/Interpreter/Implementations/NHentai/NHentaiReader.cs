using ComicReader.Helper;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.NHentai
{
	public class NHentaiReader : IReader
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string Title => "NHentai";

		public bool IsEnabled { get; set; } = false;

		public string HomeUrl => "https://nhentai.net";

		public bool ShowReader { get; set; } = false;

		public NHentaiReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			for (int i = 1; i <= 8; i++) {
				string url = "https://nhentai.net/?page=" + i;
				var response = await requestHelper.DoGetRequest(url, 3);
				l.AddRange(GetMangasFromResponse(response));
			}

			return l;
		}

		private List<IManga> GetMangasFromResponse(string response)
		{
			// TODO: read data-tags for language
			var allMangaHtmls = htmlHelper.ElementsByClass(response, "gallery");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				var m = ParseManga(r);
				if (m.Name.Contains("[English]")) {
					mangas.Add(ParseManga(r));
				}
			}

			return mangas;
		}

		private IManga ParseManga(string mangaToParse)
		{
			var homeUrlADiv = htmlHelper.ElementByType(mangaToParse, "a");

			var homeUrl = HomeUrl + htmlHelper.GetAttribute(mangaToParse, "href");
			var preViewImage = htmlHelper.GetAttribute(homeUrlADiv, "data-src");

			var title = htmlHelper.ElementByType(htmlHelper.ElementByType(mangaToParse, "a"), "div");

			var autor = "unknown";
			var status = "completed";
			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";
			var desc = "...";
			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			return new NHentaiManga(title, homeUrl, preViewImage, autor, status, langFlagUrl, desc, genres, Title, requestHelper, htmlHelper);
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			List<IManga> l = new List<IManga>();

			for (int i = 1; i <= 8; i++) {
				string url = $"https://nhentai.net/search/?q={keyWords.Replace(" ", "+")}&page={i}";
				var response = await requestHelper.DoGetRequest(url, 3);
				l.AddRange(GetMangasFromResponse(response));
			}

			return l;
		}
	}
}
