using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Archive
{
	public class ArchiveEditor : EditorBase
	{
		private readonly Lazy<ArchiveModel> _model;
		private readonly Lazy<ArchiveViewModel> _viewModel;
		private readonly Lazy<ArchiveView> _view;

		public ArchiveEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_model = new Lazy<ArchiveModel>(CreateModel);
			_viewModel = new Lazy<ArchiveViewModel>(CreateViewModel);
			_view = new Lazy<ArchiveView>(CreateView);
		}

		private ArchiveViewModel CreateViewModel()
		{
			return new ArchiveViewModel(_model.Value);
		}

		private ArchiveView CreateView()
		{
			return new ArchiveView { DataContext = _viewModel.Value };
		}

		private ArchiveModel CreateModel()
		{
			return new ArchiveModel(_file);
		}

		public override bool HasEditorCommands
		{
			get { return true; }
		}

		public override IList<Control> EditorCommands
		{
			get
			{
				return new Control[]
				{
					new MenuItem { Header = "Extract Selected Items...", Command = _viewModel.Value.ExtractCommand },
					new MenuItem { Header = "Extract All", Command = _viewModel.Value.ExtractAllCommand }
				};
			}
		}

		public override void Dispose()
		{
			if (_model.IsValueCreated)
				_model.Value.Dispose();

			base.Dispose();
		}

		public override FrameworkElement EditorControl
		{
			get { return _view.Value; }
		}
    }
}
