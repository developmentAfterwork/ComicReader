using MauiPermissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace ComicReader.Services
{
	public class RuntimePermission
	{
		public bool IsGranted()
		{
			var overallPermissionGranted = false;

			var overallPermissionCheck = Task.Run(async () => {

				var storageRead = await MauiPermissions.CheckStatusAsync<MauiPermissions.StorageRead>();
				var storageWrite = await MauiPermissions.CheckStatusAsync<MauiPermissions.StorageWrite>();

				if (storageRead != PermissionStatus.Granted ||
					storageWrite != PermissionStatus.Granted) {

					overallPermissionGranted = false;
				} else {
					overallPermissionGranted = true;
				}
			});

			overallPermissionCheck.Wait();

			return overallPermissionGranted;
		}

		public void Request()
		{
			MainThread.BeginInvokeOnMainThread(async () => {
				await MauiPermissions.RequestAsync<MauiPermissions.StorageRead>();
				await MauiPermissions.RequestAsync<MauiPermissions.StorageWrite>();
			});
		}
	}
}
