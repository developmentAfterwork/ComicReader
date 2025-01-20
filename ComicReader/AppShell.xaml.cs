using ComicReader.Services;

namespace ComicReader
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			Services.Navigation.Init();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var perm = new RuntimePermission();

			if (!perm.IsGranted()) {
				perm.Request();
			}
		}
	}
}
