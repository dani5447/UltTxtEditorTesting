using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEditor.Base.Mvvm;

namespace UniversalEditor.Modules.OpenRelativeFileModule
{
	public class OpenRelativeFileViewModel : ViewModelBase
	{
		private readonly OpenRelativeFileModel _model;

		internal OpenRelativeFileViewModel(OpenRelativeFileModel model)
		{
			if (model == null) 
				throw new ArgumentNullException("model");

			_model = model;
			_model.IsOpenChanged += IsOpenChanged;
			_model.FilesChanged += FilesChanged;
		}

		private void FilesChanged()
		{
			RaisePropertyChanged("Files");
		}

		private void IsOpenChanged()
		{
			RaisePropertyChanged("IsOpen");
		}

		public string FileName
		{
			set { _model.FileName = value; }
		}

		public IList Files
		{
			get { return _model.Files; }
		}

		public bool IsOpen
		{
			get { return _model.IsOpen; }
			set { _model.IsOpen = value; }
		}

		public static OpenRelativeFileViewModel Designer
		{
			get { return new OpenRelativeFileViewModel(new OpenRelativeFileModel()); }
		}

		public void OpenSelectedFile()
		{
			_model.OpenSelectedFile();
		}

		public void OpenFile(string fileName)
		{
			_model.OpenFile(fileName);
		}
	}
}
