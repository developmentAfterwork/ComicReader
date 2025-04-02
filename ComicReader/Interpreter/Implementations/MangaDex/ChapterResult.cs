namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class ChapterResult
	{
		public class ChapterAttribute
		{
			public string Volume { get; set; }

			public string Chapter { get; set; }

			public string UpdatedAt { get; set; }

			public string TranslatedLanguage { get; set; }
		}

		public string Id { get; set; }

		public ChapterAttribute Attributes { get; set; }

		public List<MangaDexPair> Relationships { get; set; }
	}
}
