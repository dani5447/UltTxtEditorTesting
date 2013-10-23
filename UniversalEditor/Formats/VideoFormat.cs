using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Video;

namespace UniversalEditor.Formats
{
	internal class VideoFormat : IFormat
	{
		public string DisplayName { get { return "Video"; } }

		public string Extension { get { return ".avi|.mkv|.mp4|.ts|.mpg"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new VideoEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return 13; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is VideoEditor;
		}

		public bool IsReadonly
		{
			get { return true; }
		}
	}
}
