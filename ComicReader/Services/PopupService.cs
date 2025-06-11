using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace ComicReader.Services
{
	public class PopupService
	{
		public Task ShowPopupAsync(string title, string message, string cancel)
		{
			return Shell.Current.CurrentPage.DisplayAlert(title, message, cancel);
		}

		public Task<bool> ShowPopupAsync(string title, string message, string accept, string cancel)
		{
			return Shell.Current.CurrentPage.DisplayAlert(title, message, accept, cancel);
		}

		public Task<IPopupResult> ShowPopupAsync(Popup popup)
		{
			return Shell.Current.CurrentPage.ShowPopupAsync(popup);
		}
	}
}
