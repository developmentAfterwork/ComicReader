using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicReader
{
	public class WindowCreator : IWindowCreator
	{
		public Window CreateWindow(Application app, IActivationState? activationState)
		{
			var window = new Window();
			if (!app.Windows.Any()) {
				window.Page = activationState?.Context.Services.GetRequiredService<AppShell>();
			}
			return window;
		}
	}
}
