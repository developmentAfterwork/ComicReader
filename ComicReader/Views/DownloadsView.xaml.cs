using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class DownloadsView : ContentPage
{
	private readonly DownloadsViewModel downloadsViewModel;

	public DownloadsView(DownloadsViewModel downloadsViewModel)
	{
		InitializeComponent();
		BindingContext = downloadsViewModel;
		this.downloadsViewModel = downloadsViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = downloadsViewModel.OnAppearing();
	}
}
