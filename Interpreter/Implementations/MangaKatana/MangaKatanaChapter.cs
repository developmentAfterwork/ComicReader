using ComicReader.Helper;
using ComicReader.Interpreter.Implementations;
using Interpreter.Interface;

namespace ComicReader.Interpreter
{
	public class MangaKatanaChapter : BaseChapter, IChapter
	{
		private static Dictionary<string, string>? RequestHeaders => new Dictionary<string, string>() {
			{ "referer", "https://mangakatana.com/" }
		};

		public MangaKatanaChapter(
			string title,
			string homeUrl,
			string lastUpdate,
			string mangaName,
			string source,
			TimeSpan timeout,
			IRequest requestHelper,
			HtmlHelper htmlHelper) : base(null, title, homeUrl, lastUpdate, mangaName, source, timeout, requestHelper, htmlHelper, RequestHeaders)
		{ }

		public override async Task<List<string>> ImplGetPageUrls()
		{
			var response = await RequestHelper.DoGetRequest(HomeUrl, 6, true, Timeout, RequestHeaders);

			var scripts = HtmlHelper.ElementsByType(response, "script");

			if (scripts == null || scripts.Count == 0) {
				throw new Exception("scripts are empty");
			}

			var scriptWithUrls = scripts.Where(s => s.Contains("thzq") && s.Contains("i1.mangakatana.com")).First();

			string start = "var thzq=[";
			string end = "]";

			scriptWithUrls = scriptWithUrls.Substring(scriptWithUrls.IndexOf(start));

			var startIndex = scriptWithUrls.IndexOf(start) + start.Length;
			var endIndex = scriptWithUrls.IndexOf(end);
			var l = endIndex - startIndex;

			string allUrlsWithComma = scriptWithUrls.Substring(startIndex, l);

			var urlsAsList = allUrlsWithComma.Split(',');

			var final = urlsAsList.Where(u => !string.IsNullOrEmpty(u)).Select(u => u.Replace("'", "").Trim()).ToList();

			return final;
		}
	}
}
