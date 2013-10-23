using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;

namespace UniversalEditor.Base
{
	public abstract class EditorBase : IDisposable
	{
		protected readonly IEditorOwner _owner;
		protected readonly FileWrapper _file;

		protected EditorBase(IEditorOwner owner, FileWrapper file)
		{
			if (owner == null) 
				throw new ArgumentNullException("owner");
			if (file == null)
				throw new ArgumentNullException("file");

			_owner = owner;
			_file = file;
			_file.HasChangesChanged += owner.RefreshActiveTitle;
		}

		public FileWrapper FileWrapper
		{
			get { return _file; }
		}

		public string FilePath
		{
			get { return _file.FilePath; }
		}

		public string DisplayFilePath
		{
			get { return IsNew ? "New File" : _file.FilePath; }
		}

		public bool IsNew
		{
			get { return string.IsNullOrEmpty(_file.FilePath); }
		}

		public string FileName
		{
			get
			{
				return
					string.IsNullOrEmpty(_file.FilePath) ? "New File" :
					_file.HasChanges ? Path.GetFileName(_file.FilePath) + "*" : Path.GetFileName(_file.FilePath);
			}
		}

		public virtual void SaveFile()
		{
			throw new NotImplementedException();
		}

		public virtual void SaveFileAs(string filePath)
		{
			throw new NotImplementedException();
		}

		public virtual void SaveCopyAs(string filePath)
		{
			throw new NotImplementedException();
		}

		public virtual IList<Control> EditorCommands
		{
			get { return null; }
		}

		public virtual bool HasEditorCommands
		{
			get { return false; }
		}

		public abstract FrameworkElement EditorControl { get; }

		public virtual ICommand UndoCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand CopyCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand RedoCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand PasteCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand DeleteCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand SelectAllCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual ICommand CutCommand
		{
			get 
			{
				return new SimpleCommand(() => { }, () => false);
			}
		}

		public virtual void Reload()
		{ }

		public virtual bool HasChanges
		{
			get { return _file.HasChanges; }
		}

		public virtual void Dispose()
		{
			_file.HasChangesChanged -= _owner.RefreshActiveTitle;
		}

		public virtual void Refresh()
		{ }

		public virtual void Focus()
		{
			if (EditorControl == null)
				return;

			EditorControl.UpdateLayout();
			EditorControl.Focus();
		}

		public override string ToString()
		{
			return FileName;
		}

		public virtual void OnKeyDown(KeyEventArgs keyEventArgs) { }

		public virtual void OnTextInput(TextCompositionEventArgs textCompositionEventArgs) { }

		public virtual void SaveStatus(XElement file) { }

		public virtual void LoadStatus(XElement file) { }

		public virtual void OnFirstView() { }
	}
}
