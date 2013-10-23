
using System.Windows.Controls;
using System.Windows.Input;
using UniversalEditor.Base;

namespace UniversalEditor.Modules.TabSwitchModule
{
	/// <summary>
	/// Interaction logic for TabSwitchView.xaml
	/// </summary>
	public partial class TabSwitchView
	{
		public TabSwitchView()
		{
			InitializeComponent();
		}

		private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			TextBlock textBlock = (TextBlock) sender;
			EditorBase editor = (EditorBase)textBlock.Tag;

			list.SelectedItem = editor;
			IsOpen = false;
		}
	}
}
