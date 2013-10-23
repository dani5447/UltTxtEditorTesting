using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using TextEditor.Properties;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Utils.LoggerModule;
using Brush = System.Windows.Media.Brush;
using SystemColors = System.Windows.SystemColors;

namespace TextEditor
{
	public class UfeTextDocument
	{
		public delegate void UpdateCursorPositionDelegate(CharPosition startSelection, CharPosition endSelection);

		public event ThreadStart ContentChanged;
		public event ThreadStart ContentReloaded;
		public event ThreadStart FileDeleted;

		protected readonly FileWrapper _file;
		private readonly object _arg;
		private readonly List<UfeTextLineBase> _lines = new List<UfeTextLineBase>();

		public void SetIsChangedToTrue()
		{
			_file.SetIsChangedToTrue();
		}

		public UfeTextDocument(FileWrapper file, object arg)
		{
			if (file == null) 
				throw new ArgumentNullException("file");

			_file = file;
			_arg = arg;

			ReloadContentInner();

			_file.ContentChanged += OnContentChanged;
			_file.FileDeleted += OnFileDeleted;
		}

		internal string FilePath
		{
			get { return _file.FilePath; }
		}

		protected virtual void ReloadContentInner()
		{
			_lines.Clear();

			// add lines
			for (int index = 0, count = _file.GetLinesCount(); index < count; index++)
				_lines.Add(ParseLine(_file.GetLine(index), _arg));

			if (_lines.Count == 0)
				_lines.Add(ParseLine(string.Empty, _arg));

			// set prev & next lines
			if (_lines.Count > 1)
			{
				_lines[0].SetNextLine(_lines[1]);
			}

			for (int index = 1; index < _lines.Count - 1; index++)
			{
				_lines[index].SetPrevLine(_lines[index - 1]);
				_lines[index].SetNextLine(_lines[index + 1]);
			}

			if (_lines.Count > 2)
			{
				_lines[_lines.Count - 1].SetPrevLine(_lines[_lines.Count - 2]);
			}

			// initialize
			for (int index = 0; index < _lines.Count; index++)
				_lines[index].Initialize();

			if (ContentReloaded != null)
				ContentReloaded();
		}

		private void OnFileDeleted()
		{
			if (FileDeleted != null)
				FileDeleted();
		}

		private void OnContentChanged()
		{
			if (ContentChanged != null)
				ContentChanged();
		}

		public bool IsNewFile
		{
			get { return string.IsNullOrEmpty(_file.FilePath); }
		}

		public virtual int LineCount
		{
			get { return _lines.Count; }
		}

		public virtual bool IsCommentLineSupported
		{
			get { return false; }
		}

		public virtual bool IsCommentTextSupported
		{
			get { return false; }
		}

		public virtual bool IsFormattingSupported
		{
			get { return false; }
		}

		public double GetLineWidthInPixels(int index)
		{
			return UfeTextBoxInner.GetTextWidth(GetLineText(index));
		}

		public virtual string GetLineText(int index)
		{
			return _lines[index].TextAsString;
		}

		public int GetLineLength(int index)
		{
			return GetLineText(index).Length;
		}

		public virtual void InsertLine(int index)
		{
			UfeTextLineBase line = ParseLine(string.Empty, _arg);

			UfeTextLineBase prevLine = null;
			UfeTextLineBase nextLine = null;

			if (index > 0)
			{
				prevLine = _lines[index - 1];
				line.SetPrevLine(prevLine);
				prevLine.SetNextLine(line);
			}

			if (index < _lines.Count)
			{
				nextLine = _lines[index];
				line.SetNextLine(nextLine);
				nextLine.SetPrevLine(line);
			}

			_lines.Insert(index, line);

			if (prevLine != null)
				prevLine.Initialize();

			line.Initialize();

			if (nextLine != null)
				nextLine.Initialize();
		}

		public void Save(string filePath)
		{
			_file.ClearLines();

			for (int index = 0; index < LineCount; index++)
				_file.SetLine(index, GetLineText(index));

			_file.SaveAs(filePath);
		}

		public void SaveCopy(string filePath)
		{
			_file.ClearLines();

			for (int index = 0; index < LineCount; index++)
				_file.SetLine(index, GetLineText(index));

			_file.SaveCopyAs(filePath);
		}

		protected virtual UfeTextLineBase ParseLine(string text, object arg)
		{
			return new UfeTextLine(text);
		}

		public virtual void RemoveLine(int index)
		{
			UfeTextLineBase prevLine = null;
			UfeTextLineBase nextLine = null;

			if (index > 0)
				prevLine = _lines[index - 1];

			if (index < _lines.Count - 1)
				nextLine = _lines[index + 1];

			if (prevLine != null)
				prevLine.SetNextLine(nextLine);
			if (nextLine != null)
				nextLine.SetPrevLine(prevLine);

			if (prevLine != null)
				prevLine.Initialize();
			
			if (nextLine != null)
				nextLine.Initialize();

			_lines.RemoveAt(index);
		}

		public void Reload()
		{
			_file.Reload();
			ReloadContentInner();
		}

		public virtual IList<UfeTextStyle> GetLineStyle(int index)
		{
			return _lines[index].Text;
		}

		public virtual IList<UfeTextStyle> GetLineStyle(int index, int startCol, int length)
		{
			return _lines[index].Substring(startCol, length);
		}

		public virtual object GetLineHandle(int index)
		{
			return _lines[index];
		}

		public virtual int GetLineIndex(object lineHandle)
		{
			return _lines.IndexOf((UfeTextLineBase)lineHandle);
		}

		public virtual void SetText(int rowIndex, string text)
		{
			UfeTextLineBase line = _lines[rowIndex];
			line.Remove(0, line.Length);
			line.Append(text);
		}
		
		public virtual HintItem[] GetHints(int rowIndex, int position)
		{
			return _lines[rowIndex].GetHints(position);
		}

		public virtual string DoAutocomplition(CharPosition endSelection)
		{
			return null;
		}

		public virtual IList<TextLineChangesActionBase> CommentText(CharPosition start, CharPosition end)
		{
			return null;
		}

		public virtual IList<TextLineChangesActionBase> CommentLines(int startRow, int endRow)
		{
			return null;
		}

		public virtual string[] GetFormattedLines(int startRow, int endRow)
		{
			return Enumerable.Range(startRow, endRow - startRow + 1).Select(GetLineText).ToArray();
		}
	}

	public abstract class TextLineChangesActionBase
	{
		private readonly CharPosition _position;

		public CharPosition Position
		{
			get { return _position; }
		}

		protected TextLineChangesActionBase(CharPosition position)
		{
			_position = position;
		}
	}

	public class TextLineInsertAction : TextLineChangesActionBase
	{
		private readonly string _text;

		public string Text
		{
			get { return _text; }
		}

		public TextLineInsertAction(CharPosition position, string text)
			: base(position)
		{
			_text = text;
		}
	}

	public class TextLineRemoveAction : TextLineChangesActionBase
	{
		private readonly int _textLength;

		public int TextLength
		{
			get { return _textLength; }
		}

		public TextLineRemoveAction(CharPosition position, int textLength)
			: base(position)
		{
			_textLength = textLength;
		}
	}

	public abstract class UfeTextLineBase
	{
		private UfeTextLineBase _prevLine;
		private UfeTextLineBase _nextLine;

		public abstract UfeTextStyle[] Substring(int start, int lenght);
		public abstract UfeTextStyle[] Substring(int start);
		public abstract UfeTextStyle[] Text { get; }
		public abstract int Length { get; }

		public abstract void Remove(int start, int lenght);
		public abstract void Append(string text);
		public abstract void Insert(int start, string text);

		public abstract HintItem[] GetHints(int position);

		public abstract string TextAsString { get; }

		internal double GetWidthInPixels()
		{
			return UfeTextBoxInner.GetTextWidth(TextAsString);
		}

		internal void SetPrevLine(UfeTextLineBase line)
		{
			_prevLine = line;
		}

		internal void SetNextLine(UfeTextLineBase line)
		{
			_nextLine = line;
		}

		protected UfeTextLineBase PrevLine
		{
			get { return _prevLine; }
		}

		protected UfeTextLineBase NextLine
		{
			get { return _nextLine; }
		}

		public abstract void Initialize();

		public virtual bool CommentLine()
		{
			return false;
		}

		public override string ToString()
		{
			return TextAsString;
		}
	}

	public class HintItem
	{
		public enum ImageType { Default, Method, Variable }

		private readonly string _value;
		private readonly string _prefix;
		private readonly string _displayName;
		private readonly ImageType _imageType;

		public HintItem(string value)
			: this(value, ImageType.Default, value, string.Empty)
		{}

		public HintItem(string displayName, ImageType imageType, string value, string prefix)
		{
			_value = value;
			_prefix = prefix;
			_displayName = displayName;
			_imageType = imageType;
		}

		public string Value
		{
			get { return _value; }
		}

		public string Prefix
		{
			get { return _prefix; }
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public ImageSource Image
		{
			get
			{
				if (_imageType == ImageType.Method)
					return Resources.hint_method.ToImageSource();

				if (_imageType == ImageType.Variable)
					return Resources.hint_variable.ToImageSource();

				return Resources.hint_default.ToImageSource();
			}
		}

		public override string ToString()
		{
			return DisplayName;
		}
	}

	public class UfeTextStyle
	{
		private readonly Brush _brush;
		private readonly StringBuilder _text;

		public UfeTextStyle(string text, Brush brush)
		{
			_brush = brush;
			_text = new StringBuilder(text);
		}

		public Brush Brush
		{
			get { return _brush; }
		}

		public StringBuilder Text
		{
			get { return _text; }
		}

		public override string ToString()
		{
			return _text.ToString();
		}
	}

	public class UfeTextLine : UfeTextLineBase
	{
		private readonly UfeTextStyle _text;

		public UfeTextLine(string text)
		{
			_text = new UfeTextStyle(text, SystemColors.ControlTextBrush);
		}

		public override void Initialize()
		{}

		public override void Remove(int start, int lenght)
		{
			_text.Text.Remove(start, lenght);
		}

		public override UfeTextStyle[] Substring(int start, int lenght)
		{
			return new[] { new UfeTextStyle(_text.Text.ToString().Substring(start, lenght), SystemColors.ControlTextBrush) };
		}

		public override UfeTextStyle[] Substring(int start)
		{
			return new[] { new UfeTextStyle(_text.Text.ToString().Substring(start), SystemColors.ControlTextBrush) };
		}

		public override UfeTextStyle[] Text
		{
			get { return new[] {_text}; }
		}

		public override int Length
		{
			get { return _text.Text.Length; }
		}

		public override void Append(string text)
		{
			_text.Text.Append(text);
		}

		public override void Insert(int start, string text)
		{
			_text.Text.Insert(start, text);
		}

		public override HintItem[] GetHints(int position)
		{
			return new HintItem[0];
		}

		public override string TextAsString
		{
			get { return _text.Text.ToString(); }
		}

		public override string ToString()
		{
			return _text.Text.ToString();
		}
	}

	public class CharPosition
	{
		private readonly int _col;
		private readonly int _row;

		public CharPosition(CharPosition source)
			: this(source.Col, source.Row)
		{}

		public CharPosition(int col, int row)
		{
			_col = col;
			_row = row;
		}

		public int Col
		{
			get { return _col; }
		}

		public int Row
		{
			get { return _row; }
		}

		public static bool operator <(CharPosition first, CharPosition second)
		{
			return first.Row < second.Row || (first.Row == second.Row && first.Col < second.Col);
		}

		public static bool operator >(CharPosition first, CharPosition second)
		{
			return first.Row > second.Row || (first.Row == second.Row && first.Col > second.Col);
		}
		public static bool operator ==(CharPosition first, CharPosition second)
		{
			if (ReferenceEquals(first, null) && ReferenceEquals(second, null))
				return true;
			if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
				return false;

			return first.Row == second.Row && first.Col == second.Col;
		}

		public static bool operator !=(CharPosition first, CharPosition second)
		{
			return !(first == second);
		}

		public override bool Equals(object obj)
		{
			return this == obj as CharPosition;
		}

		public override int GetHashCode()
		{
			return _col*47 + _row*53;
		}

		public override string ToString()
		{
			return string.Format("Row {0}, Col {1}", _row, _col);
		}
	}
}
