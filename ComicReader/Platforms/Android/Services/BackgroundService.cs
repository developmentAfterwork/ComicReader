using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using ComicReader.Platforms.Android.Services;

namespace ComicReader.Services
{
	[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
	public partial class BackgroundService : Service
	{
		private static readonly Dictionary<string, CancellationTokenSource> _runningJobs = new();

		public override IBinder? OnBind(Intent? intent) => null;

		public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
		{
			var actionId = intent?.GetStringExtra("actionId");
			if (string.IsNullOrEmpty(actionId)) {
				StopSelfResult(startId);
				return StartCommandResult.NotSticky;
			}

			var action = ServiceActionRegistry.Get(actionId);
			if (action == null) {
				StopSelfResult(startId);
				return StartCommandResult.NotSticky;
			}

			var serviceNotification = new NotificationCompat.Builder(this, "comic_channel")
				.SetContentTitle("ComicReader läuft")?
				.SetContentText("Hintergrundaufgaben aktiv …")?
				.SetOngoing(true)?
				.SetSmallIcon(Android.Resource.Drawable.IcMenuInfoDetails)?
				.Build();

			StartForeground(999999, serviceNotification);

			var displayText = intent?.GetStringExtra("displayText");
			var jobNotification = new NotificationCompat.Builder(this, "comic_channel")
				.SetContentTitle("ComicReader")?
				.SetContentText(displayText ?? "Task läuft …")?
				.SetOngoing(true)?
				.SetSmallIcon(Android.Resource.Drawable.IcMenuInfoDetails)?
				.Build();

			var notifId = actionId.GetHashCode();
			var mgr = NotificationManager.FromContext(this);
			mgr?.Notify(notifId, jobNotification);

			var cts = new CancellationTokenSource();
			lock (_runningJobs) {
				_runningJobs[actionId] = cts;
			}

			Task.Run(async () => {
				try {
					await action(cts.Token);
				} catch {

				} finally {
					ServiceActionRegistry.Remove(actionId);
					lock (_runningJobs) {
						_runningJobs.Remove(actionId);
					}

					var mgr = NotificationManager.FromContext(this);
					mgr?.Cancel(notifId);

					if (_runningJobs.Count == 0) {
						StopSelf();
					}
				}
			}, cts.Token);

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy()
		{
			lock (_runningJobs) {
				foreach (var cts in _runningJobs.Values) {
					cts.Cancel();
				}
				_runningJobs.Clear();
			}

			base.OnDestroy();
		}

		public partial void Start(string actionId, string displayText)
		{
			var context = Android.App.Application.Context;
			var intent = new Intent(context, typeof(BackgroundService));
			intent.PutExtra("actionId", actionId);
			intent.PutExtra("displayText", displayText);

			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
				context.StartForegroundService(intent);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
			} else {
				context.StartService(intent);
			}
		}

		public partial void Register(string actionId, Func<CancellationToken, Task> action)
		{
			ServiceActionRegistry.Register(actionId, action);
		}
	}
}
