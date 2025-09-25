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

		public async Task<string> DoGetRequest(string url, int repeatCount, bool withFallback, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (withFallback)
				throw new NotSupportedException("Fallback handling not implemented");

			for (int i = 0; i < repeatCount; i++) {
				try {
					using var request = new HttpRequestMessage(HttpMethod.Get, url);
					if (header != null) {
						foreach (var pair in header)
							request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
					}

					using var response = await _httpClient.SendAsync(request, cancellationToken ?? CancellationToken.None);
					response.EnsureSuccessStatusCode();

					var text = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
					if (string.IsNullOrEmpty(text))
						throw new HttpRequestException("Response is empty");

					return text;
				} catch (Exception ex) when (i < repeatCount - 1) {
					await Task.Delay(500, cancellationToken ?? CancellationToken.None);
				}
			}

			throw new HttpRequestException($"Request to {url} failed after {repeatCount} retries.");
		}

		public async Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null, CancellationToken? cancellationToken = null)
		{
			if (url == "") { return; }

			for (int i = 0; i < repeatCount; i++) {
				try {
					using var request = new HttpRequestMessage(HttpMethod.Get, url);
					if (header != null)
						foreach (var pair in header)
							request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);

					using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
					response.EnsureSuccessStatusCode();

					await using var stream = await response.Content.ReadAsStreamAsync();
					var content = await stream.ToArrayAsync(cancellationToken ?? CancellationToken.None);
					await fileSaverService.SaveFile(path, content);
				} catch {
					await Task.Delay(500);
				}
			}
		}

		public async Task<MemoryStream?> DoGetRequestStream(string url, Dictionary<string, string>? header = null)
		{
			if (url == "") {
				return await Task.FromResult<MemoryStream?>(null);
			}

			for (int i = 0; i < 3; i++) {
				try {

					if (header is not null) {
						_httpClient.DefaultRequestHeaders.Clear();
						foreach (var pair in header) {
							_httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
						}
					}

					CancellationToken token = new CancellationToken();
					using (var stream = await _httpClient.GetStreamAsync(url, token).ConfigureAwait(false)) {
						var memorySteam = new MemoryStream();
						stream.CopyTo(memorySteam);
						memorySteam.Position = 0;

						return memorySteam;
					}
				} catch {
					await Task.Delay(500);
				}
			}

			return await Task.FromResult<MemoryStream?>(null);
		}
	}
}
