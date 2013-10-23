using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.Pdf;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class PdfFormat : IFormat
	{
		public string DisplayName { get { return "PDF"; } }

		public string Extension { get { return ".pdf"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new PdfEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 10; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is PdfEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
