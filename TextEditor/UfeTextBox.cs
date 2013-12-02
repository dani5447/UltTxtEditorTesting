using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using UniversalEditor.Base;
using UniversalEditor.Base.Mvvm;

namespace TextEditor
{
	public delegate void KeyDownDelegate(KeyEventArgs e);

	public class UfeTextBox : Grid
	{
		private readonly IEditorOwner _owner;

		public event KeyDownDelegate OnKeyDownEvent
		{
			add { _textBox.OnKeyDownEvent += value; }
			remove { _textBox.OnKeyDownEvent -= value; }
		}

		private readonly ScrollBar _verticalScroll;
		private readonly ScrollBar _horizontalScroll;
		private readonly FindLineCtrl _findLineCtrl;
		private readonly  FindWordCtrl _findWordCtrl;
		private readonly  DocumentChangedCtrl _documentChangedCtrl;
		private readonly  UfeTextBoxInner _textBox;
		private readonly UfePopup _popup;

		private readonly MenuItem _readonlyMenuItem = new MenuItem();
		private readonly MenuItem _findMenuItem = new MenuItem();
		private readonly MenuItem _gotoMenuItem = new MenuItem();
		private readonly MenuItem _duplicateMenuItem = new MenuItem();
		private readonly MenuItem _formattingMenuItem = null;
		private readonly MenuItem _commentTextMenuItem = null;
		private readonly MenuItem _commentLineMenuItem = null;
		
		private readonly ReadonlyCtrl _readonlyCtrl;
		private readonly DocumentDeletedCtrl _documentDeletedCtrl;

		public UfeTextBoxInner TextBox
		{
			get { return _textBox; }
		}

		internal IEditorOwner Owner
		{
			get { return _owner; }
		}

		public UfeTextBox(IEditorOwner owner, UfeTextDocument doc)
		{
			if (owner == null) 
				throw new ArgumentNullException("owner");

			_owner = owner;

			BeginInit();

			// background
			UserControl background = new UserControl();
			background.Background = Brushes.Transparent;
			SetColumnSpan(background, 2);
			Children.Add(background);

			// scroll
			_verticalScroll = new ScrollBar();
//			_verticalScroll.MinWidth = 10;
			_verticalScroll.Width = 17;
			_verticalScroll.Visibility = Visibility.Collapsed;
			_verticalScroll.Orientation = Orientation.Vertical;
			_verticalScroll.Margin = new Thickness(0);
			_verticalScroll.Padding = new Thickness(0);

			SetColumn(_verticalScroll, 2);
			SetRow(_verticalScroll, 0);
			SetRowSpan(_verticalScroll, 3);
			Children.Add(_verticalScroll);

			_horizontalScroll = new ScrollBar();
//			_horizontalScroll.MinHeight = 10;
			_horizontalScroll.Height = 17;
			_horizontalScroll.Visibility = Visibility.Collapsed;
			_horizontalScroll.Orientation = Orientation.Horizontal;
			_horizontalScroll.Margin = new Thickness(0);
			_horizontalScroll.Padding = new Thickness(0);

			SetColumnSpan(_horizontalScroll, 2);
			SetRow(_horizontalScroll, 3);
			Children.Add(_horizontalScroll);
			
			// find line
			_findLineCtrl = new FindLineCtrl(this);
			_findLineCtrl.Visibility = Visibility.Collapsed;
			_findLineCtrl.Margin = new Thickness(0);
			_findLineCtrl.Padding = new Thickness(0);
			SetColumn(_findLineCtrl, 1);
			SetRow(_findLineCtrl, 0);

			// find word
			_findWordCtrl = new FindWordCtrl(this);
			_findWordCtrl.Visibility = Visibility.Collapsed;
			_findWordCtrl.Margin = new Thickness(0);
			_findWordCtrl.Padding = new Thickness(0);
			SetColumn(_findWordCtrl, 1);
			SetRow(_findWordCtrl, 0);

			// readonly
			_readonlyCtrl = new ReadonlyCtrl(this);
			_readonlyCtrl.Visibility = Visibility.Collapsed;
			_readonlyCtrl.HorizontalAlignment = HorizontalAlignment.Center;
			_readonlyCtrl.Margin = new Thickness(0);
			_readonlyCtrl.Padding = new Thickness(0);
			SetColumnSpan(_readonlyCtrl, 2);
			SetRow(_readonlyCtrl, 2);

			// popup
			_popup = new UfePopup(this);
			_popup.Visibility = Visibility.Collapsed;
			_popup.Placement = PlacementMode.Custom;
			_popup.CustomPopupPlacementCallback = CustomPopupPlacementCallback;

			// file changed
			_documentChangedCtrl = new DocumentChangedCtrl(this);
			_documentChangedCtrl.Visibility = Visibility.Collapsed;
			_documentChangedCtrl.HorizontalAlignment = HorizontalAlignment.Center;
			_documentChangedCtrl.Margin = new Thickness(0);
			_documentChangedCtrl.Padding = new Thickness(0);
			SetColumnSpan(_documentChangedCtrl, 2);
			SetRow(_documentChangedCtrl, 2);

			// file changed
			_documentDeletedCtrl = new DocumentDeletedCtrl(owner, this);
			_documentDeletedCtrl.Visibility = Visibility.Collapsed;
			_documentDeletedCtrl.HorizontalAlignment = HorizontalAlignment.Center;
			_documentDeletedCtrl.Margin = new Thickness(0);
			_documentDeletedCtrl.Padding = new Thickness(0);
			SetColumnSpan(_documentDeletedCtrl, 2);
			SetRow(_documentDeletedCtrl, 2);

			// _textBox
			_textBox = new UfeTextBoxInner(this, doc, _verticalScroll, _horizontalScroll, _findLineCtrl, _findWordCtrl, _documentChangedCtrl, _documentDeletedCtrl, _readonlyCtrl, _popup);
			_textBox.ClipToBounds = true;
			_textBox.Cursor = Cursors.IBeam;
			SetRowSpan(_textBox, 3);
			SetColumnSpan(_textBox, 2);

			Children.Add(_textBox);
			Children.Add(_findLineCtrl);
			Children.Add(_findWordCtrl);
			Children.Add(_readonlyCtrl);
			Children.Add(_popup);
			Children.Add(_documentChangedCtrl);
			Children.Add(_documentDeletedCtrl);

			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

			RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Star) });
			RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

			ClipToBounds = true;
			Margin = new Thickness(0);
			_popup.PlacementTarget = _textBox;

			// initialize menu
			_readonlyMenuItem.Header = "Readonly";
			_readonlyMenuItem.IsCheckable = true;
			_readonlyMenuItem.IsChecked = IsReadonly;
			_readonlyMenuItem.Checked += (sender, args) => IsReadonly = true;
			_readonlyMenuItem.Unchecked += (sender, args) => IsReadonly = false;

			// find menu
			_findMenuItem.Header = "Find...";
			_findMenuItem.InputGestureText = "Ctrl+F";
			_findMenuItem.Command = new SimpleCommand(() => _textBox.SetTopPanelVisibility(UfeTextBoxInner.TopPanelVisibility.Word));

			// goto menu
			_gotoMenuItem.Header = "Go To...";
			_gotoMenuItem.InputGestureText = "Ctrl+G";
			_gotoMenuItem.Command = new SimpleCommand(() => _textBox.SetTopPanelVisibility(UfeTextBoxInner.TopPanelVisibility.Line));

			// goto menu
			_duplicateMenuItem.Header = "Duplicate...";
			_duplicateMenuItem.InputGestureText = "Ctrl+D";
			_duplicateMenuItem.Command = new SimpleCommand(_textBox.OnDuplicate);

			// comment line menu
			if (doc.IsCommentLineSupported)
			{
				_commentLineMenuItem = new MenuItem();
				_commentLineMenuItem.Header = "Comment Line";
				_commentLineMenuItem.InputGestureText = "Ctrl+/";
				_commentLineMenuItem.Command = _textBox.CommentSelectedLinesCommand;
			}

			// comment text menu
			if (doc.IsCommentTextSupported)
			{
				_commentTextMenuItem = new MenuItem();
				_commentTextMenuItem.Header = "Comment Text";
				_commentTextMenuItem.InputGestureText = "Ctrl+Shift+/";
				_commentTextMenuItem.Command = _textBox.CommentSelectedTextCommand;
			}

			// formatting text menu
			if (doc.IsFormattingSupported)
			{
				_formattingMenuItem = new MenuItem();
				_formattingMenuItem.Header = "Update text format";
				_formattingMenuItem.InputGestureText = "Ctrl+Alt+F";
				_formattingMenuItem.Command = _textBox.FormattingCommand;
			}

			if (doc.IsNewFile)
				IsReadonly = false;

			_textBox.TabIndex = 1;
			_textBox.Focus();

			EndInit();
		}

		private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			CustomPopupPlacement placement1 = new CustomPopupPlacement(_textBox.GetCursorPosition(), PopupPrimaryAxis.Vertical);
			CustomPopupPlacement placement2 = new CustomPopupPlacement(_textBox.GetCursorPosition(), PopupPrimaryAxis.Horizontal);

			CustomPopupPlacement[] ttplaces = new[] { placement1, placement2 };
			return ttplaces;
		}

		public void ShowLine(int number)
		{
			_textBox.ShowLine(number);
		}

		public UfeTextDocument Document
		{
			get { return _textBox.Document; }
		}

		public void FindText(string text, bool useRegister)
		{
			_textBox.FindText(text, useRegister);
		}

		internal int ReplaceAllText(string text, bool useRegister, string value)
		{
			return _textBox.ReplaceAllText(text, useRegister, value);
		}

		internal void ReplaceText(string text, bool useRegister, string value)
		{
			_textBox.ReplaceText(text, useRegister, value);
		}

		public void HideAllFindControls()
		{
			_textBox.SetTopPanelVisibility(UfeTextBoxInner.TopPanelVisibility.None);
		}

		public void HideAllBottomControls()
		{
			_textBox.SetBottomPanelVisibility(UfeTextBoxInner.BottomPanelVisibility.None);
		}

		public bool IsReadonly
		{
			get { return _textBox.IsReadonly; }
			set
			{
				if (_textBox.IsReadonly == value)
					return;

				_textBox.IsReadonly = value;
				_readonlyMenuItem.IsChecked = value;

				HideAllBottomControls();
			}
		}

		public void CallOnRender()
		{
			_textBox.InvalidateVisual();
		}

		public List<Control> GenerateBasicMenu()
		{
			List<Control> menu = new List<Control>();
			menu.Add(_readonlyMenuItem);
			menu.Add(new Separator());
			menu.Add(_findMenuItem);
			menu.Add(_gotoMenuItem);
			menu.Add(new Separator());
			menu.Add(_duplicateMenuItem);

			if (_commentLineMenuItem != null || _commentTextMenuItem != null)
			{
				menu.Add(new Separator());

				if (_commentLineMenuItem != null)
					menu.Add(_commentLineMenuItem);
				if (_commentTextMenuItem != null)
					menu.Add(_commentTextMenuItem);
			}

			if (_formattingMenuItem != null)
			{
				menu.Add(new Separator());
				menu.Add(_formattingMenuItem);
			}

			return menu;
		}

		public void Insert(HintItem word)
		{
			_textBox.Insert(word);
		}

		public void SaveStatus(XElement file)
		{
			file.Add(new XElement("VScrollOffset", _verticalScroll.Value.ToString(CultureInfo.InvariantCulture)));
			file.Add(new XElement("HScrollOffset", _horizontalScroll.Value.ToString(CultureInfo.InvariantCulture)));
			file.Add(new XElement("StartSelectionCol", _textBox.StartSelection.Col));
			file.Add(new XElement("StartSelectionRow", _textBox.StartSelection.Row));
			file.Add(new XElement("EndSelectionCol", _textBox.EndSelection.Col));
			file.Add(new XElement("EndSelectionRow", _textBox.EndSelection.Row));
		}

		internal void LoadState(XElement xStatusElement)
		{
			if (xStatusElement == null)
				return;

			XElement vScrollOffset = xStatusElement.Element("VScrollOffset");
			if (vScrollOffset != null)
				_verticalScroll.Value = double.Parse(vScrollOffset.Value, CultureInfo.InvariantCulture);

			XElement hScrollOffset = xStatusElement.Element("HScrollOffset");
			if (hScrollOffset != null)
				_horizontalScroll.Value = double.Parse(hScrollOffset.Value, CultureInfo.InvariantCulture);

			XElement startSelectionCol = xStatusElement.Element("StartSelectionCol");
			XElement startSelectionRow = xStatusElement.Element("StartSelectionRow");
			XElement endSelectionCol = xStatusElement.Element("EndSelectionCol");
			XElement endSelectionRow = xStatusElement.Element("EndSelectionRow");
			if (startSelectionCol != null && startSelectionRow != null && endSelectionCol != null && endSelectionRow != null)
				_textBox.SetPosition(
					new CharPosition(int.Parse(startSelectionCol.Value, CultureInfo.InvariantCulture), int.Parse(startSelectionRow.Value, CultureInfo.InvariantCulture)),
					new CharPosition(int.Parse(endSelectionCol.Value, CultureInfo.InvariantCulture), int.Parse(endSelectionRow.Value, CultureInfo.InvariantCulture)), true);
		}
	}
}
