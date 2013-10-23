using System;
using System.Text;

namespace UniversalEditor.Base.Utils.LoggerModule
{
	public class LoggerViewModel
	{
		private readonly Exception _exception;

		public LoggerViewModel(Exception exception)
		{
			_exception = exception;
		}

		public string Message
		{
			get { return _exception.Message; }
		}

		public string Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();

				Exception ex = _exception;
				while (ex != null)
				{
					builder.AppendLine(ex.Message);
					builder.AppendLine(ex.StackTrace);

					ex = ex.InnerException;
				}

				return builder.ToString();
			}
		}

		public static LoggerViewModel Designer
		{
			get { return new LoggerViewModel(new Exception("Message...")); }
		}
	}
}
