using System;
using TextEditor;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Nsis
{
	public class NsisEditor : TextEditorBase
	{
		public NsisEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{ }

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, new NsisTextDocument(_file));
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}
	}
}
