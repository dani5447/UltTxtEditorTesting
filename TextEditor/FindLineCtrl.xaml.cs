using System;
using System.Windows;
using System.Windows.Input;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for FindLineCtrl.xaml
	/// </summary>
	public partial class FindLineCtrl
	{
		private readonly UfeTextBox _ufeTextBox;

		public FindLineCtrl(UfeTextBox ufeTextBox)
		{
			if (ufeTextBox == null) 
				throw new ArgumentNullException("ufeTextBox");

			_ufeTextBox = ufeTextBox;

			InitializeComponent();
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			int number;

			if (int.TryParse(txtNumber.Text.Trim(), out number))
			{
				_ufeTextBox.ShowLine(number - 1);
				Visibility = Visibility.Collapsed;
			}
		}
		
		private void TxtNumber_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				int number;

				if (int.TryParse(txtNumber.Text.Trim(), out number))
				{
					_ufeTextBox.ShowLine(number - 1);
					_ufeTextBox.HideAllFindControls();
					e.Handled = true;
				}
			}
			else if (e.Key == Key.Escape)
			{
				_ufeTextBox.HideAllFindControls();
				e.Handled = true;
			}
		}

		public void SetLine(int row)
		{
			txtNumber.Text = (row + 1).ToString();
		}

		private void TxtNumber_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			int number;
			e.Handled = !int.TryParse(e.Text, out number);
		}

		private void OnHide(object sender, RoutedEventArgs e)
		{
			_ufeTextBox.HideAllFindControls();
		}
	}
}
