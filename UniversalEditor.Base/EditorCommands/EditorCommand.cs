using System.Threading;
using UniversalEditor.Base.Mvvm;

namespace UniversalEditor.Base.EditorCommands
{
	public class EditorCommand : SimpleCommand
	{
		private readonly string _displayName;

		public string DisplayName
		{
			get { return _displayName; }
		}

		public EditorCommand(string displayName, ThreadStart method)
			: base(method)
		{
			_displayName = displayName;
		}
	}
}
