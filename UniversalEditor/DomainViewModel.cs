using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using Raccoom.Xml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using UniversalEditor.Base;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Utils.LoggerModule;
using UniversalEditor.Formats;
using UniversalEditor.Modules.NewFileModule;
using UniversalEditor.Modules.OpenRelativeFileModule;
using UniversalEditor.Modules.OpenedTabModule;
using UniversalEditor.Modules.TabSwitchModule;
using UniversalEditor.OptionsModule;

namespace UniversalEditor
{
	public class DomainViewModel : ViewModelBase
	{
		private readonly DomainModel _model;
		private readonly OpenRelativeFileModel _openRelativeFileModel;
		private readonly OpenedTabViewModel _openedTabViewModel;
		private readonly TabSwitchViewModel _tabSwitchViewModel;

		private readonly SimpleCommand _saveCommand;
		private readonly SimpleCommand _saveAllCommand;
		private readonly SimpleCommand _saveAsCommand;
		private readonly SimpleCommand _saveCopyAsCommand;
		private readonly SimpleCommand _reloadCommand;
		private readonly SimpleCommand _closeActiveCommand;
		private readonly SimpleCommand<EditorBase> _closeCommand;
		private readonly SimpleCommand _closeAllCommand;
		private readonly SimpleCommand<EditorBase> _closeAllButThisCommand;
		private readonly SimpleCommand _closeAllButActiveCommand;

		private readonly FormatViewModel[] _editors;

		internal DomainViewModel(DomainModel model)
		{
			if (model == null) 
				throw new ArgumentNullException("model");

			_model = model;
			_model.SelectedEditorChanged += OnSelectedEditorChanged;
			_model.HeaderChanged += OnHeaderChanged;
			_model.AllHeaderChanged += OnHeaderChanged;
			_model.OpenedEditors.CollectionChanged += OnOpenedEditorsCollectionChanged;

			_openRelativeFileModel = new OpenRelativeFileModel();
			_openedTabViewModel = new OpenedTabViewModel(_model);
			_tabSwitchViewModel = new TabSwitchViewModel(_model);

			_saveCommand = new SimpleCommand(OnSaveFile, () => HasOpenedEditor);
			_saveAllCommand = new SimpleCommand(_model.SaveAll, () => HasOpenedEditor);
			_saveAsCommand = new SimpleCommand(OnSaveAsFile, () => HasOpenedEditor);
			_saveCopyAsCommand = new SimpleCommand(OnSaveCopyAsFile, () => HasOpenedEditor);
			_reloadCommand = new SimpleCommand(_model.Reload, () => HasOpenedEditor);
			_closeActiveCommand = new SimpleCommand(CloseActiveEditor, () => HasOpenedEditor);
			_closeCommand = new SimpleCommand<EditorBase>(CloseEditor, (editor) => true);
			_closeAllCommand = new SimpleCommand(CloseAllEditors, () => HasOpenedEditor);
			_closeAllButThisCommand = new SimpleCommand<EditorBase>(CloseAllEditorsButThis, editor => _model.OpenedEditors.Count > 1);
			_closeAllButActiveCommand = new SimpleCommand(CloseAllEditorsButActive, () => _model.OpenedEditors.Count > 1);

			_editors = _model.Editors.Select(x => new FormatViewModel(x, this)).OrderBy(x => x.DisplayName).ToArray();
		}

		private void OnOpenedEditorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			_closeAllButThisCommand.RiseCanExecuteChanged();
			RaisePropertyChanged("OpenedEditorsCount");
			RaisePropertyChanged("WindowsMenuItemContent");
		}

		public OpenRelativeFileViewModel OpenRelativeFileViewModel
		{
			get { return new OpenRelativeFileViewModel(_openRelativeFileModel); }
		}

		public TabSwitchViewModel TabSwitchViewModel
		{
			get { return _tabSwitchViewModel; }
		}
		
		public ObservableCollection<string> RecentFiles
		{
			get { return _model.RecentFilesHolder.Files; }
		}

		private void CloseEditor(EditorBase editor)
		{
			_model.CloseSpecifiedEditor(editor);
			_closeAllButThisCommand.RiseCanExecuteChanged();
		}

		private void CloseActiveEditor()
		{
			_model.CloseActiveEditor();
			_closeAllButThisCommand.RiseCanExecuteChanged();
		}

		private void CloseAllEditors()
		{
			_model.CloseAllEditors();
			_closeAllButThisCommand.RiseCanExecuteChanged();
		}

		private void CloseAllEditorsButThis(EditorBase editor)
		{
			_model.CloseAllEditorsButThis(editor);

			_closeAllButThisCommand.RiseCanExecuteChanged();
			_closeAllButActiveCommand.RiseCanExecuteChanged();
		}

		private void CloseAllEditorsButActive()
		{
			_model.CloseAllEditorsButThis();

			_closeAllButThisCommand.RiseCanExecuteChanged();
			_closeAllButActiveCommand.RiseCanExecuteChanged();
		}

		private void OnHeaderChanged()
		{
			RaisePropertyChanged("WindowTitle");
		}

		private void OnSelectedEditorChanged()
		{
			_saveCommand.RiseCanExecuteChanged();
			_saveAllCommand.RiseCanExecuteChanged();
			_saveAsCommand.RiseCanExecuteChanged();
			_reloadCommand.RiseCanExecuteChanged();
			_closeCommand.RiseCanExecuteChanged();
			_closeAllCommand.RiseCanExecuteChanged();

			RaisePropertyChanged("WindowTitle");
			RaisePropertyChanged("SelectedEditor");
			RaisePropertyChanged("HasOpenedEditor");

			if (_model.SelectedEditor != null && File.Exists(_model.SelectedEditor.FilePath))
				_openRelativeFileModel.Folder = Path.GetDirectoryName(_model.SelectedEditor.FilePath);
			else
				_openRelativeFileModel.Folder = string.Empty;

			for (int index = 0; index < _editors.Length; index++)
				_editors[index].UpdateIsChecked();
		}

		public ObservableCollection<EditorBase> OpenedEditors
		{
			get { return _model.OpenedEditors; }
		}

		public IList<object> WindowsMenuItemContent
		{
			get
			{
				List<object> items = new List<object>();
				items.AddRange(_model.OpenedEditors);
				items.Add(new Separator());
				items.Add(new Tuple<string, ICommand>("Windows...", ShowWindowsManagerCommand));
				
				return items;
			}
		}

		private ICommand ShowWindowsManagerCommand
		{
			get { return new SimpleCommand(ShowWindowsManager); }
		}

		private void ShowWindowsManager()
		{
			OpenedTabView window = new OpenedTabView { DataContext = _openedTabViewModel };
			window.Owner = Application.Current.MainWindow;
			window.ShowDialog();
		}

		public int OpenedEditorsCount
		{
			get { return _model.OpenedEditors.Count; }
		}

		internal DomainModel Model
		{
			get { return _model; }
		}

		public string WindowTitle
		{
			get { return string.IsNullOrEmpty(_model.FilePath) ? "Universal File Editor" : _model.FilePath; }
		}

		public FormatViewModel[] Editors
		{
			get { return _editors; }
		}

		public ICommand OpenCommand
		{
			get {return new SimpleCommand(OnOpenFile);}
		}

		public ICommand OpenRelativeFileCommand
		{
			get { return new SimpleCommand(OnOpenRelativeFile); }
		}

		public ICommand NewCommand
		{
			get {return new SimpleCommand(CreateNewFile);}
		}
		
		public ICommand SaveCommand
		{
			get { return _saveCommand; }
		}

		public ICommand SaveAllCommand
		{
			get { return _saveAllCommand; }
		}

		public ICommand RealoadCommand
		{
			get { return _reloadCommand; }
		}

		public ICommand SaveAsCommand
		{
			get { return _saveAsCommand; }
		}

		public ICommand SaveCopyAsCommand
		{
			get { return _saveCopyAsCommand; }
		}

		public ICommand CloseCommand
		{
			get { return _closeCommand; }
		}

		public ICommand CloseActiveCommand
		{
			get { return _closeActiveCommand; }
		}

		public ICommand CopyFileNameCommand
		{
			get { return new SimpleCommand(CopyFileName); }
		}

		public ICommand CopyFilePathCommand
		{
			get { return new SimpleCommand(CopyFilePath); }
		}

		public ICommand CopyFileDirectoryCommand
		{
			get { return new SimpleCommand(CopyFileDirectory); }
		}
		
		public ICommand CloseSpecifiedEditorCommand
		{
			get { return new SimpleCommand<EditorBase>(x => _model.CloseSpecifiedEditor(x)); }
		}

		public ICommand CloseAllCommand
		{
			get { return _closeAllCommand; }
		}

		public ICommand OptionsCommand
		{
			get { return new SimpleCommand(OnOptions); }
		}

		private void OnOptions()
		{
			OptionsView view = new OptionsView();
			view.Owner = Application.Current.MainWindow;
			view.ShowDialog();

			if (SelectedEditor != null)
				SelectedEditor.Refresh();
		}

		public ICommand CloseAllButThisCommand
		{
			get { return _closeAllButThisCommand; }
		}

		public ICommand CloseAllButActiveCommand
		{
			get { return _closeAllButActiveCommand; }
		}

		public ICommand ExitCommand
		{
			get { return new SimpleCommand(() => Application.Current.Shutdown()); }
		}

		public ICommand AboutCommand
		{
			get { return new SimpleCommand(() => new AboutBox().ShowDialog()); }
		}

		public ICommand UpdateCommand
		{
			get { return new SimpleCommand(OnCheckUpdate); }
		}

		public ICommand WebsiteCommand
		{
			get { return new SimpleCommand(OnWebsite); }
		}

		private void OnWebsite()
		{
			Process.Start("http://universalfileeditor.codeplex.com");
		}

		public ICommand DocumentationCommand
		{
			get { return new SimpleCommand(OnDocumentation); }
		}

		private void OnDocumentation()
		{
			Process.Start("http://universalfileeditor.codeplex.com/documentation");
		}

		private void OnCheckUpdate()
		{
			try
			{
				Uri uri = new Uri("http://universalfileeditor.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2frelease%2funiversalfileeditor");
				RssChannel myRssChannel = new RssChannel(uri);

				RssItem item = myRssChannel.Items[0];
				string title = item.Title;
				string appKey = "Universal File Editor";


				string appName = title.Substring(title.IndexOf(appKey));

				string version = appName.Substring(appKey.Length);
				version = version.Substring(0, version.IndexOf("(")).Trim();

				if (version == Assembly.GetExecutingAssembly().GetName().Version.ToString())
				{
					MessageBox.Show(App.Current.MainWindow, "You have the latest version of this program.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					Process.Start(item.Link);
				}
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
			}
		}

		public ICommand ContentTypeChangeCommand
		{
			get { return new SimpleCommand<IFormat>(_model.ChangeContentType); }
		}

		public ICommand OpenSpecifiedFileCommand
		{
			get { return new SimpleCommand<string>(OnOpenSpecifiedFile); }
		}

		private void OnOpenSpecifiedFile(string path)
		{
			if (!File.Exists(path))
			{
				MessageBox.Show(Application.Current.MainWindow, string.Format("File not found: '{0}'", path));
				return;
			}

			_model.OpenFile(path, 0, false, true, true, null);
		}

		public ICommand SelectEditorCommand
		{
			get { return new SimpleCommand<EditorBase>(_model.SelectEditorCommand); }
		}

		private void CopyFileName()
		{
			Clipboard.SetText(Path.GetFileName(_model.FilePath));
		}

		private void CopyFilePath()
		{
			Clipboard.SetText(_model.FilePath);
		}

		private void CopyFileDirectory()
		{
			if (SelectedEditor == null || string.IsNullOrEmpty(SelectedEditor.FilePath))
				Clipboard.SetText("New File");
			else
				Clipboard.SetText(Path.GetDirectoryName(SelectedEditor.FilePath));
		}

		internal void OpenFileLocation(EditorBase editor)
		{
			if (string.IsNullOrEmpty(editor.FilePath))
				return;

			Process.Start("explorer.exe", @"/select, " + editor.FilePath);
		}

		public EditorBase SelectedEditor
		{
			get { return _model.SelectedEditor; }
			set
			{
				_model.SelectedEditor = value;
			}
		}

		public bool HasOpenedEditor
		{
			get { return _model.SelectedEditor != null; }
		}
		
//		public ICommand ShowTabSwitchCommand
//		{
//			get {return new SimpleCommand(() => ShowTabSwitch(true));}
//		}
//
//		public ICommand ShowNextTabSwitchCommand
//		{
//			get {return new SimpleCommand(() => ShowNextTabSwitch());}
//		}

		internal void ShowTabSwitch(bool show)
		{
			_tabSwitchViewModel.IsOpen = show;
		}

		internal void ShowNextTabSwitch()
		{
			_tabSwitchViewModel.ShowNext();
		}

		private void OnSaveFile()
		{
			OnSaveFile(_model.SelectedEditor);
		}

		internal void OnSaveFile(EditorBase editor)
		{
			if (string.IsNullOrEmpty(editor.FilePath))
				_model.OnSaveAsFile(editor);
			else
				editor.SaveFile();
		}

		private void OnSaveAsFile()
		{
			_model.OnSaveAsFile(_model.SelectedEditor);
		}

		private void OnSaveCopyAsFile()
		{
			OnSaveCopyAsFile(_model.SelectedEditor);
		}

		internal void OnSaveCopyAsFile(EditorBase editor)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			bool? result = dialog.ShowDialog(Application.Current.MainWindow);

			if (!result.Value)
				return;

			editor.SaveCopyAs(dialog.FileName);
		}

		private void OnOpenRelativeFile()
		{
			if (!string.IsNullOrEmpty(_openRelativeFileModel.Folder))
				_openRelativeFileModel.IsOpen = true;
		}

		private void OnOpenFile()
		{
			string defaultPath = _model.SelectedEditor != null && !_model.SelectedEditor.IsNew ? Path.GetDirectoryName(_model.SelectedEditor.FilePath) : string.Empty;

			OpenFileDialog dialog = new OpenFileDialog();

			if (!string.IsNullOrEmpty(defaultPath))
				dialog.InitialDirectory = defaultPath;

			bool? result = dialog.ShowDialog(Application.Current.MainWindow);

			if (!result.Value)
				return;

			_model.OpenFile(dialog.FileName, 0, false, true, true, null);

			_closeAllButThisCommand.RiseCanExecuteChanged();
		}

		private void CreateNewFile()
		{
			NewFileModel model = new NewFileModel();
			NewFileView window = new NewFileView { DataContext = new NewFileViewModel(model), Owner = App.Current.MainWindow };

			bool? result = window.ShowDialog();
			if (result.HasValue && result.Value && model.CurrentFormat != null)
			{
				_model.CreateNewFile(model.CurrentFormat);
				_closeAllButThisCommand.RiseCanExecuteChanged();
			}
		}
	}

	public class FormatViewModel : ViewModelBase
	{
		private readonly IFormat _format;
		private readonly DomainViewModel _domainViewModel;

		private bool _isChecked;

		public FormatViewModel(IFormat format, DomainViewModel domainViewModel)
		{
			if (format == null) 
				throw new ArgumentNullException("format");
			if (domainViewModel == null) 
				throw new ArgumentNullException("domainViewModel");

			_format = format;
			_domainViewModel = domainViewModel;
		}

		internal void UpdateIsChecked()
		{
			IsChecked = _format.IsChild(_domainViewModel.SelectedEditor);
		}

		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (_isChecked == value)
					return;

				_isChecked = value;
				RaisePropertyChanged("IsChecked");
			}
		}

		public string DisplayName
		{
			get { return _format.DisplayName; }
		}

		public IFormat Format
		{
			get { return _format; }
		}
	}
}
