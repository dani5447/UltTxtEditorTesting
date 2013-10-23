using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace TextEditor
{
	public abstract class TextEditorBase : EditorBase
	{
		protected readonly Lazy<UfeTextBox> _textBox;
		private XElement _xStatusElement;

		protected TextEditorBase(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_textBox = new Lazy<UfeTextBox>(InitializeTextBox);
		}

		private UfeTextBox InitializeTextBox()
		{
			UfeTextBox textBox = CreateTextBox();
			textBox.LoadState(_xStatusElement);

			return textBox;
		}

		protected abstract UfeTextBox CreateTextBox();

		public override FrameworkElement EditorControl
		{
			get { return _textBox.Value; }
		}

		public override void Refresh()
		{
			_textBox.Value.TextBox.InvalidateVisual();
		}

		public override void Reload()
		{
			if (_textBox.IsValueCreated)
			{
				_textBox.Value.Document.Reload();
				_textBox.Value.CallOnRender();
			}
		}

		public override bool HasChanges
		{
			get { return _file.HasChanges; }
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}

		public override void SaveStatus(XElement file)
		{
			if (_textBox.IsValueCreated)
				_textBox.Value.SaveStatus(file);
			else
			{
				if (_xStatusElement != null)
				{
					foreach (XElement item in _xStatusElement.Elements())
					{
						if (file.Element(item.Name) == null)
							file.Add(new XElement(item));
					}
				}
			}
		}

		public override void LoadStatus(XElement file)
		{
			_xStatusElement = file;
		}

		public override void SaveFile()
		{
			if (_textBox.IsValueCreated)
				_textBox.Value.Document.Save(_file.FilePath);
		}

		public override void SaveFileAs(string filePath)
		{
			_textBox.Value.Document.Save(filePath);
		}

		public override void SaveCopyAs(string filePath)
		{
			_textBox.Value.Document.SaveCopy(filePath);
		}

		public override IList<Control> EditorCommands
		{
			get { return _textBox.Value.GenerateBasicMenu(); }
		}

		public override void OnKeyDown(KeyEventArgs keyEventArgs)
		{
			_textBox.Value.TextBox.OnKeyDownEngine(keyEventArgs);
			base.OnKeyDown(keyEventArgs);
		}

		public override void OnTextInput(TextCompositionEventArgs textCompositionEventArgs)
		{
			_textBox.Value.TextBox.OnTextInputEngine(textCompositionEventArgs);
			base.OnTextInput(textCompositionEventArgs);
		}

		public override bool HasEditorCommands
		{
			get { return true; }
		}

		public override ICommand UndoCommand
		{
			get { return _textBox.Value.TextBox.UndoCommand; }
		}

		public override ICommand CopyCommand
		{
			get { return _textBox.Value.TextBox.CopyCommand; }
		}

		public override ICommand RedoCommand
		{
			get { return _textBox.Value.TextBox.RedoCommand; }
		}

		public override ICommand PasteCommand
		{
			get { return _textBox.Value.TextBox.PasteCommand; }
		}

		public override ICommand DeleteCommand
		{
			get { return _textBox.Value.TextBox.DeleteCommand; }
		}

		public override ICommand SelectAllCommand
		{
			get { return _textBox.Value.TextBox.SelectAllCommand; }
		}

		public override ICommand CutCommand
		{
			get { return _textBox.Value.TextBox.CutCommand; }
		}
	}
}
