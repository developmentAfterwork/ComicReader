namespace Interpreter.Interface
{
	public interface IRequest
	{
		Task<string> DoGetRequest(string url, int repeatCount, bool withFallback, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null);

		Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null);
	}
}
