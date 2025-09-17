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

		public string SourceKey => "MangaDex";

		public MangaDexFactory(IRequest requestHelper, HtmlHelper htmlHelper)
		{
			_requestHelper = requestHelper;
			_htmlHelper = htmlHelper;
		}

		public IReader CreateReader()
		{
			return new MangaDexReader(_requestHelper, _htmlHelper);
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
