using ComicReader.Helper;
using ComicReader.Interpreter;

namespace ComicReader.Reader
{
	internal class MangaKatanaReader : IReader
	{
		public MangaKatanaReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			RequestHelper = requestHelper;
			HtmlHelper = htmlHelper;
		}

		public string Title => "Manga Katana";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangakatana.com/";

		private RequestHelper RequestHelper { get; }
		private HtmlHelper HtmlHelper { get; }

		public async Task<List<IManga>> Search(string keyWords)
		{
			var text = keyWords.Replace(" ", "+");
			var url = $"https://mangakatana.com/?search={text}&search_by=bo";
			var response = await RequestHelper.DoGetRequest(url, 3);

			var mangas = GetMangasFromResponse(response);

			return mangas;
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

			return new MangaKatanaManga(title, homeUrl, preViewImage, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper);
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			string url = "https://mangakatana.com/latest";

			var response = await RequestHelper.DoGetRequest(url, 3);

			var mangas = GetMangasFromResponse(response);

			return mangas;
		}
	}
}
