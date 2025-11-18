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

			return FixPath(path);
		}

		private static List<string> GetAddonsForDocuments(string source, string mangaName)
		{
			return FileSaverService.PathAddOns.Concat(new List<string>() { "Books", source, mangaName }).ToList();
		}


		private string GetChapterJsonPath(IChapter chapter)
		{
			return FixPath(GetChapterJsonPath(chapter.Source, chapter.MangaName, chapter.Title));
		}

		private string GetChapterJsonPath(string source, string mangaName, string title)
		{
			var start = FileSaverService.RootDirectoryDocuments;
			List<string> addOns = GetAddonsForDocuments(source, mangaName);

			var path = CheckFolderExists(start, addOns);

			var _title = GetFilename(title);

			path = Path.Combine(path, $"{_title}.json");

			return FixPath(path);
		}

		public static string GetChapterImageFolder(IChapter chapter, bool createFolderIfMissing = true)
		{
			var root = FileSaverService.RootDirectoryImages;

			List<string> addon = GetAddonsForImages(chapter);

			var path = CheckFolderExists(root, addon, createFolderIfMissing);

			return FixPath(path);
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
			var p = FixPath(filePath);
			await File.WriteAllBytesAsync(p, content);
		}

		private static string FixPath(string path)
		{
			return path.Replace("\"", "").Replace("'", "").Replace("[", "").Replace("]", "").Replace("|", "");
		}

		public async Task SaveFile(string filePath, string content)
		{
			var p = FixPath(filePath);
			await File.WriteAllTextAsync(p, content);
		}

		public async Task<string> LoadFile(string filePath)
		{
			var p = FixPath(filePath);
			return await File.ReadAllTextAsync(p);
		}

		public async Task SaveFile(IManga manga)
		{
			var jsonString = JsonConvert.SerializeObject(manga);

			var path = GetMangaJsonPath(manga);
			var p = FixPath(path);

			await SaveFile(p, jsonString);
		}

		public async Task SaveFile(IChapter chapter)
		{
			var jsonString = JsonConvert.SerializeObject(chapter);

			var path = GetChapterJsonPath(chapter);
			var p = FixPath(path);

			await SaveFile(p, jsonString);
		}

		public bool FileExists(IChapter chapter)
		{
			var path = GetChapterJsonPath(chapter);
			var p = FixPath(path);

			return File.Exists(p);
		}

		public void DeleteChapterFile(IChapter chapter)
		{
			var path = GetChapterJsonPath(chapter);
			var p = FixPath(path);

			File.Delete(p);
		}

		public void DeleteFile(string path)
		{
			var p = FixPath(path);
			File.Delete(p);
		}

		public async Task DeleteImagesFromChapter(IChapter chapter, Factory factory)
		{
			var urls = await chapter.GetPageUrls(false, factory);
			foreach (var url in urls) {
				var filePath = chapter.UrlToLocalFileMapper[url];
				var p = FixPath(filePath);

				if (FileExists(p)) {
					File.Delete(p);
				}
			}
		}

		public bool MangaFileExists(string source, string name)
		{
			var path = GetMangaJsonPath(source, name);
			var p = FixPath(path);

			return File.Exists(p);
		}

		public async Task<IManga> LoadMangaFile(string source, string name)
		{
			var path = GetMangaJsonPath(source, name);
			var p = FixPath(path);

			var content = await LoadFile(p);

			return JsonConvert.DeserializeObject<SaveableManga>(content)!;
		}

		public async Task<SaveableChapter> LoadMangaChapterFile(string source, string mangeName, string chapter)
		{
			var path = GetChapterJsonPath(source, mangeName, chapter);
			var p = FixPath(path);

			var content = await LoadFile(p);

			return JsonConvert.DeserializeObject<SaveableChapter>(content)!;
		}

		public static string CheckFolderExists(string start, List<string> addons, bool createFolderIfMissing = true)
		{
			var _sstart = FixPath(start);
			if (!Directory.Exists(_sstart)) {
				if (createFolderIfMissing) {
					Directory.CreateDirectory(_sstart);
				} else {
					return "";
				}
			}

			if (addons.Count > 0) {
				var firstAddon = GetFilename(addons.First());
				var newStart = Path.Combine(_sstart, firstAddon);
				var newAddons = addons.Skip(1).ToList();

				return FixPath(CheckFolderExists(newStart, newAddons, createFolderIfMissing));
			} else {
				return FixPath(_sstart);
			}
		}

		public static string GetFilename(string filename)
		{
			//var invalidChars = "|\\?*<\":>+[]/'".ToArray();
			return filename.Replace(":", "").Replace("+", "").Replace("?", "");
		}

		public bool FileExists(string value)
		{
			return File.Exists(FixPath(value));
		}

		public string GetSecurePathToDocuments(string filename)
		{
			var root = FileSaverService.RootDirectoryDocuments;
			var addons = FileSaverService.PathAddOns.ToList();

			var path = FileSaverService.CheckFolderExists(root, addons);
			path = Path.Combine(path, filename);

			return FixPath(path);
		}

		public string GetSecurePathToDocuments(string filename, List<string> addons)
		{
			var root = FileSaverService.RootDirectoryDocuments;
			var combinedAddons = FileSaverService.PathAddOns.Concat(addons).ToList();

			var path = FileSaverService.CheckFolderExists(root, combinedAddons);
			path = Path.Combine(path, filename);

			return FixPath(path);
		}

		public static string GetSecurePathToImages()
		{
			var root = FileSaverService.RootDirectoryImages;
			var addons = FileSaverService.PathAddOns.ToList();

			var path = FileSaverService.CheckFolderExists(root, addons);

			return FixPath(path);
		}

		public void DeleteManga(IManga manga)
		{
			var jsonString = JsonConvert.SerializeObject(manga);

			var path = GetMangaJsonPath(manga);
			File.Delete(FixPath(path));

			var start = FileSaverService.RootDirectoryDocuments;
			List<string> addOns = GetAddonsForDocuments(manga.Source, manga.Name);

			path = FixPath(CheckFolderExists(start, addOns));
			Directory.Delete(path, true);

			start = FileSaverService.RootDirectoryImages;
			addOns = GetAddonsForDocuments(manga.Source, manga.Name);

			path = FixPath(CheckFolderExists(start, addOns));
			Directory.Delete(path, true);
		}

		public void DeleteAllEmptyFolders()
		{
			var imagesPath = Path.Combine(RootDirectoryImages, "ComicReader");
			var docPath = Path.Combine(RootDirectoryDocuments, "ComicReader");

			DeleteEmptyFolder(imagesPath);
			DeleteEmptyFolder(docPath);
		}

		private void DeleteEmptyFolder(string path)
		{
			foreach (var folder in Directory.GetDirectories(path)) {
				DeleteEmptyFolder(folder);
			}

			var folders = Directory.GetDirectories(path);
			var files = Directory.GetFiles(path);

			if (!folders.Any() && !files.Any()) {
				Directory.Delete(path, false);
			}
		}

		public void CheckFiles(List<string> filesToCheck)
		{
			foreach (var f in filesToCheck) {
				if (FileExists(f)) {
					if (!IsSizeGreaterZero(f)) {
						File.Delete(f);
					}
				}
			}
		}

		public bool IsSizeGreaterZero(string file)
		{
			if (!File.Exists(file)) {
				return false;
			}

			FileInfo fileInfo = new FileInfo(file);
			return fileInfo.Length > 0;
		}

		internal Stream OpenWrite(string path)
		{
			return File.OpenWrite(FixPath(path));
		}
	}
}
