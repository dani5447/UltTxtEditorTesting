using System;
using System.Drawing;
using System.Windows;
using UniversalEditor.Base.Utils.LoggerModule;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for DocumentChangedCtrl.xaml
	/// </summary>
	public partial class DocumentChangedCtrl
	{
		private readonly UfeTextBox _textBox;

		public DocumentChangedCtrl(UfeTextBox textBox)
		{
			if (textBox == null) 
				throw new ArgumentNullException("textBox");
			
			_textBox = textBox;
			
			InitializeComponent();

			image.Source = SystemIcons.Warning.ToImageSource();
		}

		private void OnReaload(object sender, RoutedEventArgs e)
		{
			_textBox.Document.Reload();
			_textBox.CallOnRender();

			_textBox.HideAllBottomControls();
		}

		private void OnHide(object sender, RoutedEventArgs e)
		{
			_textBox.HideAllBottomControls();
		}
	}
}
