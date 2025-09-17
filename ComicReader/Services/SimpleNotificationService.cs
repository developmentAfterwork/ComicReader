using Interpreter.Interface;
using Plugin.LocalNotification;

namespace ComicReader.Services
{
	public class SimpleNotificationService : INotification
	{
		public static string ChannelId => "comicreader_public_channel";

		public static string GroupName => "com.companyname.comicreader.Info";

		private int _id = 100;

		public static int ErrorId => 100000;

		public static int ProgressId => 200000;

		public async Task ShowNotification(string message)
		{
			await ShowNotification("ComicReader", message, "Empty", null);
		}

		public async Task ShowNotification(string title, string message)
		{
			await ShowNotification(title, message, "Empty", null);
		}

		public async Task ShowNotification(string message, DateTime? notifyTime)
		{
			await ShowNotification("ComicReader", message, "Empty", notifyTime);
		}

		public async Task ShowNotification(string title, string message, DateTime? notifyTime)
		{
			await ShowNotification(title, message, "Empty", notifyTime);
		}

		public async Task ShowNotification(string title, string message, string returningData, DateTime? notifyTime)
		{
			await CheckPermission();
			NotificationRequest notification = await BuildNotification(_id++, title, message, returningData, notifyTime, false);

			await LocalNotificationCenter.Current.Show(notification);
		}

		private async Task<NotificationRequest> BuildNotification(int id, string title, string message, string returningData, DateTime? notifyTime, bool asSummary)
		{
			var notification = new NotificationRequest {
				NotificationId = id,
				Title = title,
				Description = message,
				ReturningData = returningData, // Returning data when tapped on notification.
				Schedule = {
					NotifyTime = notifyTime // This is Used for Scheduling local notifications; if not specified, the notification will show immediately.
				},
				CategoryType = NotificationCategoryType.Service,
				Silent = true,
				Group = GroupName,
				Android = {
					IsGroupSummary = asSummary,
					ChannelId = ChannelId,
				},
				BadgeNumber = (await LocalNotificationCenter.Current.GetDeliveredNotificationList()).Count
			};
			return notification;
		}

		public async Task ShowNotificationSummary(string title, string message, string returningData, DateTime? notifyTime)
		{
			await CheckPermission();
			NotificationRequest notification = await BuildNotification(_id++, title, message, returningData, notifyTime, true);

			await LocalNotificationCenter.Current.Show(notification);
		}

		public async Task ShowError(string title, string message)
		{
			await CheckPermission();
			NotificationRequest notification = await BuildNotification(ErrorId, title, message, "Empty", null, false);

			await LocalNotificationCenter.Current.Show(notification);
		}

		private async Task CheckPermission()
		{
			if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false) {
				await LocalNotificationCenter.Current.RequestNotificationPermission();
			}
		}

		public async Task ShowProgress(string title, string message, int progress, int max)
		{
			await ShowProgress(SimpleNotificationService.ProgressId, title, message, progress, max);
		}

		public async Task ShowProgress(int id, string title, string message, int progress, int max)
		{
			var notification = new NotificationRequest {
				NotificationId = id,
				Title = title,
				Description = message,
				ReturningData = "", // Returning data when tapped on notification.
				Schedule = {
					NotifyTime = null // This is Used for Scheduling local notifications; if not specified, the notification will show immediately.
				},
				CategoryType = NotificationCategoryType.Service,
				Silent = true,
				Group = GroupName,
				Android = {
					IsGroupSummary = false,
					ChannelId = ChannelId,
					ProgressBar = new() {
						IsIndeterminate = false,
						Max = max,
						Progress = progress
					}
				},
				BadgeNumber = (await LocalNotificationCenter.Current.GetDeliveredNotificationList()).Count
			};

			await LocalNotificationCenter.Current.Show(notification);
		}

		internal void Close(int? id = null)
		{
			LocalNotificationCenter.Current.Clear(id ?? SimpleNotificationService.ProgressId);
		}
	}
}
