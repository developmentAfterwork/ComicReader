using ComicReader.Interpreter;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace ComicReader.Services
{
	public class FileSaverService
	{
		private static string RootDirectoryImages { get; } = "/storage/emulated/0/pictures";

		private static string RootDirectoryDocuments { get; } = "/storage/emulated/0/documents";

		private static IReadOnlyList<string> PathAddOns = new List<string>() { "ComicReader", "Cache" };

		private string GetMangaJsonPath(IManga manga)
		{
			return GetMangaJsonPath(manga.Source, manga.Name);
		}

		private string GetMangaJsonPath(string source, string name)
		{
			var start = FileSaverService.RootDirectoryDocuments;
			List<string> addOns = GetAddonsForDocuments(source, name);

			var path = CheckFolderExists(start, addOns);
			path = Path.Combine(path, "manga.json");

			return path;
		}

		private static List<string> GetAddonsForDocuments(string source, string mangaName)
		{
			return FileSaverService.PathAddOns.Concat(new List<string>() { "Books", source, mangaName }).ToList();
		}


		private string GetChapterJsonPath(IChapter chapter)
		{
			return GetChapterJsonPath(chapter.Source, chapter.MangaName, chapter.Title);
		}

		private string GetChapterJsonPath(string source, string mangaName, string title)
		{
			var start = FileSaverService.RootDirectoryDocuments;
			List<string> addOns = GetAddonsForDocuments(source, mangaName);

			var path = CheckFolderExists(start, addOns);

			var _title = GetFilename(title);

			path = Path.Combine(path, $"{_title}.json");

			return path;
		}

		public static string GetChapterImageFolder(IChapter chapter)
		{
			var root = FileSaverService.RootDirectoryImages;

			List<string> addon = GetAddonsForImages(chapter);

			var path = CheckFolderExists(root, addon);

			return path;
		}

		private static List<string> GetAddonsForImages(IChapter chapter)
		{
			var bytes = UTF8Encoding.UTF8.GetBytes(chapter.Title);
			var hashBytes = MD5.HashData(bytes);
			var hashInt = BitConverter.ToInt32(hashBytes, 0);
			var hex = hashInt.ToString("x2");

			return FileSaverService.PathAddOns.Concat(new List<string>() { "Books", chapter.Source, chapter.MangaName, hex }).ToList();
		}

		public async Task SaveFile(string filePath, byte[] content)
		{
			await File.WriteAllBytesAsync(filePath, content);
		}

		public async Task SaveFile(string filePath, string content)
		{
			await File.WriteAllTextAsync(filePath, content);
		}

		public async Task<string> LoadFile(string filePath)
		{
			return await File.ReadAllTextAsync(filePath);
		}

		public async Task SaveFile(IManga manga)
		{
			var jsonString = JsonConvert.SerializeObject(manga);

			var path = GetMangaJsonPath(manga);

			await SaveFile(path, jsonString);
		}

		public async Task SaveFile(IChapter chapter)
		{
			var jsonString = JsonConvert.SerializeObject(chapter);

			var path = GetChapterJsonPath(chapter);

			await SaveFile(path, jsonString);
		}

		public bool FileExists(IChapter chapter)
		{
			var path = GetChapterJsonPath(chapter);

			return File.Exists(path);
		}

		public void DeleteChapterFile(IChapter chapter)
		{
			var path = GetChapterJsonPath(chapter);

			File.Delete(path);
		}

		public void DeleteFile(string path)
		{
			File.Delete(path);
		}

		public async Task DeleteImagesFromChapter(IChapter chapter, Factory factory)
		{
			var urls = await chapter.GetPageUrls(false, factory);
			foreach (var url in urls) {
				var filePath = chapter.UrlToLocalFileMapper[url];
				if (FileExists(filePath)) {
					File.Delete(filePath);
				}
			}
		}

		public async Task<IManga> LoadMangaFile(string source, string name)
		{
			var path = GetMangaJsonPath(source, name);

			var content = await LoadFile(path);

			return JsonConvert.DeserializeObject<SaveableManga>(content)!;
		}

		public async Task<SaveableChapter> LoadMangaChapterFile(string source, string mangeName, string chapter)
		{
			var path = GetChapterJsonPath(source, mangeName, chapter);

			var content = await LoadFile(path);

			return JsonConvert.DeserializeObject<SaveableChapter>(content)!;
		}

		public static string CheckFolderExists(string start, List<string> addons)
		{
			if (!Directory.Exists(start)) {
				Directory.CreateDirectory(start);
			}

			if (addons.Count > 0) {
				var firstAddon = GetFilename(addons.First());
				var newStart = Path.Combine(start, firstAddon);
				var newAddons = addons.Skip(1).ToList();

				return CheckFolderExists(newStart, newAddons);
			} else {
				return start;
			}
		}

		public static string GetFilename(string filename)
		{
			//var invalidChars = "|\\?*<\":>+[]/'".ToArray();
			return filename.Replace(":", "").Replace("+", "").Replace("?", "");
		}

		internal bool FileExists(string value)
		{
			return File.Exists(value);
		}

		public string GetSecurePathToDocuments(string filename)
		{
			var root = FileSaverService.RootDirectoryDocuments;
			var addons = FileSaverService.PathAddOns.ToList();

			var path = FileSaverService.CheckFolderExists(root, addons);
			path = Path.Combine(path, filename);

			return path;
		}

		public string GetSecurePathToDocuments(string filename, List<string> addons)
		{
			var root = FileSaverService.RootDirectoryDocuments;
			var combinedAddons = FileSaverService.PathAddOns.Concat(addons).ToList();

			var path = FileSaverService.CheckFolderExists(root, combinedAddons);
			path = Path.Combine(path, filename);

			return path;
		}

		public static string GetSecurePathToImages()
		{
			var root = FileSaverService.RootDirectoryImages;
			var addons = FileSaverService.PathAddOns.ToList();

			var path = FileSaverService.CheckFolderExists(root, addons);

			return path;
		}
	}
}
