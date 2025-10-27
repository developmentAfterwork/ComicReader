using ComicReader.Helper;
using ComicReader.Reader;
using Interpreter.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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

				var mangas = await GetMangasFromResponse(response);

				return mangas;
			} catch (Exception ex) {
				await Notification.ShowError($"Error", ex.Message);
				return new();
			}
		}

		private async Task<List<IManga>> GetMangasFromResponse(string response)
		{
			var bookListHtml = HtmlHelper.ElementsByClass(response, "panel_story_list").First();
			var allMangaHtmls = HtmlHelper.ElementsByClass(bookListHtml, "story_item");

			List<IManga> mangas = new List<IManga>();

			foreach (var r in allMangaHtmls) {
				mangas.Add(await ParseManga(r));
			}

			return mangas;
		}

		private async Task<IManga> ParseManga(string mangaToParse)
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
			if (desc == "...") {
				string response;
				try {
					response = await RequestHelper.DoGetRequest(homeUrl, 3, true, RequestHeaders);
					var cont = HtmlHelper.ElementById(response, "contentBox");
					desc = cont;
				} catch (Exception ex) {
					await Notification.ShowError($"Error", ex.Message);
				}
			}

			List<string> genres = new List<string>() { "Action", "Adventure", "Comedy", "School Life", "Shounen", "Supernatural", "Manhwa", "Webtoon" };

			desc = HtmlToPlainText_Regex(desc).Replace($"{title} summary:", "").Trim();
			title = HtmlToPlainText_Regex(title);
			return new MangaKakalotManga(title, homeUrl, prevImageUrl, autor, status, langFlagUrl, desc, genres, RequestHelper, HtmlHelper);
		}

		private static string HtmlToPlainText_Regex(string html)
		{
			if (string.IsNullOrWhiteSpace(html)) return string.Empty;

			// Ersetze <br> und </p> durch neue Zeile, sonst alle Tags entfernen
			var withBreaks = Regex.Replace(html, @"<(br|br\s*/|/p|/div|/li)\s*>", "\n", RegexOptions.IgnoreCase);
			var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
			var decoded = WebUtility.HtmlDecode(noTags);

			// whitespace normalisieren
			decoded = Regex.Replace(decoded, @"\r\n|\r|\n", "\n");
			decoded = Regex.Replace(decoded, @"[ \t]+", " ");
			decoded = Regex.Replace(decoded, @"\n\s*\n+", "\n\n");

			return decoded.Trim();
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
				mangas.Add(await ParseManga(r));
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
				mangas.Add(await ParseManga(r));
			}

			return mangas;
		}
	}
}
