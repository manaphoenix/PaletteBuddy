using System.Windows;
using System.Windows.Threading;

namespace PaletteBuddy
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			// Process unhandled exception
			MessageBox.Show(e.Exception.ToString());
			// Prevent default unhandled exception processing
			e.Handled = true;
		}
	}
}