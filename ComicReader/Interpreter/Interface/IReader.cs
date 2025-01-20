using ComicReader.Interpreter;

namespace ComicReader.Reader
{
	public interface IReader
	{
		string Title { get; }
		bool IsEnabled { get; set; }
		string HomeUrl { get; }

		Task<List<IManga>> Search(string keyWords);

		Task<List<IManga>> LoadUpdatesAndNewMangs();
	}
}
