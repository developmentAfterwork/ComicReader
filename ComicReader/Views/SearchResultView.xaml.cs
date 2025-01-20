using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class SearchResultView : ContentPage
{
	private readonly SearchResultViewModel searchResultViewModel;

	public SearchResultView(SearchResultViewModel searchResultViewModel)
	{
		InitializeComponent();
		BindingContext = searchResultViewModel;
		this.searchResultViewModel = searchResultViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		searchResultViewModel.OnAppearing();
	}
}
