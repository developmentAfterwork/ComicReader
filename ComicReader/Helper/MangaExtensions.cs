using ComicReader.Interpreter;

namespace ComicReader.Helper
{
	internal static class MangaExtensions
	{
		public static async Task Save(this IManga manga)
		{
			var books = await manga.GetBooks();
			var saveManga = new SaveableManga(manga) {
				Books = books.Select(s => new SaveableChapter(s)).ToList()
			};
			await saveManga.Save();
		}

		public static string GetUniqIdentifier(this IManga manga)
		{
			return $"{manga.Source}|{manga.Name}";
		}
	}
}
