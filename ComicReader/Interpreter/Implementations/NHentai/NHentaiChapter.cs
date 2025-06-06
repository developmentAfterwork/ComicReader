using ComicReader.Helper;

namespace ComicReader.Interpreter.Implementations.NHentai
{
	public class NHentaiChapter : BaseChapter, IChapter
	{
		public NHentaiChapter(string? id, string source, string mangaName, string title, string homeUrl, string lastUpdate, RequestHelper requestHelper, HtmlHelper htmlHelper) : base(id, title, homeUrl, lastUpdate, mangaName, source, requestHelper, htmlHelper) { }

		public override async Task<List<string>> ImplGetPageUrls()
		{
			var result = await RequestHelper.DoGetRequest(HomeUrl, 3);
			var galerie = HtmlHelper.ElementById(result, "thumbnail-container");
			var all = HtmlHelper.ElementsByClass(galerie, "gallerythumb");

			List<string> urls = new List<string>();
			int i = 1;
			foreach (var html in all) {
				var newSiteUrl = $"{HomeUrl}{i++}";
				var resultNewSite = await RequestHelper.DoGetRequest(newSiteUrl, 3);

				var section = HtmlHelper.ElementById(resultNewSite, "image-container");
				var a = HtmlHelper.ElementByType(section, "a");
				var u = HtmlHelper.GetAttribute(a, "src");

				urls.Add(u);
			}

			return urls;
		}
	}
}
