using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.Icon;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class IconFormat : IFormat
	{
		public string DisplayName { get { return "Icon"; } }

		public string Extension { get { return ".ico"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new IconEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 6; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is IconEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
