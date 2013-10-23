using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UniversalEditor.Base;
using UniversalEditor.Base.Mvvm;

namespace UniversalEditor.Modules.OpenedTabModule
{
	public class OpenedTabViewModel
	{
		private readonly DomainModel _model;

		internal OpenedTabViewModel(DomainModel model)
		{
			if (model == null) 
				throw new ArgumentNullException("model");

			_model = model;
		}

		public ObservableCollection<EditorBase> OpenedDocuments
		{
			get { return _model.OpenedEditors; }
		}

		public EditorBase ActiveEditor
		{
			get { return _model.SelectedEditor; }
		}

		public static OpenedTabViewModel Designer
		{
			get { return new OpenedTabViewModel(new DomainModel()); }
		}

		public ICommand SaveCommand
		{
			get { return new SimpleCommand<EditorBase>(OnSave); }
		}

		public ICommand ActivateCommand
		{
			get { return new SimpleCommand<EditorBase>(OnActivate); }
		}

		public ICommand CloseCommand
		{
			get { return new SimpleCommand<EditorBase>(OnClose); }
		}

		private void OnClose(EditorBase editor)
		{
			_model.CloseSpecifiedEditor(editor);
		}

		private void OnActivate(EditorBase editor)
		{
			_model.SelectedEditor = editor;
		}

		private void OnSave(EditorBase editor)
		{
			if (string.IsNullOrEmpty(editor.FilePath))
				_model.OnSaveAsFile(editor);
			else
				editor.SaveFile();
		}
	}
}
