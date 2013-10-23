using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor.PlainImage
{
	public class ImageViewer : FrameworkElement
	{
		public static readonly DependencyProperty FileSourceProperty =
			DependencyProperty.Register("FileSource", typeof (FileWrapper), typeof (ImageViewer), new PropertyMetadata(null, OnFileSourceChanged));

		public FileWrapper FileSource
		{
			get { return (FileWrapper) GetValue(FileSourceProperty); }
			set { SetValue(FileSourceProperty, value); }
		}
		
		private WriteableBitmap _writeableBitmap;
		private bool _hasSelected;
		private Point _startSelection, _endSelection;

		public ImageViewer()
		{
			ContextMenu = new ContextMenu();
			ContextMenu.Items.Add(new MenuItem { Header = "Crop", Command = CropCommand });
		}

		private static void OnFileSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
				throw new ArgumentException("This value cannot be null");

			ImageViewer target = (ImageViewer) d;
			FileWrapper file = (FileWrapper) e.NewValue;

			WriteableBitmap bSource = string.IsNullOrEmpty(file.FileSource) ? CreateEmptyImage() : CreateImage(file.FileSource);
			bSource.Freeze();

			target._writeableBitmap = bSource;
			target.InvalidateVisual();
		}

		private static WriteableBitmap CreateImage(string fileSource)
		{
			try
			{
				BitmapImage result = new BitmapImage();

				using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fileSource)))
				{
					result.BeginInit();
					result.StreamSource = stream;
					result.EndInit();

					WriteableBitmap writeableBitmap = new WriteableBitmap(result);
					return writeableBitmap;
				}
			}
			catch (Exception exception)
			{
				Exception wrapper = new Exception("This content cannot be displayed as Image.", exception);
				Logger.Log(wrapper);

				return CreateEmptyImage();
			}
		}

		private static WriteableBitmap CreateEmptyImage()
		{
			PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);

			double dpiX = 96.0, dpiY = 96.0;
			if (source != null) 
			{
				dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
				dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
			}

			WriteableBitmap target = new WriteableBitmap(
				  Convert.ToInt32(SystemParameters.PrimaryScreenWidth),
				  Convert.ToInt32(SystemParameters.PrimaryScreenHeight),
				  dpiX, dpiY, PixelFormats.Pbgra32, null);

			return target;
		}

		public WriteableBitmap WriteableBitmap
		{
			get { return _writeableBitmap; }
		}

		public ICommand CropCommand
		{
			get { return new SimpleCommand(OnCrop); }
		}

		public ICommand CopyCommand
		{
			get { return new SimpleCommand(OnCopy); }
		}

		public ICommand PasteCommand
		{
			get { return new SimpleCommand(OnPaste); }
		}

		private void OnCopy()
		{
			Clipboard.SetImage(GetCroppedImage());	
		}

		private void OnPaste()
		{
			BitmapSource bitmap = Clipboard.GetImage();

			if (bitmap == null)
				return;

			WriteableBitmap target = new WriteableBitmap(
				  Math.Max(_writeableBitmap.PixelWidth, bitmap.PixelWidth),
				  Math.Max(_writeableBitmap.PixelHeight, bitmap.PixelHeight),
				  _writeableBitmap.DpiX, _writeableBitmap.DpiY,
				  _writeableBitmap.Format, null);

			CopyPixels(_writeableBitmap, target);
			CopyPixels(bitmap, target);
			target.Freeze();

			_writeableBitmap = target;
			InvalidateVisual();
		}

		protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
		{
			ContextMenu.Visibility = Visibility.Visible;

			base.OnMouseRightButtonUp(e);
		}

		private void CopyPixels(BitmapSource from, WriteableBitmap to)
		{
			int stride = from.PixelWidth * (from.Format.BitsPerPixel / 8);
			byte[] data = new byte[stride * from.PixelHeight];
			from.CopyPixels(data, stride, 0);

			to.WritePixels(new Int32Rect(0, 0, from.PixelWidth, from.PixelHeight), data, stride, 0);
		}

		private WriteableBitmap GetCroppedImage()
		{
			if (!_hasSelected)
				return _writeableBitmap;

			double dx = RenderSize.Width / _writeableBitmap.PixelWidth;
			double dy = RenderSize.Height / _writeableBitmap.PixelHeight;

			double height = dy < dx ? RenderSize.Height : _writeableBitmap.PixelHeight * dx;
			double width = dx < dy ? RenderSize.Width : _writeableBitmap.PixelWidth * dy;

			Rect boundsImage = new Rect((RenderSize.Width - width) / 2d, (RenderSize.Height - height) / 2d, width, height);
			Rect boundsCrop = new Rect(Math.Min(_startSelection.X, _endSelection.X) - boundsImage.X, Math.Min(_startSelection.Y, _endSelection.Y) - boundsImage.Y,
				Math.Abs(_startSelection.X - _endSelection.X), Math.Abs(_startSelection.Y - _endSelection.Y));

			double k = Math.Min(dx, dy);
			CroppedBitmap croppedBitmap = new CroppedBitmap(_writeableBitmap, new Int32Rect((int)(boundsCrop.X / k), (int)(boundsCrop.Y / k), (int)(boundsCrop.Width / k), (int)(boundsCrop.Height / k)));
			WriteableBitmap result = new WriteableBitmap(croppedBitmap);
			result.Freeze();

			return result;
		}

		private void OnCrop()
		{
			if (!_hasSelected)
				return;

			_writeableBitmap = GetCroppedImage();

			_hasSelected = false;
			InvalidateVisual();
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			_startSelection = _endSelection = e.GetPosition(this);
			_hasSelected = true;
			base.OnMouseLeftButtonDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				_endSelection = e.GetPosition(this);
				InvalidateVisual();
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_endSelection = e.GetPosition(this);
			InvalidateVisual();

			base.OnMouseLeftButtonUp(e);
		}

		public void OnKeyDownInner(KeyEventArgs e)
		{
			OnPreviewKeyDown(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			bool hasCtrl = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) > 0;
			bool hasAlt = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) > 0;
			bool hasShift = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) > 0;

			if (e.Key == Key.C && hasCtrl)
			{
				OnCopy();
				e.Handled = true;
			}
			else if (e.Key == Key.V && hasCtrl)
			{
				OnPaste();
				e.Handled = true;
			}
			else if (e.Key == Key.Escape)
			{
				_hasSelected = false;
				InvalidateVisual();

				e.Handled = true;
			}

			base.OnPreviewKeyDown(e);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (_writeableBitmap == null)
				return;
			
			double dx = RenderSize.Width / _writeableBitmap.PixelWidth;
			double dy = RenderSize.Height / _writeableBitmap.PixelHeight;

			double height = dy < dx ? RenderSize.Height : _writeableBitmap.PixelHeight * dx;
			double width = dx < dy ? RenderSize.Width : _writeableBitmap.PixelWidth * dy; 
			
			drawingContext.DrawImage(_writeableBitmap, new Rect((RenderSize.Width - width) / 2d, (RenderSize.Height - height) / 2d, width, height));

			if (_hasSelected)
			{
				drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(120, 120, 120, 120)), null, 
					new Rect(Math.Min(_startSelection.X, _endSelection.X), Math.Min(_startSelection.Y, _endSelection.Y), Math.Abs(_startSelection.X - _endSelection.X), Math.Abs(_startSelection.Y - _endSelection.Y)));
			}
		}
	}
}
