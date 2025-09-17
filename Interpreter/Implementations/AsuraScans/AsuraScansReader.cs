using ComicReader.Helper;
using ComicReader.Reader;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansReader : IReader
	{
		private readonly IRequest requestHelper;
		private readonly HtmlHelper htmlHelper;
		private readonly INotification notification;

		public string Title => "AsuraScans";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://asuracomic.net/";

		public bool ShowReader { get; set; } = true;

		public AsuraScansReader(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			try {
				var url = $"https://asuracomic.net/series?order=update";
				var response = await requestHelper.DoGetRequest(url, 3, true);

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
			} catch (Exception ex) {
				await notification.ShowError($"Error", ex.Message);
			}

			return l;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			List<IManga> l = new List<IManga>();

			for (int i = 1; i <= 2; i++) {
				try {
					var url = $"https://asuracomic.net/series?page={i}&name={keyWords.Replace(" ", "%20")}";
					var response = await requestHelper.DoGetRequest(url, 3, true);

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
				} catch (Exception ex) {
					await notification.ShowError($"Error", ex.Message);
				}
			}

			return l;
		}

		private async Task<AsuraScansManga> ParseManga(string? aOuter, string? a)
		{
			var mUrl = $"{HomeUrl}{htmlHelper.GetAttribute(aOuter ?? string.Empty, "href")}";
			var title = htmlHelper.ElementsByClass(a ?? string.Empty, "block")[2];
			var coverImg = htmlHelper.ElementsByTypeOuter(a ?? string.Empty, "img");
			var cover = htmlHelper.GetAttribute(coverImg.FirstOrDefault() ?? string.Empty, "src");

			var autor = "unknown";
			var status = "completed";
			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";

			var response = await requestHelper.DoGetRequest(mUrl, 3, true);
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

			return new AsuraScansManga(title, mUrl, cover, autor, status, langFlagUrl, desc, genres, Title, requestHelper, htmlHelper, notification);
		}

		private string FixDesc(string desc)
		{
			return desc.Replace("<strong>", "").Replace("<em>", "").Replace("<br>", Environment.NewLine).Replace("&#8217;", "").Replace("</strong>", "").Replace("</em>", "").Replace("&nbsp;", "").Replace("&lt;", "").Replace("&gt;", "").Replace("<p>", "").Replace("</p>", "").Replace("&quot;", "");
		}
	}
}
