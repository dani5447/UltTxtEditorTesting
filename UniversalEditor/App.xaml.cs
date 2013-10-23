using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UniversalEditor.Base.Utils;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private static readonly DomainModel _domainModel;
		private static readonly DomainViewModel _domainViewModel;

		private NamedPipeServerStream _pipeServer;
		private Thread _pipeServerThread; 

		public static DomainViewModel DomainModel
		{
			get { return _domainViewModel; }
		}

		private string _startFilePath;

		public string StartFilePath
		{
			get { return _startFilePath; }
		}

		static App()
		{
			_domainModel = new DomainModel();
			_domainViewModel = new DomainViewModel(_domainModel);
		}

		public App()
		{
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Logger.Log(e.Exception);
			e.Handled = true;
		}

		protected override void OnStartup(System.Windows.StartupEventArgs e)
		{
			base.OnStartup(e);

			if (e.Args.Length > 0)
				_startFilePath = e.Args[0];

			try
			{
				_pipeServer = new NamedPipeServerStream("ufe.ServerPipe", PipeDirection.In);
			}
			catch (IOException)
			{
				NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "ufe.ServerPipe",
					                                                             PipeDirection.Out);
				pipeClient.Connect();
				StreamString stream = new StreamString(pipeClient);
				stream.WriteString(_startFilePath ?? string.Empty);
				pipeClient.Close();
				
				Shutdown();
			}

			_pipeServerThread = new Thread(ServerThread) { IsBackground = true };
			_pipeServerThread.Start();
		}

		private void ServerThread()
		{
			while (true)
			{
				_pipeServer.WaitForConnection();

				try
				{
					StreamString ss = new StreamString(_pipeServer);
					string fileName = ss.ReadString();

					Dispatcher.Invoke(() =>
					{
						if (!string.IsNullOrEmpty(fileName))
							_domainModel.OpenFile(fileName, false, true);

						MainWindow.GlobalActivate();
					});
				}
				catch (IOException e)
				{
					Console.WriteLine("ERROR: {0}", e.Message);
				}

				_pipeServer.Disconnect();
			}
		}

		private class StreamString
		{
			private Stream ioStream;
			private UnicodeEncoding streamEncoding;

			public StreamString(Stream ioStream)
			{
				this.ioStream = ioStream;
				streamEncoding = new UnicodeEncoding();
			}

			public string ReadString()
			{
				int len = 0;

				len = ioStream.ReadByte() * 256;
				len += ioStream.ReadByte();
				byte[] inBuffer = new byte[len];
				ioStream.Read(inBuffer, 0, len);

				return streamEncoding.GetString(inBuffer);
			}

			public int WriteString(string outString)
			{
				byte[] outBuffer = streamEncoding.GetBytes(outString);
				int len = outBuffer.Length;
				if (len > UInt16.MaxValue)
				{
					len = (int)UInt16.MaxValue;
				}
				ioStream.WriteByte((byte)(len / 256));
				ioStream.WriteByte((byte)(len & 255));
				ioStream.Write(outBuffer, 0, len);
				ioStream.Flush();

				return outBuffer.Length + 2;
			}
		}

	}
}
