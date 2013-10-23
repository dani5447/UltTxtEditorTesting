using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using TextEditor;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Xml.Parser;

namespace UniversalEditor.Xml
{
	internal class XmlTextDocument : UfeTextDocument
	{
		private readonly XmlParser _xmlParser = new XmlParser();

		public XmlTextDocument(FileWrapper file)
			: base(file, null)
		{ }

		protected override void ReloadContentInner()
		{
			_xmlParser.Clear();

			for (int index = 0, count = _file.GetLinesCount(); index < count; index++)
				_xmlParser.InsertLine(index, _file.GetLine(index));

			if (_file.GetLinesCount() == 0)
				_xmlParser.InsertLine(0, string.Empty);
		}
		
		public override string GetLineText(int index)
		{
			return _xmlParser.GetLineText(index);
		}

		public override int LineCount
		{
			get { return _xmlParser.LineCount; }
		}

		public override IList<UfeTextStyle> GetLineStyle(int index)
		{
			return _xmlParser.GetLineStyle(index);
		}

		public override object GetLineHandle(int index)
		{
			return _xmlParser.GetLineHandle(index);
		}

		public override int GetLineIndex(object lineHandle)
		{
			return _xmlParser.GetLineIndex(lineHandle);
		}

		public override string[] GetFormattedLines(int startRow, int endRow)
		{
			return _xmlParser.GetFormattedLines(startRow, endRow);
		}

		public override IList<UfeTextStyle> GetLineStyle(int rowIndex, int start, int length)
		{
			IList<UfeTextStyle> line = GetLineStyle(rowIndex);
			IList<UfeTextStyle> result = new List<UfeTextStyle>(line.Count);

			for (int index = 0; index < line.Count; index++)
			{
				if (length == 0)
					break;

				UfeTextStyle target = line[index];

				Brush brush = target.Brush;
				string text = target.Text.ToString();

				if (start > text.Length)
				{
					start -= text.Length;
					continue;
				}

				if (start > 0)
				{
					text = text.Substring(start);
					start = 0;
				}

				if (length < text.Length)
				{
					text = text.Substring(0, length);
				}

				if (length > 0)
				{
					result.Add(new UfeTextStyle(text, brush));
					length -= text.Length;
				}
			}

			return result;
		}

		public override void SetText(int rowIndex, string text)
		{
			_xmlParser.SetText(rowIndex, text);
		}
		
		public override HintItem[] GetHints(int rowIndex, int position)
		{
			return _xmlParser.GetHints(rowIndex, position);
		}

		public override void InsertLine(int index)
		{
			_xmlParser.InsertLine(index, string.Empty);
		}

		public override void RemoveLine(int index)
		{
			_xmlParser.RemoveLine(index);
		}

		public override string DoAutocomplition(CharPosition position)
		{
			return _xmlParser.DoAutocomplition(position.Row, position.Col);
		}

		public override IList<TextLineChangesActionBase> CommentText(CharPosition start, CharPosition end)
		{
			string startLineText = GetLineText(start.Row);
			string startText = startLineText.Length == 0 || startLineText.Length == start.Col ? string.Empty : startLineText.Substring(start.Col);

			string endLineText = GetLineText(end.Row);
			string endText = endLineText.Length == 0 || end.Col == 0 ? string.Empty : endLineText.Substring(0, end.Col);

			if (startText.StartsWith("<!--") && endText.EndsWith("-->"))
			{
				return new TextLineChangesActionBase[]
				{
					new TextLineRemoveAction(new CharPosition(end.Col - 3, end.Row), 3),
					new TextLineRemoveAction(start, 4)
				};
			}

			return new TextLineChangesActionBase[]
			{
				new TextLineInsertAction(end, "-->"), 
				new TextLineInsertAction(start, "<!--") 
			};
		}

		public override bool IsFormattingSupported
		{
			get { return true; }
		}

		public override IList<TextLineChangesActionBase> CommentLines(int startRow, int endRow)
		{
			string startRowText = GetLineText(startRow);
			bool isCommented = startRowText.TrimStart(' ', '\t').StartsWith("<!--") && startRowText.TrimEnd(' ', '\t').EndsWith("-->");

			List<TextLineChangesActionBase> actions = new List<TextLineChangesActionBase>();

			if (isCommented)
			{
				for (int index = startRow; index <= endRow; index++)
				{
					string rowText = GetLineText(index);
					bool commented = rowText.TrimStart(' ', '\t').StartsWith("<!--") && rowText.TrimEnd(' ', '\t').EndsWith("-->");

					if (commented)
					{
						actions.Add(new TextLineRemoveAction(new CharPosition(rowText.LastIndexOf("-->", StringComparison.InvariantCulture), index), 3));
						actions.Add(new TextLineRemoveAction(new CharPosition(rowText.IndexOf("<!--", StringComparison.InvariantCulture), index), 4));
					}
				}
			}
			else
			{
				for (int index = startRow; index <= endRow; index++)
				{
					int lineLength = GetLineLength(index);

					actions.Add(new TextLineInsertAction(new CharPosition(lineLength, index), "-->"));
					actions.Add(new TextLineInsertAction(new CharPosition(0, index), "<!--"));
				}
			}

			return actions;
		}

		public override bool IsCommentLineSupported
		{
			get { return true; }
		}

		public override bool IsCommentTextSupported
		{
			get { return true; }
		}
	}
}
