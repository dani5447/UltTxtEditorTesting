using System.Windows.Input;
using System.Windows.Media;
using Raccoom.Xml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using UniversalEditor.Base;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const string WindowPositionFileName = "window.position.xml";
		private const string DocumentListFileName = "opened.documents.xml";

		public MainWindow()
		{
			DataContextChanged += OnDataContextChanged;
			InitializeComponent();
		}
		
		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			DomainViewModel viewModel = (DomainViewModel) DataContext;
			viewModel.OpenedEditors.CollectionChanged += OnCollectionChanged;
			viewModel.Model.SelectedEditorChanged += OnModelSelectedEditorChanged;
			
		}
		
		private void OnModelSelectedEditorChanged()
		{
			DomainViewModel viewModel = (DomainViewModel)DataContext;
			EditorBase tabSelected = tabControl.SelectedItem == null ? null : (EditorBase)((TabItem)tabControl.SelectedItem).Tag;

			if (viewModel.SelectedEditor == tabSelected)
				return;

			if (viewModel.SelectedEditor == null)
			{
				tabControl.SelectedItem = tabControl.Items.OfType<TabItem>().LastOrDefault();
				return;
			}

			tabControl.SelectedItem = tabControl.Items.OfType<TabItem>().First(x => x.Tag == viewModel.SelectedEditor);
		}
		
		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DomainViewModel viewModel = (DomainViewModel)DataContext;
			ObservableCollection<EditorBase> collection = (ObservableCollection<EditorBase>)sender;

			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				EditorBase oldEditor = (EditorBase)e.OldItems[0];
				EditorBase newEditor = (EditorBase)e.NewItems[0];

				TabItem tabItem = tabControl.Items.OfType<TabItem>().First(x => x.Tag == oldEditor);
				tabItem.Tag = newEditor;
				tabItem.Content = newEditor.EditorControl;
				
				return;
			}

			if (e.OldItems != null)
			{
				// remove
				EditorBase[] editors = e.OldItems.OfType<EditorBase>().ToArray();

				for (int index = 0; index < editors.Length; index++)
				{
					EditorBase editor = editors[index];
					tabControl.Items.RemoveAt(tabControl.Items.OfType<TabItem>().ToList().FindIndex(x => x.Tag == editor));
				}
			}

			if (e.NewItems != null)
			{
				// add
				EditorBase[] editors = e.NewItems.OfType<EditorBase>().ToArray();

				for (int index = 0; index < editors.Length; index++)
				{
					EditorBase editor = editors[index];
					TabItem tabItem = new TabItem
					{
						Tag = editor,
						Header = new EditorBaseViewModel(viewModel, editor)
					};

					tabControl.Items.Insert(e.NewStartingIndex + index, tabItem);
					tabItem.UpdateLayout();
				}
			}

			if (tabControl.SelectedItem == null)
				tabControl.SelectedItem = tabControl.Items.OfType<TabItem>().LastOrDefault();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (!e.Handled)
			{
				DomainViewModel viewModel = (DomainViewModel) DataContext;
				if (viewModel.SelectedEditor != null)
				{
					viewModel.SelectedEditor.OnKeyDown(e);
				}
			}

			base.OnKeyDown(e);
		}

		protected override void OnTextInput(TextCompositionEventArgs e)
		{
			if (!e.Handled)
			{
				DomainViewModel viewModel = (DomainViewModel)DataContext;
				if (viewModel.SelectedEditor != null)
				{
					viewModel.SelectedEditor.OnTextInput(e);
				}
			}

			base.OnTextInput(e);
		}

		protected override void OnInitialized(EventArgs e)
		{
			LoadWindowPosition();

			base.OnInitialized(e);

			LoadOpenedDocumentsList();

			if (!string.IsNullOrEmpty(((App) Application.Current).StartFilePath))
				App.DomainModel.Model.OpenFile(((App)Application.Current).StartFilePath, false, true);

			Thread updateThread = new Thread(() =>
			{
				try
				{
					const string appKey = "Universal File Editor";

					Uri uri = new Uri("http://universalfileeditor.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2frelease%2funiversalfileeditor");
					RssChannel myRssChannel = new RssChannel(uri);

					for (int index = 0; index < myRssChannel.Items.Count; index++)
					{
						RssItem item = myRssChannel.Items[index];
						string title = item.Title;

						if (!title.Contains(appKey + " 0.4"))
							continue;

						string appName = title.Substring(title.IndexOf(appKey));
						string version = appName.Substring(appKey.Length);
						version = version.Substring(0, version.IndexOf("(")).Trim();

						if (version == Assembly.GetExecutingAssembly().GetName().Version.ToString())
							return;

						Application.Current.Dispatcher.Invoke(() =>
						{
							MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "The newer version of this program is available. Do you want to download it?", "Update",
																	  MessageBoxButton.OKCancel, MessageBoxImage.Information);

							if (result == MessageBoxResult.OK)
								Process.Start(item.Link);
						});

						return;
					}
				}
				catch { }
			});

			if (Options.Instance.CheckUpdate)
			{
				updateThread.IsBackground = true;
				updateThread.SetApartmentState(ApartmentState.STA);
				updateThread.Start();
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			bool isSuccess = SaveEditedFiles();

			if (!isSuccess)
			{
				e.Cancel = true;
				return;
			}

			SaveWindowPosition();
			SaveOpenedDocumentsList();

			Options.Instance.Save();

			base.OnClosing(e);
		}

		private bool SaveEditedFiles()
		{
			DomainViewModel viewModel = (DomainViewModel)DataContext;

			for (int index = 0; index < viewModel.OpenedEditors.Count; index++)
			{
				EditorBase editor = viewModel.OpenedEditors[index];

				if (string.IsNullOrEmpty(editor.FilePath))
					continue;

				if (!editor.HasChanges)
					continue;

				MessageBoxResult result = MessageBox.Show(this, string.Format("File '{0}' has been changed. Do you want to save it?", editor.FilePath),
				                "File has been changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
					editor.SaveFile();
				else if (result == MessageBoxResult.No)
					continue;
				else
					return false;
			}

			return true;
		}

		private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tabControl.SelectedItem == null)
				return;

			TabItem tabItem = (TabItem) tabControl.SelectedItem;
			EditorBase editorBase = (EditorBase)tabItem.Tag;

			DomainViewModel viewModel = (DomainViewModel)DataContext;

			bool isFirstView = tabItem.Content == null;

			tabItem.Content = editorBase.EditorControl;
			viewModel.SelectedEditor = editorBase;
			
			viewModel.SelectedEditor.Focus();

			if (isFirstView)
				editorBase.OnFirstView();
		}

		private void SaveWindowPosition()
		{
			XDocument doc = new XDocument(
				new XElement("Window",
					new XElement("Left", Left),
					new XElement("Top", Top),
					new XElement("Height", Height),
					new XElement("Width", Width),
					new XElement("Maximize", WindowState == WindowState.Maximized)));

			string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor"), WindowPositionFileName);

			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			doc.Save(filePath);
		}

		private void LoadWindowPosition()
		{
			string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor"), WindowPositionFileName);

			if (!File.Exists(filePath))
				return;

			XDocument doc = XDocument.Load(filePath);

			WindowState = Convert.ToBoolean(doc.Root.Element("Maximize").Value) ? WindowState.Maximized : WindowState.Normal;
			Left = Convert.ToDouble(doc.Root.Element("Left").Value);
			Top = Convert.ToDouble(doc.Root.Element("Top").Value);
			Height = Convert.ToDouble(doc.Root.Element("Height").Value);
			Width = Convert.ToDouble(doc.Root.Element("Width").Value);
		}

		private void SaveOpenedDocumentsList()
		{
			string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string newFilesHolderDir = Path.Combine(appDir, "NewFiles");

			if (Directory.Exists(newFilesHolderDir))
				Directory.Delete(newFilesHolderDir, true);

			DomainViewModel viewModel = (DomainViewModel)DataContext;
			XDocument doc = new XDocument(new XElement("Documents"));

			for (int index = 0; index < viewModel.OpenedEditors.Count; index++)
			{
				EditorBase editor = viewModel.OpenedEditors[index];
				
				XElement file = new XElement("Document");
				file.Add(new XElement("Path", editor.FilePath));
				file.Add(new XElement("IsSelected", editor == viewModel.SelectedEditor));

				bool isNewFile = string.IsNullOrEmpty(editor.FilePath);

				file.Add(new XElement("IsNew", isNewFile));

				if (isNewFile)
				{
					if (!Directory.Exists(newFilesHolderDir))
						Directory.CreateDirectory(newFilesHolderDir);

					string tempFilePath = Path.Combine(newFilesHolderDir, Path.GetRandomFileName());
					string extension = App.DomainModel.Editors.First(x => x.Format.IsChild(editor)).Format.Extension.Split('|')[0];

					tempFilePath = Path.ChangeExtension(tempFilePath, extension);

					editor.SaveCopyAs(tempFilePath);
					file.Add(new XElement("NewFilePath", tempFilePath));
				}

				editor.SaveStatus(file);

				doc.Root.Add(file);
			}


			string filePath = Path.Combine(appDir, DocumentListFileName);

			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			doc.Save(filePath);
		}

		private void LoadOpenedDocumentsList()
		{
			try
			{
				string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor"), DocumentListFileName);

				if (!File.Exists(filePath))
					return;

				XDocument doc = XDocument.Load(filePath);

				if (doc.Root.Element("document") != null)
				{
					foreach (XElement docTag in doc.Root.Elements("document"))
					{
						string documentPath = docTag.Value;

						if (string.IsNullOrEmpty(documentPath) || !File.Exists(documentPath))
							continue;

						App.DomainModel.Model.OpenFile(documentPath, false, false);
					}
				}
				else
				{
					var files = (from docTag in doc.Root.Elements("Document")
								 let isNew = bool.Parse(docTag.Element("IsNew").Value)
								 select new
									{
										Path = docTag.Element("Path").Value,
										IsNew = isNew,
										IsSelected = bool.Parse(docTag.Element("IsSelected").Value),
										NewFilePath = isNew ? docTag.Element("NewFilePath").Value : string.Empty,
										Tag = docTag
									})
								.ToArray();

					int selectedEditorIndex = -1;

					for (int index = 0; index < files.Length; index++)
					{
						var fileItem = files[index];

						try
						{
							if (!fileItem.IsSelected)
								continue;

							string path = fileItem.IsNew ? fileItem.NewFilePath : fileItem.Path;

							if (string.IsNullOrEmpty(path) || !File.Exists(path))
								break;

							selectedEditorIndex = index;
							App.DomainModel.Model.OpenFile(path, fileItem.IsNew, true, false, fileItem.Tag);

							break;
						}
						catch (Exception)
						{
							//	Logger.Log(exception);
						}
					}

					for (int index = 0; index < files.Length; index++)
					{
						if (index == selectedEditorIndex)
							continue;

						var fileItem = files[index];

						try
						{
							string path = fileItem.IsNew ? fileItem.NewFilePath : fileItem.Path;

							if (string.IsNullOrEmpty(path) || !File.Exists(path))
								continue;

							EditorBase editor;

							if (index < selectedEditorIndex)
								editor = App.DomainModel.Model.OpenFile(path, App.DomainModel.Model.OpenedEditors.Count - 1, fileItem.IsNew, false, false, fileItem.Tag);
							else
								editor = App.DomainModel.Model.OpenFile(path, App.DomainModel.Model.OpenedEditors.Count, fileItem.IsNew, false, false, fileItem.Tag);

							if (fileItem.IsNew)
								editor.EditorControl.ToString();
						}
						catch (Exception)
						{
							//	Logger.Log(exception);
						}
					}
				}
			}
			catch (Exception)
			{
//				Logger.Log(exception);
			}
		}

		private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
		{
			bool hasData = e.Data.GetDataPresent("FileDrop");

			if (!hasData)
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
			}
		}

		private void MainWindow_OnDrop(object sender, DragEventArgs e)
		{
			string[] filesPathes = (string[])e.Data.GetData("FileDrop");

			for (int index = 0; index < filesPathes.Length; index++)
				App.DomainModel.Model.OpenFile(filesPathes[index], false, true, true, null);
		}

		private void TabControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
//			if (e.IsUp && e.Key == Key.LeftCtrl)
//			{
//				e.Handled = true;
//				App.DomainModel.ShowTabSwitch(false);
//			}
//			else 
//			if (e.Key == Key.Tab && tabSwitcher.IsOpen)
//			{
//				e.Handled = true;
//				App.DomainModel.ShowNextTabSwitch(); 
//			}
			/*else*/ if (e.Key == Key.Tab && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
			{
				e.Handled = true;
//				tabControl.Focus();
//				App.DomainModel.ShowTabSwitch(true);
//				tabSwitcher.IsOpen = true;
				App.DomainModel.ShowNextTabSwitch(); 
			}
		}

		private void TabControl_OnPreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.LeftCtrl)
			{
				e.Handled = true;
//				tabControl.Focus();
				App.DomainModel.ShowTabSwitch(false);
//				tabSwitcher.IsOpen = false;
			}
		}
	}

	public class IntToVisibilityConverter : IValueConverter
	{
		public static IntToVisibilityConverter Default
		{
			get { return new IntToVisibilityConverter(); }
		}

		public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) 
				return Visibility.Collapsed;
			
			return ((int)value > 1) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new System.NotImplementedException();
		}
	}

	public class TabVisibilityDetector : IMultiValueConverter
	{
		public static TabVisibilityDetector Default
		{
			get { return new TabVisibilityDetector(); }
		}

		public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == null) return Visibility.Visible;
			if (values[0] == null || values[1] == null || values[2] == null) return Visibility.Visible;

			Border child = (Border)values[1];
			ScrollViewer scrollViewer = (ScrollViewer) values[0];
			double viewportWidth = (double)values[2];

			Point origin = child.RenderTransform.Inverse.Transform(new Point(0, 0));
			double position = child.TransformToAncestor(scrollViewer).Transform(origin).X + child.RenderSize.Width;

			return position > viewportWidth ? Visibility.Hidden : Visibility.Visible;
		}

		public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new System.NotImplementedException();
		}
	}

	public class EditorBaseViewModel : ViewModelBase
	{
		private readonly DomainViewModel _viewModel;
		private readonly EditorBase _editor;

		internal EditorBaseViewModel(DomainViewModel viewModel, EditorBase editor)
		{
			if (viewModel == null)
				throw new ArgumentNullException("viewModel");
			if (editor == null)
				throw new ArgumentNullException("editor");

			_viewModel = viewModel;
			_editor = editor;

			viewModel.Model.HeaderChanged += () => RaisePropertyChanged("FileName");
			viewModel.Model.AllHeaderChanged += () => RaisePropertyChanged("FileName");
		}

		public EditorBase Editor
		{
			get { return _editor; }
		}

		public string FileName
		{
			get { return _editor.FileName; }
		}

		public string FilePath
		{
			get { return _editor.FilePath; }
		}

		public ICommand CopyFileNameCommand
		{
			get { return new SimpleCommand(CopyFileName); }
		}

		private void CopyFileName()
		{
			if (string.IsNullOrEmpty(_editor.FilePath))
				Clipboard.SetText("New File");
			else
				Clipboard.SetText(Path.GetFileName(_editor.FilePath));
		}

		public ICommand CopyFilePathCommand
		{
			get { return new SimpleCommand(CopyFilePath); }
		}

		public ICommand CopyFileDirectoryCommand
		{
			get { return new SimpleCommand(CopyFileDirectory); }
		}

		private void CopyFilePath()
		{
			if (string.IsNullOrEmpty(_editor.FilePath))
				Clipboard.SetText("New File");
			else
				Clipboard.SetText(_editor.FilePath);
		}

		private void CopyFileDirectory()
		{
			if (string.IsNullOrEmpty(_editor.FilePath))
				Clipboard.SetText("New File");
			else
				Clipboard.SetText(Path.GetDirectoryName(_editor.FilePath));
		}

		public ICommand OpenFileLocationCommand
		{
			get { return new SimpleCommand(() => _viewModel.OpenFileLocation(_editor)); }
		}

		public ICommand CloseAllButThisCommand
		{
			get { return _viewModel.CloseAllButThisCommand; }
		}

		public ICommand CloseAllCommand
		{
			get { return _viewModel.CloseAllCommand; }
		}

		public ICommand RealoadCommand
		{
			get { return new SimpleCommand(_editor.Reload); }
		}

		public ICommand SaveCommand
		{
			get { return new SimpleCommand(() => _viewModel.OnSaveFile(_editor)); }
		}

		public ICommand SaveAllCommand
		{
			get { return _viewModel.SaveAllCommand; }
		}

		public ICommand SaveAsCommand
		{
			get { return new SimpleCommand(() => _viewModel.Model.OnSaveAsFile(_editor)); }
		}

		public ICommand SaveCopyAsCommand
		{
			get { return new SimpleCommand(() => _viewModel.OnSaveCopyAsFile(_editor)); }
		}

		public ICommand CloseCommand
		{
			get { return _viewModel.CloseCommand; }
		}
	}

	public class WindowsMenuItemStyleSelector : StyleSelector
	{
		public override Style SelectStyle(object item, DependencyObject container)
		{
			if (item is EditorBase)
				return (Style)Application.Current.MainWindow.Resources["OpenedTabMenuItemDefaultStyle"];
			if (item is Separator)
				return (Style)Application.Current.MainWindow.Resources["OpenedTabMenuItemSeparatorStyle"];
			if (item is Tuple<string, ICommand>)
				return (Style)Application.Current.MainWindow.Resources["OpenedTabMenuItemCommandStyle"];

			return base.SelectStyle(item, container);
		}
	}
}
