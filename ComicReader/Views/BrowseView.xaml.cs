using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class BrowseView : ContentPage
{
	private readonly BrowseViewModel browseViewModel;

	public BrowseView(BrowseViewModel browseViewModel)
	{
		InitializeComponent();
		BindingContext = browseViewModel;
		this.browseViewModel = browseViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		browseViewModel.OnAppearing();
	}
}
