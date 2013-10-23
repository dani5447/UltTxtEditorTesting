using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using UniversalEditor.Base.Mvvm;

namespace TextEditor.Utils
{
	internal class ActionStackExplorer
	{
		private readonly List<IActionPair> _actionStack = new List<IActionPair>();
		private readonly List<IActionPair> _macro = new List<IActionPair>();
		private readonly UfeTextBoxInner _textBox;

		private readonly SimpleCommand _undoCommand;
		private readonly SimpleCommand _redoCommand;

		private int _actionIndex = -1;
		private bool _isMacroRecording;

		internal ActionStackExplorer(UfeTextBoxInner textBox)
		{
			if (textBox == null) 
				throw new ArgumentNullException("textBox");

			_textBox = textBox;
			_undoCommand = new SimpleCommand(UndoAction, IsUndoActionEnable);
			_redoCommand = new SimpleCommand(NextAction, IsRedoActionEnable);
		}

		private bool IsUndoActionEnable()
		{
			for (int index = 0; index <= _actionIndex; index++)
			{
				IActionPair action = _actionStack[index];
				if (!action.IsPositionChangedEvent)
					return true;
			}

			return false;
		}

		private bool IsRedoActionEnable()
		{
			for (int index = _actionIndex + 1; index < _actionStack.Count; index++)
			{
				IActionPair action = _actionStack[index];
				if (!action.IsPositionChangedEvent)
					return true;
			}

			return false;
		}

		public ICommand UndoCommand
		{
			get { return _undoCommand; }
		}

		public ICommand RedoCommand
		{
			get { return _redoCommand; }
		}

		internal void BeginMacro()
		{
//			if (_isMacroRecording)
//				throw new InvalidOperationException("Macro is already recording.");

			_isMacroRecording = true;
		}

		internal void EndMacro()
		{
//			if (!_isMacroRecording)
//				throw new InvalidOperationException("Macro isn't recording yet.");
//			if (_macro.Count == 0)
//				throw new InvalidOperationException("Macro is empty yet.");
			if (!_isMacroRecording)
				return;

			_isMacroRecording = false;

			if (_macro.Count == 0)
				return;

			_actionStack.Add(new MultiActionPair(_macro.ToArray()));
			_actionIndex = _actionStack.Count - 1;

			_macro.Clear();

			_undoCommand.RiseCanExecuteChanged();
			_redoCommand.RiseCanExecuteChanged();
		}

		internal void DoAction(IActionPair action)
		{
			action.DoAction(_textBox);

			if (_actionIndex < _actionStack.Count - 1)
				_actionStack.RemoveRange(_actionIndex + 1, _actionStack.Count - _actionIndex - 1);

			if (_isMacroRecording)
			{
				_macro.Add(action);
			}
			else
			{
				_actionStack.Add(action);
				_actionIndex = _actionStack.Count - 1;

				_undoCommand.RiseCanExecuteChanged();
				_redoCommand.RiseCanExecuteChanged();
			}
		}

		internal void UndoAction()
		{
			if (_isMacroRecording)
				throw new InvalidOperationException("Macro is already recording.");

			if (_actionIndex >= _actionStack.Count)
				return;
			if (_actionIndex < 0)
				return;

			while (_actionIndex >= 0)
			{
				IActionPair action = _actionStack[_actionIndex--];
				action.RevertAction(_textBox);

				if (!action.IsPositionChangedEvent)
					break;
			}

			_undoCommand.RiseCanExecuteChanged();
			_redoCommand.RiseCanExecuteChanged();
		}

		internal void NextAction()
		{
			if (_isMacroRecording)
				throw new InvalidOperationException("Macro is already recording.");

			if (_actionStack.Count == 0)
				return;
			if (_actionStack.Count - 1 == _actionIndex)
				return;

			while (_actionIndex + 1 < _actionStack.Count)
			{
				IActionPair action = _actionStack[++_actionIndex];
				action.DoAction(_textBox);

				if (!action.IsPositionChangedEvent)
					break;
			}
			
			_undoCommand.RiseCanExecuteChanged();
			_redoCommand.RiseCanExecuteChanged();
		}

		internal void Clear()
		{
			if (_isMacroRecording)
				throw new InvalidOperationException("Macro is already recording.");

			_actionStack.Clear();
			_actionIndex = -1;
		}
	}
}