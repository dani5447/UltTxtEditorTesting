using UniversalEditor.Audio;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;

namespace UniversalEditor.Formats
{
	internal class AudioFormat : IFormat
	{
		public string DisplayName { get { return "Audio"; } }

		public string Extension { get { return ".mp3|.wav"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new AudioEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 2; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is AudioEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
