using AndroidX.AppCompat.View.Menu;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ComicReader.ViewModels.Model {
	public class IChapterModel : ObservableObject {

		public string Title { get; set; } = "";

		public string LastUpdate { get; set; } = "";

		public string DownloadedPages { get; set; } = "";

		public IChapter Chapter { get; set; }

		public IChapterModel(IChapter chapter) {
			Title = chapter.Title;
			LastUpdate = chapter.LastUpdate;
			DownloadedPages = "";
			Chapter = chapter;
		}

		public static async Task<IChapterModel> Create(IChapter chapter, FileSaverService fileSaverService, bool showDownloadPages) {
			var c = new IChapterModel(chapter);

			if (showDownloadPages) {
				string pages;
				if (fileSaverService.FileExists(chapter)) {
					var sc = await fileSaverService.LoadMangaChapterFile(chapter.Source, chapter.MangaName, chapter.Title);
					var m = sc.UrlToLocalFileMapper;

					var max = m.Count;
					var current = 0;
					foreach (var page in m) {
						if (fileSaverService.FileExists(page.Value)) {
							current++;
						}
					}

					pages = $"{current}/{max}";
				} else {
					pages = "???";
				}

				c.DownloadedPages = pages;
			}

			return c;
		}
	}
}
