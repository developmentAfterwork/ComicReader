using ComicReader.Interpreter;

namespace ComicReader.Views.Models
{
	public class MangaChapterViewModel
	{
		public IManga Manga { get; set; }

		public IChapter Chapter { get; set; }

		public MangaChapterViewModel(IManga manga, IChapter chapter)
		{
			Manga = manga;
			Chapter = chapter;
		}
	}
}
