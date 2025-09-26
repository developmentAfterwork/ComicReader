namespace ComicReader.Services
{
	public partial class BackgroundService
	{
		public partial void Start(string actionId, string displayText);

		public partial void Register(string actionId, Func<CancellationToken, Task> action);
	}
}
