namespace Interpreter.Interface
{
	public interface IRequest
	{
		Task<string> DoGetRequest(string url, int retries, bool withFallback, Dictionary<string, string>? headers = null);

		Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null);
	}
}
