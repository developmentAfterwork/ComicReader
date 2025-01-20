using ComicReader.Interpreter.Interface;
using ComicReader.Reader;

namespace ComicReader.Interpreter
{
	public class Factory
	{
		private Dictionary<string, IFactory> _factories = new Dictionary<string, IFactory>();

		public Factory()
		{

		}

		public void Register(IFactory factory)
		{
			_factories[factory.SourceKey] = factory;
		}

		internal IEnumerable<IReader> CreateAllReaders()
		{
			return _factories.Values.Select(f => f.CreateReader());
		}

		public async Task<IManga> GetMangaFromBookmarkId(string bookmarkId)
		{
			var source = bookmarkId.Split("|")[0];
			var title = bookmarkId.Split("|")[1];
			var saveableManga = await SaveableManga.Load(source, title).ConfigureAwait(false);

			return saveableManga;
		}

		internal IManga GetOriginManga(SaveableManga saveableManga)
		{
			return _factories[saveableManga.Source].GetOriginManga(saveableManga);
		}

		internal IChapter GetOriginChapter(SaveableChapter saveableChapter)
		{
			return _factories[saveableChapter.Source].GetOriginChapter(saveableChapter);
		}
	}
}
