using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace UniversalEditor
{
	public class RecentFilesHolder
	{
		private const string FileName = "recent.files.txt";

		private readonly ObservableCollection<string> _files = new ObservableCollection<string>();

		public ObservableCollection<string> Files
		{
			get { return _files; }
		}

		internal void RegisterPath(string path)
		{
			if (_files.Contains(path))
			{
				_files.Move(_files.IndexOf(path), 0);
			}
			else
			{
				_files.Insert(0, path);
				
				while (_files.Count > 15)
					_files.RemoveAt(15);
			}

			Save();
		}

		private RecentFilesHolder()
		{}

		internal static RecentFilesHolder Load()
		{
			string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string filePath = Path.Combine(optionsDir, FileName);

			RecentFilesHolder instance = new RecentFilesHolder();

			if (!File.Exists(filePath))
				return instance;

			Array.ForEach(File.ReadAllLines(filePath), instance._files.Add);

			return instance;
		}

		internal void Save()
		{
			string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string filePath = Path.Combine(optionsDir, FileName);

			if (!Directory.Exists(optionsDir))
				Directory.CreateDirectory(optionsDir);

			File.WriteAllLines(filePath, _files.ToArray());
		}
	}
}
