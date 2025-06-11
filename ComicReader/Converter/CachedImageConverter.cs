using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using System.Globalization;

namespace ComicReader.Converter
{
	public class CachedImageConverter : IValueConverter
	{
		RequestHelper requestHelper = new RequestHelper();
		FileSaverService fileSaverService = new FileSaverService();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null && value.GetType() == typeof(string)) {
				return ConvertUrlToFile(value as string, parameter as string);
			}

			return value;
		}

		public string? ConvertUrlToFile(string? value, string? parameter)
		{
			if (value != null && value.GetType() == typeof(string)) {
				if (parameter == null) {
					string? url = value as string;

					if (url != null) {
						string pathWithFile = CheckAndGetPathFromUrl(url);

						if (File.Exists(pathWithFile)) {
							return pathWithFile;
						}
					}
				} else {
					string? url = value as string;
					string? keyWord = parameter as string;
					InMemoryDatabase? inMemoryDatabase = StaticClassHolder<Singleton<InMemoryDatabase>>.Value?.Instance;

					if (url != null && keyWord != null && inMemoryDatabase != null) {
						IChapter chapter = inMemoryDatabase.Get<IChapter>(keyWord);

						if (chapter.UrlToLocalFileMapper.ContainsKey(url)) {
							var pathWithFile = chapter.UrlToLocalFileMapper[url];

							if (string.IsNullOrEmpty(pathWithFile)) {
								throw new ArgumentException();
							}

							if (File.Exists(pathWithFile)) {
								return pathWithFile;
							}
						}
					}
				}
			}

			return value;
		}

		public static string CheckAndGetPathFromUrl(string url)
		{
			var path = url.Replace("https://", "");
			var pathSplit = path.Split('/').ToList();

			var start = FileSaverService.GetSecurePathToImages();
			var addOnsWithOutFile = pathSplit.Take(pathSplit.Count - 1).ToList();

			var finalPath = FileSaverService.CheckFolderExists(start, addOnsWithOutFile, false);
			var fileName = pathSplit.Last();

			var pathWithFile = Path.Combine(finalPath, fileName);
			return pathWithFile;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
