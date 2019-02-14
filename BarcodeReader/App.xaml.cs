using System.Windows;

namespace BarcodeReader {
	public partial class App : Application {
		public static void ShowError(string content) {
			MessageBox.Show(content, "Error - BarcodeReader", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
