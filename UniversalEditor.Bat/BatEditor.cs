using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextEditor;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Bat
{
	public class BatEditor : TextEditorBase
	{
		public BatEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_textBox.Value.OnKeyDownEvent += _textBox_OnKeyDownEvent;
		}

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, new BatTextDocument(_file));
		}

		public override void Dispose()
		{
			if (_textBox.IsValueCreated)
				_textBox.Value.OnKeyDownEvent -= _textBox_OnKeyDownEvent;
			
			base.Dispose();
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}

		private void _textBox_OnKeyDownEvent(KeyEventArgs e)
		{
			if (e.Key == Key.F5 && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
			{
				if (!string.IsNullOrEmpty(_file.FilePath))
				{
					if (_file.HasChanges)
					{
						MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "The file has been changed but hasn't been saved. Do you want to save it before launch?",
							"Bat Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

						if (result == MessageBoxResult.Yes)
							SaveFile();
						else if (result == MessageBoxResult.Cancel)
						{
							e.Handled = true;
							return;
						}
					}

					Process target = new Process();
					target.StartInfo = new ProcessStartInfo(_file.FilePath);
					target.StartInfo.WorkingDirectory = Path.GetDirectoryName(_file.FilePath);
					target.Start();
				}

				e.Handled = true;
			}
		}
	}
}
