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
