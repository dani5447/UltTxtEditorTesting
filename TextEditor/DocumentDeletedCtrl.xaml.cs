using System;
using System.Drawing;
using System.Windows;
using UniversalEditor.Base;
using UniversalEditor.Base.Utils.LoggerModule;

namespace TextEditor
{
	/// <summary>
	/// Interaction logic for DocumentChangedCtrl.xaml
	/// </summary>
	public partial class DocumentDeletedCtrl
	{
		private readonly IEditorOwner _owner;
		private readonly UfeTextBox _textBox;

		public DocumentDeletedCtrl(IEditorOwner owner, UfeTextBox textBox)
		{
			if (owner == null) 
				throw new ArgumentNullException("owner");
			if (textBox == null) 
				throw new ArgumentNullException("textBox");

			_owner = owner;
			_textBox = textBox;
			
			InitializeComponent();

			image.Source = SystemIcons.Information.ToImageSource();
		}

		private void OnCloseEditor(object sender, RoutedEventArgs e)
		{
			_owner.CloseActiveEditor();
		}

		private void OnHide(object sender, RoutedEventArgs e)
		{
			_textBox.HideAllBottomControls();
		}
	}
}
