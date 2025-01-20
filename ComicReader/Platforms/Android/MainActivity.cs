using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Browser.CustomTabs;
using Plugin.LocalNotification;

namespace ComicReader
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, TurnScreenOn = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override async void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

#pragma warning disable CA1416
			if (!Android.OS.Environment.IsExternalStorageManager) {
				Platform.CurrentActivity?.StartActivityForResult(new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission), 1);
			}

			if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false) {
				await LocalNotificationCenter.Current.RequestNotificationPermission();
			}
#pragma warning restore CA1416

			DeviceDisplay.Current.KeepScreenOn = true;
		}
	}
}
