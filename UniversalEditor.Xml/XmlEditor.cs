using TextEditor;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Xml
{
	public class XmlEditor : TextEditorBase
	{
		public XmlEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{ }

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, new XmlTextDocument(_file));
		}
	}
}
