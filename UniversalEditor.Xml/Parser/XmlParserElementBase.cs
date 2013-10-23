using System.Collections.Generic;
using System.Windows.Media;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal abstract class XmlParserElementBase
	{
		internal static readonly Brush BlackBrush = Brushes.Black;
		internal static readonly Brush BlueBrush = Brushes.Blue;
		internal static readonly Brush BrownBrush = Brushes.Brown;
		internal static readonly Brush GreenBrush = Brushes.Green;

		public XmlParserElementBase PrevElement { get; set; }
		public XmlParserElementBase NextElement { get; set; }

		public abstract IList<UfeTextStyle> AsStyle { get; }
		public abstract string AsString { get; }

		public override string ToString()
		{
			return AsString;
		}
	}
}
