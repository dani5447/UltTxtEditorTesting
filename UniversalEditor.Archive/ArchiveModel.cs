using SevenZip;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor.Archive
{
	internal class ArchiveModel : IDisposable
	{
		private readonly FileWrapper _file;
		private readonly SevenZipExtractor _extractor;

		public ArchiveModel(FileWrapper file)
		{
			_file = file;

			try
			{
				_extractor = new SevenZipExtractor(_file.FilePath);
			}
			catch (Exception exception)
			{
				Exception newOne = new Exception("Cannot open the file as archive.", exception);
				Logger.Log(newOne);
			}
		}

		internal ReadOnlyCollection<ArchiveFileInfo> FilesNames
		{
			get { return _extractor != null ? _extractor.ArchiveFileData : new ReadOnlyCollection<ArchiveFileInfo>(new ArchiveFileInfo[0]); }
		}

		public void Dispose()
		{
			if (_extractor != null)
				_extractor.Dispose();
		}

		public void Extract(ArchiveFileInfo[] archiveFileInfos, string dir)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			for (int index = 0; index < archiveFileInfos.Length; index++)
			{
				ArchiveFileInfo info = archiveFileInfos[index];

				if (info.IsDirectory)
				{
					Extract(_extractor.ArchiveFileData.Where(x => x.FileName.StartsWith(info.FileName + @"\")).ToArray(), 
						Path.Combine(dir, new DirectoryInfo(info.FileName).Name));
				}
				else
				{
					using (FileStream file = File.Create(Path.Combine(dir, Path.GetFileName(info.FileName))))
					{
						_extractor.ExtractFile(info.Index, file);
					}
				}
			}
		}
	}
}
