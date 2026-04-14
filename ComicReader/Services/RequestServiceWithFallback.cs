using ComicReader.Helper;
using Interpreter.Interface;
namespace ComicReader.Services
{
	public class RequestServiceWithFallback : IRequest
	{
		private readonly RequestHelper _request;
		private readonly WebViewRequest _webViewRequest;

		public RequestServiceWithFallback(RequestHelper request, WebViewRequest webViewRequest)
		{
			_request = request;
			_webViewRequest = webViewRequest;
		}

		public async Task<string> DoGetRequest(string url, int retries, bool withFallback, TimeSpan timeout, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (withFallback) {
				try {
					return await _request.DoGetRequest(url, retries, false, timeout, header, cancellationToken);
				} catch (Exception ex) when (ex is not OperationCanceledException) {
					return await _webViewRequest.GetHtmlAsync(url, timeout);
				}
			} else {
				return await _request.DoGetRequest(url, retries, false, timeout, header, cancellationToken);
			}
		}

		public Task DownloadFile(string url, string path, int repeatCount, TimeSpan timeout, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			return _request.DownloadFile(url, path, repeatCount, timeout, header, cancellationToken);
		}
	}
}
