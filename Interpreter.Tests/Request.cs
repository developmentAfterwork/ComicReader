using ComicReader.Helper;
using Interpreter.Interface;

namespace Interpreter.Tests
{
	internal class Request : IRequest
	{
		private RequestHelper helper = new RequestHelper();

		public Task<string> DoGetRequest(string url, int repeatCount, bool withFallback, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			return helper.DoGetRequest(url, repeatCount, false, header, cancellationToken);
		}

		public Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			return helper.DownloadFile(url, path, repeatCount, header, cancellationToken);
		}

		public Task<MemoryStream?> DoGetRequestStream(string url, Dictionary<string, string>? header = null)
		{
			return helper.DoGetRequestStream(url, header);
		}
	}
}
