using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansFactory : IFactory
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string SourceKey => "AsuraScans";

		public AsuraScansFactory(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public IReader CreateReader()
		{
			return new AsuraScansReader(requestHelper, htmlHelper);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new AsureScansChapter(saveableChapter.ID, saveableChapter.Source, saveableChapter.MangaName, saveableChapter.Title, saveableChapter.HomeUrl, saveableChapter.LastUpdate, requestHelper, htmlHelper);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new AsuraScansManga(saveManga.Name, saveManga.HomeUrl, saveManga.CoverUrl, saveManga.Autor, saveManga.Status, saveManga.LanguageFlagUrl, saveManga.Description, saveManga.Genres, saveManga.Source, requestHelper, htmlHelper);
		}
	}
}
