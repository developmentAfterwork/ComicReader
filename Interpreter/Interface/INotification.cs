
namespace Interpreter.Interface
{
	public interface INotification
	{
		Task ShowError(string title, string message);
	}
}
