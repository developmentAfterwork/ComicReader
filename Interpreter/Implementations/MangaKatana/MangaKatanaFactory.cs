using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.MangaKatana
{
	public class MangaKatanaFactory : IFactory
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string SourceKey => MangaKatanaManga.SourceKey;

		public MangaKatanaFactory(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public IReader CreateReader()
		{
			return new MangaKatanaReader(requestHelper, htmlHelper);
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
