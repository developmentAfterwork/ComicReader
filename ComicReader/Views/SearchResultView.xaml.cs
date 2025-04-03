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

	private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var c = sender as CollectionView;
		await searchResultViewModel.MangaSelected(e.CurrentSelection.FirstOrDefault());
		c.SelectedItem = null;
	}
}
