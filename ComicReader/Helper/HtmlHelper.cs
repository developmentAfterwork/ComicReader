using CsQuery;

namespace ComicReader.Helper
{
	public class HtmlHelper
	{
		public string ElementById(string html, string id)
		{
			CQ dom = CQ.CreateDocument(html);
			var bookList = dom["#" + id].ToList();
			return bookList.First().InnerHTML;
		}

		public List<string> ElementsByClass(string html, string className)
		{
			var allMangas = CQ.CreateDocument(html)["." + className].ToList();

			return allMangas.Select(m => m.InnerHTML).ToList();
		}

		public string ElementByType(string html, string type)
		{
			CQ dom = CQ.CreateDocument(html);
			var bookList = dom[type].ToList();
			return bookList.First().InnerHTML;
		}

		public List<string> ElementsByType(string html, string type)
		{
			CQ dom = CQ.CreateDocument(html);
			var bookList = dom[type].ToList();
			return bookList.Select(b => b.InnerHTML).ToList();
		}

		public List<string?> ElementsByTypeOuter(string html, string type)
		{
			CQ dom = CQ.CreateDocument(html);
			var bookList = dom[type].ToList();
			return bookList.Select(b => b.ToString()).ToList();
		}

		public string GetAttribute(string elementHtml, string attributeName)
		{
			CQ dom = CQ.CreateDocument(elementHtml);

			var att = dom["body"].FirstElement().FirstChild.Attributes.Where(e => e.Key == attributeName).First();

			return att.Value;
		}
	}
}
