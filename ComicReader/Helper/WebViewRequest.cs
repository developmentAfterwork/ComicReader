
using System.Threading.Tasks;

public class WebViewRequest
{
	private static class Settings
	{
		public static bool AutoCancelWebRequest = false;
	}

	private readonly WebView _webView;
	private readonly ContentPage _page;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly Button _done;
	private readonly Button _cancel;
	private readonly Button _autoCancel;

	private TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
	private string RequestUrl = "";
	private int _isProcessing = 0;

	public WebViewRequest()
	{
		_webView = new WebView {
			WidthRequest = 400,
			HeightRequest = 400,
			IsVisible = true,
			BackgroundColor = Colors.Red
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

		_autoCancel = new Button {
			Text = "Auto cancel",
			Margin = new Thickness(8, 8, 8, 8),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var hStack = new HorizontalStackLayout {
			Children = { _cancel, _autoCancel },
			HeightRequest = 100
		};

		var stack = new StackLayout {
			Children = { _done, hStack, _webView }
		};

		_page = new ContentPage {
			Content = stack,
			IsVisible = false,
			Opacity = 1.00
		};
	}

	public async Task<string> GetHtmlAsync(string url, TimeSpan timeout)
	{
		await _semaphore.WaitAsync();

		_done.Clicked -= Done_Clicked;
		_cancel.Clicked -= Cancel_Clicked;
		_autoCancel.Clicked -= AutoCancel_Clicked;
		_webView.Navigated -= OnNavigated;
		_page.Disappearing -= OnDisappearing;

		RequestUrl = url;
		Interlocked.Exchange(ref _isProcessing, 0);

		tcs = new TaskCompletionSource<string>();

		await MainThread.InvokeOnMainThreadAsync(async () => {
			_page.IsVisible = false;

			_webView.Source = "about:blank";

			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				await nav.PushModalAsync(_page);
			}
		});

		_done.Clicked += Done_Clicked;
		_cancel.Clicked += Cancel_Clicked;
		_autoCancel.Clicked += AutoCancel_Clicked;
		_webView.Navigated += OnNavigated;
		_page.Disappearing += OnDisappearing;

		await MainThread.InvokeOnMainThreadAsync(() => {
			_webView.Source = url;
		});

		using var cts = new CancellationTokenSource(timeout);
		await using var reg = cts.Token.Register(async () => {
			// Timeout: Page anzeigen, OnNavigated NICHT abmelden –
			// so erkennt die App automatisch wenn die Challenge gelöst ist
			await MainThread.InvokeOnMainThreadAsync(() => {
				_page.IsVisible = true;
			});
		});

		// Bug 1 fix: Variablen außerhalb des if-Blocks, sonst werden sie beim Block-Ende sofort disposed
		CancellationTokenSource? ctsCancel = null;
		CancellationTokenRegistration regCancel = default;

		if (WebViewRequest.Settings.AutoCancelWebRequest) {
			ctsCancel = new CancellationTokenSource(timeout + timeout);
			regCancel = ctsCancel.Token.Register(async () => {
				await MainThread.InvokeOnMainThreadAsync(async () => {
					await Cancel();
				});
			});
		}

		try {
			return await tcs.Task;
		} finally {
			await regCancel.DisposeAsync();
			ctsCancel?.Dispose();
		}
	}

	private void OnDisappearing(object? sender, EventArgs e)
	{
		try {
			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_autoCancel.Clicked -= AutoCancel_Clicked;
			_webView.Navigated -= OnNavigated;
			_page.Disappearing -= OnDisappearing;

			tcs.TrySetException(new Exception("Request failed"));
		} catch (Exception ex) {
			tcs.TrySetException(ex);
		} finally {
			try {
				_semaphore.Release();
			} catch { }
		}
	}

	private async void OnNavigated(object? sender, WebNavigatedEventArgs e)
	{
		if (!e.Url.StartsWith(RequestUrl))
			return;

		// Concurrent-Guard: verhindert doppelte Verarbeitung bei Cloudflare-Redirects
		if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
			return;

		try {
			var jsTask = _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
			if (await Task.WhenAny(jsTask, Task.Delay(5000)) != jsTask) {
				Interlocked.Exchange(ref _isProcessing, 0);
				return;
			}
			var html = await jsTask;
			var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");

			if (raw.Contains("Just a moment")) {
				_page.IsVisible = true;
				Interlocked.Exchange(ref _isProcessing, 0);
				return;
			}

			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_autoCancel.Clicked -= AutoCancel_Clicked;
			_webView.Navigated -= OnNavigated;
			_page.Disappearing -= OnDisappearing;

			_ = ReadResult();
		} catch {
			Interlocked.Exchange(ref _isProcessing, 0);
		}
	}

	private async void Done_Clicked(object? sender, EventArgs e)
	{
		// Prüfen ob die Challenge noch aktiv ist, bevor wir das Ergebnis lesen
		var jsTask = _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
		if (await Task.WhenAny(jsTask, Task.Delay(5000)) != jsTask)
			return;
		var html = await jsTask;
		var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");
		if (raw.Contains("Just a moment"))
			return;

		_done.Clicked -= Done_Clicked;
		_cancel.Clicked -= Cancel_Clicked;
		_autoCancel.Clicked -= AutoCancel_Clicked;
		_webView.Navigated -= OnNavigated;
		_page.Disappearing -= OnDisappearing;

		await ReadResult();
	}

	private async void AutoCancel_Clicked(object? sender, EventArgs e)
	{
		WebViewRequest.Settings.AutoCancelWebRequest = true;
		await Cancel();
	}

	private async Task ReadResult()
	{
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

	private async void Cancel_Clicked(object? sender, EventArgs e)
	{
		await Cancel();
	}

	private async Task Cancel()
	{
		try {
			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_autoCancel.Clicked -= AutoCancel_Clicked;
			_webView.Navigated -= OnNavigated;
			_page.Disappearing -= OnDisappearing;

			// Bug 3 fix: Solange poppen, bis _page weg ist – nicht nur wenn es oben liegt
			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				while (nav.ModalStack.Contains(_page)) {
					await nav.PopModalAsync();
				}
			}

			tcs.TrySetException(new Exception("Request failed"));
		} catch (Exception ex) {
			tcs.TrySetException(ex);
		} finally {
			try {
				_semaphore.Release();
			} catch { }
		}
	}
}
