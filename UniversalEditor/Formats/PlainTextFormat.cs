using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class PlainTextFormat : IFormat
	{
		public string DisplayName { get { return "Plain text"; } }

		public string Extension { get { return ".txt"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new PlainTextEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 12; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is PlainTextEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
