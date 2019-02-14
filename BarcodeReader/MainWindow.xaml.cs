using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NativeUtils;

namespace BarcodeReader {
	public partial class MainWindow : Window {
		internal static bool CaptureWndShown = false;
		private readonly MsgOnlyWindow _msgWindow = null;

		public MainWindow() {
			this.InitializeComponent();
			try {
				_msgWindow = new MsgOnlyWindow();
				_msgWindow.RegisterHotKey(0, KeyModifier.Control, 'Q');
				_msgWindow.OnHotKey += id => {
					if (id == 0) this.Dispatcher.Invoke(this.ShowCaptureWindow);
				};
				_msgWindow.OnClipboardUpdate += () => {
					this.Dispatcher.Invoke(() => {
						if (clipboardCheckBox.IsChecked == false) {
							this.ReadClipboard(false);
						}
					});
				};
			} catch (Exception e) {
				App.ShowError($"Initialization failed: {e.Message}");
				this.Close();
			}
		}

		private void Window_Closed(object sender, EventArgs e) {
			_msgWindow?.Close();
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
			StringCollection fileList;
			BitmapSource image;
			try {
				fileList = Clipboard.GetFileDropList();
				image = Clipboard.GetImage();
			} catch (Exception e) {
				if (showError) App.ShowError($"Couldn't read the clipboard: {e.Message}");
				return;
			}
			if (fileList != null && fileList.Count == 1) this.ReadFile(fileList[0], showError);
			else if (image != null) new ResultWindow(image, showError);
			else if (showError) App.ShowError("Unsupported clipboard content type.");
		}

		private void ReadFile(string fileName, bool showError) {
			BitmapImage image;
			try {
				image = new BitmapImage(new Uri(fileName));
			} catch (NotSupportedException e) {
				if (showError) App.ShowError($"Unsupported image format: {e.Message}");
				return;
			} catch (Exception e) {
				if (showError) App.ShowError($"Couldn't open the image file: {e.Message}");
				return;
			}
			new ResultWindow(image, showError);
		}
	}
}
