using System.Windows;

namespace UniversalEditor.OptionsModule
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsView
	{
		public OptionsView()
		{
			InitializeComponent();
			DataContext = new OptionsViewModel();
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
