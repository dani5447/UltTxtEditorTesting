using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.PlainImage
{
	public class PlainImageEditor : EditorBase
    {
		private readonly Lazy<ImageViewerPanel> _image;

		public PlainImageEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_image = new Lazy<ImageViewerPanel>(CreateControl);
		}

		private ImageViewerPanel CreateControl()
		{
			return new ImageViewerPanel { DataContext = new ImageViewerViewModel(_file) };
		}

		public override void OnKeyDown(KeyEventArgs keyEventArgs)
		{
			_image.Value.viewer.OnKeyDownInner(keyEventArgs);
		}

		public override ICommand CopyCommand
		{
			get { return _image.Value.viewer.CopyCommand; }
		}

		public override ICommand PasteCommand
		{
			get { return _image.Value.viewer.PasteCommand; }
		}

		public override FrameworkElement EditorControl
		{
			get { return _image.Value; }
		}

		public override void SaveFile()
		{
			SaveContent(_file.FilePath);
		}

		public override void SaveFileAs(string filePath)
		{
			SaveContent(filePath);
			_file.SetFilePath(filePath);
		}

		public override void SaveCopyAs(string filePath)
		{
			SaveContent(filePath);
		}

		private void SaveContent(string filePath)
		{
			string extention = Path.GetExtension(filePath);

			ImageFormat format =
				string.IsNullOrEmpty(extention) ? null :
				extention.Equals(".Jpeg", StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Jpeg :
				extention.Equals(".Jpg", StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Jpeg :
				extention.Equals(".Bmp", StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Bmp :
				extention.Equals(".Png", StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Png :
				extention.Equals(".Gif", StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Gif : ImageFormat.Png;

			if (format == null)
				throw new ArgumentException("Here's an unknown file extension", "filePath");

			SaveImageTo(_image.Value.viewer.WriteableBitmap, filePath, format);
		}

		internal static void SaveImageTo(ImageSource bitmapImage, string outputPath, ImageFormat format)
		{
			if (bitmapImage == null)
				throw new ArgumentNullException("bitmapImage");
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentNullException("outputPath");

			int height = Convert.ToInt32(bitmapImage.Height);
			int width = Convert.ToInt32(bitmapImage.Width);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext context = drawingVisual.RenderOpen();
			context.DrawImage(bitmapImage, new Rect(0, 0, width, height));
			context.Close();

			// The BitmapSource that is rendered with a Visual.
			RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
			rtb.Render(drawingVisual);

			// Encoding the RenderBitmapTarget as a PNG file.
			BitmapEncoder tiff = null;
			if (format == ImageFormat.Jpeg)
				tiff = new JpegBitmapEncoder();

			if (format == ImageFormat.Bmp)
				tiff = new BmpBitmapEncoder();

			if (format == ImageFormat.Png)
				tiff = new PngBitmapEncoder();

			if (format == ImageFormat.Gif)
				tiff = new GifBitmapEncoder();

			if (tiff != null)
			{
				tiff.Frames.Add(BitmapFrame.Create(rtb));

				using (Stream stm = File.Create(outputPath))
					tiff.Save(stm);
			}
		}

		public override bool HasEditorCommands
		{
			get { return true; }
		}

		public override IList<Control> EditorCommands
		{
			get
			{
				return new Control[] { new MenuItem { Header = "Crop", Command = _image.Value.viewer.CropCommand } };
			}
		}
    }
}
