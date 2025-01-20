using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class UpdatesView : ContentPage
{
	public UpdatesView(UpdateViewModel updateViewModel)
	{
		InitializeComponent();
		BindingContext = updateViewModel;
	}
}
