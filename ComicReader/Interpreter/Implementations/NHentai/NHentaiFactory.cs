using ComicReader.Helper;
using ComicReader.Interpreter.Interface;
using ComicReader.Reader;

namespace ComicReader.Interpreter.Implementations.NHentai
{
	public class NHentaiFactory : IFactory
	{
		private readonly RequestHelper requestHelper;
		private readonly HtmlHelper htmlHelper;

		public string SourceKey => "NHentai";

		public NHentaiFactory(RequestHelper requestHelper, HtmlHelper htmlHelper)
		{
			this.requestHelper = requestHelper;
			this.htmlHelper = htmlHelper;
		}

		public IReader CreateReader()
		{
			return new NHentaiReader(requestHelper, htmlHelper);
		}

		public IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return new NHentaiChapter(saveableChapter.ID, saveableChapter.Source, saveableChapter.MangaName, saveableChapter.Title, saveableChapter.HomeUrl, saveableChapter.LastUpdate, requestHelper, htmlHelper);
		}

		public IManga GetOriginManga(SaveableManga saveManga)
		{
			return new NHentaiManga(saveManga.Name, saveManga.HomeUrl, saveManga.CoverUrl, saveManga.Autor, saveManga.Status, saveManga.LanguageFlagUrl, saveManga.Description, saveManga.Genres, saveManga.Source, requestHelper, htmlHelper);
		}
	}
}
