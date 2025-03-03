using ComicReader.Helper;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations
{
	public class MangaKakalotReader : IReader
	{
		private RequestHelper RequestHelper { get; }
		private HtmlHelper HtmlHelper { get; }

		public string Title => "Manga Kakalot";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangakakalot.gg/";

		public MangaKakalotReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			RequestHelper = requestHelper;
			HtmlHelper = htmlHelper;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			var text = keyWords.Replace(" ", "_");
			var url = $"https://mangakakalot.gg/search/story/{text}";
			var response = await RequestHelper.DoGetRequest(url, 3);

			var mangas = GetMangasFromResponse(response);

			return mangas;
		}

		private List<IManga> GetMangasFromResponse(string response)
		{
			var bookListHtml = HtmlHelper.ElementsByClass(response, "panel_story_list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "story_item");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}

		private IManga ParseManga(string mangaToParse)
		{
			var prevImage = HtmlHelper.ElementByType(mangaToParse, "a");

			var prevImageUrl = HtmlHelper.GetAttribute(prevImage, "src");
			var homeUrl = HtmlHelper.GetAttribute(mangaToParse, "href");

			var title = HtmlHelper.GetAttribute(prevImage, "alt");

			var autor = "unknown";
			var status = "completed";

			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";
			var desc = (HtmlHelper.ElementsByType(mangaToParse, "p").FirstOrDefault() ?? "...").TrimStart();
			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			return new MangaKakalotManga(title, homeUrl, prevImageUrl, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper);
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			string url = "https://www.mangakakalot.gg/manga-list/new-manga";

			var response = await RequestHelper.DoGetRequest(url, 3);

			var bookListHtml = HtmlHelper.ElementsByClass(response, "truyen-list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "list-truyen-item-wrap");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}
	}
}
