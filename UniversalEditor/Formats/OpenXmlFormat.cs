using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.OpenXml;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class OpenXmlFormat : IFormat
	{
		public string DisplayName { get { return "Microsoft Word Document"; } }

		public string Extension { get { return ".docx"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new OpenXmlEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 9; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is OpenXmlEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
