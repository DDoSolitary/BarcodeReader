using System;
using System.Windows;

namespace BarcodeReader {
	public partial class App : Application {
		public static void ShowError(string content) {
			MessageBox.Show(content, "Error - BarcodeReader", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void Application_Startup(object sender, StartupEventArgs e) {
			AppDomain.CurrentDomain.UnhandledException += (s, args) => {
				ShowError($"Unexpected error:{Environment.NewLine}{args.ExceptionObject}");
			};
		}
	}
}
