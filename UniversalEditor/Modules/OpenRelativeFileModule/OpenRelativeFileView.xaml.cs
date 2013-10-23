using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UniversalEditor.Modules.OpenRelativeFileModule
{
	/// <summary>
	/// Interaction logic for OpenRelativeFileView.xaml
	/// </summary>
	public partial class OpenRelativeFileView
	{
		private bool _isFocused;

		public OpenRelativeFileView()
		{
			InitializeComponent();
		}

		protected override void OnOpened(System.EventArgs e)
		{
			base.OnOpened(e);

			Cmb.Text = string.Empty;

			UpdateLayout();
			Cmb.Focus();

			_isFocused = true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				_isFocused = false;
				IsOpen = false;
			}
			else if (e.Key == Key.Enter)
			{
				OpenRelativeFileViewModel viewModel = (OpenRelativeFileViewModel) DataContext;
				viewModel.OpenSelectedFile();

				e.Handled = true;
			}

			base.OnKeyDown(e);
//			e.Handled = true;
		}

		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			if (_isFocused)
			{
				_isFocused = false;
				IsOpen = false;
			}
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				list.SelectedIndex = Math.Min(list.SelectedIndex + 1, list.Items.Count);
			}
			else if (e.Key == Key.Up)
			{
				list.SelectedIndex = Math.Max(list.SelectedIndex - 1, 0);
			}
			else if (e.Key == Key.Enter)
			{
				if (list.Items.Count == 0)
				{
					_isFocused = false;
					IsOpen = false;

					e.Handled = true;
					return;
				}

				OpenRelativeFileViewModel viewModel = (OpenRelativeFileViewModel)DataContext;
				object target = list.SelectedItem ?? list.Items[0];
				viewModel.OpenFile(target.ToString());
				e.Handled = true;
			}
		}

		private void OnListMouseUp(object sender, MouseButtonEventArgs e)
		{
			TextBlock textBlock = list.InputHitTest(e.GetPosition(list)) as TextBlock;

			if (textBlock == null)
				return;

			OpenRelativeFileViewModel viewModel = (OpenRelativeFileViewModel)DataContext;
			viewModel.OpenFile(textBlock.Text);
			e.Handled = true;
		}
	}
}
