using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class TestView : ContentPage
{
	private readonly TestViewModel testViewModel;

	public TestView(TestViewModel testViewModel)
	{
		InitializeComponent();
		BindingContext = testViewModel;
		this.testViewModel = testViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		testViewModel.OnAppearing();
	}
}
