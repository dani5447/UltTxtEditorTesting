
using System;
using System.Windows.Input;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for UfePopup.xaml
	/// </summary>
	public partial class UfePopup
	{
		private readonly UfeTextBox _ufeTextBox;
		private HintItem[] _commands;

		public UfePopup(UfeTextBox ufeTextBox)
		{
			if (ufeTextBox == null) 
				throw new ArgumentNullException("ufeTextBox");
			
			_ufeTextBox = ufeTextBox;
			
			InitializeComponent();
		}

		public HintItem[] Commands
		{
			get { return _commands; }
			set
			{
				_commands = value;
				list.ItemsSource = value;

				if (value.Length > 0)
					list.SelectedIndex = 0;
			}
		}

		public void OnDown()
		{
			list.SelectedIndex = Math.Min(list.SelectedIndex + 1, _commands.Length - 1);
			list.ScrollIntoView(list.SelectedItem);
		}

		public void OnUp()
		{
			list.SelectedIndex = Math.Max(0, list.SelectedIndex - 1);
			list.ScrollIntoView(list.SelectedItem);
		}

		public void OnEnter()
		{
			_ufeTextBox.Insert((HintItem)list.SelectedItem);
		}

		private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OnEnter();
		}
	}
}
