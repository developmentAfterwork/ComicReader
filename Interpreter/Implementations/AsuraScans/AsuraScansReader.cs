using ComicReader.Helper;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansReader : IReader
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string Title => "AsuraScans";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://asuracomic.net/";

		public bool ShowReader { get; set; } = true;

		public AsuraScansReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			try {
				var url = $"https://asuracomic.net/series?order=update";
				var response = await requestHelper.DoGetRequest(url, 3);

				var seriesList = htmlHelper.ElementsByClass(response, "grid-cols-2");
				if (seriesList.Any()) {
					var allA = htmlHelper.ElementsByType(seriesList[1], "a");
					var allAOuter = htmlHelper.ElementsByTypeOuter(seriesList[1], "a");

					for (int j = 0; j < allA.Count; j++) {
						try {
							var m = await ParseManga(allAOuter[j], allA[j]);
							l.Add(m);
						} catch { }
					}
				}
			} catch { }

			return l;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			List<IManga> l = new List<IManga>();

			for (int i = 1; i <= 2; i++) {
				try {
					var url = $"https://asuracomic.net/series?page={i}&name={keyWords.Replace(" ", "%20")}";
					var response = await requestHelper.DoGetRequest(url, 3);

					var seriesList = htmlHelper.ElementsByClass(response, "grid-cols-2");
					if (seriesList.Any()) {
						var allA = htmlHelper.ElementsByType(seriesList[1], "a");
						var allAOuter = htmlHelper.ElementsByTypeOuter(seriesList[1], "a");

						for (int j = 0; j < allA.Count; j++) {
							try {
								var m = await ParseManga(allAOuter[j], allA[j]);
								l.Add(m);
							} catch { }
						}
					}
				} catch { }
			}

			return l;
		}

		private async Task<AsuraScansManga> ParseManga(string? aOuter, string? a)
		{
			var mUrl = $"{HomeUrl}{htmlHelper.GetAttribute(aOuter, "href")}";
			var title = htmlHelper.ElementsByClass(a, "block")[2];
			var coverImg = htmlHelper.ElementsByTypeOuter(a, "img");
			var cover = htmlHelper.GetAttribute(coverImg.FirstOrDefault(), "src");

			var autor = "unknown";
			var status = "completed";
			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";

			var response = await requestHelper.DoGetRequest(mUrl, 3);
			var spans = htmlHelper.ElementsByClass(response, "text-sm");
			var next = 0;
			for (int i = 0; i < spans.Count; i++) {
				if (spans[i].StartsWith("Synopsis")) {
					next = i + 1;
					break;
				}
			}

			var allP = spans[next];
			var desc = FixDesc(string.Concat(allP));

			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			return new AsuraScansManga(title, mUrl, cover, autor, status, langFlagUrl, desc, genres, Title, requestHelper, htmlHelper);
		}

		private string FixDesc(string desc)
		{
			return desc.Replace("<strong>", "").Replace("<em>", "").Replace("<br>", Environment.NewLine).Replace("&#8217;", "").Replace("</strong>", "").Replace("</em>", "").Replace("&nbsp;", "").Replace("&lt;", "").Replace("&gt;", "").Replace("<p>", "").Replace("</p>", "").Replace("&quot;", "");
		}
	}
}
