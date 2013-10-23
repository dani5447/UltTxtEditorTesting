using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Formats
{
	public interface IFormat
	{
		string DisplayName { get; }
		string Extension { get; }
		EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file);
		byte EditorTypeId { get; }
		bool IsChild(EditorBase editor);
		bool IsReadonly { get; }
	}
}
