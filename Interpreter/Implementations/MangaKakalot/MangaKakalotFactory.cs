using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.MangaKakalot
{
	public class MangaKakalotFactory : IFactory
	{
		private readonly IRequest requestHelper;
		private readonly HtmlHelper htmlHelper;
		private readonly INotification notification;
		private readonly IRequestTimeout timeout;

		public string SourceKey => MangaKakalotManga.SourceKey;

		public MangaKakalotFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification, IRequestTimeout timeout)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
			this.timeout = timeout;
		}

		public IReader CreateReader()
		{
			return new MangaKakalotReader(requestHelper, htmlHelper, notification, timeout.Timeout);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new MangaKakalotChapter(
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
			return new MangaKakalotManga(
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
