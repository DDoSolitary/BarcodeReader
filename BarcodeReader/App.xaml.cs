using System;
using System.Windows;

namespace BarcodeReader {
	public partial class App : Application {
		private void Application_Exit(object sender, ExitEventArgs e) {
			Environment.Exit(0);
		}

		public static void ShowError(string content) {
			MessageBox.Show(content, "Error - BarcodeReader", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
