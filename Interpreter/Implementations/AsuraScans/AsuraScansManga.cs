using ComicReader.Helper;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansManga : IManga
	{
		private readonly IRequest _requestHelper;
		private readonly HtmlHelper _htmlHelper;
		private readonly INotification _notification;

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
			INotification notification)
		{
			Name = name;
			HomeUrl = homeUrl;
			CoverUrl = coverUrl;

			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
			_notification = notification;

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
				var response = await _requestHelper.DoGetRequest(HomeUrl, 3, true);
				var allChaptersHtml = _htmlHelper.ElementsByClass(response, "scrollbar-thumb-themecolor");

				var allAHtmlOuter = _htmlHelper.ElementsByTypeOuter(allChaptersHtml[0], "a") ?? new List<string?>();
				var allAHtml = _htmlHelper.ElementsByType(allChaptersHtml[0], "a");

				for (int i = allAHtmlOuter.Count - 1; i > 0; i--) {
					var url = $"https://asuracomic.net/series/{_htmlHelper.GetAttribute(allAHtmlOuter[i] ?? string.Empty, "href")}";
					var h3s = _htmlHelper.ElementsByType(allAHtml[i], "h3");
					var last = "-";

					if (h3s.Count > 1) {
						last = _htmlHelper.ElementsByType(allAHtml[i], "h3")[1];
					} else if (h3s.Count == 1) {
						last = _htmlHelper.ElementsByType(allAHtml[i], "h3")[0];
					}

					chapters.Add(new AsureScansChapter(null, Source, Name, $"Chapter {allAHtmlOuter.Count - i}", url, last, _requestHelper, _htmlHelper, _notification));
				}
			} catch (Exception ex) {
				await _notification.ShowError($"Error", ex.Message);
			}

			return chapters;
		}
	}
}
