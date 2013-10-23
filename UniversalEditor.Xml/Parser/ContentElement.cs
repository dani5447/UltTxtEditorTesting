using System;
using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class ContentElement : XmlParserElementBase
	{
		private readonly string _content;

		public ContentElement(string content)
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
			get { return new[] { new UfeTextStyle(_content, BlackBrush) }; }
		}
	}
}
