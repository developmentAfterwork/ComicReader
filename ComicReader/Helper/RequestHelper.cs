using ComicReader.Services;
using SkiaSharp;

namespace ComicReader.Helper
{
	public class RequestHelper
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		private HttpClient _httpClient = new HttpClient();

		public async Task<string> DoGetRequest(string url, int repeatCount, Dictionary<string, string>? header = null)
		{
			for (int i = 0; i < repeatCount; i++) {
				try {
					if (header is not null) {
						_httpClient.DefaultRequestHeaders.Clear();
						foreach (var pair in header) {
							_httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
						}
					}

					using (var response = await _httpClient.GetAsync(url)) {
						var text = await response.Content.ReadAsStringAsync();

						if (String.IsNullOrEmpty(text)) {
							throw new Exception("Response is empty");
						}

						return text;
					}
				} catch {
					await Task.Delay(500);
				}
			}

			throw new NotImplementedException();
		}

		public async Task DownloadFile(string url, string path, int repeatCount, Dictionary<string, string>? header = null)
		{
			if (url == "") { return; }

			for (int i = 0; i < repeatCount; i++) {
				try {

					if (header is not null) {
						_httpClient.DefaultRequestHeaders.Clear();
						foreach (var pair in header) {
							_httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
						}
					}

					using (var stream = await _httpClient.GetStreamAsync(url)) {
						var content = await stream.ToArrayAsync(CancellationToken.None);
						await fileSaverService.SaveFile(path, content);
						return;
					}
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
