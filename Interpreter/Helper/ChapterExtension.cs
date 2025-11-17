using ComicReader.Interpreter;

namespace ComicReader.Helper {
	public static class ChapterExtension {
		public static async Task<SaveableChapter> Save(this IChapter chapter, bool preDownloadChapters, Factory factory) {
			var pages = await chapter.GetPageUrls(preDownloadChapters, factory);

			var saveChapter = new SaveableChapter(chapter) {
				Pages = pages,
				UrlToLocalFileMapper = chapter.UrlToLocalFileMapper,
			};
			await saveChapter.Save();

			return saveChapter;
		}

		public static string GetUniqIdentifier(this IChapter manga) {
			return $"{manga.Source}|{manga.MangaName}|{manga.Title}";
		}
	}
}
