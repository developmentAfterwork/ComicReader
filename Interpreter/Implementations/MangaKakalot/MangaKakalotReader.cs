using ComicReader.Helper;
using ComicReader.Reader;
using Interpreter.Interface;
using System;
using System.Collections.Generic;

namespace ComicReader.Interpreter.Implementations
{
	public class MangaKakalotReader : IReader
	{
		private IRequest RequestHelper { get; }
		private HtmlHelper HtmlHelper { get; }
		public INotification Notification { get; }

		public string Title => "MangaKakalot";

		public bool IsEnabled { get; set; } = true;

		public string HomeUrl => "https://mangakakalot.gg/";

		public bool ShowReader { get; set; } = true;

		public Dictionary<string, string>? RequestHeaders => new() {
			{ "referer", "https://www.mangakakalot.gg/" }
		};

		public MangaKakalotReader(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			RequestHelper = requestHelper;
			HtmlHelper = htmlHelper;
			Notification = notification;
		}

		public async Task<List<IManga>> Search(string keyWords)
		{
			try {
				var text = keyWords.Replace(" ", "_");
				var url = $"https://mangakakalot.gg/search/story/{text}";
				var response = await RequestHelper.DoGetRequest(url, 3, true, RequestHeaders);

				var mangas = GetMangasFromResponse(response);

				return mangas;
			} catch (Exception ex) {
				await Notification.ShowError($"Error", ex.Message);
				return new();
			}
		}

		private List<IManga> GetMangasFromResponse(string response)
		{
			var bookListHtml = HtmlHelper.ElementsByClass(response, "panel_story_list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "story_item");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}

		private IManga ParseManga(string mangaToParse)
		{
			var prevImage = HtmlHelper.ElementByType(mangaToParse, "a");

			var prevImageUrl = HtmlHelper.GetAttribute(prevImage, "src");
			var homeUrl = HtmlHelper.GetAttribute(mangaToParse, "href");

			var titleElement = HtmlHelper.ElementsByClass(mangaToParse, "story_name").FirstOrDefault() ?? "";
			string title;
			if (titleElement == "") {
				title = HtmlHelper.GetAttribute(mangaToParse, "title");
			} else {
				title = HtmlHelper.ElementByType(titleElement, "a");
			}

			var autor = "unknown";
			var status = "completed";

			var langFlagUrl = "https://www.nordisch.info/wp-content/uploads/2019/05/union-jack.png";
			var desc = (HtmlHelper.ElementsByType(mangaToParse, "p").FirstOrDefault() ?? "...").TrimStart();
			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			desc = FixDescription(desc);
			title = FixDescription(title);
			return new MangaKakalotManga(title, homeUrl, prevImageUrl, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper);
		}

		private string FixDescription(string desc)
		{
			return desc.Replace("<br>", "").Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "").Replace("&quot;", "").Replace("&#8230;", "").Replace("&#8220;", "").Replace("&#8217;", "").Replace("&#8221;", "").Replace("&#8213;", "").Replace("&#333;;", "");
		}

		public async Task<List<IManga>> LoadUpdatesAndNewMangs()
		{
			List<IManga> l = new List<IManga>();

			string url = "https://www.mangakakalot.gg/manga-list/new-manga";
			try {
				List<IManga> mangas = await TryGetList(() => { return GetMangasFromResponseUpdateComic(url); });
				l.AddRange(mangas);
			} catch {
				try {
					List<IManga> mangas = await TryGetList(() => { return GetMangasFromResponseUpdateTruyen(url); });
					l.AddRange(mangas);
				} catch (Exception ex) {
					await Notification.ShowError($"Error", ex.Message);
				}
			}

			url = "https://www.mangakakalot.gg/manga-list/latest-manga";
			try {
				List<IManga> mangas = await TryGetList(() => { return GetMangasFromResponseUpdateComic(url); });
				l.AddRange(mangas);
			} catch {
				try {
					List<IManga> mangas = await TryGetList(() => { return GetMangasFromResponseUpdateTruyen(url); });
					l.AddRange(mangas);
				} catch (Exception ex) {
					await Notification.ShowError($"Error", ex.Message);
				}
			}

			return l;
		}

		private async Task<List<IManga>> TryGetList(Func<Task<List<IManga>>> manga)
		{
			List<IManga> mangas = new();

			mangas.AddRange(await manga());

			return mangas;
		}

		private async Task<List<IManga>> GetMangasFromResponseUpdateTruyen(string url)
		{
			var response = await RequestHelper.DoGetRequest(url, 1, true, RequestHeaders);

			var bookListHtml = HtmlHelper.ElementsByClass(response, "truyen-list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "list-truyen-item-wrap");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}

		private async Task<List<IManga>> GetMangasFromResponseUpdateComic(string url)
		{
			var response = await RequestHelper.DoGetRequest(url, 1, true, RequestHeaders);

			var bookListHtml = HtmlHelper.ElementsByClass(response, "comic-list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "list-comic-item-wrap");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(ParseManga(r));
			}

			return mangas;
		}
	}
}
