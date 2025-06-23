using ComicReader.Reader;

namespace ComicReader.Interpreter.Interface
{
	public interface IFactory
	{
		string SourceKey { get; }
		IManga GetOriginManga(SaveableManga saveManga);
		IChapter GetOriginChapter(SaveableChapter saveableChapter);
		IReader CreateReader();
	}
}
