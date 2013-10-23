using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.CSharp;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class CsharpFormat : IFormat
	{
		public string DisplayName { get { return "C#"; } }

		public string Extension { get { return ".cs"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new CsharpEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 4; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is CsharpEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
