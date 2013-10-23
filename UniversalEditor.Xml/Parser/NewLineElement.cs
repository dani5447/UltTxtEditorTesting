using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class NewLineElement : XmlParserElementBase
	{
		public override IList<UfeTextStyle> AsStyle
		{
			get { return new UfeTextStyle[0]; }
		}

		public override string AsString
		{
			get { return string.Empty; }
		}

		public bool IsComment { get; set; }

		public override string ToString()
		{
			return "New Line";
		}
	}
}
