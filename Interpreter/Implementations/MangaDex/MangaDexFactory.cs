using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;
using Interpreter.Interface;

namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexFactory : IFactory
	{
		private readonly IRequest _requestHelper;
		private readonly HtmlHelper _htmlHelper;
		private readonly INotification _notification;

		public string SourceKey => "MangaDex";

		public MangaDexFactory(IRequest requestHelper, HtmlHelper htmlHelper, INotification notification)
		{
			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
			_notification = notification;
		}

		public IReader CreateReader()
		{
			return new MangaDexReader(_requestHelper, _htmlHelper, _notification);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new MangaDexChapter(
				saveableChapter.ID,
				saveableChapter.Title,
				saveableChapter.HomeUrl,
				saveableChapter.LastUpdate,
				saveableChapter.MangaName,
				saveableChapter.Source,
				_requestHelper,
				_htmlHelper
			);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new MangaDexManga(
				saveManga.ID,
				saveManga.Source,
				saveManga.Name,
				saveManga.HomeUrl,
				saveManga.CoverUrl,
				saveManga.Autor,
				saveManga.Status,
				saveManga.LanguageFlagUrl,
				saveManga.Description,
				saveManga.Genres,
				_requestHelper,
				_htmlHelper
			);
		}
	}
}
