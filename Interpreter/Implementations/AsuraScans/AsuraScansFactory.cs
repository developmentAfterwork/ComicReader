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
		private readonly IRequestTimeout timeout;

		public string SourceKey => "AsuraScans";

		public AsuraScansFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification, IRequestTimeout timeout)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
			this.timeout = timeout;
		}

		public IReader CreateReader()
		{
			return new AsuraScansReader(requestHelper, htmlHelper, notification, timeout.Timeout);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new AsureScansChapter(saveableChapter.ID, saveableChapter.Source, saveableChapter.MangaName, saveableChapter.Title, saveableChapter.HomeUrl, saveableChapter.LastUpdate, timeout.Timeout, requestHelper, htmlHelper, notification);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new AsuraScansManga(saveManga.Name, saveManga.HomeUrl, saveManga.CoverUrl, saveManga.Autor, saveManga.Status, saveManga.LanguageFlagUrl, saveManga.Description, saveManga.Genres, saveManga.Source, requestHelper, htmlHelper, notification, timeout.Timeout);
		}
	}
}
