namespace ComicReader.Interpreter.Implementations.MangaDex
{
	public class MangaDexResult<T>
	{
		public string Result { get; set; }

		public string Response { get; set; }

		public int Limit { get; set; }

		public int Offset { get; set; }

		public int Total { get; set; }

		public T Data { get; set; }
	}

	public class MangaDexCoverResult
	{
		public string Result { get; set; }

		public CoverResult Data { get; set; }
	}

	public class MangaDexPagesResult
	{
		public class ResultChapter
		{
			public string Hash { get; set; }

			public List<string> Data { get; set; }
		}

		public string Result { get; set; }

		public string BaseUrl { get; set; }

		public ResultChapter Chapter { get; set; }
	}
}
