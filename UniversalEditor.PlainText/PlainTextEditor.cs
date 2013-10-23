using System;
using TextEditor;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.PlainText
{
    public class PlainTextEditor : TextEditorBase
    {
		public PlainTextEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{}

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, new UfeTextDocument(_file, null));
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}
    }
}
