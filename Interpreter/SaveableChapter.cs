using ComicReader.Helper;
using ComicReader.Services;

namespace ComicReader.Interpreter
{
	public class SaveableChapter : IChapter
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		public string? ID { get; set; } = null;

		public string MangaName { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;

		public string HomeUrl { get; set; } = string.Empty;

		public string LastUpdate { get; set; } = string.Empty;

		public string Source { get; set; } = string.Empty;

		public List<string> Pages { get; set; } = new();

		public Dictionary<string, string> UrlToLocalFileMapper { get; set; } = new();

		public Dictionary<string, string>? RequestHeaders { get; } = null;

		public SaveableChapter()
		{

		}

		public SaveableChapter(IChapter chapter)
		{
			ID = chapter.ID;
			MangaName = chapter.MangaName;
			Title = chapter.Title;
			HomeUrl = chapter.HomeUrl;
			LastUpdate = chapter.LastUpdate;
			Source = chapter.Source;
			UrlToLocalFileMapper = chapter.UrlToLocalFileMapper;
		}

		public async Task Save()
		{
			await fileSaverService.SaveFile(this);
		}

		public async Task<List<string>> GetPageUrls(bool preDownloadChapters, Factory factory)
		{
			try {
				var chapter = await fileSaverService.LoadMangaChapterFile(Source, MangaName, Title);

				Pages = chapter.Pages;
				UrlToLocalFileMapper = chapter.UrlToLocalFileMapper;

				return chapter.Pages;
			} catch {
				var chapter = factory.GetOriginChapter(this);

				Pages = await chapter.GetPageUrls(preDownloadChapters, factory);
				UrlToLocalFileMapper = chapter.UrlToLocalFileMapper;
				await chapter.Save(preDownloadChapters, factory);

				return Pages;
			}
		}
	}
}
