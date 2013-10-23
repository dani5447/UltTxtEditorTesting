using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversalEditor.Modules.OpenRelativeFileModule
{
	internal class OpenRelativeFileModel
	{
		internal event Action IsOpenChanged;
		internal event Action FilesChanged;

		private bool _isOpen;
		private string _folder = string.Empty;
		private string _fileName = string.Empty;
		private string[] _files = new string[0];

		internal bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				if (_isOpen == value)
					return;

				_isOpen = value;

				if (IsOpenChanged != null)
					IsOpenChanged();
			}
		}

		public string FileName
		{
			get { return _fileName; }
			set
			{
				if (_fileName == value)
					return;

				_fileName = value;
				UpdateFilesList();
			}
		}

		public string[] Files
		{
			get { return _files; }
		}

		internal string Folder
		{
			get { return _folder; }
			set
			{
				if (_folder.Equals(value, StringComparison.InvariantCultureIgnoreCase))
					return;

				_folder = value;
				UpdateFilesList();
			}
		}

		private static string GetFileNameMask(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return "*";

			StringBuilder result = new StringBuilder();
			result.Append('*');

			for (int index = 0; index < fileName.Length; index++)
			{
				char ch = fileName[index];
				
				if (Char.IsUpper(ch))
					result.Append("*");

				result.Append(ch);
			}

			result.Append('*');
			return result.ToString();
		}

		private void UpdateFilesList()
		{
			List<string> files = new List<string>();
			
			if (!string.IsNullOrEmpty(_folder) && Directory.Exists(_folder))
			{
				files.AddRange(Directory.GetFileSystemEntries(_folder, GetFileNameMask(_fileName), SearchOption.TopDirectoryOnly).Where(File.Exists).Select(Path.GetFileName));
				files.Sort();
			}

			if (_files.SequenceEqual(files))
				return;

			_files = files.ToArray();

			if (FilesChanged != null)
				FilesChanged();
		}

		internal void OpenSelectedFile()
		{
			OpenFile(_fileName);
		}

		internal void OpenFile(string fileName)
		{
			if (string.IsNullOrEmpty(_folder) || string.IsNullOrEmpty(fileName))
				return;

			string path = Path.Combine(_folder, fileName);

			if (File.Exists(path))
			{
				App.DomainModel.Model.OpenFile(path, 0, false, true, true, null);
				IsOpen = false;
			}
		}
	}
}
