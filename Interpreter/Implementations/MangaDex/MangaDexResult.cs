namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexResult<T>
	{
		public string Result { get; set; } = default!;

		public string Response { get; set; } = default!;

		public int Limit { get; set; }

		public int Offset { get; set; }

		public int Total { get; set; }

		public T Data { get; set; } = default!;
	}

	public class MangaDexCoverResult
	{
		public string Result { get; set; } = default!;

		public CoverResult Data { get; set; } = default!;
	}

	public class MangaDexPagesResult
	{
		public class ResultChapter
		{
			public string Hash { get; set; } = default!;

			public List<string> Data { get; set; } = default!;
		}

		public string Result { get; set; } = default!;

		public string BaseUrl { get; set; } = default!;

		public ResultChapter Chapter { get; set; } = default!;
	}
}
