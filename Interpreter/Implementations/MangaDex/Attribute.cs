namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class Attribute
	{
		public Dictionary<string, string> Title { get; set; } = default!;

		public Dictionary<string, string> Description { get; set; } = default!;

		public Dictionary<string, string> Links { get; set; } = default!;

		public string Status { get; set; } = default!;
	}
}
