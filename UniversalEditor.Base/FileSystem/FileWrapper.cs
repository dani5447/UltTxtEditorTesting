using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace UniversalEditor.Base.FileSystem
{
	public class FileWrapper
	{
		public event ThreadStart ContentChanged;
		public event ThreadStart FileDeleted;
		public event ThreadStart HasChangesChanged;

		private readonly FileSystemWatcher _fileWatcher = new FileSystemWatcher();
		private readonly Timer _timer;

		private string _filePath = string.Empty;
		private bool _isNew;
		private bool _hasChanges = false;
//		private StringBuilder _fileContent = null;

		private bool _isFileLoaded = false;
		private readonly List<string> _fileLines = new List<string>(100000);

		public bool HasChanges
		{
			get { return _hasChanges; }
			private set
			{
				if (_hasChanges == value)
					return;

				_hasChanges = value;

				if (HasChangesChanged != null)
					HasChangesChanged();
			}
		}

		public void SetIsChangedToTrue()
		{
			HasChanges = true;
		}

		public string FileSource
		{
			get { return _filePath; }
		}

		public string FilePath
		{
			get { return _isNew ? string.Empty : _filePath; }
		}

		public bool IsNew
		{
			get { return _isNew; }
		}

		public bool CanBeXml
		{
			get
			{
				if (!File.Exists(FilePath))
					return false;

				using (FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						char[] symbol = new char[1];
						while (!reader.EndOfStream)
						{
							reader.Read(symbol, 0, 1);

							char ch = symbol[0];

							if (ch == '<')
								return true;

							if (ch != ' ' && ch != '\t' && ch != '\n' && ch != '\r')
								return false;
						}

						return false;
					}
				}
			}
		}

		public FileWrapper(string filePath, bool isNew)
		{
			_timer = new Timer(OnFileChangedRaised);

			_filePath = filePath;
			_isNew = isNew;
			_hasChanges = isNew;

			if (!string.IsNullOrEmpty(filePath) && !isNew)
				InitializeWatcher();
		}

		private void OnFileChangedRaised(object state)
		{
			if (ContentChanged != null)
				ContentChanged();
		}

		private void OnFileDeleted(object sender, FileSystemEventArgs e)
		{
			if (FileDeleted != null)
				FileDeleted();
		}

		private void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			if (e.OldName == Path.GetFileName(_filePath))
			{
				if (FileDeleted != null)
					FileDeleted();
			}
			else
			{
				if (ContentChanged != null)
					ContentChanged();
			}
		}

		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			_timer.Change(500, int.MaxValue);
		}

		public string GetLine(int index)
		{
			if (!_isFileLoaded)
				LoadFileContent();

			return _fileLines[index];
		}

		public void SetLine(int index, string text)
		{
			if (!_isFileLoaded)
				LoadFileContent();

			if (index >= _fileLines.Count)
				_fileLines.AddRange(Enumerable.Repeat(string.Empty, index - _fileLines.Count + 1));

			_fileLines[index] = text;
		}

		public void ClearLines()
		{
			if (!_isFileLoaded)
				LoadFileContent();

			_fileLines.Clear();
		}

		public int GetLinesCount()
		{
			if (!_isFileLoaded)
				LoadFileContent();

			return _fileLines.Count;
		}

		private void LoadFileContent()
		{
			_fileLines.Clear();

			if (!string.IsNullOrEmpty(_filePath))
			{
				using (FileStream stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						while (!reader.EndOfStream)
							_fileLines.Add(reader.ReadLine());
					}
				}
			}

			_isFileLoaded = true;
		}
		
		public void Save()
		{
			if (!_isFileLoaded)
				return;

			_fileWatcher.EnableRaisingEvents = false;
			File.WriteAllLines(_filePath, _fileLines);
			_fileWatcher.EnableRaisingEvents = true;

			_isNew = false;
			HasChanges = false;
		}

		public void Dispose()
		{
			_fileWatcher.Dispose();
		}

		private void InitializeWatcher()
		{
			_fileWatcher.Path = Path.GetDirectoryName(_filePath);
			_fileWatcher.Filter = Path.GetFileName(_filePath);
			_fileWatcher.Changed += OnFileChanged;
			_fileWatcher.Created += OnFileChanged;
			_fileWatcher.Deleted += OnFileDeleted;
			_fileWatcher.Renamed += OnFileRenamed;
			_fileWatcher.EnableRaisingEvents = true;
		}

		private void UnitializeWatcher()
		{
			_fileWatcher.EnableRaisingEvents = false;
			_fileWatcher.Changed -= OnFileChanged;
			_fileWatcher.Created -= OnFileChanged;
			_fileWatcher.Deleted -= OnFileDeleted;
			_fileWatcher.Renamed -= OnFileRenamed;
		}

		public void SaveAs(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			if (!string.IsNullOrEmpty(_filePath) && !_isNew)
				UnitializeWatcher();

			_filePath = filePath;

			string dir = Path.GetDirectoryName(_filePath);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			if (!_isFileLoaded)
				LoadFileContent();

			File.WriteAllLines(_filePath, _fileLines);
			
			InitializeWatcher();

			_isNew = false;
			HasChanges = false;
		}

		public void SetFilePath(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			if (!string.IsNullOrEmpty(_filePath) && !_isNew)
				UnitializeWatcher();

			_filePath = filePath;

			InitializeWatcher();
		}

		public void SaveCopyAs(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			string dir = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			if (!_isFileLoaded)
				LoadFileContent();

			File.WriteAllLines(filePath, _fileLines);
		}

		public void Reload()
		{
			_isFileLoaded = false;
			HasChanges = false;
		}
	}
}
