using ComicReader.Helper;
using ComicReader.Services;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations
{
	public abstract class BaseChapter : IChapter
	{
		protected readonly IRequest RequestHelper;
		protected readonly HtmlHelper HtmlHelper;

		public string? ID { get; } = null;

		public string Title { get; }

		public string HomeUrl { get; }

		public string LastUpdate { get; }

		public string MangaName { get; }

		public string Source { get; }

		public Dictionary<string, string> UrlToLocalFileMapper { get; } = new();

		public virtual Dictionary<string, string>? RequestHeaders { get; } = null;

		public async Task<List<string>> GetPageUrls(bool preDownloadChapters, Factory factory)
		{
			return await ProcessPageUrls(preDownloadChapters, factory).ConfigureAwait(false);
		}

		public abstract Task<List<string>> ImplGetPageUrls();

		public BaseChapter(
			string? id,
			string title,
			string homeUrl,
			string lastUpdate,
			string mangaName,
			string source,
			IRequest requestHelper,
			HtmlHelper htmlHelper)
		{
			ID = id;
			Title = title;
			HomeUrl = homeUrl;
			LastUpdate = lastUpdate;
			MangaName = mangaName;
			Source = source;

			this.RequestHelper = requestHelper;
			this.HtmlHelper = htmlHelper;
		}

		private async Task<List<string>> ProcessPageUrls(bool preDownloadChapters, Factory factory)
		{
			var pages = await ImplGetPageUrls();
			FillMapper(pages, preDownloadChapters);
			if (preDownloadChapters) {
				await PreDownloadChapters();
			}

			return pages;
		}

		private void FillMapper(List<string> urls, bool createFolderIfMissing = true)
		{
			foreach (var url in urls) {
				if (!UrlToLocalFileMapper.ContainsKey(url)) {
					var path = FileSaverService.GetChapterImageFolder(this, createFolderIfMissing);

					var ext = url.Substring(url.LastIndexOf("."));
					var fileName = $"{Guid.NewGuid()}{ext}";

					path = Path.Combine(path, fileName);

					UrlToLocalFileMapper[url] = path;
				}
			}
		}

		private async Task PreDownloadChapters()
		{
			foreach (var pair in UrlToLocalFileMapper) {
				if (!File.Exists(pair.Value)) {
					var pathWithFile = UrlToLocalFileMapper[pair.Key];
					await RequestHelper.DownloadFile(pair.Key, pathWithFile, 3, RequestHeaders);
				}
			}
		}
	}
}
