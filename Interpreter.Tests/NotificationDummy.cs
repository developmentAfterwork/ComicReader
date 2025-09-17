using Interpreter.Interface;

namespace Interpreter.Tests
{
	internal class NotificationDummy : INotification
	{
		public Task ShowError(string title, string message)
		{
			return Task.CompletedTask;
		}
	}
}
