
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
			IsVisible = true,
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

		tcs = new TaskCompletionSource<string>();

		await MainThread.InvokeOnMainThreadAsync(async () => {
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

		if (WebViewRequest.Settings.AutoCancelWebRequest) {
			using var ctsCancel = new CancellationTokenSource(timeout + timeout);
			await using var regCancel = ctsCancel.Token.Register(() => {
				MainThread.BeginInvokeOnMainThread(async () => {
					await Cancel();
				});
			});
		}

		return await tcs.Task;
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
		if (e.Url.StartsWith(RequestUrl)) {
			var html = await _webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
			var raw = System.Text.RegularExpressions.Regex.Unescape(html ?? "");

			if (raw.Contains("Just a moment")) {
				return;
			}

			_done.Clicked -= Done_Clicked;
			_cancel.Clicked -= Cancel_Clicked;
			_autoCancel.Clicked -= AutoCancel_Clicked;
			_webView.Navigated -= OnNavigated;
			_page.Disappearing -= OnDisappearing;

			_ = ReadResult();
		}
	}

	private async void Done_Clicked(object? sender, EventArgs e)
	{
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

			var nav = Application.Current?.MainPage?.Navigation;
			if (nav != null) {
				var cur = nav.ModalStack.LastOrDefault();
				if (cur == _page)
					await nav.PopModalAsync();
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
