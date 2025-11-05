using ComicReader.Helper;
using Interpreter.Interface;
using Newtonsoft.Json;

namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexChapter : BaseChapter, IChapter
	{
		public MangaDexChapter(
			string? id,
			string title,
			string homeUrl,
			string lastUpdate,
			string mangaName,
			string source,
			TimeSpan timeout,
			IRequest requestHelper,
			HtmlHelper htmlHelper) : base(id, title, homeUrl, lastUpdate, mangaName, source, timeout, requestHelper, htmlHelper)
		{ }

		public override async Task<List<string>> ImplGetPageUrls()
		{
			var url = $"https://api.mangadex.org/at-home/server/{ID}?forcePort443=true";
			var result = await RequestHelper.DoGetRequest(url, 3, false, Timeout).ConfigureAwait(false);
			var data = JsonConvert.DeserializeObject<MangaDexPagesResult>(result);

			List<string> pageUrls = new List<string>();

			if (data != null && data.Result == "ok") {
				foreach (var page in data.Chapter.Data) {
					pageUrls.Add($"{data.BaseUrl}/data/{data.Chapter.Hash}/{page}");
				}
			}

			return pageUrls;
		}
	}
}
