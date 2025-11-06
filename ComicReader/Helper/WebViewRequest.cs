
public class WebViewRequest {
	private readonly WebView _webView;
	private readonly ContentPage _page;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly Button _done;
	private readonly Button _cancel;

	private TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
	private string RequestUrl = "";

	public WebViewRequest() {
		_webView = new WebView {
			WidthRequest = 400,
			HeightRequest = 400,
			IsVisible = true
		};

		_done = new Button {
			Text = "Done",
			Margin = new Thickness(8, 8, 8, 8),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		_cancel = new Button {
			Text = "Cancel",
			Margin = new Thickness(8, 8, 8, 8),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var stack = new StackLayout {
			Children = { _done, _cancel, _webView }
		};

		_page = new ContentPage {
			Content = stack,
			IsVisible = false,
			Opacity = 1.00
		};
	}

	public async Task<string> GetHtmlAsync(string url, TimeSpan timeout) {
		await _semaphore.WaitAsync();

		RequestUrl = url;

		tcs = new TaskCompletionSource<string>();

		await MainThread.InvokeOnMainThreadAsync(() => {
			_page.IsVisible = false;
			_webView.Source = "about:blank";
		});

		await MainThread.InvokeOnMainThreadAsync(async () => {
			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				await nav.PushModalAsync(_page);
			}
		});

		_done.Clicked += Done_Clicked;
		_cancel.Clicked += Cancel_Clicked;
		_webView.Navigated += OnNavigated;

		await MainThread.InvokeOnMainThreadAsync(() => {
			_webView.Source = url;
		});

		using var cts = new CancellationTokenSource(timeout);
		await using var reg = cts.Token.Register(async () => {
			await MainThread.InvokeOnMainThreadAsync(() => {
				_webView.Navigated -= OnNavigated;
				_page.IsVisible = true;
			});
		});

		return await tcs.Task;
	}

	private async void OnNavigated(object? sender, WebNavigatedEventArgs e) {
		if (e.Url.StartsWith(RequestUrl)) {
			var html = await _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
			var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");

			if (raw.Contains("Just a moment")) {
				_page.IsVisible = true;
				return;
			}

			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_webView.Navigated -= OnNavigated;

			_ = ReadResult();
		}
	}

	private async void Done_Clicked(object? sender, EventArgs e) {
		_done.Clicked -= Done_Clicked;
		_cancel.Clicked -= Cancel_Clicked;
		_webView.Navigated -= OnNavigated;

		await ReadResult();
	}

	private async Task ReadResult() {
		try {
			var html = await _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
			var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");

			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				var cur = nav.ModalStack.LastOrDefault();
				if (cur == _page)
					await nav.PopModalAsync();
			}

			tcs.TrySetResult(raw ?? "");
		} catch (Exception ex) {
			tcs.TrySetException(ex);
		} finally {
			try {
				_semaphore.Release();
			} catch { }
		}
	}

	private async void Cancel_Clicked(object? sender, EventArgs e) {
		try {
			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_webView.Navigated -= OnNavigated;

			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				await nav.PopModalAsync();
			}

			tcs.TrySetException(new Exception("Request failed"));
		} catch (Exception ex) {
			tcs.TrySetException(ex);
		} finally {
			_semaphore.Release();
		}
	}
}
