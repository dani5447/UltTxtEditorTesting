using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class CsvFormat : IFormat
	{
		public string DisplayName { get { return "Csv"; } }

		public string Extension { get { return ".csv"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new CsvEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 5; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is CsvEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
