using System;
using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class XmlHeaderElement : XmlParserElementBase
	{
		private readonly string _content;

		public XmlHeaderElement(string content)
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
			get { return new[] { new UfeTextStyle(_content, BlueBrush) }; }
		}
	}
}
