using ComicReader.Helper;

namespace ComicReader.Interpreter.Implementations
{
	internal class MangaKakalotChapter : BaseChapter
	{
		public override Dictionary<string, string>? RequestHeaders { get; } = new() {
			{ "referer", "https://chapmanganato.to/" }
		};

		public override async Task<List<string>> ImplGetPageUrls()
		{
			var response = await RequestHelper.DoGetRequest(HomeUrl, 6);

			var div = HtmlHelper.ElementsByClass(response, "container-chapter-reader").First();
			var allImgs = HtmlHelper.ElementsByTypeOuter(div, "img");

			var pageUrls = new List<string>();
			foreach (var img in allImgs.Where(i => i != null)) {
				pageUrls.Add(HtmlHelper.GetAttribute(img!, "src"));
			}

			return pageUrls;
		}

		public MangaKakalotChapter(
			string title,
			string homeUrl,
			string lastUpdate,
			string mangaName,
			string source,
			RequestHelper requestHelper,
			HtmlHelper htmlHelper) : base(title, homeUrl, lastUpdate, mangaName, source, requestHelper, htmlHelper)
		{ }
	}
}
