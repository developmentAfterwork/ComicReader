using ComicReader.Services;
using Interpreter.Interface;
using System.Threading;

namespace ComicReader.Helper
{
	public class RequestHelper : IRequest
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		private HttpClient _httpClient = new HttpClient();

		public RequestHelper(TimeSpan timeout)
		{
			_httpClient.Timeout = timeout;
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36");
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
			if (!path.StartsWith("/")) {
				throw new Exception("Invalid path");
			}

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

					using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
					response.EnsureSuccessStatusCode();

					await using var stream = await response.Content.ReadAsStreamAsync(linkedCts.Token);
					await using var outStream = fileSaverService.OpenWrite(path);
					await stream.CopyToAsync(outStream, 81920, linkedCts.Token);

					if (!fileSaverService.IsSizeGreaterZero(path)) {
						fileSaverService.DeleteFile(path);

						throw new Exception("Downloaded file size is zero");
					}

					return;
				} catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
					throw new TimeoutException($"Request to {url} timed out after {timeout.TotalSeconds:F1} seconds.");
				} catch (HttpRequestException) {
					throw;
				} catch (Exception) {
					await Task.Delay(500, linkedCts.Token);
				}
			}

			throw new Exception("Download failed");
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
