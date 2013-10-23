using System;
using System.Collections.Generic;
using TextEditor;

namespace UniversalEditor.Xml.Parser
{
	internal class StartTagElement : XmlParserElementBase
	{
		private readonly string _tagName;

		public StartTagElement(string tagName)
		{
			if (tagName == null) 
				throw new ArgumentNullException("tagName");

			_tagName = tagName;
		}

		public string Name
		{
			get { return _tagName; }
		}

		public override string AsString
		{
			get { return "<" + _tagName; }
		}

		public override IList<UfeTextStyle> AsStyle
		{
			get 
			{
				return new[]
				{
					new UfeTextStyle("<", BlueBrush),
					new UfeTextStyle(_tagName, BrownBrush)
				}; 
			}
		}
	}
}
