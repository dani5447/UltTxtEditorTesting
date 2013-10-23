using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class CloseAnyTagElement : XmlParserElementBase
	{
		public override string AsString
		{
			get { return ">"; }
		}

		public override IList<UfeTextStyle> AsStyle
		{
			get { return new[] { new UfeTextStyle(">", BlueBrush) }; }
		}
	}
}
