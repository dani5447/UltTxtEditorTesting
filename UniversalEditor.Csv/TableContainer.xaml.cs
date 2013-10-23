using System.Windows.Controls;

namespace UniversalEditor.Csv
{
	/// <summary>
	/// Interaction logic for TableContainer.xaml
	/// </summary>
	public partial class TableContainer
	{
		public TableContainer()
		{
			InitializeComponent();
		}

		private void OnLoadingRow(object sender, DataGridRowEventArgs e)
		{
			if (e.Row.IsNewItem)
				e.Row.Header = string.Empty;
			else
				e.Row.Header = (e.Row.GetIndex() + 1).ToString();
		}
	}
}
