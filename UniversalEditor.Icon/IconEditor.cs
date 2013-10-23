using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor.Icon
{
	public class IconEditor : EditorBase
    {
		private readonly Border _border;
		private readonly Image _image;

		public IconEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_border = new Border();

			// initializing
			_image = new Image();
			_image.Stretch = Stretch.None;

			try
			{
				_image.Source = new BitmapImage(new Uri(_file.FilePath));
			}
			catch (Exception exception)
			{
				Exception newOne = new Exception("This content cannot be opened as image.", exception);
				Logger.Log(newOne);
			}

			_border.Child = _image;
		}

		public override FrameworkElement EditorControl
		{
			get { return _border; }
		}

		public override void Focus()
		{
			base.Focus();
		}
    }
}
