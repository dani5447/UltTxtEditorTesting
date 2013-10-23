using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEditor.Base;
using UniversalEditor.Base.Mvvm;

namespace UniversalEditor.Modules.TabSwitchModule
{
	public class TabSwitchViewModel : ViewModelBase
	{
		private readonly DomainModel _model;
		private readonly EditorByOrderComparer _editorByOrderComparer;

		private bool _isOpen;
		private int _selectionIndex;

		internal TabSwitchViewModel(DomainModel model)
		{
			if (model == null) 
				throw new ArgumentNullException("model");

			_model = model;
			_editorByOrderComparer = new EditorByOrderComparer(_model.RecentOpenedEditors);
		}

		public static TabSwitchViewModel Designer
		{
			get { return new TabSwitchViewModel(new DomainModel()); }
		}

		public IList<EditorBase> OpenedEditors
		{
			get
			{
				List<EditorBase> result = new List<EditorBase>(_model.OpenedEditors);
				result.Sort(_editorByOrderComparer);

				return result;
			}
		}

		public EditorBase SelectedEditor
		{
			get { return _model.SelectedEditor; }
		}

		public double MaxWidth
		{
			get
			{
				return Math.Max(Math.Ceiling(OpenedEditors.Count/10.0), 1) * 170 + 20;
			}
		}

		public int SelectionIndex
		{
			get { return _selectionIndex; }
			set
			{
				if (_selectionIndex == value)
					return;

				_selectionIndex = value;
				RaisePropertyChanged("SelectionIndex");
			}
		}

		public void ShowNext()
		{
			if (!IsOpen)
				IsOpen = true;

			if (_selectionIndex == OpenedEditors.Count - 1)
				SelectionIndex = 0;
			else
				SelectionIndex++;
		}

		public bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				if (_isOpen == value)
					return;
				
//				Console.Beep();
				_isOpen = value;
				RaisePropertyChanged("IsOpen");

				if (_isOpen)
				{
					RaisePropertyChanged("MaxWidth");
					RaisePropertyChanged("OpenedEditors");

					SelectionIndex = 0;
				}

				if (!_isOpen)
					_model.SelectEditorCommand(OpenedEditors[_selectionIndex]);
			}
		}

		private class EditorByOrderComparer : IComparer<EditorBase>
		{
			private readonly IList<EditorBase> _orderSource;

			public EditorByOrderComparer(IList<EditorBase> orderSource)
			{
				if (orderSource == null) 
					throw new ArgumentNullException("orderSource");

				_orderSource = orderSource;
			}

			public int Compare(EditorBase x, EditorBase y)
			{
				if (_orderSource.Contains(x) && _orderSource.Contains(y))
					return _orderSource.IndexOf(x) < _orderSource.IndexOf(y) ? -1 : 1;

				if (_orderSource.Contains(x) && !_orderSource.Contains(y))
					return -1;

				if (!_orderSource.Contains(x) && _orderSource.Contains(y))
					return 1;

				return 0;
			}
		}
	}
}
