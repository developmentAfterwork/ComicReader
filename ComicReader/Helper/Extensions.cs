using ComicReader.Interpreter;
using ComicReader.Services;

namespace ComicReader.Helper {
	public static class Extensions {
		public static async Task Refresh(this IManga manga, Factory factory, FileSaverService fileSaverService, SimpleNotificationService simpleNotificationService) {
			var allChapters = await manga.GetBooks();

			foreach (var chapter in allChapters) {
				try {
					if (fileSaverService.FileExists(chapter)) {
						IChapter chapterObj = chapter;
						if (chapter is SaveableChapter) {
							var allPageUrls = await chapter.GetPageUrls(false, factory);
							var mappedUrls = chapter.UrlToLocalFileMapper;

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
	}
}
