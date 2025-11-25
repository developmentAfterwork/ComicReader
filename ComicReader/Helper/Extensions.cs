using ComicReader.Interpreter;
using ComicReader.Services;
using Interpreter.Interface;
using System.Collections.Concurrent;

namespace ComicReader.Helper
{
	public static class Extensions
	{
		public static async Task Refresh(this IManga manga, Factory factory, FileSaverService fileSaverService, SimpleNotificationService simpleNotificationService)
		{
			var allChapters = await manga.GetBooks();

			foreach (var chapter in allChapters) {
				try {
					if (fileSaverService.FileExists(chapter)) {
						IChapter chapterObj = chapter;
						if (chapter is SaveableChapter) {
							var allPageUrls = await chapter.GetPageUrls(false, factory);
							var mappedUrls = chapter.UrlToLocalFileMapper;

							fileSaverService.CheckFiles(mappedUrls.Values.ToList());

							// check that all images are downloaded
							var allImagesExists = mappedUrls.Values.All(f => File.Exists(f));
							if (!allImagesExists) {
								await fileSaverService.DeleteImagesFromChapter(chapterObj, factory);
								fileSaverService.DeleteChapterFile(chapterObj);
							}
						}
					}
				} catch (Exception ex) {
					await simpleNotificationService.ShowError($"Error", $"{chapter.Title} - {ex.Message}");
				}
			}
		}

		public static async Task DownloadChapterPages(this IChapter chapter, TimeSpan timeout, Factory factory, Func<Task> onProgress, Action<Exception> onError, SettingsService settingsService, IRequest requestHelper, FileSaverService fileSaverService)
		{
			if (!chapter.UrlToLocalFileMapper?.Any() ?? false) {
				_ = await chapter.GetPageUrls(settingsService.GetPreDownloadImages(), factory);
			}

			await DownloadPages(chapter.UrlToLocalFileMapper ?? new(), chapter.RequestHeaders, chapter.Source, chapter.MangaName, chapter.Title, timeout, factory, onProgress, onError, settingsService, requestHelper, fileSaverService);
		}

		public static async Task DownloadPages(this Dictionary<string, string> urlToLocalFileMapper, Dictionary<string, string>? requestHeaders, string source, string mangaName, string title, TimeSpan timeout, Factory factory, Func<Task> onProgress, Action<Exception> onError, SettingsService settingsService, IRequest requestHelper, FileSaverService fileSaverService)
		{
			int index = -1;
			foreach (var urlPair in urlToLocalFileMapper.ToDictionary(c => c.Key, c => c.Value)) {
				index++;
				await onProgress();

				try {
					if (!fileSaverService.FileExists(urlPair.Value)) {
						var header = requestHeaders;
						if (header is null || header.Count == 0) {
							header = factory.GetOriginChapterRequestHeaders(source);
						}

						await requestHelper.DownloadFile(urlPair.Key, urlPair.Value, 5, timeout, header);
					}
				} catch (HttpRequestException) {
					var c = await GetOriginChapter(settingsService, timeout, factory, source, mangaName, title, index, urlPair);
					if (c != null) {
						var headers = c.RequestHeaders;
						if (headers is null || headers.Count == 0) {
							headers = factory.GetOriginChapterRequestHeaders(c.Source);
						}

						var pair = c.UrlToLocalFileMapper.ToList()[index];

						try {
							await requestHelper.DownloadFile(pair.Key, urlPair.Value, 5, timeout, headers);
						} catch (Exception ex) {
							onError(ex);
						}
					}
				} catch (Exception ex) {
					onError(ex);
				}
			}
		}

		private static ConcurrentDictionary<(string Source, string Name, string Title), IChapter> _originChapterCache = new();
		private static async Task<IChapter?> GetOriginChapter(SettingsService settingsService, TimeSpan timeout, Factory factory, string source, string mangaName, string title, int index, KeyValuePair<string, string> urlPair)
		{
			if (_originChapterCache.TryGetValue((source, mangaName, title), out var cachedChapter)) {
				return cachedChapter;
			}

			var allUnig = settingsService.GetBookmarkedMangaUniqIdentifiers();

			foreach (var bookmarkId in allUnig.Where(s => s.Contains("|"))) {
				try {
					IManga? manga = await factory.GetMangaFromBookmarkId(bookmarkId);

					if (manga != null && source == manga.Source && mangaName == manga.Name) {
						var chapters = await manga.GetBooks();
						var c = chapters.SingleOrDefault(c => c.Title == title);

						if (c != null) {
							var orgC = factory.GetOriginChapter(c);
							_ = await orgC.GetPageUrls(false, factory);

							_originChapterCache[(source, mangaName, title)] = orgC;

							return orgC;
						}
					}
				} catch (Exception) {
					break;
				}
			}

			return null;
		}
	}
}
