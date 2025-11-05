using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.MangaKatana
{
	public class MangaKatanaFactory : IFactory
	{
		private readonly IRequest requestHelper;
		private readonly HtmlHelper htmlHelper;
		private readonly INotification notification;
		private readonly IRequestTimeout timeout;

		public string SourceKey => MangaKatanaManga.SourceKey;

		public MangaKatanaFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification, IRequestTimeout timeout)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
			this.timeout = timeout;
		}

		public IReader CreateReader()
		{
			return new MangaKatanaReader(requestHelper, htmlHelper, notification, timeout.Timeout);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new MangaKatanaChapter(
				saveableChapter.Title,
				saveableChapter.HomeUrl,
				saveableChapter.LastUpdate,
				saveableChapter.MangaName,
				saveableChapter.Source,
				timeout.Timeout,
				requestHelper,
				htmlHelper);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new MangaKatanaManga(
				saveManga.Name,
				saveManga.HomeUrl,
				saveManga.CoverUrl,
				saveManga.Autor,
				saveManga.Status,
				saveManga.LanguageFlagUrl,
				saveManga.Description,
				saveManga.Genres,
				requestHelper,
				htmlHelper,
				timeout.Timeout
			);
		}
	}
}
