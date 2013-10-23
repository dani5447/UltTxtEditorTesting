using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Csv;
using UniversalEditor.PlainText;
using UniversalEditor.Xml;

namespace UniversalEditor.Formats
{
	internal class XmlFormat : IFormat
	{
		internal const byte FormatId = 14;

		public string DisplayName { get { return "Xml"; } }

		public string Extension { get { return ".xml|.csproj|.xaml|.resx"; } }

		public EditorBase CreateNewEditor(IEditorOwner owner, FileWrapper file)
		{
			return new XmlEditor(owner, file);
		}

		public byte EditorTypeId
		{
			get { return FormatId; }
		}

		public bool IsChild(EditorBase editor)
		{
			return editor is XmlEditor;
		}

		public bool IsReadonly
		{
			get { return false; }
		}
	}
}
