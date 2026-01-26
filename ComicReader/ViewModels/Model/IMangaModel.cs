using ComicReader.Converter;
using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Interpreter.Interface;
using System.Collections.ObjectModel;


namespace ComicReader.ViewModels.Model
{
	public partial class IMangaModelGroup : ObservableObject
	{
		public string Source { get; set; } = default!;

		[ObservableProperty]
		private bool _IsSearching;

		[ObservableProperty]
		private ObservableCollection<IMangaModel> _Mangas = new();
	}

	public partial class IMangaModel : ObservableObject
	{
		public string Name { get; set; } = default!;

		public string Source { get; set; } = default!;

		public string Description { get; set; } = default!;

		[ObservableProperty]
		private ImageSource? _sourcCoverUrlSource;

		[ObservableProperty]
		private string _coverUrl = default!;

		public IManga Manga { get; set; } = default!;

		public static IMangaModel Create(IManga manga, IRequest request, SettingsService settingsService, Dictionary<string, string>? requestHeaders)
		{
			var model = new IMangaModel();

			model.Name = manga.Name;
			model.Source = manga.Source;
			model.Description = manga.Description;
			model.CoverUrl = manga.CoverUrl;
			model.SourcCoverUrlSource = null;

			_ = Task.Run(async () => {
				await Task.Delay(500);

				string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
				if (File.Exists(pathWithFile)) {
					model.SourcCoverUrlSource = ImageSource.FromFile(pathWithFile);
				} else {
					if (!File.Exists(pathWithFile)) {
						await request.DownloadFile(manga.CoverUrl, pathWithFile, 3, settingsService.GetRequestTimeout(), manga.RequestHeaders);
					}

					if (File.Exists(pathWithFile)) {
						model.SourcCoverUrlSource = ImageSource.FromFile(pathWithFile);
					}
				}
			});

			model.Manga = manga;

			return model;
		}
	}
}
