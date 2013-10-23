using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TextEditor.Utils;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace TextEditor
{
	public class UfeTextBoxInner : UserControl
	{
		private const double fontSize = 14;
		private static readonly double fontHeight;
		private static readonly double fontWidth;

		internal enum TopPanelVisibility { None, Line, Word }
		internal enum BottomPanelVisibility { None, Readonly, Changed, Deleted }

		public event KeyDownDelegate OnKeyDownEvent;

		private double _lineStartOffset = 30;
		public readonly static char[] CtrlSplitChars = new[] { ' ', ')', '(', '/', '\\', '\t', '.', ',', '"', '\'', ':', ';', '<', '>', '|' };
		internal static readonly Typeface Typeface;

		private readonly UfeTextBox _ufeTextBox;
		private readonly UfeTextDocument _document;
		private readonly ScrollBar _verticalScroll;
		private readonly ScrollBar _horizontalScroll;
		private readonly FindLineCtrl _findLineCtrl;
		private readonly FindWordCtrl _findWordCtrl;
		private readonly DocumentChangedCtrl _documentChangedCtrl;
		private readonly DocumentDeletedCtrl _documentDeletedCtrl;
		private readonly ReadonlyCtrl _readonlyCtrl;
		private readonly UfePopup _popup;

		private readonly List<TextPosition> _linesGeometry = new List<TextPosition>();

		private readonly Pen _defaultPen = new Pen(Brushes.Black, 1);
		private readonly Pen _cursorPen = CreateCursorPen();

		private readonly ActionStackExplorer _actionStackExplorer;

		private readonly SimpleCommand _copyCommand;
		private readonly SimpleCommand _cutCommand;
		private readonly SimpleCommand _pasteCommand;
		private readonly SimpleCommand _deleteCommand;
		private readonly SimpleCommand _selectAllCommand;
		private readonly SimpleCommand _commentSelectedLinesCommand;
		private readonly SimpleCommand _formattingCommand;
		private readonly SimpleCommand _commentSelectedTextCommand;

		private readonly ContextMenu _contextMenu = new ContextMenu();

		private readonly GlyphTypeface _glyphTypeface;
		private readonly ushort _glyphSpaceIndex;
		private readonly ushort _glyphUnknownIndex;
		private readonly List<ushort> _glyphIndexes = new List<ushort>(100);
		private readonly List<double> _advanceWidths = new List<double>(100);

		private int _renderedStartLineIndex = 0;
		private double _renderedStartLineOffset = 0;
		private int _renderedLineCount = 0;
		private DateTime _lastMouseDownEvent = DateTime.MinValue;
		private CharPosition _lastMouseDownPosition;
		private bool _isLastMouseDownDouble = false;

		private Point _cursorPosition = new Point();

		private object _longestLineHandle;
		private double _longestLineWidth = -1;

		private CharPosition _startSelection = new CharPosition(0, 0);
		private CharPosition _endSelection = new CharPosition(0, 0);
		private bool _isReadonly = false;
		
#if DEBUG
		private DateTime _lastRenderingTime = DateTime.UtcNow;
		private int _lastRenderersCount = 0;
		private int _renderersCount = 0;
#endif

		static UfeTextBoxInner()
		{
			Typeface = new Typeface(new FontFamily("Consolas"), Application.Current.MainWindow.FontStyle, Application.Current.MainWindow.FontWeight, Application.Current.MainWindow.FontStretch);
			FormattedText testText = new FormattedText("X", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, fontSize, Application.Current.MainWindow.Foreground, new NumberSubstitution(), TextFormattingMode.Display);

			fontHeight = testText.Height;
			fontWidth = testText.Width;
		}

		public UfeTextDocument Document
		{
			get { return _document; }
		}

		public bool IsReadonly
		{
			get { return _isReadonly; }
			set
			{
				if (_isReadonly == value)
					return;

				_isReadonly = value;

				_commentSelectedLinesCommand.RiseCanExecuteChanged();
				_commentSelectedTextCommand.RiseCanExecuteChanged();
				_pasteCommand.RiseCanExecuteChanged();
				_cutCommand.RiseCanExecuteChanged();
				InvalidateVisual();
			}
		}
		
		internal SimpleCommand CutCommand
		{
			get { return _cutCommand; }
		}

		internal SimpleCommand CopyCommand
		{
			get { return _copyCommand; }
		}

		internal CharPosition StartSelection
		{
			get { return _startSelection; }
		}

		internal CharPosition EndSelection
		{
			get { return _endSelection; }
		}

		internal SimpleCommand PasteCommand
		{
			get { return _pasteCommand; }
		}

		internal SimpleCommand DeleteCommand
		{
			get { return _deleteCommand; }
		}

		internal SimpleCommand SelectAllCommand
		{
			get { return _selectAllCommand; }
		}

		public SimpleCommand CommentSelectedLinesCommand
		{
			get { return _commentSelectedLinesCommand; }
		}

		public SimpleCommand FormattingCommand
		{
			get { return _formattingCommand; }
		}

		public SimpleCommand CommentSelectedTextCommand
		{
			get { return _commentSelectedTextCommand; }
		}

		public UfeTextBoxInner(UfeTextBox ufeTextBox, UfeTextDocument document, ScrollBar verticalScroll, ScrollBar horizontalScroll, FindLineCtrl findLineCtrl, FindWordCtrl findWordCtrl, DocumentChangedCtrl documentChangedCtrl, DocumentDeletedCtrl documentDeletedCtrl, ReadonlyCtrl readonlyCtrl, UfePopup popup)
		{
			if (ufeTextBox == null) 
				throw new ArgumentNullException("ufeTextBox");
			if (document == null) 
				throw new ArgumentNullException("document");
			if (verticalScroll == null)
				throw new ArgumentNullException("verticalScroll");
			if (horizontalScroll == null)
				throw new ArgumentNullException("horizontalScroll");
			if (findLineCtrl == null)
				throw new ArgumentNullException("findLineCtrl");
			if (findWordCtrl == null)
				throw new ArgumentNullException("findWordCtrl");
			if (documentChangedCtrl == null)
				throw new ArgumentNullException("documentChangedCtrl");
			if (documentDeletedCtrl == null) 
				throw new ArgumentNullException("documentDeletedCtrl");
			if (readonlyCtrl == null) 
				throw new ArgumentNullException("readonlyCtrl");
			if (popup == null)
				throw new ArgumentNullException("popup");

			_ufeTextBox = ufeTextBox;
			_document = document;
			_verticalScroll = verticalScroll;
			_horizontalScroll = horizontalScroll;
			_findLineCtrl = findLineCtrl;
			_findWordCtrl = findWordCtrl;
			_documentChangedCtrl = documentChangedCtrl;
			_documentDeletedCtrl = documentDeletedCtrl;
			_readonlyCtrl = readonlyCtrl;
			_popup = popup;
			_actionStackExplorer = new ActionStackExplorer(this);
			
			// initialize glyph
			if (!Typeface.TryGetGlyphTypeface(out _glyphTypeface))
				throw new InvalidOperationException("No glyphtypeface found");

			_glyphSpaceIndex = _glyphTypeface.CharacterToGlyphMap[' '];
			_glyphUnknownIndex = _glyphTypeface.CharacterToGlyphMap[0x2666];
			//

			_verticalScroll.ValueChanged += _verticalScroll_ValueChanged;
			_verticalScroll.MouseLeftButtonDown += VerticalScrollOnMouseLeftButtonDown;
			_verticalScroll.MouseLeftButtonUp += VerticalScrollOnMouseLeftButtonDown;
			_verticalScroll.MouseMove += _verticalScroll_MouseMove;

			_horizontalScroll.ValueChanged += _horizontalScroll_ValueChanged;
			_horizontalScroll.MouseLeftButtonDown += HorizontalScrollOnMouseLeftButtonDown;
			_horizontalScroll.MouseLeftButtonUp += HorizontalScrollOnMouseLeftButtonDown;
			_horizontalScroll.MouseMove += _horizontalScroll_MouseMove;

			_document.ContentChanged += OnContentChanged;
			_document.ContentReloaded += OnDocumentContentReloaded;
			_document.FileDeleted += OnFileDeleted;

			FindTheLongestLine();

			_copyCommand = new SimpleCommand(OnCopy, IsCopyEnable);
			_cutCommand = new SimpleCommand(OnCut, IsCutEnable);
			_pasteCommand = new SimpleCommand(OnPaste, () => !IsReadonly);
			_deleteCommand = new SimpleCommand(OnRemoveSelection, IsCutEnable);
			_selectAllCommand = new SimpleCommand(OnSelectAll);
			_commentSelectedLinesCommand = new SimpleCommand(CommentSelectedLines, () => !IsReadonly);
			_formattingCommand = new SimpleCommand(FormatSelectedLines, () => !IsReadonly);
			_commentSelectedTextCommand = new SimpleCommand(CommentSelectedText, () => !IsReadonly);

			_contextMenu.Items.Add(new MenuItem {Header = "Undo", Command = UndoCommand});
			_contextMenu.Items.Add(new MenuItem {Header = "Redo", Command = RedoCommand});
			_contextMenu.Items.Add(new Separator());
			_contextMenu.Items.Add(new MenuItem {Header = "Cut", Command = CutCommand});
			_contextMenu.Items.Add(new MenuItem {Header = "Copy", Command = CopyCommand});
			_contextMenu.Items.Add(new MenuItem {Header = "Paste", Command = PasteCommand});
			_contextMenu.Items.Add(new MenuItem {Header = "Delete", Command = DeleteCommand});
			_contextMenu.Items.Add(new MenuItem {Header = "Select All", Command = SelectAllCommand});

			ContextMenu = _contextMenu;

			Background = Brushes.Transparent;
			Focusable = true;
			Keyboard.Focus(this);

			if (IsReadonly)
				SetBottomPanelVisibility(BottomPanelVisibility.Readonly);

			FocusVisualStyle = null;
		}
		
		private void OnDocumentContentReloaded()
		{
			_actionStackExplorer.Clear();
		}

		private void FindTheLongestLine()
		{
			for (int index = 0; index < _document.LineCount; index++)
			{
				double lineWidth = _document.GetLineWidthInPixels(index);

				if (_longestLineWidth < lineWidth)
				{
					_longestLineHandle = _document.GetLineHandle(index);
					_longestLineWidth = lineWidth;
				}
			}
		}

		private void OnFileDeleted()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(OnFileDeleted);
				return;
			}

			SetBottomPanelVisibility(BottomPanelVisibility.Deleted);
		}

		private void OnContentChanged()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(OnContentChanged);
				return;
			}

			if (_isReadonly)
			{
				_document.Reload();
				InvalidateVisual();
			}
			else
			{
				SetBottomPanelVisibility(BottomPanelVisibility.Changed);
			}
		}

		void _verticalScroll_MouseMove(object sender, MouseEventArgs e)
		{
			e.Handled = true;
		}

		void _horizontalScroll_MouseMove(object sender, MouseEventArgs e)
		{
			e.Handled = true;
		}

		private void VerticalScrollOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			mouseButtonEventArgs.Handled = true;
		}

		private void HorizontalScrollOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			mouseButtonEventArgs.Handled = true;
		}

		private void _verticalScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_renderedStartLineIndex = Convert.ToInt32(_verticalScroll.Value);
			InvalidateVisual();
		}

		private void _horizontalScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_renderedStartLineOffset = Convert.ToInt32(_horizontalScroll.Value);
			InvalidateVisual();
		}

		internal static string ReplaceTabs(string text)
		{
			return ReplaceTabs(text, 0);
		}
		
		internal static double GetTextWidth(string text)
		{
			return ReplaceTabs(text).Length * fontWidth;
		}
		
		internal static string ReplaceTabs(string text, int beginOffset)
		{
			StringBuilder builder = new StringBuilder();

			for (int index = 0; index < text.Length; index++)
			{
				char ch = text[index];

				if (ch == '\t')
				{
					int tabCount = 4 - beginOffset % 4;

					for (int i = 0; i < tabCount; i++)
						builder.Append(' ');

					beginOffset += tabCount;
				}
				else
				{
					builder.Append(ch);
					beginOffset++;
				}
			}

			return builder.ToString();
		}

		public ICommand UndoCommand
		{
			get { return _actionStackExplorer.UndoCommand; }
		}

		public ICommand RedoCommand
		{
			get { return _actionStackExplorer.RedoCommand; }
		}
		
		internal void SetTopPanelVisibility(TopPanelVisibility visibility)
		{
			switch (visibility)
			{
				case TopPanelVisibility.Line:
					_findWordCtrl.Visibility = Visibility.Collapsed;
					_findLineCtrl.Visibility = Visibility.Visible;

					_findLineCtrl.txtNumber.UpdateLayout();
					_findLineCtrl.txtNumber.Focus();
					_findLineCtrl.txtNumber.SelectAll();
					break;
				case TopPanelVisibility.Word:
					_findLineCtrl.Visibility = Visibility.Collapsed;
					_findWordCtrl.Visibility = Visibility.Visible;

					_findWordCtrl.txtWord.UpdateLayout();
					_findWordCtrl.txtWord.Focus();
					_findWordCtrl.txtWord.SelectAll();
					break;
				case TopPanelVisibility.None:
					_findLineCtrl.Visibility = Visibility.Collapsed;
					_findWordCtrl.Visibility = Visibility.Collapsed;

					Keyboard.Focus(this);
					break;
			}
		}

		internal void SetBottomPanelVisibility(BottomPanelVisibility visibility)
		{
			switch (visibility)
			{
				case BottomPanelVisibility.Deleted:
					_readonlyCtrl.Visibility = Visibility.Collapsed;
					_documentChangedCtrl.Visibility = Visibility.Collapsed;
					_documentDeletedCtrl.Visibility = Visibility.Visible;
					Keyboard.Focus(this);
					break;
				case BottomPanelVisibility.Changed:
					_readonlyCtrl.Visibility = Visibility.Collapsed;
					_documentChangedCtrl.Visibility = Visibility.Visible;
					_documentDeletedCtrl.Visibility = Visibility.Collapsed;
					Keyboard.Focus(this);
					break;
				case BottomPanelVisibility.Readonly:
					_readonlyCtrl.Visibility = Visibility.Visible;
					_documentChangedCtrl.Visibility = Visibility.Collapsed;
					_documentDeletedCtrl.Visibility = Visibility.Collapsed;
					Keyboard.Focus(this);
					break;
				case BottomPanelVisibility.None:
					_readonlyCtrl.Visibility = Visibility.Collapsed;
					_documentChangedCtrl.Visibility = Visibility.Collapsed;
					_documentDeletedCtrl.Visibility = Visibility.Collapsed;
					Keyboard.Focus(this);
					break;
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			
			_linesGeometry.Clear();

			double renderHeight = RenderSize.Height;
			
			double linePosition = 0;
			_renderedLineCount = 0;

			bool drawLineNumbers = Options.Instance.ShowLineNumbers;

			double areaNumberWidth = _lineStartOffset = drawLineNumbers ? ((_renderedStartLineIndex + 1) * 10).ToString().Length * fontWidth + 5 : 0;
			_lineStartOffset += 10 - _renderedStartLineOffset;

			while (_linesGeometry.Count > 0 && _linesGeometry[0].Line < _renderedStartLineIndex)
				_linesGeometry.RemoveAt(0);
			
			for (int index = _renderedStartLineIndex; index < _document.LineCount && linePosition < renderHeight; index++, _renderedLineCount++)
			{
				TextPosition textLine = null;

				bool hasCachedVariant = _linesGeometry.Count > _renderedLineCount && _linesGeometry[_renderedLineCount].Line == index;
				if (hasCachedVariant && _endSelection.Row >= index && _startSelection.Row <= index)
				{
					string lineAsString = _document.GetLineText(index);
					textLine = new TextPosition(lineAsString, index) { Top = linePosition };
					_linesGeometry[_renderedLineCount] = textLine;
				}
				else if (hasCachedVariant)
				{
					textLine = _linesGeometry[_renderedLineCount];
				}
				else
				{
					string lineAsString = _document.GetLineText(index);
					textLine = new TextPosition(lineAsString, index) { Top = linePosition };
					_linesGeometry.Insert(_renderedLineCount, textLine);
				}

				if (textLine.TextAsString.Length == 0)
				{
					if (index == _startSelection.Row && _startSelection == _endSelection)
					{
						_cursorPosition = new Point(_lineStartOffset, linePosition);
						drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(_lineStartOffset, linePosition), new Point(_lineStartOffset, linePosition + fontHeight));
					}

					linePosition += fontHeight;
				}
				else
				{
					long startRow = Math.Min(_startSelection.Row, _endSelection.Row);
					long endRow = Math.Max(_startSelection.Row, _endSelection.Row);

					int startCol =
						_startSelection.Row < _endSelection.Row ? _startSelection.Col :
						_startSelection.Row > _endSelection.Row ? _endSelection.Col :
						Math.Min(_startSelection.Col, _endSelection.Col);

					int endCol =
						_startSelection.Row > _endSelection.Row ? _startSelection.Col :
						_startSelection.Row < _endSelection.Row ? _endSelection.Col :
						Math.Max(_startSelection.Col, _endSelection.Col);

					int startSel =
						startRow < index && endRow >= index ? 0 :
						startRow == index ? Math.Min(startCol, textLine.TextAsString.Length) : -1;
					int endSel =
						endRow < index ? -1 :
						endRow == index ? Math.Min(endCol, textLine.TextAsString.Length) :
						endRow > index && startRow <= index ? textLine.TextAsString.Length : -1;

					IList<UfeTextStyle> lineAsStyle = textLine.Glyphs == null ? _document.GetLineStyle(index) : null;
					DrawLine(drawingContext, textLine, lineAsStyle, startSel, endSel, linePosition);

					linePosition += fontHeight;
				}						
				
			}

			// draw line number
			if (drawLineNumbers)
			{
				drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, areaNumberWidth, renderHeight));
				drawingContext.DrawLine(_defaultPen, new Point(areaNumberWidth, 0), new Point(areaNumberWidth, renderHeight));

				for (int index = 0; index < _linesGeometry.Count; index++)
				{
					TextPosition textPosition = _linesGeometry[index];
					string text = (textPosition.Line + 1).ToString();
					int temp = 0;
					DrawText(drawingContext, text, ref temp, Application.Current.MainWindow.Foreground, new Point((areaNumberWidth - text.Length * fontWidth) / 2.0, textPosition.Top));
				}
			}
			//

#if DEBUG
			DateTime now = DateTime.UtcNow;
			double diff = (now - _lastRenderingTime).TotalSeconds;
			if (diff > 1)
			{
				_lastRenderersCount = _renderersCount;
				_renderersCount = 0;
				_lastRenderingTime = now;
			}

			_renderersCount++;

			{
				int ttt = 0;
				DrawText(drawingContext, _lastRenderersCount.ToString(), ref ttt, Brushes.Black, new Point(RenderSize.Width - 50, 0));
			}
#endif
			if (_renderedLineCount != _document.LineCount)
			{
				_verticalScroll.Maximum = _document.LineCount - _renderedLineCount;
				_verticalScroll.Value = _renderedStartLineIndex;
				_verticalScroll.ViewportSize = _renderedLineCount;
				_verticalScroll.Visibility = Visibility.Visible;
			}
			else
			{
				_verticalScroll.Visibility = Visibility.Collapsed;
			}

			double neededWidth = _longestLineWidth + areaNumberWidth + 10;
			if (neededWidth > RenderSize.Width)
			{
				_horizontalScroll.Maximum = neededWidth - RenderSize.Width;
				_horizontalScroll.Value = _renderedStartLineOffset;
				_horizontalScroll.ViewportSize = RenderSize.Width;
				_horizontalScroll.Visibility = Visibility.Visible;
			}
			else
			{
				_horizontalScroll.Visibility = Visibility.Collapsed;
			}
		}

		private void DrawLine(DrawingContext drawingContext, TextPosition textLine, IList<UfeTextStyle> line, int startSel, int endSel, double linePosition)
		{
//			if (textLine.Glyphs != null)
//			{
//				for (int index = 0; index < textLine.Glyphs.Count; index++)
//				{
//					GeometryDrawing run = textLine.Glyphs[index];
//
//					if (run == null)
//						continue;
//
//					drawingContext.PushTransform(new TranslateTransform(0, linePosition + fontSize));
//					drawingContext.DrawDrawing(textLine.Glyphs[index]);
//					drawingContext.Pop();
//				}
//
//				return;
//			}

//			List<GeometryDrawing> runs = new List<GeometryDrawing>(25);

//			double x = _lineStartOffset;
			int chCount = 0;

			if (startSel == -1 && endSel == -1)
			{
				for (int index = 0; index < line.Count; index++)
				{
					UfeTextStyle ufeTextStyle = line[index];
//					string text = ReplaceTabs(ufeTextStyle.Text.ToString(), chCount);
					//FormattedText part1 = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, fontSize, ufeTextStyle.Brush, new NumberSubstitution(), TextFormattingMode.Display) { Trimming = TextTrimming.CharacterEllipsis, MaxLineCount = 1 };
//					drawingContext.DrawText(part1, new Point(x, linePosition));
					DrawText(drawingContext, ufeTextStyle.Text.ToString(), ref chCount, ufeTextStyle.Brush, new Point(_lineStartOffset + chCount * fontWidth, linePosition));
//					runs.Add(run);

//					x += ufeTextStyle.Text.Length * fontWidth;// part1.WidthIncludingTrailingWhitespace;
//					chCount += text.Length;
				}
			}

			if (startSel != -1)
			{
				int start = 0, length = startSel;
				for (int index = 0; index < line.Count; index++)
				{
					if (length == 0)
						break;

					UfeTextStyle target = line[index];

					Brush brush = target.Brush;
					string text = target.Text.ToString();

					if (start > text.Length)
					{
						start -= text.Length;
						continue;
					}

					if (start > 0)
					{
						text = text.Substring(start);
						start = 0;
					}

					if (length < text.Length)
					{
						text = text.Substring(0, length);
					}

					if (length > 0)
					{
						//
//						string formattedText = ReplaceTabs(text, chCount);
//						FormattedText part1 = new FormattedText(formattedText, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, fontSize, brush, new NumberSubstitution(), TextFormattingMode.Display) { Trimming = TextTrimming.CharacterEllipsis, MaxLineCount = 1 };
//						drawingContext.DrawText(part1, new Point(x, linePosition));

						DrawText(drawingContext, text, ref chCount, brush, new Point(_lineStartOffset + chCount * fontWidth, linePosition));
//						runs.Add(run);

//						x += text.Length * fontWidth;// part1.WidthIncludingTrailingWhitespace;
//						chCount += formattedText.Length;
						//

						length -= text.Length;
					}
				}
			}

			if (startSel != -1 && endSel != -1)
			{
				if (startSel == endSel)
				{
					_cursorPosition = new Point(_lineStartOffset + chCount * fontWidth, linePosition);
					drawingContext.DrawLine(_cursorPen, new Point(_lineStartOffset + chCount * fontWidth, linePosition), new Point(_lineStartOffset + chCount * fontWidth, linePosition + fontSize));
				}
				else
				{
					int start = startSel, length = endSel - startSel;
					for (int index = 0; index < line.Count; index++)
					{
						if (length == 0)
							break;

						UfeTextStyle target = line[index];

						Brush brush = target.Brush;
						string text = target.Text.ToString();

						if (start > text.Length)
						{
							start -= text.Length;
							continue;
						}

						if (start > 0)
						{
							text = text.Substring(start);
							start = 0;
						}

						if (length < text.Length)
						{
							text = text.Substring(0, length);
						}

						if (length > 0)
						{
							//
							string formattedText = ReplaceTabs(text, chCount);
							FormattedText part2 = new FormattedText(formattedText, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, fontSize, brush, new NumberSubstitution(), TextFormattingMode.Display) { Trimming = TextTrimming.CharacterEllipsis, MaxLineCount = 1 };
							drawingContext.DrawGeometry(Brushes.LightSkyBlue, null, part2.BuildHighlightGeometry(new Point(_lineStartOffset + chCount * fontWidth, linePosition)));
//							drawingContext.DrawText(part2, new Point(x, linePosition));

							DrawText(drawingContext, text, ref chCount, brush, new Point(_lineStartOffset + chCount * fontWidth, linePosition));
//							runs.Add(run);

//							x += formattedText.Length * fontWidth;// part2.WidthIncludingTrailingWhitespace;
//							chCount += formattedText.Length;
							//

							length -= text.Length;
						}
					}
				}
			}

			if (endSel != -1)
			{
				int start = endSel;
				for (int index = 0; index < line.Count; index++)
				{
					UfeTextStyle target = line[index];

					Brush brush = target.Brush;
					string text = target.Text.ToString();

					if (start > text.Length)
					{
						start -= text.Length;
						continue;
					}

					if (start > 0)
					{
						text = text.Substring(start);
						start = 0;
					}
					
					//
//					string formattedText = ReplaceTabs(text, chCount);
//					FormattedText part3 = new FormattedText(formattedText, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, fontSize, brush, new NumberSubstitution(), TextFormattingMode.Display) { Trimming = TextTrimming.CharacterEllipsis, MaxLineCount = 1 };
//					drawingContext.DrawText(part3, new Point(x, linePosition));

					DrawText(drawingContext, text, ref chCount, brush, new Point(_lineStartOffset + chCount * fontWidth, linePosition));
//					runs.Add(run);

//					x += formattedText.Length * fontWidth;// part3.WidthIncludingTrailingWhitespace;
//					chCount += formattedText.Length;
					//
				}
			}

//			textLine.Glyphs = runs;
		}

		private void DrawText(DrawingContext drawingContext, string text, ref int chCount, Brush brush, Point location)
		{
			if (string.IsNullOrEmpty(text))
				return;
			
			_glyphIndexes.Clear();
			_advanceWidths.Clear();

			for (int n = 0, max = text.Length; n < max; n++)
			{
				char ch = text[n];

				if (ch == '\t')
				{
					int count = 4 - chCount % 4;
					chCount += count;

					for (int marker = 0; marker < count; marker++)
					{
						_glyphIndexes.Add(_glyphSpaceIndex);
						_advanceWidths.Add(fontWidth);
					}
				}
				else
				{
					chCount++;

					ushort glyphIndex;
					if (!_glyphTypeface.CharacterToGlyphMap.TryGetValue(ch, out glyphIndex))
						glyphIndex = _glyphUnknownIndex;
					
					_glyphIndexes.Add(glyphIndex);
					_advanceWidths.Add(fontWidth);
				}
			}
			
			GlyphRun glyphRun = new GlyphRun(_glyphTypeface, 0, false, fontSize,
				_glyphIndexes.ToArray(), new Point(location.X, location.Y + fontSize), _advanceWidths.ToArray(), null, null, null, null,
				null, null);

			drawingContext.DrawGlyphRun(brush, glyphRun);
		}

		private static Pen CreateCursorPen()
		{
			DoubleAnimationUsingKeyFrames keyFrames = new DoubleAnimationUsingKeyFrames();
			keyFrames.RepeatBehavior = RepeatBehavior.Forever;
			keyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
			keyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));
			keyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));

			SolidColorBrush brush = new SolidColorBrush(Colors.Black);
			brush.BeginAnimation(SolidColorBrush.OpacityProperty, keyFrames);

			Pen pen = new Pen(brush, 1);

			return pen;
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			_renderedStartLineIndex = Math.Min(_document.LineCount ,Math.Max(0, _renderedStartLineIndex - e.Delta / 40));
			InvalidateVisual();
			e.Handled = true;
		}

		internal Point GetCursorPosition()
		{
			return new Point(_cursorPosition.X, _cursorPosition.Y + fontSize);
		}

		private CharPosition GetChartPosition(Point location)
		{
			for (int index = 0; index < _linesGeometry.Count; index++)
			{
				TextPosition text = _linesGeometry[index];

				if (text == null || text.Top > location.Y || text.Buttom < location.Y)
					continue;

				if (string.IsNullOrEmpty(text.TextAsString.Trim()))
					return new CharPosition(text.TextAsString.Length, text.Line);

				double x = _lineStartOffset - 2;
				int chCount = 0;
				for (int col = 0; col < text.TextAsString.Length; col++)
				{
					char ch = text.TextAsString[col];

					if (ch == '\t')
					{
						chCount += 4 - chCount % 4;
					}
					else
					{
						chCount++;
					}

					double newX = _lineStartOffset + chCount * fontWidth - 2;

					if (location.X <= (x + newX) / 2d)
						return new CharPosition(col, text.Line);
					if ((x + newX) / 2d < location.X && location.X <= newX)
						return new CharPosition(col + 1, text.Line);

					x = newX;
				}

				return new CharPosition(text.TextAsString.Length, text.Line);
			}

			if (_linesGeometry.Count > 0)
				return new CharPosition(_linesGeometry.Last().TextAsString.Length, _linesGeometry.Last().Line);

			return null;
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			OnMouseButtonDown(e);
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			OnTextInputEngine(e);
		}

		public void Insert(int row, int col, string text)
		{
			string line = _document.GetLineText(row);

			SingleActionPair pair = new SingleActionPair(
						new SetTextAction(row, line.Insert(col, text)),
						new SetTextAction(row, line));
			_actionStackExplorer.DoAction(pair);
		}

		public void SetText(int row, string text)
		{
			string line = _document.GetLineText(row);

			SingleActionPair pair = new SingleActionPair(
						new SetTextAction(row, text),
						new SetTextAction(row, line));
			_actionStackExplorer.DoAction(pair);
		}

		public void OnTextInputEngine(TextCompositionEventArgs e)
		{
			string text = e.Text;

			if (_isReadonly)
				return;

			if (string.IsNullOrEmpty(text))
				return;

			int newRow;

			try
			{
				_actionStackExplorer.BeginMacro();

				OnRemoveSelection();

				int newCol = _endSelection.Col;
				newRow = _endSelection.Row;

				Insert(newRow, newCol, text);

				newCol += text.Length;
				SetPosition(new CharPosition(newCol, newRow));
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}

			try
			{
				_actionStackExplorer.BeginMacro();
				DoAutocomplition();
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			} 
			
			ShowHint();
			_document.SetIsChangedToTrue();

			// recalc longest line
			object lineHandle = _document.GetLineHandle(newRow);
			if (_longestLineHandle == lineHandle)
			{
				_longestLineWidth = _document.GetLineWidthInPixels(newRow);
			}
			else
			{
				int lineLength = _document.GetLineLength(newRow);
				int longestLineIndex = _document.GetLineIndex(_longestLineHandle);

				if (longestLineIndex == -1 || lineLength > _document.GetLineLength(longestLineIndex))
				{
					_longestLineHandle = _document.GetLineHandle(newRow);
					_longestLineWidth = _document.GetLineWidthInPixels(newRow);
				}
			}
			//

			e.Handled = true;
		}

		private void DoAutocomplition()
		{
			if (_startSelection != _endSelection)
				return;

			string value = _document.DoAutocomplition(_endSelection);

			if (string.IsNullOrEmpty(value))
				return;

			Insert(_endSelection.Row, _endSelection.Col, value);
			SetPosition(new CharPosition(_endSelection.Col + value.Length, _endSelection.Row));
		}

		internal void RemoveRow(int index)
		{
			string line = _document.GetLineText(index);

			SingleActionPair singleActionPair = new SingleActionPair(
					new SetTextAction(index, string.Empty),
					new SetTextAction(index, line));
			_actionStackExplorer.DoAction(singleActionPair);

			SingleActionPair pair = new SingleActionPair(
						new RemoveLineAction(index),
						new AddLineAction(index));
			_actionStackExplorer.DoAction(pair);
		}

		public void Remove(CharPosition startSelection, CharPosition endSelection)
		{
			if (startSelection == endSelection)
				return;

			CharPosition begin = startSelection < endSelection ? startSelection : endSelection;
			CharPosition end = begin == startSelection ? endSelection : startSelection;

			if (begin.Row == end.Row)
			{
				string line = _document.GetLineText(begin.Row);
				string nextText = line.Substring(0, begin.Col) + line.Substring(end.Col, line.Length - end.Col);

				SingleActionPair pair = new SingleActionPair(
					new SetTextAction(begin.Row, nextText),
					new SetTextAction(begin.Row, line));
				_actionStackExplorer.DoAction(pair);
			}
			else
			{
				for (int index = end.Row - 1; index > begin.Row; index--)
				{
					SingleActionPair action = new SingleActionPair(
						new SetTextAction(index, string.Empty),
						new SetTextAction(index, _document.GetLineText(index)));
					_actionStackExplorer.DoAction(action);

					SingleActionPair pair = new SingleActionPair(
						new RemoveLineAction(index),
						new AddLineAction(index));
					_actionStackExplorer.DoAction(pair);
				}

				string lastPart = _document.GetLineText(begin.Row + 1).Substring(end.Col);

				SingleActionPair action1 = new SingleActionPair(
						new SetTextAction(begin.Row + 1, string.Empty),
						new SetTextAction(begin.Row + 1, _document.GetLineText(begin.Row + 1)));
				_actionStackExplorer.DoAction(action1);

				SingleActionPair actionPair = new SingleActionPair(
					new RemoveLineAction(begin.Row + 1),
					new AddLineAction(begin.Row + 1));
				_actionStackExplorer.DoAction(actionPair);

				string line = _document.GetLineText(begin.Row);
				string nextText = line.Substring(0, begin.Col) + lastPart;

				SingleActionPair singleActionPair = new SingleActionPair(
					new SetTextAction(begin.Row, nextText),
					new SetTextAction(begin.Row, line));
				_actionStackExplorer.DoAction(singleActionPair);
			}
		}

		public void Remove(int row, int colStart, int length)
		{
			Remove(new CharPosition(colStart, row), new CharPosition(colStart + length, row));
		}

		public void Insert(HintItem word)
		{
			try
			{
				_actionStackExplorer.BeginMacro();

				Remove(_endSelection.Row, _endSelection.Col - word.Prefix.Length, word.Prefix.Length);
				
				Insert(_endSelection.Row, _endSelection.Col - word.Prefix.Length, word.Prefix);
				Insert(_endSelection.Row, _endSelection.Col, word.Value);

				SetPosition(new CharPosition(_endSelection.Col + word.Value.Length, _endSelection.Row));

				if (_longestLineHandle == _document.GetLineHandle(_endSelection.Row))
				{
					_longestLineWidth = _document.GetLineWidthInPixels(_endSelection.Row);
				}
				else 
				{
					int lineLength = _document.GetLineLength(_endSelection.Row);
					int longestLineIndex = _document.GetLineIndex(_longestLineHandle);

					if (longestLineIndex == -1 || lineLength > _document.GetLineLength(longestLineIndex))
					{
						_longestLineHandle = _document.GetLineHandle(_endSelection.Row);
						_longestLineWidth = _document.GetLineWidthInPixels(_endSelection.Row);
					}
				}

				_document.SetIsChangedToTrue();
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}
		}

		public int ReplaceAllText(string text, bool useRegister, string value)
		{
			StringComparison stringComparison = useRegister ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			if (!useRegister)
				text = text.Trim();

			try
			{
				_actionStackExplorer.BeginMacro();

				int count = 0, startPos = -1;
				for (int index = 0; index < _document.LineCount; index++)
				{
					string line = _document.GetLineText(index);
					int currentCount = count;

					while ((startPos = line.IndexOf(text, stringComparison)) >= 0)
					{
						line = line.Remove(startPos, text.Length).Insert(startPos, value);
						count++;
					}

					if (count > currentCount)
						SetText(index, line);
				}

				return count;
			}
			finally
			{
				_actionStackExplorer.EndMacro();
				
				InvalidateVisual();
			}
		}

		public void ReplaceText(string text, bool useRegister, string value)
		{
			try
			{
				_actionStackExplorer.BeginMacro();

				string selectedValue = GetSelectedText();

				StringComparison stringComparison = useRegister ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

				if (!useRegister)
					text = text.Trim();

				if (text.Equals(selectedValue, stringComparison))
				{
					OnRemoveSelection();
					Insert(_startSelection.Row, _startSelection.Col, value);
				}

				FindText(text, useRegister);
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}
		}

		public void FindText(string text, bool useRegister)
		{
			StringComparison stringComparison = useRegister ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			if (!useRegister)
				text = text.Trim();

			string currentLine = _document.GetLineText(_endSelection.Row);
			int startColSearch = _endSelection.Col;

			if (_startSelection.Row == _endSelection.Row && Math.Abs(_startSelection.Col - _endSelection.Col) != text.Length)
				startColSearch = Math.Min(_startSelection.Col, _endSelection.Col);

			string currentText = startColSearch >= currentLine.Length ? string.Empty : currentLine.Substring(startColSearch);

			if (currentText.IndexOf(text, stringComparison) >= 0)
			{
				int row = _endSelection.Row;

				ShowLine(row);

				int startIndex = currentLine.Length - (currentText.Length - currentText.IndexOf(text, stringComparison));
				int endIndex = startIndex + text.Length;

				SetPosition(new CharPosition(startIndex, row), new CharPosition(endIndex, row), true);
				return;
			}

			for (int index = _endSelection.Row + 1; index < _document.LineCount; index++)
			{
				string line = _document.GetLineText(index);

				if (line.IndexOf(text, stringComparison) >= 0)
				{
					ShowLine(index);

					int startIndex = line.IndexOf(text, stringComparison);
					int endIndex = startIndex + text.Length;

					SetPosition(new CharPosition(startIndex, index), new CharPosition(endIndex, index), true);
					return;
				}
			}

			for (int index = 0; index < _endSelection.Row; index++)
			{
				string line = _document.GetLineText(index);

				if (line.IndexOf(text, stringComparison) >= 0)
				{
					ShowLine(index);

					int startIndex = line.IndexOf(text, stringComparison);
					int endIndex = startIndex + text.Length;

					SetPosition(new CharPosition(startIndex, index), new CharPosition(endIndex, index), true);
					return;
				}
			}
		}

		public void ShowLine(int number)
		{
			_renderedStartLineIndex = Math.Min(Math.Max(0, number - _renderedLineCount/2), _document.LineCount - 1);
			SetPosition(new CharPosition(0, Math.Min(Math.Max(0, number), _document.LineCount - 1)));
		}
		
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			OnKeyDownEngine(e);
		}

		public void Append(int row, string text)
		{
			string line = _document.GetLineText(row);

			SingleActionPair pair = new SingleActionPair(
						new SetTextAction(row, line + text),
						new SetTextAction(row, line));
			_actionStackExplorer.DoAction(pair);
		}

		public int InsertLine(int index, string text, bool addTabsToStart)
		{
			StringBuilder prefix = new StringBuilder();

			if (addTabsToStart && index > 0)
			{
				string prevText = _document.GetLineText(index - 1);

				for (int marker = 0; marker < prevText.Length; marker++)
				{
					char ch = prevText[marker];

					if (ch == ' ' || ch == '\t')
						prefix.Append(ch);
					else break;
				}
			}

			SingleActionPair newLine = new SingleActionPair(
				new AddLineAction(index),
				new RemoveLineAction(index));
			_actionStackExplorer.DoAction(newLine);

			SingleActionPair addText = new SingleActionPair(
				new SetTextAction(index, prefix + text),
				new SetTextAction(index, string.Empty));
			_actionStackExplorer.DoAction(addText);

			return prefix.Length;
		}

		public void OnKeyDownEngine(KeyEventArgs e)
		{
			if (OnKeyDownEvent != null)
			{
				OnKeyDownEvent(e);

				if (e.Handled)
					return;
			}

			bool hasCtrl = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) > 0;
			bool hasAlt = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) > 0;
			bool hasShift = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) > 0;

			if (e.Key == Key.F3 && !hasAlt && !hasCtrl && !hasShift)
			{
				SetTopPanelVisibility(TopPanelVisibility.Word);
				FindText(_findWordCtrl.GetText(), Options.Instance.IsStrongSearch);

				e.Handled = true;
			}
			else if (e.Key == Key.F && hasCtrl && hasAlt)
			{
				FormatSelectedLines();
				e.Handled = true;
			}
			else if (e.Key == Key.F && hasCtrl)
			{
				if (_startSelection.Row == _endSelection.Row)
				{
					string line = _document.GetLineText(_endSelection.Row);
					string text = line.Substring(Math.Min(_startSelection.Col, _endSelection.Col), Math.Abs(_startSelection.Col - _endSelection.Col));

					_findWordCtrl.SetText(text);
				}
				else
				{
					_findWordCtrl.SetText(string.Empty);
				}
				
				SetTopPanelVisibility(TopPanelVisibility.Word);
				Keyboard.Focus(_findWordCtrl.txtWord);

				e.Handled = true;
			}
			else if (e.Key == Key.H && hasCtrl)
			{
				if (_startSelection.Row == _endSelection.Row)
				{
					string line = _document.GetLineText(_endSelection.Row);
					string text = line.Substring(Math.Min(_startSelection.Col, _endSelection.Col), Math.Abs(_startSelection.Col - _endSelection.Col));

					_findWordCtrl.SetText(text, false, true);
				}
				else
				{
					_findWordCtrl.SetText(string.Empty, false, true);
				}
				
				SetTopPanelVisibility(TopPanelVisibility.Word);
				Keyboard.Focus(_findWordCtrl.txtWord);

				e.Handled = true;
			}
			else if (e.Key == Key.I && hasCtrl)
			{
				_findWordCtrl.SetText(string.Empty, true, false);
				
				SetTopPanelVisibility(TopPanelVisibility.Word);
				Keyboard.Focus(_findWordCtrl.txtWord);

				e.Handled = true;
			}
			else if (e.Key == Key.G && hasCtrl)
			{
				_findLineCtrl.SetLine(_endSelection.Row);
				SetTopPanelVisibility(TopPanelVisibility.Line);
				Keyboard.Focus(_findLineCtrl.txtNumber);

				e.Handled = true;
			}
			else if (e.Key == Key.Tab && !hasCtrl && !hasAlt)
			{
				if (_isReadonly)
					return;

				try
				{
					_actionStackExplorer.BeginMacro();

					if (_startSelection.Row == _endSelection.Row)
					{
						OnRemoveSelection();

						Insert(_startSelection.Row, _startSelection.Col, "\t");
						SetPosition(new CharPosition(_startSelection.Col + 1, _startSelection.Row));

						if (_longestLineHandle == _document.GetLineHandle(_startSelection.Row))
						{
							_longestLineWidth = _document.GetLineWidthInPixels(_startSelection.Row);
						}
						else 
						{
							int lineLength = _document.GetLineLength(_startSelection.Row);
							int longestLineIndex = _document.GetLineIndex(_longestLineHandle);

							if (longestLineIndex == -1 || lineLength > _document.GetLineLength(longestLineIndex))
							{
								_longestLineHandle = _document.GetLineHandle(_startSelection.Row);
								_longestLineWidth = _document.GetLineWidthInPixels(_startSelection.Row);
							}
						}
					}
					else
					{
						for (int rowIndex = Math.Min(_startSelection.Row, _endSelection.Row), endRow = Math.Max(_startSelection.Row, _endSelection.Row); rowIndex <= endRow; rowIndex++)
						{
							Insert(rowIndex, 0, "\t");

							if (_longestLineHandle == _document.GetLineHandle(rowIndex))
							{
								_longestLineWidth = _document.GetLineWidthInPixels(rowIndex);
							}
							else
							{
								int lineLength = _document.GetLineLength(rowIndex);
								int longestLineIndex = _document.GetLineIndex(_longestLineHandle);

								if (longestLineIndex == -1 || lineLength > _document.GetLineLength(longestLineIndex))
								{
									_longestLineHandle = _document.GetLineHandle(rowIndex);
									_longestLineWidth = _document.GetLineWidthInPixels(rowIndex);
								}
							}
						}

						SetPosition(new CharPosition(_startSelection.Col + 1, _startSelection.Row),
										new CharPosition(_endSelection.Col + 1, _endSelection.Row), true);
					}

					_document.SetIsChangedToTrue();
				}
				finally
				{
					_actionStackExplorer.EndMacro();
				}

				e.Handled = true;
			}
			else if (e.Key == Key.D && hasCtrl)
			{
				if (_isReadonly)
					return;

				OnDuplicate();

				e.Handled = true;
			}
			else if (e.Key == Key.C && hasCtrl)
			{
				OnCopy();
				e.Handled = true;
			}
			else if (e.Key == Key.B && hasCtrl)
			{
				if (_endSelection == _startSelection)
					TryOpenFileUnderCursor(_startSelection);
				
				e.Handled = true;
			}
			else if (e.Key == Key.X && hasCtrl)
			{
				OnCut();

				e.Handled = true;
				InvalidateVisual();
			}
			else if (e.Key == Key.Z && hasCtrl)
			{
				_actionStackExplorer.UndoAction();

				e.Handled = true;
				InvalidateVisual();
			}
			else if (e.Key == Key.Y && hasCtrl)
			{
				_actionStackExplorer.NextAction();

				e.Handled = true;
				InvalidateVisual();
			}
			else if (e.Key == Key.V && hasCtrl)
			{
				if (_isReadonly)
					return;

				OnPaste();

				e.Handled = true;
			}
			else if (e.Key == Key.A && hasCtrl)
			{
				OnSelectAll();
				
				e.Handled = true;
			}
			else if (e.Key == Key.Delete)
			{
				if (_isReadonly)
					return;

				try
				{
					_actionStackExplorer.BeginMacro();

					if (_startSelection != _endSelection)
					{
						OnRemoveSelection();
					}
					else
					{
						CharPosition newPosition = GetNextPosition();

						if (_startSelection == newPosition)
						{
							// nothing
						}
						else if (_startSelection.Row != newPosition.Row)
						{
							Append(_startSelection.Row, _document.GetLineText(newPosition.Row));
							RemoveRow(newPosition.Row);
						}
						else
						{
							Remove(_startSelection.Row, _startSelection.Col, 1);
						}
					}

					ShowHint();
					_document.SetIsChangedToTrue();
				}
				finally
				{
					_actionStackExplorer.EndMacro();
				}

				e.Handled = true;
				InvalidateVisual();
			}
			else if (e.Key == Key.Back)
			{
				if (_isReadonly)
					return;

				try
				{
					_actionStackExplorer.BeginMacro();

					if (_startSelection != _endSelection)
					{
						OnRemoveSelection();
					}
					else
					{
						CharPosition newPosition = GetBackPosition();

						if (_startSelection == newPosition)
						{
							// nothing
						}
						else if (_startSelection.Row != newPosition.Row)
						{
							Append(newPosition.Row, _document.GetLineText(_startSelection.Row));
							RemoveRow(_startSelection.Row);

							SetPosition(newPosition);
						}
						else
						{
							Remove(_startSelection.Row, _startSelection.Col - 1, 1);
							SetPosition(new CharPosition(_startSelection.Col - 1, _startSelection.Row));
						}
					}

					ShowHint();
					_document.SetIsChangedToTrue();
				}
				finally
				{
					_actionStackExplorer.EndMacro();
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Left && hasCtrl && hasShift)
			{
				SetPosition(null, GetBackCtrlPosition(), true);
			
				e.Handled = true;
			}
			else if (e.Key == Key.Left && hasShift)
			{
				SetPosition(null, GetBackPosition(), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Left && hasCtrl)
			{
				if (_startSelection != _endSelection)
				{
					int col = _startSelection < _endSelection ? _startSelection.Col : _endSelection.Col;
					int row = _startSelection < _endSelection ? _startSelection.Row : _endSelection.Row;

					SetPosition(new CharPosition(col, row));
				}
				else
				{
					SetPosition(GetBackCtrlPosition());
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Left)
			{
				if (_startSelection != _endSelection)
				{
					int col = _startSelection < _endSelection ? _startSelection.Col : _endSelection.Col;
					int row = _startSelection < _endSelection ? _startSelection.Row : _endSelection.Row;

					SetPosition(new CharPosition(col, row));
				}
				else
				{
					CharPosition position = GetBackPosition();
					SetPosition(position);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Right && hasCtrl && hasShift)
			{
				SetPosition(null, GetNextCtrlPosition(), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Right && hasShift)
			{
				SetPosition(null, GetNextPosition(), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Right && hasCtrl)
			{
				if (_startSelection != _endSelection)
				{
					int col = _startSelection > _endSelection ? _startSelection.Col : _endSelection.Col;
					int row = _startSelection > _endSelection ? _startSelection.Row : _endSelection.Row;

					SetPosition(new CharPosition(col, row));
				}
				else
				{
					SetPosition(GetNextCtrlPosition());
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Right)
			{
				if (_startSelection != _endSelection)
				{
					int col = _startSelection > _endSelection ? _startSelection.Col : _endSelection.Col;
					int row = _startSelection > _endSelection ? _startSelection.Row : _endSelection.Row;

					SetPosition(new CharPosition(col, row));
				}
				else
				{
					CharPosition position = GetNextPosition();
					SetPosition(position);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Up && hasShift)
			{
				SetPosition(null, GetUpPosition(), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Up)
			{
				if (_popup.IsOpen)
				{
					_popup.OnUp();
				}
				else
				{
					SetPosition(GetUpPosition());
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Down && hasShift)
			{
				SetPosition(null, GetDownPosition(), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				if (_popup.IsOpen)
				{
					_popup.OnDown();
				}
				else
				{
					SetPosition(GetDownPosition());
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				if (_isReadonly)
					return;

				if (_popup.IsOpen)
				{
					_popup.OnEnter();
					e.Handled = true;
				}
				else
				{
					try
					{
						_actionStackExplorer.BeginMacro();
						OnRemoveSelection();

						string line = _document.GetLineText(_startSelection.Row);
						string text = line.Substring(_startSelection.Col);

						Remove(_startSelection.Row, _startSelection.Col, text.Length);

						int row = _startSelection.Row + 1;
						int col = InsertLine(row, text, true);
						SetPosition(new CharPosition(col, row));
					}
					finally
					{
						_actionStackExplorer.EndMacro();
					}

					e.Handled = true;
				}

				_document.SetIsChangedToTrue();
			}
			else if (e.Key == Key.Escape)
			{
				SetPosition(_endSelection);
				SetTopPanelVisibility(TopPanelVisibility.None);

				e.Handled = true;
				_popup.IsOpen = false;
			}
			else if (e.Key == Key.Home && hasShift)
			{
				SetPosition(null, new CharPosition(0, _endSelection.Row), true);

				e.Handled = true;
			}
			else if (e.Key == Key.Home)
			{
				SetPosition(new CharPosition(0, _endSelection.Row));

				e.Handled = true;
			}
			else if (e.Key == Key.End && hasShift)
			{
				SetPosition(null, new CharPosition(_document.GetLineLength(_endSelection.Row), _endSelection.Row), true);

				e.Handled = true;
			}
			else if (e.Key == Key.End)
			{
				SetPosition(new CharPosition(_document.GetLineLength(_endSelection.Row), _endSelection.Row));
				e.Handled = true;
			}
			else if (e.Key == Key.Oem2 && hasCtrl && hasShift)
			{
				CommentSelectedText();
				e.Handled = true;
			}
			else if (e.Key == Key.Oem2 && hasCtrl)
			{
				CommentSelectedLines();
				e.Handled = true;
			}
			else if (e.Key == Key.Space && hasCtrl)
			{
				if (!IsReadonly)
					ShowHint();

				e.Handled = true;
			}
		}

		private void FormatSelectedLines()
		{
			int startRow = Math.Min(_startSelection.Row, _endSelection.Row);
			int endRow = Math.Max(_startSelection.Row, _endSelection.Row);

			FormatLines(startRow, endRow);
			_document.SetIsChangedToTrue();
			InvalidateVisual();
		}

		private void FormatLines(int startRow, int endRow)
		{
			try
			{
				_actionStackExplorer.BeginMacro();

				string[] rows = _document.GetFormattedLines(startRow, endRow);

				if (rows.Length == 0)
					return;

				SetPosition(new CharPosition(0, startRow), new CharPosition(rows.Last().Length, startRow + rows.Length - 1), true);

				for (int index = endRow; index >= startRow; index--)
					RemoveRow(index);

				for (int index = rows.Length - 1; index >= 0; index--)
					InsertLine(startRow, rows[index], false);

				SetPosition(new CharPosition(0, startRow), new CharPosition(rows.Last().Length, startRow + rows.Length - 1), true);
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}
		}

		private void CommentSelectedLines()
		{
			if (!IsReadonly)
			{
				CharPosition maxPosition = _startSelection > _endSelection ? _startSelection : _endSelection;
				CharPosition minPosition = _startSelection < _endSelection ? _startSelection : _endSelection;

				try
				{
					_actionStackExplorer.BeginMacro();

					int startRow = minPosition.Row;
					int endRow = maxPosition.Row;

					int startCol = minPosition.Col;
					int endCol = maxPosition.Col;

					IList<TextLineChangesActionBase> info = _document.CommentLines(startRow, endRow);

					if (info != null && info.Count > 0)
					{
						for (int marker = 0; marker < info.Count; marker++)
						{
							TextLineChangesActionBase item = info[marker];
							MakeTextChangesAction(item, startRow, ref startCol, endRow, ref endCol);
						}
					}

					SetPosition(new CharPosition(startCol, startRow), new CharPosition(endCol, endRow), true);
				}
				finally
				{
					_actionStackExplorer.EndMacro();
				}

				InvalidateVisual();
				_document.SetIsChangedToTrue();
				
			}
		}

		private void MakeTextChangesAction(TextLineChangesActionBase action, int startRow, ref int startCol, int endRow, ref int endCol)
		{
			if (action is TextLineInsertAction)
			{
				TextLineInsertAction item = (TextLineInsertAction)action;
				Insert(item.Position.Row, item.Position.Col, item.Text);

				if (item.Position.Row == startRow && item.Position.Col < startCol)
					startCol += item.Text.Length;
				if (item.Position.Row == endRow && item.Position.Col <= endCol)
					endCol += item.Text.Length;
			}
			else if (action is TextLineRemoveAction)
			{
				TextLineRemoveAction item = (TextLineRemoveAction)action;
				Remove(item.Position, new CharPosition(item.Position.Col + item.TextLength, item.Position.Row));

				// TODO: make it smarter
				if (item.Position.Row == startRow && item.Position.Col < startCol)
					startCol -= item.TextLength;
				if (item.Position.Row == endRow && item.Position.Col <= endCol)
					endCol -= item.TextLength;
			}
			else
			{
				Debug.Fail("Here's unsupported action.");
			}
		}

		private void CommentSelectedText()
		{
			if (!IsReadonly)
			{
				CharPosition maxPosition = _startSelection > _endSelection ? _startSelection : _endSelection;
				CharPosition minPosition = _startSelection < _endSelection ? _startSelection : _endSelection;

				IList<TextLineChangesActionBase> info = _document.CommentText(minPosition, maxPosition);

				if (info != null && info.Count > 0)
				{
					try
					{
						_actionStackExplorer.BeginMacro();

						int start = minPosition.Col;
						int end = maxPosition.Col;

						for (int index = 0; index < info.Count; index++)
						{
							TextLineChangesActionBase target = info[index];
							MakeTextChangesAction(target, minPosition.Row, ref start, maxPosition.Row, ref end);
						}

						SetPosition(new CharPosition(start, minPosition.Row), new CharPosition(end, maxPosition.Row), true);
					}
					finally
					{
						_actionStackExplorer.EndMacro();
					}

					InvalidateVisual();
					_document.SetIsChangedToTrue();
				}
			}
		}

		internal void OnDuplicate()
		{
			try
			{
				_actionStackExplorer.BeginMacro();

				if (_startSelection == _endSelection)
				{
					InsertLine(_startSelection.Row + 1, _document.GetLineText(_startSelection.Row), false);
					SetPosition(new CharPosition(_startSelection.Col, _startSelection.Row + 1));
					_document.SetIsChangedToTrue();
				}
				else
				{
					string selectedText = GetSelectedText();
					string[] selectedLines = selectedText.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

					if (selectedLines.Length > 0)
					{
						CharPosition start = _startSelection < _endSelection ? _startSelection : _endSelection;
						CharPosition end = _startSelection > _endSelection ? _startSelection : _endSelection;

						for (int index = 0, col = end.Col; index < selectedLines.Length; index++, col = 0)
						{
							string pasteText = selectedLines[index];

							Insert(end.Row + index, col, pasteText);
							string line = _document.GetLineText(end.Row + index);

							if (index != selectedLines.Length - 1)
							{
								string newLine = line.Substring(col + pasteText.Length);
								InsertLine(end.Row + index + 1, newLine, false);
								Remove(end.Row + index, line.Length - newLine.Length, newLine.Length);
							}
							else
							{
								SetPosition(new CharPosition(end), new CharPosition(col + pasteText.Length, end.Row + index), true);
							}
						}
					}

					//					SetPosition(new CharPosition(_startSelection > _endSelection ? _startSelection : _endSelection), new CharPosition(col, row));
					_document.SetIsChangedToTrue();
				}
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}
		}

		private void OnPaste()
		{
			try
			{
				_actionStackExplorer.BeginMacro();

				string text = Clipboard.GetText();
				string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

				OnRemoveSelection();

				if (lines.Length > 0)
				{
					for (int index = 0, col = _startSelection.Col; index < lines.Length; index++, col = 0)
					{
//						string line = _document.GetLineText(_startSelection.Row + index);
						string pasteText = lines[index];

						Insert(_startSelection.Row + index, col, pasteText);
						string line = _document.GetLineText(_startSelection.Row + index);

						if (index != lines.Length - 1)
						{
							string newLine = line.Substring(col + pasteText.Length);
							InsertLine(_startSelection.Row + index + 1, newLine, false);
							Remove(_startSelection.Row + index, line.Length - newLine.Length, newLine.Length);
						}
						else
						{
							SetPosition(new CharPosition(col + pasteText.Length, _startSelection.Row + index));
						}
					}
				}

				_document.SetIsChangedToTrue();

				_longestLineWidth = -1;
				FindTheLongestLine();
			}
			finally
			{
				_actionStackExplorer.EndMacro();
			}
		}
		
		private void OnSelectAll()
		{
			SetPosition(new CharPosition(0, 0), new CharPosition(_document.GetLineLength(_document.LineCount - 1), _document.LineCount - 1), true);
		}

		private void OnCut()
		{
			OnCopy();
			OnRemoveSelection();
			_document.SetIsChangedToTrue();
		}

		private bool TryOpenFileUnderCursor(CharPosition charPosition)
		{
			string text = _document.GetLineText(charPosition.Row);

			if (string.IsNullOrEmpty(text))
				return false;

			char[] invalidPathChars = Path.GetInvalidPathChars();

			int start = charPosition.Col;
			for (int index = charPosition.Col - 1; index >= 0; index--)
			{
				char ch = text[index];
				if (invalidPathChars.Contains(ch) || ch == '%' || ch == '$')
					break;

				start = index;
			}

			int end = charPosition.Col;
			for (int index = charPosition.Col + 1; index < text.Length; index++)
			{
				char ch = text[index];

				if (invalidPathChars.Contains(ch) || ch == '%' || ch == '$')
					break;

				end = index;
			}

			string path = text.Substring(start, Math.Min(Math.Max(0, end - start + 1), text.Length - start));

			if (!invalidPathChars.Any(path.Contains))
			{
				path = Path.Combine(Path.GetDirectoryName(_document.FilePath), path);
				if (File.Exists(path))
				{
					_ufeTextBox.Owner.OpenEditor(path);
					return true;
				}
			}

			return false;
		}

		private void ShowHint()
		{
			_popup.Commands = _document.GetHints(_endSelection.Row, _endSelection.Col);
			_popup.IsOpen = _popup.Commands.Length > 0;
		}

		private void HideHint()
		{
			_popup.IsOpen = false;
		}

		private void SetPosition(CharPosition position)
		{
			SetPosition(position, position, true);
		}

		internal void SetPosition(CharPosition start, CharPosition end, bool registerInActionStack)
		{
			if (start != null)
			{
				if (start.Row >= _document.LineCount)
					start = new CharPosition(start.Col, _document.LineCount - 1);
				if (start.Row < 0)
					start = new CharPosition(start.Col, 0);

				int lineLength = _document.GetLineLength(start.Row);
				if (start.Col > lineLength)
					start = new CharPosition(lineLength, start.Row);
				if (start.Col < 0)
					start = new CharPosition(0, start.Row);
			}

			if (end != null)
			{
				if (end.Row >= _document.LineCount)
					end = new CharPosition(end.Col, _document.LineCount - 1);
				if (end.Row < 0)
					end = new CharPosition(end.Col, 0);

				int lineLength = _document.GetLineLength(end.Row);
				if (end.Col > lineLength)
					end = new CharPosition(lineLength, end.Row);
				if (end.Col < 0)
					end = new CharPosition(0, end.Row);
			}

			if (!registerInActionStack)
			{
				_startSelection = start;
				_endSelection = end;

				InvalidateVisual();
				HideHint();

				_copyCommand.RiseCanExecuteChanged();
				_cutCommand.RiseCanExecuteChanged();
				_deleteCommand.RiseCanExecuteChanged();

				return;
			}
			
			if (start != null || end != null)
				AddPositionChangedEvent(start ?? _startSelection, end ?? _endSelection, _startSelection, _endSelection);
		}

		public void AddPositionChangedEvent(CharPosition startSelection, CharPosition endSelection, CharPosition oldStartSelection, CharPosition oldEndSelection)
		{
			SingleActionPair pair = new SingleActionPair(
						new SetPositionAction(startSelection, endSelection),
						new SetPositionAction(oldStartSelection, oldEndSelection));
			_actionStackExplorer.DoAction(pair);
		}

		private CharPosition GetBackCtrlPosition()
		{
			int position = _endSelection.Col;

			if (position == 0)
				return GetBackPosition();

			string line = _document.GetLineText(_endSelection.Row);

			string text = line.Substring(0, position).TrimEnd(CtrlSplitChars);
			int newIndex = Math.Max(text.LastIndexOfAny(CtrlSplitChars) + 1, 0);
			return new CharPosition(newIndex, _endSelection.Row);
		}

		private CharPosition GetNextCtrlPosition()
		{
			string line = _document.GetLineText(_endSelection.Row);
			int position = _endSelection.Col;

			if (position == line.Length)
				return GetNextPosition();

			string text = line.Substring(position, line.Length - position).TrimStart(CtrlSplitChars);
			int firstIndex = text.IndexOfAny(CtrlSplitChars);

			if (firstIndex < 0)
				return new CharPosition(line.Length, _endSelection.Row);

			int newIndex = line.Length - (text.Length - firstIndex);
			newIndex = line.Length - line.Substring(newIndex).TrimStart(CtrlSplitChars).Length;
			return new CharPosition(newIndex, _endSelection.Row);
		}

		private CharPosition GetDownPosition()
		{
			int col = _endSelection.Col;
			int row = _endSelection.Row;

			if (row < _document.LineCount - 1)
				row++;

			col = Math.Min(col, _document.GetLineLength(row));

			return new CharPosition(col, row);
		}

		private CharPosition GetUpPosition()
		{
			int col = _endSelection.Col;
			int row = _endSelection.Row;

			if (row > 0)
				row--;

			col = Math.Min(col, _document.GetLineLength(row));

			return new CharPosition(col, row);
		}

		private CharPosition GetBackPosition()
		{
			int col = _endSelection.Col;
			int row = _endSelection.Row;

			if (col > 0)
				col--;
			else if (row > 0)
			{
				row--;
				col = _document.GetLineLength(row);
			}

			return new CharPosition(col, row);
		}

		private CharPosition GetNextPosition()
		{
			int col = _endSelection.Col;
			int row = _endSelection.Row;

			if (col < _document.GetLineLength(row))
				col++;
			else if (row < _document.LineCount - 1)
			{
				row++;
				col = 0;
			}

			return new CharPosition(col, row);
		}

		private void OnRemoveSelection()
		{
			Remove(_startSelection, _endSelection);

			int newCol = _startSelection < _endSelection ? _startSelection.Col : _endSelection.Col;
			int newRow = _startSelection < _endSelection ? _startSelection.Row : _endSelection.Row;

			SetPosition(new CharPosition(newCol, newRow));
			_document.SetIsChangedToTrue();
		}

		private void OnCopy()
		{
			DataObject data = new DataObject();

			CopyAsText(data);
			CopyAsRtf(data);

			Clipboard.SetDataObject(data, true);
		}

		private bool IsCopyEnable()
		{
			return _startSelection != _endSelection;
		}

		private bool IsCutEnable()
		{
			return !IsReadonly && IsCopyEnable();
		}

		private void CopyAsRtf(DataObject data)
		{
			FlowDocument flowDocument = new FlowDocument();

			if (_startSelection.Row == _endSelection.Row)
			{
				Paragraph line = new Paragraph();
				flowDocument.Blocks.Add(line);

				foreach (UfeTextStyle style in _document.GetLineStyle(_startSelection.Row, Math.Min(_startSelection.Col, _endSelection.Col), Math.Abs(_startSelection.Col - _endSelection.Col)))
				{
					line.Inlines.Add(new Run(style.Text.ToString()) { Foreground = style.Brush, FontFamily = Typeface.FontFamily, FontSize = fontSize, FontStyle = Typeface.Style, FontWeight = Typeface.Weight, FontStretch = Typeface.Stretch });
				}
			}
			else
			{
				CharPosition start = _startSelection < _endSelection ? _startSelection : _endSelection;
				CharPosition end = start == _startSelection ? _endSelection : _startSelection;

				for (int index = start.Row; index <= end.Row; index++)
				{
					Paragraph paragraph = new Paragraph();
					flowDocument.Blocks.Add(paragraph);

					if (index == start.Row)
					{
						foreach (UfeTextStyle style in _document.GetLineStyle(index, start.Col, _document.GetLineLength(index) - start.Col))
						{
							paragraph.Inlines.Add(new Run(style.Text.ToString()) { Foreground = style.Brush, FontFamily = Typeface.FontFamily, FontSize = fontSize, FontStyle = Typeface.Style, FontWeight = Typeface.Weight, FontStretch = Typeface.Stretch });
						}
					}
					else if (index == end.Row)
					{
						foreach (UfeTextStyle style in _document.GetLineStyle(index, 0, end.Col))
						{
							paragraph.Inlines.Add(new Run(style.Text.ToString()) { Foreground = style.Brush, FontFamily = Typeface.FontFamily, FontSize = fontSize, FontStyle = Typeface.Style, FontWeight = Typeface.Weight, FontStretch = Typeface.Stretch });
						}
					}
					else
					{
						foreach (UfeTextStyle style in _document.GetLineStyle(index))
						{
							paragraph.Inlines.Add(new Run(style.Text.ToString()) { Foreground = style.Brush, FontFamily = Typeface.FontFamily, FontSize = fontSize, FontStyle = Typeface.Style, FontWeight = Typeface.Weight, FontStretch = Typeface.Stretch });
						}
					}
				}
			}

			TextRange content = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
			StringBuilder result = new StringBuilder();

			using (MemoryStream stream = new MemoryStream())
			{
				content.Save(stream, DataFormats.Rtf);
				stream.Flush();
				stream.Position = 0;
				result.Append(new StreamReader(stream).ReadToEnd());
			}

			data.SetText(result.ToString(), TextDataFormat.Rtf);
		}

		private string GetSelectedText()
		{
			StringBuilder builder = new StringBuilder();

			CharPosition start = _startSelection < _endSelection ? _startSelection : _endSelection;
			CharPosition end = start == _startSelection ? _endSelection : _startSelection;

			if (start == end)
			{
				return string.Empty;
			}
			else if (start.Row == end.Row)
			{
				return _document.GetLineText(start.Row).Substring(start.Col, end.Col - start.Col);
			}
			else
			{
				for (int index = start.Row; index <= end.Row; index++)
				{
					string line = _document.GetLineText(index);

					if (index == start.Row)
					{
						builder.AppendLine(line.Substring(start.Col));
					}
					else if (index == end.Row)
					{
						builder.Append(line.Substring(0, end.Col));
					}
					else
					{
						builder.AppendLine(line);
					}
				}
			}

			return builder.ToString();
		}

		private void CopyAsText(DataObject data)
		{
			string selectedString = GetSelectedText();

			data.SetText(selectedString, TextDataFormat.Text);
			data.SetText(selectedString, TextDataFormat.UnicodeText);
		}

		private Tuple<CharPosition, CharPosition> GetWordSelection(CharPosition middleOfWord)
		{
			string line = _document.GetLineText(middleOfWord.Row);
			int start = Math.Max(line.LastIndexOfAny(CtrlSplitChars, Math.Min(middleOfWord.Col, line.Length - 1)) + 1, 0);
			int end = line.IndexOfAny(CtrlSplitChars, middleOfWord.Col);

			if (end < 0)
				end = line.Length;

			return new Tuple<CharPosition, CharPosition>(new CharPosition(start, middleOfWord.Row), new CharPosition(end, middleOfWord.Row));
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed /*|| e.RightButton == MouseButtonState.Pressed*/)
			{
				Point location = e.GetPosition(this);
				CharPosition temp = GetChartPosition(location);

				if (temp != null)
				{
					if (_isLastMouseDownDouble)
					{
						Tuple<CharPosition, CharPosition> startSelection = GetWordSelection(_lastMouseDownPosition);
						Tuple<CharPosition, CharPosition> endSelection = GetWordSelection(temp);

						if (temp < _lastMouseDownPosition)
						{
							SetPosition(startSelection.Item2, endSelection.Item1, true);
						}
						else
						{
							SetPosition(startSelection.Item1, endSelection.Item2, true);

						}
					}
					else
					{
						SetPosition(null, temp, true);
					}
				}
			}

			e.Handled = true;
		}

		protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
		{
//			OnMouseButtonUp(e);
			_contextMenu.IsOpen = true;
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
//			OnMouseButtonDown(e);
		}

		private void OnMouseButtonUp(MouseButtonEventArgs e)
		{
			if (_isLastMouseDownDouble)
			{
				/*Point location = e.GetPosition(this);
				CharPosition temp = GetChartPosition(location);

				if (temp != null)
				{
					UfeTextLineBase line = _document.GetLine(temp.Row);
//					int start = Math.Max(line.TextAsString.LastIndexOfAny(CtrlSplitChars, Math.Min(temp.Col, line.TextAsString.Length - 1)) + 1, 0);
					int end = line.TextAsString.IndexOfAny(CtrlSplitChars, temp.Col);

					if (end < 0)
						end = line.TextAsString.Length;

					SetPosition(null, new CharPosition(end, temp.Row));
				}*/
			}
			else
			{
				Point location = e.GetPosition(this);
				CharPosition temp = GetChartPosition(location);

				if (temp != null)
					SetPosition(null, temp, true);
			}

			e.Handled = true;
			Keyboard.Focus(this);
			ReleaseMouseCapture();
		}

		private void OnMouseButtonDown(MouseButtonEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				Point location = e.GetPosition(this);
				CharPosition temp = GetChartPosition(location);
				bool success = TryOpenFileUnderCursor(temp);

				if (success)
					return;
			}

			if (DateTime.UtcNow - _lastMouseDownEvent > TimeSpan.FromMilliseconds(300))
			{
				_isLastMouseDownDouble = false;

				if (!Keyboard.IsKeyDown(Key.LeftShift))
				{
					Point location = e.GetPosition(this);
					CharPosition temp = GetChartPosition(location);

					if (temp != null)
						SetPosition(temp);

					_lastMouseDownPosition = temp;
				}
			}
			else
			{
				_isLastMouseDownDouble = true;

				Point location = e.GetPosition(this);
				CharPosition temp = GetChartPosition(location);

				_lastMouseDownPosition = temp;

				if (temp != null)
				{
					string line = _document.GetLineText(temp.Row);
					int start = Math.Max(line.LastIndexOfAny(CtrlSplitChars, Math.Min(temp.Col, line.Length - 1)) + 1, 0);
					int end = line.IndexOfAny(CtrlSplitChars, temp.Col);

					if (end < 0)
						end = line.Length;

					SetPosition(new CharPosition(start, temp.Row), new CharPosition(end, temp.Row), true);
				}
			}

			_lastMouseDownEvent = DateTime.UtcNow;

			e.Handled = true;
			Keyboard.Focus(this);
			CaptureMouse();
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			OnMouseButtonUp(e);
		}

		private class TextPosition
		{
			private readonly string _textAsString;
			private readonly int _line;

			private IList<GeometryDrawing> _glyphs;
			private double _top;

			public TextPosition(string textAsString, int line)
			{
				_textAsString = textAsString;
				_line = line;
			}

			public string TextAsString
			{
				get { return _textAsString; }
			}

			public int Line
			{
				get { return _line; }
			}

			public IList<GeometryDrawing> Glyphs
			{
				get { return _glyphs; }
				set { _glyphs = value; }
			}

			public double Top
			{
				get { return _top; }
				set { _top = value; }
			}

			public double Buttom
			{
				get { return _top + fontHeight; }
			}
		}

		internal class SetPositionAction : ISingleTextAction
		{
			private readonly CharPosition _startSelection;
			private readonly CharPosition _endSelection;

			public SetPositionAction(CharPosition startSelection, CharPosition endSelection)
			{
				_startSelection = startSelection;
				_endSelection = endSelection;
			}

			public void DoAction(UfeTextBoxInner textBox)
			{
				textBox.SetPosition(_startSelection, _endSelection, false);
			}

			public bool IsPositionChangedEvent
			{
				get { return true; }
			}
		}

		internal class AddLineAction : ISingleTextAction
		{
			private readonly int _rowIndex;

			public AddLineAction(int rowIndex)
			{
				_rowIndex = rowIndex;
			}

			public void DoAction(UfeTextBoxInner textBox)
			{
				textBox.Document.InsertLine(_rowIndex);
			}

			public bool IsPositionChangedEvent
			{
				get { return false; }
			}
		}

		internal class RemoveLineAction : ISingleTextAction
		{
			private readonly int _rowIndex;

			public RemoveLineAction(int rowIndex)
			{
				_rowIndex = rowIndex;
			}

			public void DoAction(UfeTextBoxInner textBox)
			{
				textBox.Document.RemoveLine(_rowIndex);
			}

			public bool IsPositionChangedEvent
			{
				get { return false; }
			}
		}
	}

	internal class SetTextAction : ISingleTextAction
	{
		private readonly int _rowIndex;
		private readonly string _text;

		public SetTextAction(int rowIndex, string text)
		{
			_rowIndex = rowIndex;
			_text = text;
		}

		public void DoAction(UfeTextBoxInner textBox)
		{
			textBox.Document.SetText(_rowIndex, _text);
		}

		public bool IsPositionChangedEvent
		{
			get { return false; }
		}
	}

	internal interface ISingleTextAction
	{
		void DoAction(UfeTextBoxInner textBox);
		bool IsPositionChangedEvent { get; }
	}

	internal interface IActionPair
	{
		void DoAction(UfeTextBoxInner textBox);
		void RevertAction(UfeTextBoxInner textBox);
		bool IsPositionChangedEvent { get; }
	}

	internal class SingleActionPair : IActionPair
	{
		private readonly ISingleTextAction _action;
		private readonly ISingleTextAction _undoAction;

		public SingleActionPair(ISingleTextAction action, ISingleTextAction undoAction)
		{
			if (action == null) 
				throw new ArgumentNullException("action");
			if (undoAction == null)
				throw new ArgumentNullException("undoAction");

			_action = action;
			_undoAction = undoAction;
		}

		public void DoAction(UfeTextBoxInner textBox)
		{
			_action.DoAction(textBox);
		}

		public void RevertAction(UfeTextBoxInner textBox)
		{
			_undoAction.DoAction(textBox);
		}

		public bool IsPositionChangedEvent
		{
			get { return _action.IsPositionChangedEvent; }
		}
	}

	internal class MultiActionPair : IActionPair
	{
		private readonly IActionPair[] _actions;

		public MultiActionPair(IActionPair[] actions)
		{
			if (actions == null) 
				throw new ArgumentNullException("actions");
			if (actions.Length == 0) 
				throw new ArgumentException("This collection cannot be empty.", "actions");

			_actions = actions;
		}

		public void DoAction(UfeTextBoxInner textBox)
		{
			for (int index = 0; index < _actions.Length; index++)
				_actions[index].DoAction(textBox);
		}

		public void RevertAction(UfeTextBoxInner textBox)
		{
			for (int index = _actions.Length - 1; index >= 0; index--)
				_actions[index].RevertAction(textBox);
		}

		public bool IsPositionChangedEvent
		{
			get { return false; }
		}
	}
}
