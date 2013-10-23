using System;
using System.Drawing;
using System.Windows;
using UniversalEditor.Base.Utils.LoggerModule;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for ReadonlyCtrl.xaml
	/// </summary>
	public partial class ReadonlyCtrl
	{
		private readonly UfeTextBox _ufeTextBox;

		public ReadonlyCtrl(UfeTextBox ufeTextBox)
		{
			if (ufeTextBox == null) 
				throw new ArgumentNullException("ufeTextBox");
			
			_ufeTextBox = ufeTextBox;
			
			InitializeComponent();

			image.Source = SystemIcons.Information.ToImageSource();
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			_ufeTextBox.IsReadonly = false;
		}

		private void OnHide(object sender, RoutedEventArgs e)
		{
			_ufeTextBox.HideAllBottomControls();
		}
	}
}
