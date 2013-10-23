using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Formats;
using UniversalEditor.PlainText;

namespace UniversalEditor
{
	internal class DomainModel : IEditorOwner
	{
		internal event ThreadStart SelectedEditorChanged;
		internal event ThreadStart HeaderChanged;
		internal event ThreadStart AllHeaderChanged;

		private readonly IFormat[] _editors = new IFormat[]
		{
			new PlainTextFormat(), 
			new PlainImageFormat(),
			new CsvFormat(), 
			new PdfFormat(),
 			new OpenXmlFormat(), 
			new VideoFormat(), 
			new IconFormat(), 
			new AudioFormat(), 
			new ArchiveFormat(), 
			new XmlFormat(),
			new BatFormat(), 
			new NsisFormat(),
			new CsharpFormat(), 
			new JavaFormat(),
			new ActionscriptFormat(), 
		};

		private readonly ObservableCollection<EditorBase> _openedEditors = new ObservableCollection<EditorBase>();
		private readonly List<EditorBase> _recentOpenedEditors = new List<EditorBase>();
		private readonly SpecialEdoitorsHolder _specialEdoitorsHolder = SpecialEdoitorsHolder.Load();

		private readonly RecentFilesHolder _recentFilesHolder = RecentFilesHolder.Load();

		private EditorBase _selectedEditor;

		public ObservableCollection<EditorBase> OpenedEditors
		{
			get { return _openedEditors; }
		}

		public List<EditorBase> RecentOpenedEditors
		{
			get { return _recentOpenedEditors; }
		}

		internal string FilePath
		{
			get
			{
				if (_selectedEditor == null) 
					return null;

				if (string.IsNullOrEmpty(_selectedEditor.FilePath))
					return "New File";

				return _selectedEditor.FilePath;
			}
		}

		public RecentFilesHolder RecentFilesHolder
		{
			get { return _recentFilesHolder; }
		}

		internal IFormat[] Editors
		{
			get { return _editors; }
		}

		public EditorBase OpenFile(string fileName, bool isNew, bool needSelect)
		{
			return OpenFile(fileName, _openedEditors.Count, isNew, needSelect, true, null);
		}

		public EditorBase OpenFile(string fileName, bool isNew, bool needSelect, bool needRegisterAsRecentOpened, XElement docStatus)
		{
			return OpenFile(fileName, _openedEditors.Count, isNew, needSelect, needRegisterAsRecentOpened, docStatus);
		}

		public EditorBase OpenFile(string fileName, int indexInTabOrder, bool isNew, bool needSelect, bool needRegisterAsRecentOpened, XElement docStatus)
		{
			EditorBase existedEditor = _openedEditors.FirstOrDefault(x => x.FilePath != null && x.FilePath.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));

			if (existedEditor != null && !isNew)
			{
				SelectedEditor = existedEditor;
				_recentFilesHolder.RegisterPath(fileName);
				return existedEditor;
			}

			FileWrapper file = new FileWrapper(fileName, isNew);

			IFormat format;
			if (_specialEdoitorsHolder.ContainsInfo(fileName))
			{
				byte id = _specialEdoitorsHolder.GetInfo(fileName);
				format = _editors.First(x => x.EditorTypeId == id);
			}
			else
			{
				string fileExtension = Path.GetExtension(fileName).ToLower();
				format = _editors.FirstOrDefault(x => x.Extension.Split('|').Contains(fileExtension));
				
				if (format == null && file.CanBeXml)
				{
					format = _editors.First(x => x.EditorTypeId == XmlFormat.FormatId);
				}

				if (format == null)
					format = _editors.First();
			}
			
			EditorBase openedEditor = format.CreateNewEditor(this, file);
			openedEditor.LoadStatus(docStatus);

			_openedEditors.Insert(indexInTabOrder, openedEditor);

			if (needSelect)
				SelectedEditor = openedEditor;

			if (!isNew && needRegisterAsRecentOpened)
				_recentFilesHolder.RegisterPath(fileName);

			return openedEditor;
		}

		internal void CreateNewFile(IFormat currentFormat)
		{
			EditorBase openedEditor = currentFormat.CreateNewEditor(this, new FileWrapper(string.Empty, true));

			_openedEditors.Insert(0, openedEditor);
			SelectedEditor = openedEditor;
		}

		internal void OnSaveAsFile(EditorBase editor)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			bool? result = dialog.ShowDialog(Application.Current.MainWindow);

			if (!result.Value)
				return;

			SaveFileAs(editor, dialog.FileName);
		}

		internal EditorBase SelectedEditor
		{
			get { return _selectedEditor; }
			set
			{
				if (_selectedEditor == value)
					return;

				_selectedEditor = value;

				if (SelectedEditorChanged != null)
					SelectedEditorChanged();

				RegisterOpenedEditor(_selectedEditor);
			}
		}

		internal void SaveFile()
		{
			_selectedEditor.SaveFile();
		}

		internal void SaveAll()
		{
			_openedEditors.ToList().ForEach(x =>
			{
				if (!string.IsNullOrEmpty(x.FilePath))
					x.SaveFile();
			});

			if (AllHeaderChanged != null)
				AllHeaderChanged();
		}

		internal void SaveFileAs(string fileName)
		{
			SaveFileAs(_selectedEditor, fileName);
		}

		internal void SaveFileAs(EditorBase editor, string fileName)
		{
			editor.SaveFileAs(fileName);
			_recentFilesHolder.RegisterPath(fileName);

			if (AllHeaderChanged != null)
				AllHeaderChanged();
		}

		internal void SaveCopyFileAs(string fileName)
		{
			_selectedEditor.SaveCopyAs(fileName);
		}

		void IEditorOwner.OpenEditor(string filePath)
		{
			OpenFile(filePath, false, true);
		}

		private void RegisterOpenedEditor(EditorBase editor)
		{
			if (_recentOpenedEditors.Contains(editor))
				_recentOpenedEditors.Remove(editor);

			_recentOpenedEditors.Insert(0, editor);
		}

		internal void CloseActiveEditor()
		{
			CloseSpecifiedEditor(SelectedEditor);
		}

		public void RefreshActiveTitle()
		{
			if (HeaderChanged != null)
				HeaderChanged();
		}

		internal bool CloseAllEditors()
		{
			for (int index = _openedEditors.Count - 1; index >= 0; index--)
			{
				EditorBase target = _openedEditors[index];

				if (target != SelectedEditor)
				{
					bool isSuccess = CloseSpecifiedEditor(target);

					if (!isSuccess)
						return false;
				}
			}

			return CloseSpecifiedEditor(SelectedEditor);
		}

		internal void CloseAllEditorsButThis()
		{
			CloseAllEditorsButThis(_selectedEditor);
		}

		internal void CloseAllEditorsButThis(EditorBase target)
		{
			for (int index = _openedEditors.Count - 1; index >= 0; index--)
			{
				EditorBase editor = _openedEditors[index];

				if (editor != target && editor != SelectedEditor)
				{
					bool isSuccess = CloseSpecifiedEditor(editor);

					if (!isSuccess)
						return;
				}
			}

			if (SelectedEditor != target)
				CloseSpecifiedEditor(SelectedEditor);
		}

		internal bool CloseSpecifiedEditor(EditorBase editor)
		{
			if (editor == null)
				return true;

			if (editor.HasChanges)
			{
				MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, string.Format("File '{0}' has been changed. Do you want to save it?", editor.FilePath),
									"File has been changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
					editor.SaveFile();
				else if (result == MessageBoxResult.Cancel)
					return false;	
			}

			if (SelectedEditor == editor)
				SelectedEditor = null;

			if (!editor.IsNew)
				_recentFilesHolder.RegisterPath(editor.FilePath);

			editor.FileWrapper.Dispose();
			editor.Dispose();
			_openedEditors.Remove(editor);

			return true;
		}

		internal void SelectEditorCommand(EditorBase arg)
		{
			SelectedEditor = arg;
		}

		internal void ChangeContentType(IFormat arg)
		{
			string fileExtension = Path.GetExtension(_selectedEditor.FileWrapper.FilePath);
			bool isDefaultEditor = arg.Extension.Split('|').Any(x => x.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));

			if (isDefaultEditor)
				_specialEdoitorsHolder.UnregisterPath(_selectedEditor.FileWrapper.FilePath);
			else
				_specialEdoitorsHolder.RegisterPath(_selectedEditor.FileWrapper.FilePath, arg.EditorTypeId);

			EditorBase openedEditor = arg.CreateNewEditor(this, _selectedEditor.FileWrapper);

			_selectedEditor.Dispose();
			_openedEditors[_openedEditors.IndexOf(_selectedEditor)] = openedEditor;

			_selectedEditor = openedEditor;

			if (SelectedEditorChanged != null)
				SelectedEditorChanged();
		}

		internal void Reload()
		{
			_selectedEditor.Reload();
		}

		void IEditorOwner.CloseEditor(EditorBase target)
		{
			CloseSpecifiedEditor(target);
		}

		void IEditorOwner.CloseActiveEditor()
		{
			CloseActiveEditor();
		}
	}
}
