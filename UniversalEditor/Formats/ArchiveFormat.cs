using UniversalEditor.Archive;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Formats
{
	internal class ArchiveFormat : IFormat
	{
		public string DisplayName { get { return "Archive"; } }

		public string Extension { get { return ".zip|.rar|.7z|.tar|.gz|.xz|.bz2|.iso|.exe"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new ArchiveEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 15; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is ArchiveEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
