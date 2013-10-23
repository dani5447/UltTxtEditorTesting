using System;
using System.Collections.Generic;
using System.Windows.Media;
using TextEditor;
using TextEditor.ScriptHighlight;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.CSharp
{
	public class CsharpEditor : TextEditorBase
	{
		public CsharpEditor(IEditorOwner owner, FileWrapper file)
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
			highlightInfo.Add(Brushes.Blue, "abstract as base break case catch checked continue default delegate do else event explicit extern partial false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
			highlightInfo.Add(Brushes.Blue, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void var");
			highlightInfo.Add(new SolidColorBrush(Color.FromRgb(43, 145, 175)), "AssemblyTitle AssemblyDescription AssemblyConfiguration AssemblyCompany AssemblyProduct AssemblyCopyright AssemblyTrademark AssemblyCulture ComVisible AssemblyVersion AssemblyFileVersion ThemeInfo");

			CSharpDocument result = new CSharpDocument(file, highlightInfo);
			return result;
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}
	}

	internal class CSharpDocument : ScriptHighlightDocument
	{
		public CSharpDocument(FileWrapper file, ScriptHighlightInfo info)
			: base(file, info)
		{ }

		public override IList<TextLineChangesActionBase> CommentText(CharPosition start, CharPosition end)
		{
			string startLineText = GetLineText(start.Row);
			string startText = startLineText.Length == 0 || startLineText.Length == start.Col ? string.Empty : startLineText.Substring(start.Col);

			string endLineText = GetLineText(end.Row);
			string endText = endLineText.Length == 0 || end.Col == 0 ? string.Empty : endLineText.Substring(0, end.Col);

			if (startText.StartsWith("/*") && endText.EndsWith("*/"))
			{
				return new TextLineChangesActionBase[]
				{
					new TextLineRemoveAction(new CharPosition(end.Col - 2, end.Row), 2),
					new TextLineRemoveAction(start, 2)
				};
			}

			return new TextLineChangesActionBase[]
			{
				new TextLineInsertAction(end, "*/"), 
				new TextLineInsertAction(start, "/*") 
			};
		}

		public override IList<TextLineChangesActionBase> CommentLines(int startRow, int endRow)
		{
			string startRowText = GetLineText(startRow);
			bool isCommented = startRowText.TrimStart(' ', '\t').StartsWith("//");

			List<TextLineChangesActionBase> actions = new List<TextLineChangesActionBase>();

			if (isCommented)
			{
				for (int index = startRow; index <= endRow; index++)
				{
					string rowText = GetLineText(index);
					bool commented = rowText.TrimStart(' ', '\t').StartsWith("//");

					if (commented)
						actions.Add(new TextLineRemoveAction(new CharPosition(rowText.IndexOf("//", StringComparison.InvariantCulture), index), 2));
				}
			}
			else
			{
				for (int index = startRow; index <= endRow; index++)
					actions.Add(new TextLineInsertAction(new CharPosition(0, index), "//"));
			}

			return actions;
		}
	}
}
