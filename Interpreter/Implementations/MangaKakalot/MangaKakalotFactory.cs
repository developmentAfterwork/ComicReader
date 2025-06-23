using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.MangaKakalot
{
	public class MangaKakalotFactory : IFactory
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string SourceKey => MangaKakalotManga.SourceKey;

		public MangaKakalotFactory(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public IReader CreateReader()
		{
			return new MangaKakalotReader(requestHelper, htmlHelper);
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
