using System;
using System.Windows.Media;
using TextEditor;
using TextEditor.ScriptHighlight;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Java
{
	public class JavaEditor : TextEditorBase
	{
		public JavaEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{ }

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, CreateDocument(_file));
		}

		private static ScriptHighlightDocument CreateDocument(FileWrapper file)
		{
			ScriptHighlightInfo highlightInfo = new ScriptHighlightInfo();
			highlightInfo.CommentLine = "//";
			highlightInfo.Add(Brushes.Blue, "instanceof assert if else switch case default break goto return for while do continue new throw throws try catch finally this super extends implements import true false null");
			highlightInfo.Add(Brushes.Violet, "package transient strictfp void char short int long double float const static volatile byte boolean class interface native private protected public final abstract synchronized enum");
		
			ScriptHighlightDocument result = new ScriptHighlightDocument(file, highlightInfo);
			return result;
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}
	}
}
