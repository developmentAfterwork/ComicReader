namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class ChapterResult
	{
		public class ChapterAttribute
		{
			public string Volume { get; set; } = default!;

			public string Chapter { get; set; } = default!;

			public string UpdatedAt { get; set; } = default!;

			public string TranslatedLanguage { get; set; } = default!;
		}

		public string Id { get; set; }

		public ChapterAttribute Attributes { get; set; } = default!;

		public List<MangaDexPair> Relationships { get; set; } = default!;
	}
}
