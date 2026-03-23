using ComicReader.Helper;
using ComicReader.Interpreter;
using Interpreter.Interface;
using System.Net;
using System.Text.RegularExpressions;

namespace ComicReader.Reader
{
	public class MangaKatanaReader : IReader
	{
		private readonly TimeSpan timeout;

		public MangaKatanaReader(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification, TimeSpan timeout)
		{
			RequestHelper = requestHelper;
			HtmlHelper = htmlHelper;
			Notification = notification;
			this.timeout = timeout;
		}

		public string Title => "MangaKatana";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangakatana.com/";

		public bool ShowReader { get; set; } = true;

		private IRequest RequestHelper { get; }
		private HtmlHelper HtmlHelper { get; }
		public INotification Notification { get; }

		public Dictionary<string, string>? RequestHeaders => new Dictionary<string, string>() {
			{ "referer", "https://mangakatana.com/" }
		};

		public async Task<List<IManga>> Search(string keyWords)
		{
			try {
				var text = keyWords.Replace(" ", "+");
				var url = $"https://mangakatana.com/?search={text}&search_by=bo";
				var response = await RequestHelper.DoGetRequest(url, 3, true, timeout, RequestHeaders);

				var mangas = GetMangasFromResponse(response);

				return mangas;
			} catch (Exception ex) {
				await Notification.ShowError($"Error", $"{Title} - {ex.Message}");
				return new();
			}
		}

		private List<IManga> GetMangasFromResponse(string response)
		{
			var bookListHtml = HtmlHelper.ElementById(response, "book_list");
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "item");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				try {
					mangas.Add(ParseManga(r));
				} catch { }
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

			desc = HtmlToPlainText_Regex(desc);
			title = HtmlToPlainText_Regex(title);

			return new MangaKatanaManga(title, homeUrl, preViewImage, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper, timeout);
		}

		private static string HtmlToPlainText_Regex(string html)
		{
			if (string.IsNullOrWhiteSpace(html)) return string.Empty;

			// Ersetze <br> und </p> durch neue Zeile, sonst alle Tags entfernen
			var withBreaks = Regex.Replace(html, @"<(br|br\s*/|/p|/div|/li)\s*>", "\n", RegexOptions.IgnoreCase);
			var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
			var decoded = WebUtility.HtmlDecode(noTags);

			// whitespace normalisieren
			decoded = Regex.Replace(decoded, @"\r\n|\r|\n", "\n");
			decoded = Regex.Replace(decoded, @"[ \t]+", " ");
			decoded = Regex.Replace(decoded, @"\n\s*\n+", "\n\n");

			return decoded.Trim();
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			string url = "https://mangakatana.com/new-manga";
			var response = await RequestHelper.DoGetRequest(url, 3, true, timeout, RequestHeaders);

			try {
				l.AddRange(GetMangasFromResponse(response));
			} catch (Exception ex) {
				await Notification.ShowError($"Error", $"{Title} - {ex.Message}");
			}

			url = "https://mangakatana.com/latest";
			response = await RequestHelper.DoGetRequest(url, 3, true, timeout, RequestHeaders);
			try {
				l.AddRange(GetMangasFromResponse(response));
			} catch (Exception ex) {
				await Notification.ShowError($"Error", $"{Title} - {ex.Message}");
			}

			return l;
		}
	}
}
