using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Bat;
using UniversalEditor.Csv;
using UniversalEditor.Nsis;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class NsisFormat : IFormat
	{
		public string DisplayName { get { return "Nsis"; } }

		public string Extension { get { return ".nsi|.nsh"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new NsisEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 8; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is NsisEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
