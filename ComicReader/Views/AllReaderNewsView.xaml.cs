using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class AllReaderNewsView : ContentPage
{
	private readonly AllReaderNewsViewModel allReaderNewsViewModel;

	public AllReaderNewsView(AllReaderNewsViewModel allReaderNewsViewModel)
	{
		InitializeComponent();
		BindingContext = allReaderNewsViewModel;
		this.allReaderNewsViewModel = allReaderNewsViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		allReaderNewsViewModel.OnAppearing();
	}

	private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var c = sender as CollectionView;
		await allReaderNewsViewModel.MangaSelected(e.CurrentSelection.FirstOrDefault());
		c.SelectedItem = null;
	}
}
