using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.PlainImage
{
	public class ImageViewerViewModel
	{
		private readonly FileWrapper _file;

		public ImageViewerViewModel(FileWrapper file)
		{
			if (file == null) 
				throw new ArgumentNullException("file");

			_file = file;
		}

		public FileWrapper File
		{
			get { return _file; }
		}
	}
}
