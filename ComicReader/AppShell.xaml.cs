using ComicReader.Interpreter;
using ComicReader.Services;
using ComicReader.ViewModels;

namespace ComicReader
{
	public partial class AppShell : Shell
	{
		private readonly SettingsService settingsService;
		private readonly Factory factory;
		private readonly FileSaverService fileSaverService;

		public AppShell(SettingsService settingsService, Factory factory, FileSaverService fileSaverService)
		{
			InitializeComponent();

			Services.Navigation.Init();
			this.settingsService = settingsService;
			this.factory = factory;
			this.fileSaverService = fileSaverService;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var perm = new RuntimePermission();

			if (!perm.IsGranted()) {
				perm.Request();
			}

			_ = Task.Run(async () => {
				try {
					await SettingsViewModel.WriteBackup("backup.auto.json", settingsService, factory, fileSaverService);
				} catch { }
			});
		}
	}
}
