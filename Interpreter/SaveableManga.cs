using ComicReader.Services;

namespace ComicReader.Interpreter
{
	public record SaveableManga : IManga
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		public string? ID { get; set; } = null;

		public string Name { get; set; } = string.Empty;

		public string HomeUrl { get; set; } = string.Empty;

		public string CoverUrl { get; set; } = string.Empty;

		public string Autor { get; set; } = string.Empty;

		public string Status { get; set; } = string.Empty;

		public string LanguageFlagUrl { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public List<string> Genres { get; set; } = new();

		public List<SaveableChapter> Books { get; set; } = new();

		public string Source { get; set; } = string.Empty;

		public Dictionary<string, string>? RequestHeaders { get; set; } = null;

		public Task<List<IChapter>> GetBooks()
		{
			return Task.FromResult(Books.Cast<IChapter>().ToList());
		}

		public SaveableManga()
		{

		}

		public SaveableManga(IManga manga)
		{
			ID = manga.ID;
			Name = manga.Name;
			HomeUrl = manga.HomeUrl;
			CoverUrl = manga.CoverUrl;
			Autor = manga.Autor;
			Status = manga.Status;
			LanguageFlagUrl = manga.LanguageFlagUrl;
			Description = manga.Description;
			Genres = manga.Genres;
			Source = manga.Source;
			RequestHeaders = manga.RequestHeaders;
		}

		public async Task Save()
		{
			await fileSaverService.SaveFile(this);
		}

		internal static async Task<IManga> Load(string source, string title)
		{
			FileSaverService fileSaverService = new FileSaverService();

			return await fileSaverService.LoadMangaFile(source, title);
		}
	}
}
