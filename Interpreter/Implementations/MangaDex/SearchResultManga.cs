namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class SearchResultManga
	{
		public string Id { get; set; }

		public Attribute Attributes { get; set; }

		public List<MangaDexPair> Relationships { get; set; }
	}

	public class CoverResult
	{
		public class CovterAttribute
		{
			public string FileName { get; set; }
		}

		public string Id { get; set; }

		public CovterAttribute Attributes { get; set; }
	}
}
