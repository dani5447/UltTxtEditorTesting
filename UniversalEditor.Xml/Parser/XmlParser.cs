using System;
using System.Collections.Generic;
using System.Text;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class XmlParser
	{
		private readonly List<XmlParserElementBase> _elements = new List<XmlParserElementBase>(10000);
		private readonly List<NewLineElement> _lines = new List<NewLineElement>(10000);

		public void InsertLine(int index, string text)
		{
			if (index > _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "index");

			XmlParserElementBase nextElement = _lines.Count > index ? _lines[index] : null;
			XmlParserElementBase prevElement = nextElement != null ? nextElement.PrevElement : 
				_elements.Count > 0 ? _elements[_elements.Count - 1] : null;

			// create new elements
			List<XmlParserElementBase> newElements = new List<XmlParserElementBase>();

			NewLineElement newLine = new NewLineElement();
			newElements.Add(newLine);
			newElements.AddRange(ParseString(prevElement, text));

			int elementIndex = nextElement != null ? _elements.IndexOf(nextElement) : _elements.Count;
			for (int marker = 0; marker < newElements.Count; marker++)
			{
				XmlParserElementBase target = newElements[marker];

				if (prevElement != null)
				{
					prevElement.NextElement = target;
					target.PrevElement = prevElement;
				}
				
				prevElement = target;
			}
			
			if (nextElement != null)
			{
				nextElement.PrevElement = newElements[newElements.Count - 1];
				newElements[newElements.Count - 1].NextElement = nextElement;
			}

			_elements.InsertRange(elementIndex, newElements);
			_lines.Insert(index, newLine);

			UpdateIsCommentProperty(index);
		}

		public string GetLineText(int index)
		{
			if (index >= _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "index");

			StringBuilder builder = new StringBuilder();

			XmlParserElementBase currentElement = _lines[index];
			while (currentElement.NextElement != null && !(currentElement.NextElement is NewLineElement))
			{
				currentElement = currentElement.NextElement;
				builder.Append(currentElement.AsString);
			}

			return builder.ToString();
		}

		private IList<XmlParserElementBase> ParseString(XmlParserElementBase prevElement, string text)
		{
			bool isInTag = false;
			XmlParserElementBase element = prevElement;
			while (element != null)
			{
				if (element is EndTagElement)
					break;
				if (element is EndShortTagElement)
					break;
				if (element is CloseAnyTagElement)
					break;
				if (element is StartTagElement)
				{
					isInTag = true;
					break;
				}

				element = element.PrevElement;
			}
			
			List<XmlParserElementBase> result = new List<XmlParserElementBase>(10);

			StringBuilder builder = new StringBuilder(100);
			bool isHeaderTag = false;
			bool isOpenTag = false;
			bool isCloseTag = false;
			bool isInAttributeValue = false;

			for (int index = 0; index < text.Length; index++)
			{
				char ch = text[index];

				if (ch == '-' && IsChar(text, index + 1, '-') && IsChar(text, index + 2, '>'))
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					result.Add(new EndCommentBlockElement());

					isHeaderTag = false;
					isOpenTag = false;
					isCloseTag = false;
					isInAttributeValue = false;

					index += 2;
				}
				else if (ch == '<' && IsChar(text, index + 1, '!') && IsChar(text, index + 2, '-') && IsChar(text, index + 3, '-'))
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					result.Add(new StartCommentBlockElement());

					isHeaderTag = false;
					isOpenTag = false;
					isCloseTag = false;
					isInAttributeValue = false;

					index += 3;
				}
				else if (isHeaderTag)
				{
					if (ch == '>' && IsChar(text, index - 1, '?'))
					{
						builder.Append(ch);
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));
						isHeaderTag = false;
					}
					else
					{
						builder.Append(ch);
					}
				}
				else if (isOpenTag)
				{
					if (ch == ' ' || ch == '\t')
					{
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));
						isOpenTag = false;
						builder.Append(ch);
					}
					else if (ch == '>')
					{
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));
						result.Add(new CloseAnyTagElement());
						isOpenTag = false;
						isInTag = false;
					}
					else
					{
						builder.Append(ch);
					}
				}
				else if (isCloseTag)
				{
					if (ch == ' ' || ch == '\t')
					{
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));
						isCloseTag = false;
						builder.Append(ch);
					}
					else if (ch == '>')
					{
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));
						result.Add(new CloseAnyTagElement());
						isCloseTag = false;
						isInTag = false;
					}
					else
					{
						builder.Append(ch);
					}
				}
				else if (ch == '<' && IsChar(text, index + 1, '?'))
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					isHeaderTag = true;
					builder.Append(ch);
				}
				else if (ch == '<' && IsChar(text, index + 1, '/'))
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					isCloseTag = true;
					isInTag = true;
					index++;
				}
				else if (ch == '/' && IsChar(text, index + 1, '>'))
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					result.Add(new EndShortTagElement());
					isInTag = false;
					index++;
				}
				else if (ch == '>')
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					result.Add(new CloseAnyTagElement());
					isInTag = false;
				}
				else if (ch == '<')
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					isOpenTag = true;
					isInTag = true;
				}
				else if (isInTag && builder.ToString().Trim().Length == 0 && ch != ' ' && ch != '\t')
				{
					if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
						result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					builder.Append(ch);
				}
				else if (isInTag && !isInAttributeValue && builder.ToString().Trim().Length != 0 && (ch == ' ' || ch == '\t'))
				{
					result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

					builder.Append(ch);
				}
				else if (isInTag && builder.ToString().Trim().Length != 0 && (ch == '\'' || ch == '\"') && IsChar(text, index - 1, '='))
				{
					isInAttributeValue = true;
					builder.Append(ch);
				}
				else if (isInTag && isInAttributeValue  && builder.ToString().Trim().Length != 0 && (ch == '\'' || ch == '\"'))
				{
					isInAttributeValue = false;
					builder.Append(ch);
				}
				else
				{
					builder.Append(ch);
				}
			}

			if (builder.Length > 0 || isHeaderTag || isOpenTag || isCloseTag)
				result.Add(GenerateElement(builder, isHeaderTag, isOpenTag, isCloseTag, isInTag));

			return result;
		}

		private static XmlParserElementBase GenerateElement(StringBuilder text, bool isHeaderTag, bool isOpenTag, bool isCloseTag, bool isInTag)
		{
			if (text == null) 
				throw new ArgumentNullException("text");

			try
			{
				if (isHeaderTag)
				{
					XmlHeaderElement result = new XmlHeaderElement(text.ToString());
					return result;
				}
				else if (isOpenTag)
				{
					StartTagElement result = new StartTagElement(text.ToString());
					return result;
				}
				else if (isCloseTag)
				{
					EndTagElement result = new EndTagElement(text.ToString());
					return result;
				}
				else if (isInTag && string.IsNullOrEmpty(text.ToString().Trim()))
				{
					ContentElement result = new ContentElement(text.ToString());
					return result;
				}
				else if (isInTag)
				{
					AttributeElement result = new AttributeElement(text.ToString());
					return result;
				}
				else
				{
					ContentElement result = new ContentElement(text.ToString());
					return result;
				}
			}
			finally
			{
				text.Clear();
			}
		}

		private static bool IsChar(string text, int index, char ch)
		{
			if (index < 0)
				return false;
			if (index >= text.Length)
				return false;

			return text[index] == ch;
		}

		public int LineCount
		{
			get { return _lines.Count; }
		}

		public IList<UfeTextStyle> GetLineStyle(int index)
		{
			if (index >= _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "index");

			List<UfeTextStyle> styles = new List<UfeTextStyle>();

			NewLineElement lineBeginElement = _lines[index];
			XmlParserElementBase currentElement = lineBeginElement;
			bool isComment = lineBeginElement.IsComment;

			while (currentElement.NextElement != null && !(currentElement.NextElement is NewLineElement))
			{
				currentElement = currentElement.NextElement;

				if (currentElement is StartCommentBlockElement)
					isComment = true;

				if (isComment)
				{
					styles.Add(new UfeTextStyle(currentElement.AsString, XmlParserElementBase.GreenBrush));
				}
				else
				{
					styles.AddRange(currentElement.AsStyle);
				}

				if (currentElement is EndCommentBlockElement)
					isComment = false;
			}

			return styles;
		}

		public object GetLineHandle(int index)
		{
			if (index >= _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "index");

			return _lines[index];
		}

		public int GetLineIndex(object lineHandle)
		{
			return _lines.IndexOf((NewLineElement)lineHandle);
		}

		public void SetText(int rowIndex, string text)
		{
			if (rowIndex >= _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "rowIndex");

			XmlParserElementBase element = _lines[rowIndex];
			int elementIndex = _elements.IndexOf(element) + 1;

			while (elementIndex < _elements.Count && !(_elements[elementIndex] is NewLineElement))
			{
				_elements.RemoveAt(elementIndex);
			}

			XmlParserElementBase prevElement = elementIndex > 0 ? _elements[elementIndex - 1] : null;

			IList<XmlParserElementBase> newElements = ParseString(prevElement, text);
			_elements.InsertRange(elementIndex, newElements);

			for (int index = elementIndex; index < elementIndex + newElements.Count; index++)
			{
				XmlParserElementBase target = _elements[index];

				target.PrevElement = prevElement;
				
				if (prevElement != null)
					prevElement.NextElement = target;

				prevElement = target;
			}

			if (elementIndex + newElements.Count < _elements.Count)
			{
				XmlParserElementBase target = _elements[elementIndex + newElements.Count];

				target.PrevElement = prevElement;

				if (prevElement != null)
					prevElement.NextElement = target;
			}
			else if (prevElement != null)
			{
				prevElement.NextElement = null;
			}

			UpdateIsCommentProperty(rowIndex);
		}

		private void UpdateIsCommentProperty(int lineIndex)
		{
			NewLineElement newLine = _lines[lineIndex];
			NewLineElement prevLine = _lines[Math.Max(lineIndex - 1, 0)];

			bool isComment = prevLine.IsComment;
			XmlParserElementBase element = prevLine.NextElement;
			while (element != null && !(element != newLine && element is NewLineElement && ((NewLineElement)element).IsComment == isComment))
			{
				if (element is StartCommentBlockElement)
					isComment = true;
				else if (element is EndCommentBlockElement)
					isComment = false;
				else if (element is NewLineElement)
					((NewLineElement)element).IsComment = isComment;

				element = element.NextElement;
			}
		}

		public bool CommentLine(int rowIndex)
		{
			string text = GetLineText(rowIndex);
			string trimmedText = text.Trim();

			if (trimmedText.StartsWith("<!--") && trimmedText.EndsWith("-->"))
			{
				text = text.Remove(text.IndexOf("<!--"), 4);
				text = text.Remove(text.IndexOf("-->", 3));
			}
			else
			{
				text = "<!--" + text + "-->";
			}

			SetText(rowIndex, text);
			return true;
		}

		public HintItem[] GetHints(int rowIndex, int position)
		{
			string text = GetLineText(rowIndex);

			if (position > 1 && text[position - 2] == '<' && text[position - 1] == '/')
			{
				StartTagElement startTag = GetOpenedTag(rowIndex, position);

				if (startTag != null)
					return new[] { new HintItem(startTag.Name, HintItem.ImageType.Default, startTag.Name + ">", "</") };
			}

			return new HintItem[0];
		}

		public string DoAutocomplition(int rowIndex, int position)
		{
			string text = GetLineText(rowIndex);

			if (position > 0 && text[position - 1] == '>' && !IsChar(text, position - 2, '<'))
			{
				StartTagElement startTag = GetOpenedTag(rowIndex, position);
				Tuple<XmlParserElementBase, int> currentTag = GetCurrentTag(rowIndex, position);

				if (startTag != null)
				{
					XmlParserElementBase temp = startTag.NextElement;

					while (temp is ContentElement || temp is AttributeElement || temp is NewLineElement)
						temp = temp.NextElement;

					if (temp == currentTag.Item1)
						return "</" + startTag.Name + '>';
				}
			}
			else if (position > 1 && text[position - 2] == '<' && text[position - 1] == '/')
			{
				StartTagElement startTag = GetOpenedTag(rowIndex, position);

				if (startTag != null)
					return startTag.Name + '>';
			}
			else if (position > 0 && text[position - 1] == '=' && (text.Length == position || (text[position] != '\'' && text[position] != '\"')))
			{
				Tuple<XmlParserElementBase, int> element = GetCurrentTag(rowIndex, position);
				if (element.Item1 is AttributeElement)
				{
					string elementText = ((AttributeElement) element.Item1).AsString;
					if (elementText.IndexOf('=', 0, element.Item2 - 1) == -1)
						return "\"\"";
				}
			}

			return null;
		}

		private StartTagElement GetOpenedTag(int rowIndex, int position)
		{
			XmlParserElementBase element = GetCurrentTag(rowIndex, position).Item1;
			element = element.PrevElement;

			int isClosed = 0;
			while (element != null)
			{
				if (element is StartTagElement && isClosed == 0)
					break;

				if (element is StartTagElement)
				{
					isClosed--;
				}
				else if (element is EndTagElement)
				{
					isClosed++;
				}
				else if (element is EndShortTagElement)
				{
					isClosed++;
				}

				element = element.PrevElement;
			}

			return element as StartTagElement;
		}

		private Tuple<XmlParserElementBase, int> GetCurrentTag(int rowIndex, int position)
		{
			int col = 0;
			XmlParserElementBase element = _lines[rowIndex];

			while (col < position)
			{
				element = element.NextElement;
				col += element.AsString.Length;
			}

			return new Tuple<XmlParserElementBase, int>(element, position - (col - element.AsString.Length));
		}

		public void RemoveLine(int index)
		{
			if (index >= _lines.Count)
				throw new ArgumentException("Parameter value is too big.", "index");

			XmlParserElementBase element = _lines[index];
			int elementIndex = _elements.IndexOf(element);

			do
			{
				_elements.RemoveAt(elementIndex);
			}
			while (elementIndex < _elements.Count && !(_elements[elementIndex] is NewLineElement));

			XmlParserElementBase prevElement = elementIndex > 0 ? _elements[elementIndex - 1] : null;
			XmlParserElementBase nextElement = elementIndex < _elements.Count ? _elements[elementIndex] : null;

			if (prevElement != null)
				prevElement.NextElement = nextElement;

			if (nextElement != null)
				nextElement.PrevElement = prevElement;

			_lines.RemoveAt(index);
		}

		public void Clear()
		{
			_lines.Clear();
			_elements.Clear();
		}

		public string[] GetFormattedLines(int startRow, int endRow)
		{
			List<string> result = new List<string>(endRow - startRow + 1);

			string startOffset = string.Empty;
			StartTagElement openedTag = null;

			if (startRow > 0)
			{
				openedTag = GetOpenedTag(startRow - 1, GetLineText(startRow - 1).Length);
				
				if (openedTag != null)
				{
					ContentElement offsetElement = openedTag.PrevElement as ContentElement;
					XmlParserElementBase temp = openedTag.PrevElement;

					while (!(temp is NewLineElement))
					{
						offsetElement = temp as ContentElement;
						temp = temp.PrevElement;
					}

					if (offsetElement != null && string.IsNullOrEmpty(offsetElement.AsString.Trim()))
					{
						startOffset = offsetElement.AsString;
					}
				}
			}

			if (openedTag != null)
				startOffset = startOffset.Insert(0, "\t");

			List<XmlParserElementBase> data = new List<XmlParserElementBase>(10);
			XmlParserElementBase currentTag = _lines[startRow].NextElement;
			int currentRow = startRow;

			while (currentRow <= endRow && currentTag != null)
			{
				if (currentTag is NewLineElement)
				{
					currentRow++;
					data.Add(currentTag);
				}
				else if (currentTag is ContentElement && string.IsNullOrEmpty(currentTag.AsString.Trim()))
				{
					;
				}
				else
				{
					data.Add(currentTag);
				}

				currentTag = currentTag.NextElement;
			}

			StringBuilder builder = new StringBuilder();
			for (int index = 0; index < data.Count; index++)
			{
				XmlParserElementBase temp = data[index];
				
				if (temp is StartTagElement)
				{
					AppendTo(temp, startOffset, builder);
					startOffset = startOffset.Insert(0, "\t");
				}
				else if (temp is XmlHeaderElement)
				{
					AppendTo(temp, startOffset, builder);
					result.Add(builder.ToString());
					builder.Clear();
				}
				else if (temp is EndTagElement)
				{
					startOffset = 
						startOffset.Length == 0 ? startOffset :
						startOffset.Contains("\t") ? startOffset.Remove(startOffset.IndexOf('\t'), 1) :
						startOffset.Length >= 4 ? startOffset.Substring(4) : string.Empty;

					AppendTo(temp, startOffset, builder);
				}
				else if (temp is AttributeElement)
				{
					if (builder.Length != 0)
						builder.Append(" ");

					AppendTo(temp, startOffset, builder);
				}
				else if (temp is EndShortTagElement && Is<StartCommentBlockElement>(data, index + 1) && Is<ContentElement>(data, index + 2))
				{
					if (builder.Length > 0)
						builder.Append(" ");
					AppendTo(temp, startOffset, builder);
					builder.Append(" ");

					startOffset =
						startOffset.Length == 0 ? startOffset :
						startOffset.Contains("\t") ? startOffset.Remove(startOffset.IndexOf('\t'), 1) :
						startOffset.Length >= 4 ? startOffset.Substring(4) : string.Empty;
				}
				else if (temp is EndShortTagElement)
				{
					if (builder.Length > 0)
						builder.Append(" ");
					AppendTo(temp, startOffset, builder);

					startOffset =
						startOffset.Length == 0 ? startOffset :
						startOffset.Contains("\t") ? startOffset.Remove(startOffset.IndexOf('\t'), 1) :
						startOffset.Length >= 4 ? startOffset.Substring(4) : string.Empty;

					result.Add(builder.ToString());
					builder.Clear();
				}
				else if (temp is CloseAnyTagElement && Is<ContentElement>(data, index + 1))
				{
					AppendTo(temp, startOffset, builder);
				}
				else if (temp is CloseAnyTagElement && Is<StartCommentBlockElement>(data, index + 1) && Is<ContentElement>(data, index + 2))
				{
					AppendTo(temp, startOffset, builder);
					builder.Append(" ");
				}
				else if (temp is CloseAnyTagElement)
				{
					AppendTo(temp, startOffset, builder);

					result.Add(builder.ToString());
					builder.Clear();
				}
				else if (temp is NewLineElement)
				{
					if (Is<NewLineElement>(data, index + 1) && result.Count > 0 && result[result.Count - 1].Length != 0)
					{
						result.Add(builder.ToString());
						builder.Clear();
					}
				}
				else if (temp is EndCommentBlockElement)
				{
					AppendTo(temp, startOffset, builder);

					result.Add(builder.ToString());
					builder.Clear();
				}
				else
				{
					AppendTo(temp, startOffset, builder);
				}
			}

			return result.ToArray();
		}

		private static bool Is<T>(IList<XmlParserElementBase> collection, int index) where T : XmlParserElementBase
		{
			if (index < 0 || index >= collection.Count)
				return false;

			return collection[index] is T;
		}

		private static bool IsFirst<T1, T2>(XmlParserElementBase startElement) where T1 : XmlParserElementBase where T2 : XmlParserElementBase
		{
			XmlParserElementBase temp = startElement;

			while (temp != null)
			{
				if (temp is T1)
					return true;
				if (temp is T2)
					return false;

				temp = temp.NextElement;
			}

			return false;
		}

		private static void AppendTo(XmlParserElementBase element, string offset, StringBuilder target)
		{
			if (target.Length == 0)
				target.Append(offset);

			target.Append(element.AsString);
		}
	}
}
