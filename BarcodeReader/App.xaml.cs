using System;
using System.Windows;

namespace BarcodeReader {
	public partial class App : Application {
		public static void ShowError(string content) {
			MessageBox.Show(content, "Error - BarcodeReader", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private static void ShowUnexpectedError(object e) {
			ShowError($"Unexpected error:{Environment.NewLine}{e}");
		}

		private void Application_Startup(object sender, StartupEventArgs e) {
			AppDomain.CurrentDomain.UnhandledException += (s, args)
				=> ShowUnexpectedError(args.ExceptionObject);
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			ShowUnexpectedError(e.Exception);
		}
	}
}
