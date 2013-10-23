using System;
using System.Linq;
using System.Collections.Generic;

namespace UniversalEditor.Modules.NewFileModule
{
	public class NewFileViewModel
	{
		private readonly NewFileModel _model;

		internal NewFileViewModel(NewFileModel model)
		{
			if (model == null) 
				throw new ArgumentNullException("model");

			_model = model;
		}

		public IList<FormatViewModel> AllFormats
		{
			get { return App.DomainModel.Editors.Where(x => !x.Format.IsReadonly).ToArray(); }
		}

		public FormatViewModel CurrentFormat
		{
			get { return AllFormats.FirstOrDefault(x => x.Format == _model.CurrentFormat); }
			set { _model.CurrentFormat = value != null ? value.Format : null; }
		}

		public static NewFileViewModel Designer
		{
			get { return new NewFileViewModel(new NewFileModel()); }
		}
	}
}
