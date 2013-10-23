using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Utils.LoggerModule;
using Border = System.Windows.Controls.Border;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace UniversalEditor.OpenXml
{
	public class OpenXmlEditor : EditorBase
    {
		private readonly Border _border;
		private readonly RichTextBox _textBox;

		public OpenXmlEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_border = new Border();

			_textBox = new RichTextBox();
			_textBox.Document = GetText(_file);
			_textBox.IsReadOnly = true;
			_textBox.AcceptsReturn = true;
			_textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			_textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

			_border.Child = _textBox;
		}

		public override FrameworkElement EditorControl
		{
			get { return _border; }
		}

		public override void Focus()
		{
			base.Focus();
		}

		private FlowDocument GetText(FileWrapper file)
		{
			try
			{
				FlowDocument builder = new FlowDocument();

				using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(file.FilePath, false))
				{
					MainDocumentPart mainPart = wordDoc.MainDocumentPart;

					foreach (OpenXmlElement child in mainPart.Document.Body)
					{
						if (child is Paragraph)
						{
							builder.Blocks.Add(new System.Windows.Documents.Paragraph(new Run(child.InnerText)));
						}
						else
						{

						}
					}
				}

				return builder;
			}
			catch (Exception exception)
			{
				Exception wrapper = new Exception("This content cannot be opened as Microsoft Word Document.", exception);
				Logger.Log(wrapper);

				return new FlowDocument();
			}
		}
    }
}
