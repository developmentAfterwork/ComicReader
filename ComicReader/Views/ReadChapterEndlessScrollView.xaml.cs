using CarouselView.Abstractions;
using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class ReadChapterEndlessScrollView : ContentPage
{
	private readonly ReadChapterEndlessScrollViewModel readChapterEndlessScrollViewModel;

	public ReadChapterEndlessScrollView(ReadChapterEndlessScrollViewModel readChapterEndlessScrollViewModel)
	{
		InitializeComponent();
		BindingContext = readChapterEndlessScrollViewModel;
		this.readChapterEndlessScrollViewModel = readChapterEndlessScrollViewModel;
	}

	private void PagesCarusel_PositionSelected(object? sender, CarouselView.Abstractions.PositionSelectedEventArgs e)
	{
		_ = readChapterEndlessScrollViewModel.Scrolled(e.NewValue);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		readChapterEndlessScrollViewModel.PageIndexChanged -= ReadChapterEndlessScrollViewModel_PageIndexChanged;
		readChapterEndlessScrollViewModel.PageIndexChanged += ReadChapterEndlessScrollViewModel_PageIndexChanged;

		readChapterEndlessScrollViewModel.OnAppearing();

		CollView.Scrolled -= CollView_Scrolled;
		CollView.Scrolled += CollView_Scrolled;
	}

	private void CollView_Scrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		if (Math.Abs(e.LastVisibleItemIndex - e.FirstVisibleItemIndex) <= 2)
			_ = readChapterEndlessScrollViewModel.Scrolled(e.LastVisibleItemIndex);
	}

	private void ReadChapterEndlessScrollViewModel_PageIndexChanged(object? sender, (int index, int targetAmount) e)
	{
		_ = Task.Run(async () => {
			while (CollView.ItemsSource.GetCount() != e.targetAmount) {
				await Task.Delay(500);
			}

			MainThread.BeginInvokeOnMainThread(() => {
				CollView.ScrollTo(e.index, position: ScrollToPosition.Start, animate: false);
			});
		});
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		readChapterEndlessScrollViewModel.PageIndexChanged -= ReadChapterEndlessScrollViewModel_PageIndexChanged;

		readChapterEndlessScrollViewModel.OnDisappearing();
	}
}
