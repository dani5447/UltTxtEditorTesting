using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Bat;
using UniversalEditor.Csv;
using UniversalEditor.Java;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class JavaFormat : IFormat
	{
		public string DisplayName { get { return "Java"; } }

		public string Extension { get { return ".java"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new JavaEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 7; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is JavaEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
