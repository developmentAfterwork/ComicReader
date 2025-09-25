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

		public async Task<string> DoGetRequest(string url, int retries, bool withFallback, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (withFallback) {
				string html = string.Empty;

				try {
					html = await _request.DoGetRequest(url, retries, false, header, cancellationToken);
				} catch {
					html = await _webViewRequest.GetHtmlAsync(url);
				}

				return html;
			} else {
				return await _request.DoGetRequest(url, retries, false, header, cancellationToken);
			}
		}

		public Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null)
		{
			return _request.DownloadFile(url, path, repeatCount, header);
		}
	}
}
