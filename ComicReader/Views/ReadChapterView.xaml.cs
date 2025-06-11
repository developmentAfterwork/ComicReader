using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class ReadChapterView : ContentPage
{
	private readonly ReadChapterViewModel readChapterViewModel;

	public ReadChapterView(ReadChapterViewModel readChapterViewModel)
	{
		InitializeComponent();
		BindingContext = readChapterViewModel;
		this.readChapterViewModel = readChapterViewModel;

		pagesCarusel.PositionSelected += PagesCarusel_PositionSelected;
	}

	private void PagesCarusel_PositionSelected(object? sender, CarouselView.Abstractions.PositionSelectedEventArgs e)
	{
		_ = readChapterViewModel.Scrolled(e.NewValue);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		readChapterViewModel.OnAppearing();
		pagesCarusel.PositionSelected += PagesCarusel_PositionSelected;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		readChapterViewModel.OnDisappearing();
		pagesCarusel.PositionSelected -= PagesCarusel_PositionSelected;
	}
}
