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
		private readonly TimeSpan timeout;

		public string Title => "AsuraScans";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://asurascans.com/";

		public bool ShowReader { get; set; } = true;

		public AsuraScansReader(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification, TimeSpan timeout)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
			this.timeout = timeout;
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			try {
				var url = $"https://asurascans.com/browse";
				var response = await requestHelper.DoGetRequest(url, 3, true, timeout);

				var seriesGrid = htmlHelper.ElementById(response, "series-grid");
				var seriesList = htmlHelper.ElementsByClass(seriesGrid, "series-card");

				if (seriesList.Any()) {
					foreach (var h in seriesList) {
						var allA = htmlHelper.ElementsByType(h, "a");
						var allAOuter = htmlHelper.ElementsByTypeOuter(h, "a");

						try {
							var m = await ParseManga(allAOuter[1], allA[1], allA[0]);
							l.Add(m);
						} catch { }
					}
				}
			} catch (Exception ex) {
				await notification.ShowError($"Error", $"{Title} - {ex.Message}");
			}

			return l;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			List<IManga> l = new List<IManga>();

			for (int i = 1; i <= 2; i++) {
				try {
					var url = $"https://asurascans.com/browse?page={i}&q={keyWords.Replace(" ", "%20")}";
					var response = await requestHelper.DoGetRequest(url, 3, true, timeout);

					var seriesGrid = htmlHelper.ElementById(response, "series-grid");
					var seriesList = htmlHelper.ElementsByClass(seriesGrid, "series-card");

					if (seriesList.Any()) {
						foreach (var h in seriesList) {
							var allA = htmlHelper.ElementsByType(h, "a");
							var allAOuter = htmlHelper.ElementsByTypeOuter(h, "a");

							try {
								var m = await ParseManga(allAOuter[1], allA[1], allA[0]);
								l.Add(m);
							} catch { }
						}
					}
				} catch (Exception ex) {
					await notification.ShowError($"Error", $"{Title} - {ex.Message}");
				}
			}

			return l;
		}

		private async Task<AsuraScansManga> ParseManga(string? aOuter, string? a, string? cover)
		{
			var mUrl = $"{HomeUrl}{htmlHelper.GetAttribute(aOuter ?? string.Empty, "href")}";
			var title = htmlHelper.ElementsByType(a ?? string.Empty, "h3").FirstOrDefault() ?? "-";
			var coverUrl = htmlHelper.GetAttribute(cover ?? string.Empty, "src");

			var autor = "unknown";
			var status = "completed";
			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";

			var response = await requestHelper.DoGetRequest(mUrl, 3, true, timeout);
			var desc = FixDesc(htmlHelper.ElementById(response, "description-text"));

			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			return new AsuraScansManga(title, mUrl, coverUrl, autor, status, langFlagUrl, desc, genres, Title, requestHelper, htmlHelper, notification, timeout);
		}

		private string FixDesc(string desc)
		{
			return desc.Replace("<strong>", "").Replace("<em>", "").Replace("<br>", Environment.NewLine).Replace("&#8217;", "").Replace("</strong>", "").Replace("</em>", "").Replace("&nbsp;", "").Replace("&lt;", "").Replace("&gt;", "").Replace("<p>", "").Replace("</p>", "").Replace("&quot;", "");
		}
	}
}
