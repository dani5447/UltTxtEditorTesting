using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using TextEditor;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Bat
{
	internal class BatTextDocument : UfeTextDocument
	{
		private readonly static string[] _commands = new[]
		{
			"rem", "set", "if", "else", "exist", "errorlevel", "for", "in", "do", "break", "call", "copy", "chcp", "cd", "chdir", "choice", "cls", "country", "ctty", "date", "del", "erase", "dir", "echo",
			"exit", "goto", "loadfix", "loadhigh", "mkdir", "md", "move", "path", "pause", "prompt", "rename", "ren", "rmdir", "rd", "shift", "time", "type", "ver", "verify", "vol", "com", "con", "lpt",
			"nul", "defined", "not", "errorlevel", "cmdextversion"
		};

		public BatTextDocument(FileWrapper file)
			: base(file, null)
		{ }

		protected override UfeTextLineBase ParseLine(string text, object arg)
		{
			return new BatTextLine(text);
		}

		private class BatTextLine : UfeTextLineBase
		{
			private readonly StringBuilder _text;
			private readonly List<UfeTextStyle> _parts = new List<UfeTextStyle>();

			public BatTextLine(string text)
			{
				_text = new StringBuilder(text);
			}

			public override void Initialize()
			{
				_parts.Clear();

				StringBuilder word = new StringBuilder();
				UfeTextStyle lastRegularStyle = null;

				ThreadStart addWord = () =>
				{
					if (_commands.Contains(word.ToString().Trim()))
					{
						_parts.Add(new UfeTextStyle(word.ToString(), Brushes.Blue));
						lastRegularStyle = null;
					}
					else if (word.ToString().Trim().StartsWith("%") && word.ToString().Trim().EndsWith("%"))
					{
						_parts.Add(new UfeTextStyle(word.ToString(), Brushes.Brown));
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

				string text = _text.ToString();
				string commentLine = text.TrimStart().StartsWith("rem", StringComparison.InvariantCultureIgnoreCase) ? text : string.Empty;

				if (!string.IsNullOrEmpty(commentLine))
					text = string.Empty;

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
				string originalString = _text.ToString().Substring(0, position);

				// variables
				if (originalString.Count(x => x == '%')%2 > 0)
				{
					string tempString = originalString.Substring(originalString.LastIndexOf('%') + 1);
					var result = from variable in Environment.GetEnvironmentVariables().Keys.OfType<string>()
					             where variable.StartsWith(tempString, StringComparison.InvariantCultureIgnoreCase)
								 select new HintItem(variable, HintItem.ImageType.Variable, variable.Substring(tempString.Length) + '%', '%' + variable.Substring(0, tempString.Length));
					return result.ToArray();
				}

				// commands
				string text = originalString.TrimStart();
				int textEnd = text.IndexOfAny(UfeTextBoxInner.CtrlSplitChars);

				if (textEnd < 0)
				{
					return _commands.Where(x => x.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
						.Select(x => new HintItem(x, HintItem.ImageType.Method, x.Substring(text.Length), x.Substring(0, text.Length)))
						.ToArray();
				}

				return new HintItem[0];
			}

			public override string TextAsString
			{
				get { return _text.ToString(); }
			}
		}
	}
}
