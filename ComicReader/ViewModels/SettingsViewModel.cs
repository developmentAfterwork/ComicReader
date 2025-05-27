using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class SettingsViewModel : ObservableObject
	{
		private readonly SettingsService settingsService;
		private readonly FileSaverService fileSaverService;
		private readonly SimpleNotificationService simpleNotificationService;
		private readonly Factory factory;

		public ICommand WriteSettings { get; set; }

		public ICommand ReadSettings { get; set; }

		[ObservableProperty]
		private bool _HideEmptyManga = true;

		[ObservableProperty]
		private bool _DeleteMangaAfterReaded = true;

		public SettingsViewModel(SettingsService settingsService, FileSaverService fileSaverService, SimpleNotificationService simpleNotificationService, Factory factory)
		{
			this.settingsService = settingsService;
			this.fileSaverService = fileSaverService;
			this.simpleNotificationService = simpleNotificationService;
			this.factory = factory;
			WriteSettings = new AsyncRelayCommand(OnWriteSettings);
			ReadSettings = new AsyncRelayCommand(OnReadSettings);

			HideEmptyManga = settingsService.GetHideEmptyManga();
			DeleteMangaAfterReaded = settingsService.GetDeleteChaptersAfterReading();
		}

		private async Task OnWriteSettings()
		{
			var bookmarkedMangasIds = settingsService.GetBookmarkedMangaUniqIdentifiers();
			Dictionary<string, int> positionsDict = new Dictionary<string, int>();
			Dictionary<string, bool> readedDict = new Dictionary<string, bool>();

			foreach (var id in bookmarkedMangasIds) {
				try {
					var manga = await factory.GetMangaFromBookmarkId(id);
					var chapters = await manga.GetBooks();
					foreach (var chapter in chapters) {
						var keyPos = settingsService.GetKeyPosition(chapter);
						var keyReaded = settingsService.GetKeyReaded(chapter);
						var pos = settingsService.GetSaveChapterPosition(chapter);
						var readed = settingsService.GetChapterReaded(chapter);

						positionsDict[keyPos] = pos;
						readedDict[keyReaded] = readed;
					}
				} catch { }
			}

			var toWrite = new {
				bookmarkedMangasIds,
				positionsDict,
				readedDict,
			};

			string path = fileSaverService.GetSecurePathToDocuments("backup.json");
			string content = JsonConvert.SerializeObject(toWrite);

			await fileSaverService.SaveFile(path, content);
		}

		private async Task OnReadSettings()
		{
			string path = fileSaverService.GetSecurePathToDocuments("backup.json");
			var backupExists = fileSaverService.FileExists(path);

			if (backupExists) {
				var content = await fileSaverService.LoadFile(path);
				var json = JsonConvert.DeserializeObject<JObject>(content);

				if (json != null) {
					var bookmarkIds = json["bookmarkedMangasIds"]!.ToObject<List<string>>() ?? new();
					var positionsDict = json["positionsDict"]!.ToObject<Dictionary<string, int>>() ?? new();
					var readedDict = json["readedDict"]!.ToObject<Dictionary<string, bool>>() ?? new();

					foreach (var bookmarkId in bookmarkIds) {
						settingsService.BookmarkManga(bookmarkId);
					}

					foreach (var pos in positionsDict) {
						settingsService.SetSaveChapterPosition(pos.Key, pos.Value);
					}

					foreach (var read in readedDict) {
						settingsService.SetChapterAsReaded(read.Key, read.Value);
					}
				}
			}
		}

		internal void OnDisappearing()
		{
			settingsService.SetHideEmptyManga(HideEmptyManga);
			settingsService.SetDeleteChaptersAfterReading(DeleteMangaAfterReaded);
		}
	}
}
