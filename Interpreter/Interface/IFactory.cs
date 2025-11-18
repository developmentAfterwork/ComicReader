using ComicReader.Reader;

namespace ComicReader.Interpreter.Interface
{
	public interface IFactory
	{
		string SourceKey { get; }
		IManga GetOriginManga(SaveableManga saveManga);
		IChapter GetOriginChapter(IChapter saveableChapter);
		IReader CreateReader();
		Dictionary<string, string>? GetOriginChapterRequestHeaders();
	}
}
