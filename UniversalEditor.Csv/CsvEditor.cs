using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Csv
{
	public class CsvEditor : EditorBase
    {
		private readonly Lazy<TableContainer> _grid;
		
		private DataTable _table;

		private bool _isReloading;
		private char _spliter = ';';

		public CsvEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_grid = new Lazy<TableContainer>(CreateControl);

			_file.ContentChanged += OnFileContentChanged;
		}

		private TableContainer CreateControl()
		{
			_table = new DataTable();

			_table.RowChanged += OnRowChanged;
			_table.RowDeleted += OnRowDeleted;
			_table.TableNewRow += OnTableNewRow;
			_table.ColumnChanged += OnColumnChanged;

			TableContainer ctrl = new TableContainer();

			_isReloading = true;
			ctrl.DataContext = GetDataView();
			_isReloading = false;

			return ctrl;
		}

		public override FrameworkElement EditorControl
		{
			get { return _grid.Value; }
		}

		private void OnColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
//			if (!_isReloading)
//			{
//				int colIndex = _table.Columns.IndexOf(e.Column);
//
//				while (colIndex > _table.Columns.Count - 4)
//				{
//					_table.Columns.Add(GetHeaderFor(_table.Columns.Count));
//				}
//
//				_grid.InvalidateProperty(DataGrid.ItemsSourceProperty);
//			}
		}

		private void OnTableNewRow(object sender, DataTableNewRowEventArgs e)
		{
			if (!_isReloading)
				_file.SetIsChangedToTrue();
		}

		private void OnRowDeleted(object sender, DataRowChangeEventArgs e)
		{
			if (!_isReloading)
				_file.SetIsChangedToTrue();
		}

		private void OnRowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (!_isReloading)
				_file.SetIsChangedToTrue();
		}

		public override void Dispose()
		{
			_file.ContentChanged -= OnFileContentChanged;
			
			if (_grid.IsValueCreated)
			{
				_grid.Value.DataContext = null;

				_table.RowChanged -= OnRowChanged;
				_table.RowDeleted -= OnRowDeleted;
				_table.TableNewRow -= OnTableNewRow;
				_table.ColumnChanged -= OnColumnChanged;
			}

			base.Dispose();
		}

		public override void Reload()
		{
			_grid.Value.Dispatcher.BeginInvoke(new Action(() =>
			{
				try
				{
					_isReloading = true;
					_grid.Value.DataContext = GetDataView();
				}
				finally
				{
					_isReloading = false;
				}
			}));
		}

		public override void SaveFile()
		{
			SaveFileAs(_file.FilePath);
		}

		public override void SaveFileAs(string filePath)
		{
			UpdateMemoryFileContent();
			_file.SaveAs(filePath);
		}

		public override void SaveCopyAs(string filePath)
		{
			UpdateMemoryFileContent();
			_file.SaveCopyAs(filePath);
		}

		private void UpdateMemoryFileContent()
		{
			_file.ClearLines();

			StringBuilder builder = new StringBuilder();
			for (int rowIndex = 0; rowIndex < _table.Rows.Count; rowIndex++)
			{
				DataRow row = _table.Rows[rowIndex];
				
				builder.Clear();

				if (row.ItemArray.Length > 0)
					builder.Append(row.ItemArray[0]);

				for (int index = 1; index < row.ItemArray.Length; index++)
				{
					builder.Append(_spliter);
					builder.Append(row.ItemArray[index]);
				}
				
				_file.SetLine(rowIndex, builder.ToString().TrimEnd(_spliter));
			}
		}

		private DataView GetDataView()
		{
			try
			{
				_table.BeginLoadData();

				_table.Clear();
				_table.Columns.Clear();

				for (int index = 0; index < 26; index++)
					_table.Columns.Add(GetHeaderFor(index));

				if (!string.IsNullOrEmpty(_file.FileSource))
				{
					using (TextReader reader = new StreamReader(File.OpenRead(_file.FileSource)))
					{
						bool isFirstLine = true;
						string line;

						while ((line = reader.ReadLine()) != null)
						{
							if (isFirstLine)
							{
								isFirstLine = false;

								_spliter =
									line.Contains(";") ? ';' :
									line.Contains(",") ? ',' : ';';
							}

							string[] data = line.Split(_spliter);

							while (data.Length > _table.Columns.Count)
								_table.Columns.Add(GetHeaderFor(_table.Columns.Count));

							_table.Rows.Add(data);
						}
					}
				}

				return _table.AsDataView();
			}
			finally
			{
				_table.EndLoadData();
			}
		}

		private string GetHeaderFor(int count)
		{
			StringBuilder header = new StringBuilder();

			while (count >= 0)
			{
				if (count == 0)
				{
					header.Insert(0, "A");
					break;
				}

				int x = count%26;
				char ch = Convert.ToChar(x + Convert.ToInt32('A'));
				count = (count - x) / 26 - 1;

				header.Insert(0, ch);
			}

			return header.ToString();
		}

		private void OnFileContentChanged()
		{
			Reload();
		}
    }
}
