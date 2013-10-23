using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Utils.LoggerModule;
using WPFPdfViewer;

namespace UniversalEditor.Pdf
{
	public class PdfEditor : EditorBase
    {
	    private readonly Border _border;
		private readonly PdfViewer _viewer;

		public PdfEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_border = new Border();

			try
			{
				_viewer = new PdfViewer();
				_viewer.LoadFile(_file.FilePath);

				_border.Child = _viewer;
			}
			catch (Exception exception)
			{
				Exception ex = exception;

				while (ex != null)
				{
					if (ex is COMException)
					{
						TextBlock block = new TextBlock();
						block.Text =
							"To use this viewer you have to install Adobe Reader free software before." + Environment.NewLine +
							"See details on http://get.adobe.com/reader/";

						_border.Child = block;
						return;
					}

					ex = ex.InnerException;
				}

				Exception wrapper = new Exception("This content cannot be opened as PDF file.", exception);
				Logger.Log(wrapper);
			}
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
