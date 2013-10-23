using System.Windows;

namespace UniversalEditor.Modules.NewFileModule
{
	/// <summary>
	/// Interaction logic for NewFileView.xaml
	/// </summary>
	public partial class NewFileView
	{
		public NewFileView()
		{
			InitializeComponent();
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnOkClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
