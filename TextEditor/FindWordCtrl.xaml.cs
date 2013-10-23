using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UniversalEditor.Base.Options;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for FindLineCtrl.xaml
	/// </summary>
	public partial class FindWordCtrl
	{
		private readonly UfeTextBox _ufeTextBox;

		private bool _autoSearch;

		public FindWordCtrl(UfeTextBox ufeTextBox)
		{
			if (ufeTextBox == null) 
				throw new ArgumentNullException("ufeTextBox");

			_ufeTextBox = ufeTextBox;

			InitializeComponent();
		}

		private void OnFindClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(txtWord.Text))
				_ufeTextBox.FindText(txtWord.Text, Options.Instance.IsStrongSearch);
		}
		
		private void TxtNumber_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				if (!string.IsNullOrEmpty(txtWord.Text))
				{
					_ufeTextBox.FindText(txtWord.Text, Options.Instance.IsStrongSearch);
					e.Handled = true;
				}
			}
			else if (e.Key == Key.Escape)
			{
				_ufeTextBox.HideAllFindControls();
				e.Handled = true;
			}
		}

		public void SetText(string text)
		{
			SetText(text, false, false);
		}

		public void SetText(string text, bool autoSearch, bool isReplace)
		{
			txtWord.Text = text;
			_autoSearch = autoSearch;

			btnReplace.Visibility = btnReplaceAll.Visibility = txtReplace.Visibility = isReplace ? Visibility.Visible : Visibility.Collapsed;
		}

		public string GetText()
		{
			return txtWord.Text;
		}

		private void OnHide(object sender, RoutedEventArgs e)
		{
			_ufeTextBox.HideAllFindControls();
		}

		private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			if (!Options.Instance.IsStrongSearch)
			{
				Options.Instance.IsStrongSearch = true;
				Options.Instance.Save();
			}
		}

		private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if (Options.Instance.IsStrongSearch)
			{
				Options.Instance.IsStrongSearch = false;
				Options.Instance.Save();
			}
		}
		
		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (_autoSearch)
			{
				if (string.IsNullOrEmpty(txtWord.Text))
					return;

				_ufeTextBox.FindText(txtWord.Text, Options.Instance.IsStrongSearch);
			}
		}

		private void OnReplaceClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(txtWord.Text))
				_ufeTextBox.ReplaceText(txtWord.Text, Options.Instance.IsStrongSearch, txtReplace.Text);
		}

		private void OnReplaceAllClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(txtWord.Text))
			{
				int count = _ufeTextBox.ReplaceAllText(txtWord.Text, Options.Instance.IsStrongSearch, txtReplace.Text);
				MessageBox.Show(Application.Current.MainWindow, string.Format("{0} occurrence(s) replaced.", count));
			}
		}
	}
}
