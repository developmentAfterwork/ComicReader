namespace ComicReader.Interpreter
{
	public interface IManga
	{
		string Source { get; }

		string Name { get; }

		string HomeUrl { get; }

		string CoverUrl { get; }

		string Autor { get; }

		string Status { get; }

		string LanguageFlagUrl { get; }

		string Description { get; }

		Task<List<IChapter>> GetBooks();

		List<string> Genres { get; }
	}
}
