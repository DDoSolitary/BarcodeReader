using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NativeUtils;

namespace BarcodeReader {
	public partial class MainWindow : Window {
		internal static bool CaptureWndShown = false;

		public MainWindow() {
			this.InitializeComponent();
			Utils.RegisterHotKey(0, KeyModifier.Control, 'Q');
			Utils.OnHotKey += id => {
				if (id == 0) this.Dispatcher.Invoke(this.ShowCaptureWindow);
			};
			Utils.OnClipboardUpdate += () => {
				if (clipboardCheckBox.IsChecked == false) {
					this.Dispatcher.Invoke(() => this.ReadClipboard(false));
				}
			};
		}

		private void ScreenButton_Click(object sender, RoutedEventArgs e) {
			this.ShowCaptureWindow();
		}

		private void FileButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {
				CheckFileExists = true,
				ValidateNames = true,
				DereferenceLinks = true,
				Title = "Open file - BarcodeReader",
				Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.gif|All files|*"
			};
			if (dialog.ShowDialog() == true) this.ReadFile(dialog.FileName, true);
		}

		private void ClipboardButton_Click(object sender, RoutedEventArgs e) {
			this.ReadClipboard(true);
		}

		private void ShowCaptureWindow() {
			if (!CaptureWndShown) {
				CaptureWndShown = true;
				new CaptureWindow();
			}
		}

		private void ReadClipboard(bool showError) {
			var fileList = Clipboard.GetFileDropList();
			var image = Clipboard.GetImage();
			if (fileList != null && fileList.Count == 1) this.ReadFile(fileList[0], showError);
			else if (image != null) new ResultWindow(image, showError);
			else if (showError) App.ShowError("Unsupported clipboard content type.");
		}

		private void ReadFile(string fileName, bool showError) {
			BitmapImage image;
			try {
				image = new BitmapImage(new Uri(fileName));
			} catch (NotSupportedException) {
				if (showError) App.ShowError("Unsupported image format.");
				return;
			} catch (Exception e) {
				if (showError) App.ShowError($"Couldn't open the image file: {e.Message}");
				return;
			}
			new ResultWindow(image, showError);
		}
	}
}
