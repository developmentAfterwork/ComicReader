using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class MangaDetailsView : ContentPage
{
	private readonly MangaDetailsViewModel mangaDetailsViewModel;

	public MangaDetailsView(MangaDetailsViewModel mangaDetailsViewModel)
	{
		InitializeComponent();
		BindingContext = mangaDetailsViewModel;
		this.mangaDetailsViewModel = mangaDetailsViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		mangaDetailsViewModel.OnAppearing();
	}
}
