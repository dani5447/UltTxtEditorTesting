using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using UniversalEditor.Base.FileSystem;

namespace TextEditor.ScriptHighlight
{
	public class ScriptHighlightDocument : UfeTextDocument
	{
		private readonly ScriptHighlightInfo _info;

		public ScriptHighlightInfo Info
		{
			get { return _info; }
		}

		public ScriptHighlightDocument(FileWrapper file, ScriptHighlightInfo info)
			: base(file, info)
		{
			_info = info;
		}

		protected override UfeTextLineBase ParseLine(string text, object arg)
		{
			return new ScriptHighlightDocumentLine(text, (ScriptHighlightInfo)arg);
		}

		private class ScriptHighlightDocumentLine : UfeTextLineBase
		{
			private readonly StringBuilder _text;
			private readonly ScriptHighlightInfo _info;

			private readonly List<UfeTextStyle> _parts = new List<UfeTextStyle>();

			public ScriptHighlightDocumentLine(string text, ScriptHighlightInfo info)
			{
				if (info == null) 
					throw new ArgumentNullException("info");
				
				_text = new StringBuilder(text);
				_info = info;
			}

			public override void Initialize()
			{
				_parts.Clear();

				StringBuilder word = new StringBuilder();
				UfeTextStyle lastRegularStyle = null;
				Brush brush;

				ThreadStart addWord = () =>
				{
					if (_info.Map.TryGetValue(word.ToString(), out brush))
					{
						_parts.Add(new UfeTextStyle(word.ToString(), brush));
						lastRegularStyle = null;
					}
					else if (lastRegularStyle == null)
					{
						_parts.Add(lastRegularStyle = new UfeTextStyle(word.ToString(), SystemColors.ControlTextBrush));
					}
					else
					{
						lastRegularStyle.Text.Append(word);
					}
				};

				int commentLineStart = string.IsNullOrEmpty(_info.CommentLine) ? -1 : _text.ToString().IndexOf(_info.CommentLine);

				string text = commentLineStart < 0 ? _text.ToString() : _text.ToString().Substring(0, commentLineStart);
				string commentLine = commentLineStart < 0 ? string.Empty : _text.ToString().Substring(commentLineStart);

				bool isChar = false;
				for (int index = 0; index < text.Length; index++)
				{
					char ch = text[index];
					
					if (UfeTextBoxInner.CtrlSplitChars.Contains(ch))
					{
						if (isChar)
						{
							addWord();
							word.Clear();
							word.Append(ch);

							isChar = false;
						}
						else
						{
							word.Append(ch);
						}
					}
					else
					{
						if (isChar)
						{
							word.Append(ch);
						}
						else if (word.Length > 0)
						{
							addWord();
							word.Clear();
							word.Append(ch);

							isChar = true;
						}
						else
						{
							word.Append(ch);

							isChar = true;
						}
					}
				}

				if (word.Length > 0)
					addWord();

				if (!string.IsNullOrEmpty(commentLine))
					_parts.Add(new UfeTextStyle(commentLine, Brushes.Green));
			}

			public override void Remove(int start, int lenght)
			{
				_text.Remove(start, lenght);
				Initialize();
			}

			public override UfeTextStyle[] Substring(int start, int lenght)
			{
				List<UfeTextStyle> result = new List<UfeTextStyle>();

				for (int index = 0; index < _parts.Count; index++)
				{
					UfeTextStyle part = _parts[index];

					if (part.Text.Length < start)
					{
						start -= part.Text.Length;
						continue;
					}

					if (part.Text.Length > start + lenght)
					{
						result.Add(new UfeTextStyle(part.Text.ToString().Substring(start, lenght), part.Brush));
						break;
					}

					int l = part.Text.Length - start;
					result.Add(new UfeTextStyle(part.Text.ToString().Substring(start, l), part.Brush));
					lenght -= l;
					start = 0;
				}

				return result.ToArray();
			}

			public override UfeTextStyle[] Substring(int start)
			{
				return Substring(start, Length - start);
			}

			public override UfeTextStyle[] Text
			{
				get { return _parts.ToArray(); }
			}

			public override int Length
			{
				get { return _text.Length; }
			}

			public override void Append(string text)
			{
				_text.Append(text);
				Initialize();
			}

			public override void Insert(int start, string text)
			{
				_text.Insert(start, text);
				Initialize();
			}

			public override HintItem[] GetHints(int position)
			{
				return new HintItem[0];
			}

			public override string TextAsString
			{
				get { return _text.ToString(); }
			}
		}
	}

	public class ScriptHighlightInfo
	{
		private readonly Dictionary<string, Brush> _map = new Dictionary<string, Brush>();

		public string CommentLine { get; set; }

		public Dictionary<string, Brush> Map
		{
			get { return _map; }
		}

		public void Add(Brush brush, string words)
		{
			Array.ForEach(words.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries), x => _map[x] = brush);
		}
	}
}
