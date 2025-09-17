namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class SearchResultManga
	{
		public string Id { get; set; } = default!;

		public Attribute Attributes { get; set; } = default!;

		public List<MangaDexPair> Relationships { get; set; } = default!;
	}

	public class CoverResult
	{
		public class CovterAttribute
		{
			public string FileName { get; set; } = default!;
		}

		public string Id { get; set; } = default!;

		public CovterAttribute Attributes { get; set; } = default!;
	}
}
