using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.AsuraScans
{
	public class AsuraScansFactory : IFactory
	{
		private readonly IRequest requestHelper;
		private readonly HtmlHelper htmlHelper;
		private readonly INotification notification;

		public string SourceKey => "AsuraScans";

		public AsuraScansFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
		}

		public IReader CreateReader()
		{
			return new AsuraScansReader(requestHelper, htmlHelper, notification);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new AsureScansChapter(saveableChapter.ID, saveableChapter.Source, saveableChapter.MangaName, saveableChapter.Title, saveableChapter.HomeUrl, saveableChapter.LastUpdate, requestHelper, htmlHelper, notification);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new AsuraScansManga(saveManga.Name, saveManga.HomeUrl, saveManga.CoverUrl, saveManga.Autor, saveManga.Status, saveManga.LanguageFlagUrl, saveManga.Description, saveManga.Genres, saveManga.Source, requestHelper, htmlHelper, notification);
		}
	}
}
