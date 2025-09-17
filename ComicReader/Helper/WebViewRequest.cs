public class WebViewRequest
{
	private readonly WebView _webView;
	private readonly ContentPage _page;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public WebViewRequest()
	{
		_webView = new WebView {
			WidthRequest = 1,
			HeightRequest = 1,
			IsVisible = false
		};

		_page = new ContentPage {
			Content = _webView,
			IsVisible = false,
			Opacity = 0.01
		};
	}

	public async Task<string> GetHtmlAsync(string url)
	{
		await _semaphore.WaitAsync();

		var tcs = new TaskCompletionSource<string>();

		await MainThread.InvokeOnMainThreadAsync(async () => {
			_webView.Source = "about:blank";
		});

		await MainThread.InvokeOnMainThreadAsync(async () => {
			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				await nav.PushModalAsync(_page);
			}
		});

		EventHandler<WebNavigatedEventArgs>? handler = null;
		handler = async (s, e) => {
			_webView.Navigated -= handler;

			try {
				if (e.Result == WebNavigationResult.Success) {
					var html = await _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
					var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");

					var nav = Application.Current?.MainPage?.Navigation;
					if (nav != null)
						await nav.PopModalAsync();

					tcs.TrySetResult(raw ?? "");
				} else {
					tcs.TrySetException(new Exception($"Navigation failed: {e.Result}"));
				}
			} catch (Exception ex) {
				tcs.TrySetException(ex);
			} finally {
				_semaphore.Release();
			}
		};

		_webView.Navigated += handler;

		await MainThread.InvokeOnMainThreadAsync(async () => {
			_webView.Source = url;
		});

		return await tcs.Task;
	}
}
