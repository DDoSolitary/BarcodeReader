using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BarcodeReader {
	public partial class ResultWindow : Window {
		public ResultWindow(BitmapSource image, bool showError) {
			this.InitializeComponent();
			var reader = new ZXing.Presentation.BarcodeReader {
				AutoRotate = true,
				Options = new ZXing.Common.DecodingOptions {
					TryHarder = true
				}
			};
			// To work around a clipboard bug in WPF
			// https://www.thomaslevesque.com/2009/02/05/wpf-paste-an-image-from-the-clipboard
			var fallbackImage = new FormatConvertedBitmap(image, PixelFormats.Bgr24, null, 0);
			var result = reader.Decode(image);
			if (result == null) {
				result = reader.Decode(fallbackImage);
				if (result == null) {
					if (showError) App.ShowError("No barcode found.");
					this.Close();
					return;
				}
				_img.Source = fallbackImage;
			} else _img.Source = image;
			_resultBox.Text = result.Text;
			this.Show();
			this.Activate();
		}
	}
}
