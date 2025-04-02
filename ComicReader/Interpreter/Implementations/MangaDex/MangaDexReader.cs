using ComicReader.Helper;
using ComicReader.Reader;
using Newtonsoft.Json;

namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexReader : IReader
	{
		private readonly RequestHelper _requestHelper;
		private readonly HtmlHelper _htmlHelper;

		public string Title => "MangaDex";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangadex.org";

		public MangaDexReader(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			var result = await _requestHelper.DoGetRequest("https://api.mangadex.org/manga?limit=20&order[latestUploadedChapter]=desc", 3);
			var data = JsonConvert.DeserializeObject<MangaDexResult<List<SearchResultManga>>>(result);

			var l = new List<IManga>();

			if (data.Result == "ok") {
				foreach (var m in data.Data) {
					try {
						List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };
						var coverId = m.Relationships.FirstOrDefault(r => r.Type == "cover_art")?.Id;
						var coverResult = await _requestHelper.DoGetRequest($"https://api.mangadex.org/cover/{coverId}", 3).ConfigureAwait(false);
						var coverResultData = JsonConvert.DeserializeObject<MangaDexCoverResult>(coverResult);

						string coverFileName = coverResultData.Data.Attributes.FileName;
						string coverUrl = $"https://uploads.mangadex.org/covers/{m.Id}/{coverFileName}";
						string homeUrl = $"https://mangadex.org/title/{m.Id}";

						try {
							var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";

							l.Add(new MangaDexManga(m.Id, "MangaDex", m.Attributes.Title["en"], homeUrl, coverUrl, "unknown", m.Attributes.Status, langFlagUrl, m.Attributes.Description["en"], genres, _requestHelper, _htmlHelper));
						} catch (Exception) { }
					} catch (Exception) { }
				}
			}

			return l;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			var result = await _requestHelper.DoGetRequest("https://api.mangadex.org/manga?limit=20&title=" + keyWords, 3);
			var data = JsonConvert.DeserializeObject<MangaDexResult<List<SearchResultManga>>>(result);

			var l = new List<IManga>();

			if (data.Result == "ok") {
				foreach (var m in data.Data) {
					try {
						List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };
						var coverId = m.Relationships.FirstOrDefault(r => r.Type == "cover_art")?.Id;
						var coverResult = await _requestHelper.DoGetRequest($"https://api.mangadex.org/cover/{coverId}", 3).ConfigureAwait(false);
						var coverResultData = JsonConvert.DeserializeObject<MangaDexCoverResult>(coverResult);

						string coverFileName = coverResultData.Data.Attributes.FileName;
						string coverUrl = $"https://uploads.mangadex.org/covers/{m.Id}/{coverFileName}";
						string homeUrl = $"https://mangadex.org/title/{m.Id}";

						try {
							var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";

							l.Add(new MangaDexManga(m.Id, "MangaDex", m.Attributes.Title["en"], homeUrl, coverUrl, "unknown", m.Attributes.Status, langFlagUrl, m.Attributes.Description["en"], genres, _requestHelper, _htmlHelper));
						} catch (Exception) { }
					} catch (Exception) { }
				}
			}

			return l;
		}
	}
}
