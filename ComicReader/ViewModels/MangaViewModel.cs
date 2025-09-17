using ComicReader.Converter;
using ComicReader.Helper;
using ComicReader.Interpreter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class MangaViewModel : ObservableObject
	{
		private readonly IManga manga;

		[ObservableProperty]
		private string _CoverUrl = string.Empty;

		[ObservableProperty]
		private ImageSource _CoverUrlImageSource = default!;

		[ObservableProperty]
		private string _Title = string.Empty;

		public ICommand MangeSelected => new RelayCommand(OnMangeSelected);

		public ICommand DownloadManga => new RelayCommand(OnDownloadManga);

		public event EventHandler<IManga>? Selected;

		public event EventHandler<IManga>? StartToDownload;

		public MangaViewModel(IManga manga)
		{
			CoverUrl = manga.CoverUrl;
			this.manga = manga;
			this.Title = manga.Name;

			string pathWithFile = CachedImageConverter.CheckAndGetPathFromUrl(manga.CoverUrl);
			if (File.Exists(pathWithFile)) {
				CoverUrlImageSource = ImageSource.FromFile(pathWithFile);
			} else {
				var mem = (new RequestHelper()).DoGetRequestStream(manga.CoverUrl, manga.RequestHeaders).Result;
				if (mem != null) {
					CoverUrlImageSource = ImageSource.FromStream(() => mem);
				}
			}
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
