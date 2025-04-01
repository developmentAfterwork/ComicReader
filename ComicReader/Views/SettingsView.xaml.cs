using ComicReader.ViewModels;

namespace ComicReader.Views;

public partial class SettingsView : ContentPage
{
	private readonly SettingsViewModel settingsViewModel;

	public SettingsView(SettingsViewModel settingsViewModel)
	{
		InitializeComponent();
		BindingContext = settingsViewModel;
		this.settingsViewModel = settingsViewModel;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		settingsViewModel.OnDisappearing();
	}
}
