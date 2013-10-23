using System;
using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class AttributeElement : XmlParserElementBase
	{
		private readonly string _content;
		private readonly List<UfeTextStyle> _styles = new List<UfeTextStyle>(5);

		public AttributeElement(string content)
		{
			if (content == null) 
				throw new ArgumentNullException("content");

			_content = content;
		}

		public override string AsString
		{
			get { return _content; }
		}

		public override IList<UfeTextStyle> AsStyle
		{
			get
			{
				if (_styles.Count > 0)
					return _styles;

				int indexSep = _content.IndexOf('=');

				if (indexSep < 0)
				{
					_styles.Add(new UfeTextStyle(_content, BrownBrush));
					return _styles;
				}

				_styles.Add(new UfeTextStyle(_content.Substring(0, indexSep), BrownBrush));
				_styles.Add(new UfeTextStyle("=", BlueBrush));

				indexSep++;

				if (_content.Length > indexSep)
				{
					char ch = _content[indexSep];

					if (ch == '\'' || ch == '\"')
					{
						_styles.Add(new UfeTextStyle(ch.ToString(), BlueBrush));
						indexSep++;
					}
				}

				if (_content.Length > indexSep)
				{
					char ch = _content[_content.Length - 1];

					if (ch == '\'' || ch == '\"')
					{
						_styles.Add(new UfeTextStyle(_content.Substring(indexSep, _content.Length - indexSep - 1), BlackBrush));
						_styles.Add(new UfeTextStyle(ch.ToString(), BlueBrush));
					}
					else
					{
						_styles.Add(new UfeTextStyle(_content.Substring(indexSep), BlackBrush));
					}
				}

				return _styles;
			}
		}
	}
}
