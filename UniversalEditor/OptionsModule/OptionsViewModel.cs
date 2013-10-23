using System.Windows.Input;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;

namespace UniversalEditor.OptionsModule
{
	public class OptionsViewModel : ViewModelBase
	{
		private bool _showLineNumbers = Options.Instance.ShowLineNumbers;
		private bool _checkUpdate = Options.Instance.CheckUpdate;

		public ICommand SaveCommand
		{
			get { return new SimpleCommand(OnSave); }
		}

		private void OnSave()
		{
			Options.Instance.ShowLineNumbers = _showLineNumbers;
			Options.Instance.CheckUpdate = _checkUpdate;
			Options.Instance.Save();
		}

		public bool ShowLineNumbers
		{
			get { return _showLineNumbers; }
			set { _showLineNumbers = value; }
		}

		public bool CheckUpdate
		{
			get { return _checkUpdate; }
			set { _checkUpdate = value; }
		}

		public static OptionsViewModel Designer
		{
			get { return new OptionsViewModel(); }
		}
	}
}
