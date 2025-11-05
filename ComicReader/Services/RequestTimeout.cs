using Interpreter.Interface;

namespace ComicReader.Services
{
	public class RequestTimeout : IRequestTimeout
	{
		private readonly SettingsService settingsService;

		public RequestTimeout(SettingsService settingsService)
		{
			this.settingsService = settingsService;
		}

		public TimeSpan Timeout => settingsService.GetRequestTimeout();
	}
}
