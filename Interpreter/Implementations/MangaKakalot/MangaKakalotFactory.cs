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

		public string SourceKey => MangaKakalotManga.SourceKey;

		public MangaKakalotFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
		}

		public IReader CreateReader()
		{
			return new MangaKakalotReader(requestHelper, htmlHelper, notification);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new MangaKakalotChapter(
				saveableChapter.Title,
				saveableChapter.HomeUrl,
				saveableChapter.LastUpdate,
				saveableChapter.MangaName,
				saveableChapter.Source,
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
				htmlHelper
			);
		}
	}
}
