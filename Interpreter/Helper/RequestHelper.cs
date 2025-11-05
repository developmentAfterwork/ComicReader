using ComicReader.Services;
using Interpreter.Interface;
using System.Threading;

namespace ComicReader.Helper
{
	public class RequestHelper : IRequest
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		private HttpClient _httpClient = new HttpClient();

		public RequestHelper()
		{
			_httpClient.Timeout = TimeSpan.FromSeconds(30);
			_httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ComicReader", "1.0.0"));
		}

		public async Task<string> DoGetRequest(string url, int repeatCount, bool withFallback, TimeSpan timeout, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (withFallback)
				throw new NotSupportedException("Fallback handling not implemented");

			using var timeoutCts = new CancellationTokenSource(timeout);
			using var linkedCts = cancellationToken is not null
				? CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken.Value)
				: timeoutCts;

			for (int i = 0; i < repeatCount; i++) {
				try {
					using var request = new HttpRequestMessage(HttpMethod.Get, url);
					if (header != null) {
						foreach (var pair in header)
							request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
					}

					using var response = await _httpClient.SendAsync(request, linkedCts.Token);
					response.EnsureSuccessStatusCode();

					var text = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
					if (string.IsNullOrEmpty(text))
						throw new HttpRequestException("Response is empty");

					return text;
				} catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
					throw new TimeoutException($"Request to {url} timed out after {timeout.TotalSeconds:F1} seconds.");
				} catch (Exception) {
					await Task.Delay(500, linkedCts.Token);
				}
			}

			throw new HttpRequestException($"Request to {url} failed after {repeatCount} retries.");
		}

		public async Task DownloadFile(string url, string path, int repeatCount, TimeSpan timeout, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (url == "") { return; }

			using var timeoutCts = new CancellationTokenSource(timeout);
			using var linkedCts = cancellationToken is not null
				? CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken.Value)
				: timeoutCts;

			for (int i = 0; i < repeatCount; i++) {
				try {
					using var request = new HttpRequestMessage(HttpMethod.Get, url);
					if (header != null)
						foreach (var pair in header)
							request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);

					_httpClient.Timeout = timeout;
					using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
					response.EnsureSuccessStatusCode();

					await using var stream = await response.Content.ReadAsStreamAsync(linkedCts.Token);
					var content = await stream.ToArrayAsync(linkedCts.Token);
					await fileSaverService.SaveFile(path, content);

					return;
				} catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
					throw new TimeoutException($"Request to {url} timed out after {timeout.TotalSeconds:F1} seconds.");
				} catch {
					await Task.Delay(500, linkedCts.Token);
				}
			}
		}

		public async Task<MemoryStream?> DoGetRequestStream(string url, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (url == "") {
				return await Task.FromResult<MemoryStream?>(null);
			}

			for (int i = 0; i < 3; i++) {
				try {
					using var request = new HttpRequestMessage(HttpMethod.Get, url);
					if (header != null)
						foreach (var pair in header)
							request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);

					using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
					response.EnsureSuccessStatusCode();

					await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken ?? CancellationToken.None);
					var memoryStream = new MemoryStream();
					await stream.CopyToAsync(memoryStream, cancellationToken ?? CancellationToken.None);
					memoryStream.Position = 0;

					return memoryStream;
				} catch {
					await Task.Delay(500);
				}
			}

			return await Task.FromResult<MemoryStream?>(null);
		}
	}
}
