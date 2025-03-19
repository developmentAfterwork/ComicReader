using ComicReader.Helper;
using ComicReader.Interpreter;
using Newtonsoft.Json;

namespace ComicReader.Services
{
	public class SettingsService
	{
		private const string BookmarkMangasKey = "BookmarkMangas";

		public SettingsService() { }

		public void BookmarkManga(IManga manga)
		{
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Add(manga.GetUniqIdentifier());

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public void RemoveManga(IManga manga)
		{
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Remove(manga.GetUniqIdentifier());

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public void BookmarkManga(string id)
		{
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Add(id);

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public List<string> GetBookmarkedMangaUniqIdentifiers()
		{
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			return mangaNames ?? new();
		}

		public string GetKey(IChapter chapter)
		{
			return $"{chapter.MangaName}{chapter.Title}{chapter.Source}";
		}

		public string GetKeyPosition(IChapter chapter)
		{
			return $"{GetKey(chapter)}_position";
		}

		public string GetKeyReaded(IChapter chapter)
		{
			return $"{GetKey(chapter)}_readed";
		}

		public int GetSaveChapterPosition(IChapter chapter)
		{
			var value = Preferences.Get(GetKeyPosition(chapter), 0);

			return value;
		}

		public void SetSaveChapterPosition(IChapter chapter, int position)
		{
			Preferences.Set(GetKeyPosition(chapter), position);
		}

		public void SetSaveChapterPosition(string chapterKey, int position)
		{
			Preferences.Set(chapterKey, position);
		}

		public void SetChapterAsReaded(IChapter chapter)
		{
			Preferences.Set(GetKeyReaded(chapter), true);
		}

		public void SetChapterAsReaded(string chapterKey, bool value)
		{
			Preferences.Set(chapterKey, value);
		}

		public bool GetChapterReaded(IChapter chapter)
		{
			return Preferences.Get(GetKeyReaded(chapter), false);
		}
	}
}
