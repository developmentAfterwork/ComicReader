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
	}

	private void PagesCarusel_PositionSelected(object? sender, CarouselView.Abstractions.PositionSelectedEventArgs e)
	{
		_ = readChapterViewModel.Scrolled(e.NewValue);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		pagesCarusel.PositionSelected -= PagesCarusel_PositionSelected;
		pagesCarusel.PositionSelected += PagesCarusel_PositionSelected;

		readChapterViewModel.OnAppearing();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		pagesCarusel.PositionSelected -= PagesCarusel_PositionSelected;

		readChapterViewModel.OnDisappearing();
	}
}
