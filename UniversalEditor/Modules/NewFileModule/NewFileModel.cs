using System.Linq;
using System.Windows;
using UniversalEditor.Formats;

namespace UniversalEditor.Modules.NewFileModule
{
	internal class NewFileModel
	{
		internal IFormat CurrentFormat { get; set; }

		public NewFileModel()
		{
			if (Clipboard.ContainsImage())
			{
				CurrentFormat = App.DomainModel.Editors.First(x => x.Format is PlainImageFormat).Format;
			}
			else
			{
				CurrentFormat = App.DomainModel.Editors.First(x => x.Format is PlainTextFormat).Format;
			}
		}
	}
}
