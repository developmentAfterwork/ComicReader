using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.LocalNotification;

namespace ComicReader
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, TurnScreenOn = true, LaunchMode = LaunchMode.SingleInstance, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override async void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

#pragma warning disable CA1416
			if (!global::Android.OS.Environment.IsExternalStorageManager) {
				Platform.CurrentActivity?.StartActivityForResult(new Intent(global::Android.Provider.Settings.ActionManageAllFilesAccessPermission), 1);
			}

			if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false) {
				await LocalNotificationCenter.Current.RequestNotificationPermission();
			}
#pragma warning restore CA1416

			DeviceDisplay.Current.KeepScreenOn = true;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
#pragma warning disable CA1416
				var channel = new NotificationChannel("comic_channel", "Comic Updates", NotificationImportance.Low) {
					Description = "Benachrichtigungen für Comic-Update-Service"
				};

				var manager = (NotificationManager?)GetSystemService(NotificationService);
				manager?.CreateNotificationChannel(channel);
#pragma warning restore CA1416
			}
		}
	}
}
