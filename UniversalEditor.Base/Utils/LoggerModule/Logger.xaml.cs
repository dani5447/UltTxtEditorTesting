using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalEditor.Base.Utils.LoggerModule
{
	/// <summary>
	/// Interaction logic for Logger.xaml
	/// </summary>
	public partial class Logger
	{
		private Logger(Exception exception)
		{
			InitializeComponent();

			DataContext = new LoggerViewModel(exception);
			image.Source = SystemIcons.Error.ToImageSource();
		}

		public static void Log(Exception exception)
		{
			if (exception == null) 
				throw new ArgumentNullException("exception");

			Application.Current.Dispatcher.Invoke(() =>
			{
				Logger logger = new Logger(exception);
				logger.Owner = Application.Current.MainWindow;
				logger.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				logger.ShowDialog();
			});
		}
	}

	public static class IconUtilities
	{
		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);

		public static ImageSource ToImageSource(this Icon icon)
		{
			Bitmap bitmap = icon.ToBitmap();
			IntPtr hBitmap = bitmap.GetHbitmap();

			ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
				hBitmap,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			if (!DeleteObject(hBitmap))
			{
				throw new Win32Exception();
			}

			return wpfBitmap;
		}
	}
}
