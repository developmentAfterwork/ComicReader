using ComicReader.Helper;
using ComicReader.Interpreter;
using Newtonsoft.Json;

namespace ComicReader.Services {
	public class SettingsService {
		private const string BookmarkMangasKey = "BookmarkMangas";
		private const string HideEmptyMangaKey = "HideEmptyManga";
		private const string DeleteChaptersAfterReadingKey = "DeleteChaptersAfterReading";
		private const string AutoAddChaptersToQueueKey = "AutoAddChaptersToQueue";
		private const string PreDownloadImagesKey = "PreDownloadImages";
		private const string RequestTimeoutKey = "RequestTimeout";
		private const string ShowDownloadedPagesNumbersKey = "ShowDownloadedPagesNumbers";

		public SettingsService() { }

		public void BookmarkManga(IManga manga) {
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Add(manga.GetUniqIdentifier());

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public void RemoveManga(IManga manga) {
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Remove(manga.GetUniqIdentifier());

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public void BookmarkManga(string id) {
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			if (mangaNames != null) {
				mangaNames.Add(id);

				mangaNames = mangaNames.Distinct().ToList();
				Preferences.Set(BookmarkMangasKey, JsonConvert.SerializeObject(mangaNames));
			}
		}

		public List<string> GetBookmarkedMangaUniqIdentifiers() {
			var allJson = Preferences.Get(BookmarkMangasKey, "[]");
			var mangaNames = JsonConvert.DeserializeObject<List<string>>(allJson);

			return mangaNames ?? new();
		}

		public string GetKey(IChapter chapter) {
			return $"{chapter.MangaName}{chapter.Title}{chapter.Source}";
		}

		public string GetKeyPosition(IChapter chapter) {
			return $"{GetKey(chapter)}_position";
		}

		public string GetKeyReaded(IChapter chapter) {
			return $"{GetKey(chapter)}_readed";
		}

		public int GetSaveChapterPosition(IChapter chapter) {
			var value = Preferences.Get(GetKeyPosition(chapter), 0);

			return value;
		}

		public void SetSaveChapterPosition(IChapter chapter, int position) {
			Preferences.Set(GetKeyPosition(chapter), position);
		}

		public void SetSaveChapterPosition(string chapterKey, int position) {
			Preferences.Set(chapterKey, position);
		}

		public void SetChapterAsReaded(IChapter chapter) {
			Preferences.Set(GetKeyReaded(chapter), true);
		}

		public void SetChapterAsReaded(string chapterKey, bool value) {
			Preferences.Set(chapterKey, value);
		}

		public bool GetChapterReaded(IChapter chapter) {
			return Preferences.Get(GetKeyReaded(chapter), false);
		}

		public bool GetHideEmptyManga() {
			return Preferences.Get(HideEmptyMangaKey, false);
		}

		public void SetHideEmptyManga(bool value) {
			Preferences.Set(HideEmptyMangaKey, value);
		}

		public bool GetDeleteChaptersAfterReading() {
			return Preferences.Get(DeleteChaptersAfterReadingKey, false);
		}

		public void SetDeleteChaptersAfterReading(bool value) {
			Preferences.Set(DeleteChaptersAfterReadingKey, value);
		}

		public bool GetAutoAddChaptersToQueue() {
			return Preferences.Get(AutoAddChaptersToQueueKey, false);
		}

		public void SetAutoAddChaptersToQueue(bool value) {
			Preferences.Set(AutoAddChaptersToQueueKey, value);
		}

		public bool GetPreDownloadImages() {
			return Preferences.Get(PreDownloadImagesKey, false);
		}

		public void SetPreDownloadImages(bool value) {
			Preferences.Set(PreDownloadImagesKey, value);
		}

		public TimeSpan GetRequestTimeout() {
			int timeoutInSeconds = Preferences.Get(RequestTimeoutKey, 30);
			return TimeSpan.FromSeconds(timeoutInSeconds);
		}

		internal void SetRequestTimeout(TimeSpan timeSpan) {
			int timeoutInSeconds = (int)timeSpan.TotalSeconds;
			Preferences.Set(RequestTimeoutKey, timeoutInSeconds);
		}

		public bool GetShowDownloadedPagesNumbers() {
			return Preferences.Get(ShowDownloadedPagesNumbersKey, true);
		}

		public void SetShowDownloadedPagesNumbers(bool value) {
			Preferences.Set(ShowDownloadedPagesNumbersKey, value);
		}
	}
}
