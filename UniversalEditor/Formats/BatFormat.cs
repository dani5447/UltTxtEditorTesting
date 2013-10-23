using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Bat;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class BatFormat : IFormat
	{
		public string DisplayName { get { return "Batch"; } }

		public string Extension { get { return ".bat|.cmd|.nt"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new BatEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 3; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is BatEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
