using ComicReader.Services;
using Interpreter.Interface;
using System.Threading;

namespace ComicReader.Helper
{
	public class RequestHelper : IRequest
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";
		private readonly FileSaverService fileSaverService = new FileSaverService();

		private HttpClient _httpClient;

		public RequestHelper(TimeSpan timeout)
		{
#if ANDROID
			_httpClient = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler());
#else
			_httpClient = new HttpClient();
#endif
			_httpClient.Timeout = timeout;
			//_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
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
					if (!response.IsSuccessStatusCode) {
						var body = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
						throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.StatusCode} — {body}", null, response.StatusCode);
					}

					var text = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
					if (string.IsNullOrEmpty(text))
						throw new HttpRequestException("Response is empty");

					if (text.Contains("Just a moment") || text.Contains("cf-browser-verification"))
						throw new HttpRequestException("Cloudflare challenge detected");

					return text;
				} catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
					throw new TimeoutException($"Request to {url} timed out after {timeout.TotalSeconds:F1} seconds.");
				} catch (HttpRequestException ex) when (ex.StatusCode.HasValue && (int)ex.StatusCode.Value < 500) {
					throw; // 4xx nicht wiederholen – der Server lehnt die Anfrage grundsätzlich ab
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
