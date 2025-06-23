using ComicReader.Helper;
using ComicReader.Interpreter;

namespace ComicReader.Reader
{
	public class MangaKatanaReader : IReader
	{
		public MangaKatanaReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			RequestHelper = requestHelper;
			HtmlHelper = htmlHelper;
		}

		public string Title => "MangaKatana";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangakatana.com/";

		public bool ShowReader { get; set; } = true;

		private RequestHelper RequestHelper { get; }
		private HtmlHelper HtmlHelper { get; }

		public async Task<List<IManga>> Search(string keyWords)
		{
			try {
				var text = keyWords.Replace(" ", "+");
				var url = $"https://mangakatana.com/?search={text}&search_by=bo";
				var response = await RequestHelper.DoGetRequest(url, 3);

				var mangas = GetMangasFromResponse(response);

				return mangas;
			} catch {
				return new();
			}
		}

		private List<IManga> GetMangasFromResponse(string response)
		{
			var bookListHtml = HtmlHelper.ElementById(response, "book_list");
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "item");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}

		private IManga ParseManga(string mangaToParse)
		{
			var homeUrlDiv = HtmlHelper.ElementsByClass(mangaToParse, "wrap_img").First();
			var homeUrlADiv = HtmlHelper.ElementByType(homeUrlDiv, "a");

			var homeUrl = HtmlHelper.GetAttribute(homeUrlDiv, "href");
			var preViewImage = HtmlHelper.GetAttribute(homeUrlADiv, "src");

			var title = HtmlHelper.ElementByType(HtmlHelper.ElementByType(mangaToParse, "h3"), "a");

			var autor = "unknown";
			var status = "completed";
			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";
			var desc = HtmlHelper.ElementsByClass(mangaToParse, "summary").FirstOrDefault() ?? "...";
			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			desc = FixDescription(desc);

			return new MangaKatanaManga(title, homeUrl, preViewImage, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper);
		}

		private string FixDescription(string desc)
		{
			return desc.Replace("<br>", "").Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "").Replace("&quot;", "").Replace("&#8230;", "").Replace("&#8220;", "").Replace("&#8217;", "").Replace("&#8221;", "").Replace("&#8213;", "").Replace("&#333;;", "");
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			string url = "https://mangakatana.com/new-manga";
			var response = await RequestHelper.DoGetRequest(url, 3);
			l.AddRange(GetMangasFromResponse(response));

			url = "https://mangakatana.com/latest";
			response = await RequestHelper.DoGetRequest(url, 3);
			l.AddRange(GetMangasFromResponse(response));

			return l;
		}
	}
}
