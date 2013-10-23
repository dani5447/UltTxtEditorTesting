using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.DirectX.AudioVideoPlayback;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using UniversalEditor.Base.Utils.LoggerModule;
using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace UniversalEditor.Video.Viewer
{
	/// <summary>
	/// Interaction logic for VideoViewerView.xaml
	/// </summary>
	public partial class VideoViewerView
	{
		private readonly string _filePath;
		private readonly PictureBox _picture;
		private readonly Timer _timer = new Timer();

		private uint _fPreviousExecutionState;
		private Microsoft.DirectX.AudioVideoPlayback.Video _video;

		private bool _isTimer;
		private bool _isStatusLoaded;
		private bool _isVolumeOn = true;
		private int _volume = Options.Instance.Volume;
		private double _progress;
		private double _duration;
		private DateTime _lastClick = DateTime.MinValue;
		
		public VideoViewerView(string filePath)
		{
			_filePath = filePath;
			InitializeComponent();

			_picture = new PictureBox();
			_picture.Image = UniversalEditor.Video.Properties.Resources.title;
			mediaBorder.Child = _picture;

			volume.Value = _volume;

			_picture.MouseUp += (sender, args) =>
				                   {
					                   DateTime time = DateTime.UtcNow;

									   OnMouseUp(null, null);

									   if (time - _lastClick < TimeSpan.FromMilliseconds(300))
										   OnMouseDoubleClick(null, null);
									 
					                   _lastClick = time;
				                   };

			_picture.KeyDown += (sender, args) =>
				                   {
									   if (args.KeyData == Keys.Escape && _video.Fullscreen)
										   _video.Fullscreen = false;
									   else if (args.KeyData == Keys.Enter)
										   _video.Fullscreen = !_video.Fullscreen;
									   else if (args.KeyData == Keys.Space)
									   {
										   if (_video != null && _video.State == StateFlags.Running)
											   Pause();
										   else
											   Play();
									   }
									   else if (args.KeyData == Keys.Up)
										   VolumeUp();
									   else if (args.KeyData == Keys.Down)
										   VolumeDown();
									   else if (args.KeyData == Keys.Right)
										   VideoPositionUp();
									   else if (args.KeyData == Keys.Left)
										   VideoPositionDown();
									   else return;

					                   args.Handled = true;
				                   };

			_picture.MouseWheel += (sender, args) =>
				                       {
										   if (args.Delta > 0)
											   VolumeUp();
										   else VolumeDown();
				                       };

			_timer.Interval = 1000;
			_timer.Tick += OnTimerTick;
			_timer.Start();
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (_video == null)
				return;

			_isTimer = true;
			progress.Value = _progress = _video.CurrentPosition;
			_isTimer = false;

			time.Content = string.Format("{0:c}/{1:c}", TimeSpan.FromSeconds(Math.Ceiling(_progress)), TimeSpan.FromSeconds(Math.Ceiling(_video.Duration)));
		}

		private void OnPlayClick(object sender, RoutedEventArgs e)
		{
			Play();
		}

		internal void Play()
		{
			if (_video != null && _video.Playing)
				return;

			if (_video == null)
			{
				try
				{
					_video = new Microsoft.DirectX.AudioVideoPlayback.Video(_filePath);
				}
				catch (Exception exception)
				{
					Exception wrapper = new Exception("This content cannot be opened as a video data.", exception);
					Logger.Log(wrapper);

					return;
				}

				_picture.Image = null;
				_video.Owner = _picture;

				border.Background = Brushes.Black;

				_duration = _video.Duration;
				_progress = Math.Min(Math.Max(0, _progress), _duration);
				
				progress.Minimum = 0;
				progress.Maximum = _video.Duration;
				progress.Value = _progress;

				UpdateSize();

				_video.Audio.Volume = _isVolumeOn ? _volume : -10000;

				_video.CurrentPosition = _progress;
				_video.Play();
			}
			else
			{
				_video.Play();
			}

			_fPreviousExecutionState = MiscNativeMethods.SetThreadExecutionState(MiscNativeMethods.ES_CONTINUOUS | MiscNativeMethods.ES_SYSTEM_REQUIRED);
		}

		internal static class MiscNativeMethods
		{
			// Import SetThreadExecutionState Win32 API and necessary flags
			[DllImport("kernel32.dll")]
			public static extern uint SetThreadExecutionState(uint esFlags);
			public const uint ES_CONTINUOUS = 0x80000000;
			public const uint ES_SYSTEM_REQUIRED = 0x00000001;
		}
		
		private void Pause()
		{
			if (_video == null)
				return;

			_video.Pause();

			MiscNativeMethods.SetThreadExecutionState(_fPreviousExecutionState);
		}

		private void Stop()
		{
			if (_video == null)
				return;

			_video.Stop();
			mediaBorder.Child = null;

			MiscNativeMethods.SetThreadExecutionState(_fPreviousExecutionState);
		}

		private void OnPauseClick(object sender, RoutedEventArgs e)
		{
			Pause();
		}

		private void OnStopClick(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		internal void Dispose()
		{
			if (_video != null)
			{
				Stop();

				_timer.Stop();
				_timer.Dispose();
			}
		}

		private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_volume == (int) e.NewValue)
				return;

			_volume = (int) e.NewValue;
			Options.Instance.Volume = _volume;

			if (_video != null)
				_video.Audio.Volume = _volume;
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (_video != null && _video.State == StateFlags.Running)
			{
				Pause();
			}
			else
			{
				Play();
			}
			
			if (e != null)
				e.Handled = true;
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			_video.Fullscreen = !_video.Fullscreen;
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			bool hasAlt = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) > 0;

			if (e.Key == Key.Escape)
			{
				if (_video.Fullscreen)
					_video.Fullscreen = false;
			}
			else if (e.Key == Key.Enter)
			{
				_video.Fullscreen = !_video.Fullscreen;
			}
			else if (e.Key == Key.Space)
			{
				if (_video != null && _video.State == StateFlags.Running)
					Pause();
				else
					Play();
			}
			else if (e.Key == Key.Up)
				VolumeUp();
			else if (e.Key == Key.Down)
				VolumeDown();
			else if (e.Key == Key.Right)
				VideoPositionUp();
			else if (e.Key == Key.Left)
				VideoPositionDown();
			else
				return;

			e.Handled = true;
		}

		private void VolumeUp()
		{
			volume.Value = Math.Min(0, _volume + 100);
		}

		private void VolumeDown()
		{
			volume.Value = Math.Max(-10000, _volume - 100);
		}

		private void VideoPositionUp()
		{
			if (_video != null)
				progress.Value = _video.CurrentPosition = Math.Min(_video.Duration, _video.CurrentPosition + 15);
		}

		private void VideoPositionDown()
		{
			if (_video != null)
				progress.Value = _video.CurrentPosition = Math.Max(0, _video.CurrentPosition - 15);
		}

		private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			_video.Fullscreen = !_video.Fullscreen;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateSize();
		}

		private void UpdateSize()
		{
			if (_video == null)
				return;

			double boundWidth = RenderSize.Width;
			double boundHeight = RenderSize.Height - tempRect.ActualHeight;

			double maxHeight = boundWidth * _video.DefaultSize.Height / _video.DefaultSize.Width;
			double maxWidth = boundHeight * _video.DefaultSize.Width / _video.DefaultSize.Height;

			if (maxHeight < boundHeight)
			{
				_picture.Size = _video.Size = new System.Drawing.Size(Convert.ToInt32(boundWidth), Convert.ToInt32(maxHeight));
			}
			else
			{
				_picture.Size = _video.Size = new System.Drawing.Size(Convert.ToInt32(maxWidth), Convert.ToInt32(boundHeight));
			}
		}

		private void OnProgressChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isTimer || _isStatusLoaded)
				return;

			_progress = e.NewValue;

			if (!double.IsNaN(_duration))
				time.Content = string.Format("{0:c}/{1:c}", TimeSpan.FromSeconds(Math.Ceiling(_progress)), TimeSpan.FromSeconds(Math.Ceiling(_duration)));

			if (_video != null)
				_video.CurrentPosition = e.NewValue;
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
				VolumeUp();
			else VolumeDown();
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			SetVolumeOn(!_isVolumeOn);
		}

		private void SetVolumeOn(bool value)
		{
			if (!value)
			{
				if (_video != null)
					_video.Audio.Volume = -10000;

				volumeImage.Template = (ControlTemplate)FindResource("volume-off");
				volume.IsEnabled = false;

				_isVolumeOn = false;
			}
			else
			{
				if (_video != null)
					_video.Audio.Volume = _volume;

				volumeImage.Template = (ControlTemplate)FindResource("volume-up");
				volume.IsEnabled = true;

				_isVolumeOn = true;
			}
		}

		private void Fullscreen()
		{
			if (_video == null)
				return;

			_video.Fullscreen = true;
		}

		public IList<Control> GetEditorCommands()
		{
			return new Control[]
				       {
					       new MenuItem {Header = "Play", Command = new SimpleCommand(Play), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Video;component/Resources/play.png", UriKind.RelativeOrAbsolute))}},
					       new MenuItem {Header = "Pause", Command = new SimpleCommand(Pause), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Video;component/Resources/pause.png", UriKind.RelativeOrAbsolute))}, InputGestureText = "Space"},
					       new MenuItem {Header = "Stop", Command = new SimpleCommand(Stop), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Video;component/Resources/stop.png", UriKind.RelativeOrAbsolute))}},
						   new Separator(), 
						   new MenuItem {Header = "Volume Up", Command = new SimpleCommand(VolumeUp), InputGestureText = "Up"},
						   new MenuItem {Header = "Volume Down", Command = new SimpleCommand(VolumeUp), InputGestureText = "Dowm"},
						   new Separator(), 
						   new MenuItem {Header = "Jump forward", Command = new SimpleCommand(VideoPositionUp), InputGestureText = "Right"},
						   new MenuItem {Header = "Jump backward", Command = new SimpleCommand(VideoPositionDown), InputGestureText = "Left"},
						   new Separator(), 
					       new MenuItem {Header = "Fullscreen", Command = new SimpleCommand(Fullscreen), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Video;component/Resources/fullscreen.png", UriKind.RelativeOrAbsolute))}, InputGestureText = "Enter"},
				       };
		}

		public void SaveStatus(XElement xElement)
		{
			xElement.Add(new XElement("Progress", _progress.ToString(CultureInfo.InvariantCulture)));
			xElement.Add(new XElement("Duration", _duration.ToString(CultureInfo.InvariantCulture)));
			xElement.Add(new XElement("VolumeOn", _isVolumeOn.ToString(CultureInfo.InvariantCulture)));
		}

		public void LoadStatus(XElement xElement)
		{
			if (xElement == null)
				return;

			try
			{
				_isStatusLoaded = true;

				XElement xVolumeOff = xElement.Element("VolumeOn");
				if (xVolumeOff != null)
				{
					_isVolumeOn = bool.Parse(xVolumeOff.Value);
					SetVolumeOn(_isVolumeOn);
				}

				XElement xProgress = xElement.Element("Progress");
				if (xProgress != null)
					_progress = double.Parse(xProgress.Value, CultureInfo.InvariantCulture);

				XElement xDuration = xElement.Element("Duration");
				if (xDuration != null)
				{
					_duration = double.Parse(xDuration.Value, CultureInfo.InvariantCulture);

					progress.Minimum = 0;
					progress.Maximum = _duration;
					progress.Value = _progress;

					time.Content = string.Format("{0:c}/{1:c}", TimeSpan.FromSeconds(Math.Ceiling(_progress)), TimeSpan.FromSeconds(Math.Ceiling(_duration)));
				}
			}
			finally
			{
				_isStatusLoaded = false;
			}
		}
	}
}
