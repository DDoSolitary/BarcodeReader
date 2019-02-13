using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BarcodeReader {
	public class SelectionInfo : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName]string propName = "") {
			PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		private double _selectTop;
		public double SelectTop {
			get => _selectTop;
			set { _selectTop = value; NotifyPropertyChanged(); }
		}

		private double _selectBottom;
		public double SelectBottom {
			get => _selectBottom;
			set { _selectBottom = value; NotifyPropertyChanged(); }
		}

		private double _selectLeft;
		public double SelectLeft {
			get => _selectLeft;
			set { _selectLeft = value; NotifyPropertyChanged(); }
		}

		private double _selectRight;
		public double SelectRight {
			get => _selectRight;
			set { _selectRight = value; NotifyPropertyChanged(); }
		}

		private double _canvasWidth;
		public double CanvasWidth {
			get => _canvasWidth;
			set { _canvasWidth = value; NotifyPropertyChanged(); }
		}

		private double _canvasHeight;
		public double CanvasHeight {
			get => _canvasHeight;
			set { _canvasHeight = value; NotifyPropertyChanged(); }
		}
	}

	public partial class CaptureWindow : Window {
		private bool _mouseDown = false;
		private System.Windows.Point _selectStart, _selectEnd;
		private readonly SelectionInfo _info;

		public CaptureWindow() {
			this.InitializeComponent();
			_info = this.DataContext as SelectionInfo;
			try {
				_bgBrush.ImageSource = this.GetScreenshot();
			} catch (Exception e) {
				App.ShowError($"Couldn't capture the screen: {e.Message}");
				this.Close();
				return;
			}
			this.Show();
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			_info.CanvasWidth = _captureCanvas.ActualWidth;
			_info.CanvasHeight = _captureCanvas.ActualHeight;
			this.Opacity = 1;
			this.Activate();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				this.Close();
			} else if (e.Key == Key.Enter) {
				var rect = new Int32Rect((int)_info.SelectLeft, (int)_info.SelectTop, (int)(_info.SelectRight - _info.SelectLeft), (int)(_info.SelectBottom - _info.SelectTop));
				new ResultWindow(new CroppedBitmap(_bgBrush.ImageSource as BitmapSource, rect), true);
				this.Close();
			}
		}
		
		private void Window_Closed(object sender, EventArgs e) {
			MainWindow.CaptureWndShown = false;
			GC.Collect();
		}

		private void CaptureCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
			_mouseDown = true;
			_selectStart = e.GetPosition(_captureCanvas);
		}

		private void CaptureCanvas_MouseMove(object sender, MouseEventArgs e) {
			if (_mouseDown) {
				_selectEnd = e.GetPosition(_captureCanvas);
				_info.SelectTop = Math.Min(_selectStart.Y, _selectEnd.Y);
				_info.SelectBottom = Math.Max(_selectStart.Y, _selectEnd.Y);
				_info.SelectLeft = Math.Min(_selectStart.X, _selectEnd.X);
				_info.SelectRight = Math.Max(_selectStart.X, _selectEnd.X);
			}
		}

		private void CaptureCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			_mouseDown = false;
		}

		private BitmapImage GetScreenshot() {
			var x = (int)SystemParameters.VirtualScreenLeft;
			var y = (int)SystemParameters.VirtualScreenTop;
			var w = (int)SystemParameters.PrimaryScreenWidth;
			var h = (int)SystemParameters.PrimaryScreenHeight;
			using (var bmp = new Bitmap(w, h)) {
				using (var g = Graphics.FromImage(bmp)) {
					g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(w, h));
				}
				var image = new BitmapImage();
				using (var ms = new MemoryStream()) {
					bmp.Save(ms, ImageFormat.Bmp);
					image.BeginInit();
					image.StreamSource = ms;
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.EndInit();
				}
				return image;
			}
		}
	}
}
