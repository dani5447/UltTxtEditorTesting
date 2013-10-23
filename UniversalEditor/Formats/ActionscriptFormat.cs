using UniversalEditor.Actionscript;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class ActionscriptFormat : IFormat
	{
		public string DisplayName { get { return "Actionscript"; } }

		public string Extension { get { return ".as|.mx"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new ActionscriptEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 1; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is ActionscriptEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
