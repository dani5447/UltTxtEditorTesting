using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.PlainImage;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class PlainImageFormat : IFormat
	{
		public string DisplayName { get { return "Plain image"; } }

		public string Extension { get { return ".bmp|.jpg|.jpeg|.png|.gif"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new PlainImageEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 11; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is PlainImageEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
