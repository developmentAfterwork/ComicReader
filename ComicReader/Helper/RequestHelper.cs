using ComicReader.Services;

namespace ComicReader.Helper
{
	public class RequestHelper
	{
		private readonly FileSaverService fileSaverService = new FileSaverService();

		private HttpClient _httpClient = new HttpClient();

		public async Task<string> DoGetRequest(string url, int repeatCount)
		{
			for (int i = 0; i < repeatCount; i++) {
				try {
					using (var response = await _httpClient.GetAsync(url)) {
						var text = await response.Content.ReadAsStringAsync();

						if (String.IsNullOrEmpty(text)) {
							throw new Exception("Response is empty");
						}

						return text;
					}
				} catch {
					await Task.Delay(15000);
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

			throw new NotImplementedException();
		}
	}
}
