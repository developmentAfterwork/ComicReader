using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class ReaderNewsView : ContentPage
{
	private readonly ReaderNewsViewModel viewModel;

	public ReaderNewsView(ReaderNewsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		this.viewModel = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.OnAppearing();
	}
}
