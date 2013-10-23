using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SevenZip;
using UniversalEditor.Archive.Utils;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Utils.LoggerModule;
using WinForms.Utils.Utils;

namespace UniversalEditor.Archive
{
	public class ArchiveViewModel : ViewModelBase
	{
		private readonly ArchiveModel _model;
		
		private Lazy<IList<ArchiveFileInfoViewModel>> _filesNames;
		private string _currentFolder = string.Empty;

		internal ArchiveViewModel(ArchiveModel model)
		{
			_model = model;
			_filesNames = new Lazy<IList<ArchiveFileInfoViewModel>>(EstimateFilesNames);
		}

		public IList<ArchiveFileInfoViewModel> FilesNames
		{
			get { return _filesNames.Value; }
		}

		private IList<ArchiveFileInfoViewModel> EstimateFilesNames()
		{
			ReadOnlyCollection<ArchiveFileInfo> allEntities = _model.FilesNames;
			List<ArchiveFileInfoViewModel> entities = new List<ArchiveFileInfoViewModel>();
			List<string> folders = new List<string>();

			for (int index = 0; index < allEntities.Count; index++)
			{
				ArchiveFileInfo info = allEntities[index];

				string directoryName = info.FileName.Contains(@"\") ? info.FileName.Substring(0, info.FileName.LastIndexOf(@"\")) : string.Empty;
				if (directoryName == _currentFolder)
				{
					entities.Add(new ArchiveFileInfoViewModel(info, false));
				}
				else if (info.FileName.StartsWith(_currentFolder, StringComparison.InvariantCultureIgnoreCase))
				{
					string temp = info.FileName.Substring(_currentFolder.Length).TrimStart('\\').TrimEnd('\\');
					if (temp.Contains(@"\"))
					{
						string dirName = temp.Substring(0, temp.IndexOf(@"\", StringComparison.InvariantCultureIgnoreCase));

						if (!folders.Contains(dirName))
							folders.Add(dirName);
					}
				}
			}

			for (int index = 0; index < folders.Count; index++)
			{
				string folderName = folders[index];

				if (entities.Exists(x => x.Info.FileName.EndsWith(folderName, StringComparison.InvariantCultureIgnoreCase)))
					continue;

				entities.Add(new ArchiveFileInfoViewModel(new ArchiveFileInfo { FileName = Path.Combine(_currentFolder, folderName), IsDirectory = true }, false));
			}

			if (!string.IsNullOrEmpty(_currentFolder))
			{
				string path = _currentFolder.Contains(@"\") ? _currentFolder.Substring(0, _currentFolder.LastIndexOf('\\')) : string.Empty;
				entities.Insert(0, new ArchiveFileInfoViewModel(new ArchiveFileInfo { FileName = path, IsDirectory = true }, true));
			}

			return entities.OrderBy(x => !x.IsDirectory).ToArray();
		}

		public static ArchiveViewModel Designer
		{
			get { return new ArchiveViewModel(new ArchiveModel(new FileWrapper(string.Empty, true))); }
		}

		public SimpleCommand ExtractCommand
		{
			get { return new SimpleCommand(OnExtract); }
		}

		public ICommand ExtractAllCommand
		{
			get { return new SimpleCommand(OnExtractAll); }
		}
		
		private void OnExtract()
		{
			IList<ArchiveFileInfoViewModel> files = FilesNames.Where(x => x.IsSelected).ToArray();

			if (files.Count == 0)
				return;

			string dir = DialogHelper.SelectFolder("Select folder to extract the files.");

			if (string.IsNullOrEmpty(dir))
				return;

			_model.Extract(files.Where(x => !x.IsParentDir).Select(x => x.Info).ToArray(), dir);
		}

		private void OnExtractAll()
		{
			IList<ArchiveFileInfoViewModel> files = FilesNames;

			if (files == null || files.Count == 0)
				return;

			string dir = DialogHelper.SelectFolder("Select folder to extract the files.");

			if (string.IsNullOrEmpty(dir))
				return;

			_model.Extract(files.Where(x => !x.IsParentDir).Select(x => x.Info).ToArray(), dir);
		}

		internal void OpenDirectory(string fullFileName)
		{
			_currentFolder = fullFileName;
			_filesNames = new Lazy<IList<ArchiveFileInfoViewModel>>(EstimateFilesNames);

			RaisePropertyChanged("FilesNames");
		}
	}

	public class ArchiveFileInfoViewModel
	{
		private readonly ArchiveFileInfo _info;
		private readonly bool _isParentDir;

		public ArchiveFileInfoViewModel(ArchiveFileInfo info, bool isParentDir)
		{
			_info = info;
			_isParentDir = isParentDir;
		}

		public string FullFileName
		{
			get { return _info.FileName; }
		}

		public string Crc
		{
			get { return _info.IsDirectory ? string.Empty : _info.Crc.ToString("X"); }
		}

		public string Size
		{
			get { return _info.IsDirectory ? string.Empty : _info.Size.ToString(); }
		}

		public string LastWriteTime
		{
			get { return _isParentDir ? string.Empty : _info.LastWriteTime.ToString(); }
		}

		public bool IsParentDir
		{
			get { return _isParentDir; }
		}

		public bool IsSelected { get; set; }

		public ImageSource Icon
		{
			get
			{
				if (IsDirectory && IsParentDir)
					return IconReader.GetFolderIcon(IconReader.IconSize.Small, IconReader.FolderType.Open).ToImageSource();

				if (IsDirectory)
					return IconReader.GetFolderIcon(IconReader.IconSize.Small, IconReader.FolderType.Closed).ToImageSource();

				return IconReader.GetFileIcon(FullFileName, IconReader.IconSize.Small, false).ToImageSource();
			}
		}

		public string DisplayName
		{
			get
			{
				return _isParentDir ? "..." : _info.FileName.Contains(@"\") ? _info.FileName.Substring(_info.FileName.LastIndexOf('\\') + 1) : _info.FileName;
			}
		}

		public bool IsDirectory
		{
			get { return _info.IsDirectory; }
		}

		public ArchiveFileInfo Info
		{
			get { return _info; }
		}
	}

	public class ArchiveFileInfoViewModelTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null && item is ArchiveFileInfoViewModel)
			{
				ArchiveFileInfoViewModel infoViewModel = item as ArchiveFileInfoViewModel;

				if (infoViewModel.IsDirectory)
					return element.FindResource("DirectoryTemplate") as DataTemplate;
				
				return element.FindResource("FileTemplate") as DataTemplate;
			}

			return null;
		}
	}

}
