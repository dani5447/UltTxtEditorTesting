using System;
using System.Threading;
using System.Windows.Input;

namespace UniversalEditor.Base.Mvvm
{
	public class SimpleCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		private readonly ThreadStart _method;
		private readonly Func<bool> _canExecute;

		public SimpleCommand(ThreadStart method)
			: this(method, null)
		{}

		public SimpleCommand(ThreadStart method, Func<bool> canExecute)
		{
			if (method == null) 
				throw new ArgumentNullException("method");

			_method = method;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter)
		{
			_method();
		}

		public void RiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}
	}

	public class SimpleCommand<T> : ICommand
	{
		public delegate void SimpleCommandDelegate(T arg);

		public event EventHandler CanExecuteChanged;

		private readonly SimpleCommandDelegate _method;
		private readonly Func<T, bool> _canExecute;

		public SimpleCommand(SimpleCommandDelegate method)
			: this(method, null)
		{}

		public SimpleCommand(SimpleCommandDelegate method, Func<T, bool> canExecute)
		{
			if (method == null) 
				throw new ArgumentNullException("method");

			_method = method;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute((T)parameter);
		}

		public void Execute(object parameter)
		{
			_method((T)parameter);
		}

		public void RiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}
	}
}
