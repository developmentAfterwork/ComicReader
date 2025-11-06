using ComicReader.Helper;
using Interpreter.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComicReader.Interpreter.Implementations.AsuraScans {
	public class AsureScansChapter : BaseChapter, IChapter {
		public AsureScansChapter(string? id, string source, string mangaName, string title, string homeUrl, string lastUpdate, TimeSpan timeout, IRequest requestHelper, HtmlHelper htmlHelper, INotification notification) : base(id, title, homeUrl, lastUpdate, mangaName, source, timeout, requestHelper, htmlHelper) { }

		public override async Task<List<string>> ImplGetPageUrls() {
			var url = HomeUrl;
			var response = await RequestHelper.DoGetRequest(url, 3, true, Timeout, RequestHeaders);

			string startWord = $"\\\"name\\\":\\\"{MangaName}\\\"";
			var startIndex = response.IndexOf(startWord) + startWord.Length;
			var part = response.Substring(startIndex);

			var matches = Regex.Matches(part, "https[:\\/.a-zA-Z0-9\\-]*(.webp|.jpg)");
			var urls = new List<string>();

			foreach (var match in matches.ToList()) {
				urls.Add(match.Value);
			}
			urls = urls.Distinct().ToList();

			return urls;
		}
	}
}
