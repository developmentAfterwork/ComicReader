using ComicReader.Helper;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansManga : IManga
	{
		private readonly IRequest _requestHelper;
		private readonly HtmlHelper _htmlHelper;
		private readonly INotification _notification;
		private readonly TimeSpan _timeout;

		public string? ID { get; }

		public string Name { get; }

		public string HomeUrl { get; }

		public string CoverUrl { get; }

		public string Autor { get; }

		public string Status { get; }

		public string LanguageFlagUrl { get; }

		public string Description { get; }

		public List<string> Genres { get; }

		public Dictionary<string, string>? RequestHeaders => null;

		public string Source { get; }

		public bool IsFavorite { get; set; } = false;

		public AsuraScansManga(
			string name,
			string homeUrl,
			string coverUrl,
			string autor,
			string status,
			string languageFlagUrl,
			string description,
			List<string> genres,
			string source,
			IRequest requestHelper,
			HtmlHelper htmlHelper,
			INotification notification,
			TimeSpan timeout)
		{
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;

			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
			_notification = notification;
			_timeout = timeout;

			Autor = autor;
			Status = status;
			LanguageFlagUrl = languageFlagUrl;
			Description = description;
			Genres = genres;
			Source = source;
		}

		public async Task<List<IChapter>> GetBooks()
		{
			List<IChapter> chapters = new List<IChapter>();

			try {
				var homeUrl = MigrateUrl(HomeUrl);
				var response = await _requestHelper.DoGetRequest(homeUrl, 3, true, _timeout);
				var allChaptersHtml = _htmlHelper.ElementsByClass(response, "divide-y");

				if (allChaptersHtml.Count == 0) {
					homeUrl = RemoveRandomValueFromUrl(homeUrl);
					response = await _requestHelper.DoGetRequest(homeUrl, 3, true, _timeout);
					allChaptersHtml = _htmlHelper.ElementsByClass(response, "divide-y");
				}

				var allAHtmlOuter = _htmlHelper.ElementsByTypeOuter(allChaptersHtml[0], "a") ?? new List<string?>();
				var allAHtml = _htmlHelper.ElementsByType(allChaptersHtml[0], "a");

				for (int i = allAHtmlOuter.Count - 1; i > 0; i--) {
					var url = $"https://asurascans.com{_htmlHelper.GetAttribute(allAHtmlOuter[i] ?? string.Empty, "href")}";
					var spans = _htmlHelper.ElementsByType(allAHtml[i], "span");
					var last = "-";

					if (spans.Count > 1) {
						last = spans[1];
					} else if (spans.Count == 1) {
						last = spans[0];
					}

					chapters.Add(new AsureScansChapter(null, Source, Name, $"Chapter {allAHtmlOuter.Count - i}", url, last, _timeout, _requestHelper, _htmlHelper, _notification));
				}
			} catch (Exception ex) {
				await _notification.ShowError($"Error", $"{Name} - {ex.Message}");
			}

			return chapters;
		}

		private string MigrateUrl(string url)
		{
			if (url.StartsWith("https://asuracomic.net/series/")) {
				return url.Replace("https://asuracomic.net/series/", "https://asurascans.com/comics/");
			} else {
				return url;
			}
		}

		private string RemoveRandomValueFromUrl(string url)
		{
			return url.Substring(0, url.LastIndexOf("-"));
		}
	}
}
