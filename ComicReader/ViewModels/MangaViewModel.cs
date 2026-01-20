using ComicReader.Converter;
using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interpreter.Interface;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class MangaViewModel : ObservableObject
	{
		private readonly IManga manga;
		private readonly IRequest request;
		private readonly SettingsService settingsService;

		[ObservableProperty]
		private string _CoverUrl = string.Empty;

		[ObservableProperty]
		private ImageSource _CoverUrlImageSource = default!;

		[ObservableProperty]
		private bool _IsLoadingImage = true;

		[ObservableProperty]
		private string _Title = string.Empty;

		public ICommand MangeSelected => new RelayCommand(OnMangeSelected);

		public ICommand DownloadManga => new RelayCommand(OnDownloadManga);

		public event EventHandler<IManga>? Selected;

		public event EventHandler<IManga>? StartToDownload;

		public MangaViewModel(IManga manga, IRequest request, SettingsService settingsService)
		{
			CoverUrl = manga.CoverUrl;

			this.manga = manga;
			this.request = request;
			this.settingsService = settingsService;
			this.Title = manga.Name;

			_ = Task.Run(async () => {
				await Task.Delay(500);

				string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
				if (File.Exists(pathWithFile)) {
					CoverUrlImageSource = ImageSource.FromFile(pathWithFile);
				} else {
					if (!File.Exists(pathWithFile)) {
						await request.DownloadFile(manga.CoverUrl, pathWithFile, 3, settingsService.GetRequestTimeout(), manga.RequestHeaders);
					}

					if (File.Exists(pathWithFile)) {
						CoverUrlImageSource = ImageSource.FromFile(pathWithFile);
					}
				}

				IsLoadingImage = false;
			});
		}

		private void OnMangeSelected()
		{
			Selected?.Invoke(this, manga);
		}

		private void OnDownloadManga()
		{
			StartToDownload?.Invoke(this, manga);
		}
	}
}
