
namespace ComicReader.Platforms.Android.Services
{
	internal static class ServiceActionRegistry
	{
		private static readonly Dictionary<string, Func<CancellationToken, Task>> _actions = new();

		public static void Register(string id, Func<CancellationToken, Task> action)
		{
			lock (_actions) {
				_actions[id] = action;
			}
		}

		public static Func<CancellationToken, Task>? Get(string id)
		{
			lock (_actions) {
				return _actions.TryGetValue(id, out var act) ? act : null;
			}
		}

		public static void Remove(string id)
		{
			lock (_actions) {
				_actions.Remove(id);
			}
		}
	}
}
