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

		public string SourceKey => MangaKatanaManga.SourceKey;

		public MangaKatanaFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
			this.notification = notification;
		}

		public IReader CreateReader()
		{
			return new MangaKatanaReader(requestHelper, htmlHelper, notification);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new MangaKatanaChapter(
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
				htmlHelper
			);
		}
	}
}
