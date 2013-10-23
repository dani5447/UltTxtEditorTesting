using System.Globalization;
using System.Net.Cache;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.DirectX.AudioVideoPlayback;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using UniversalEditor.Base.Utils.LoggerModule;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;

namespace UniversalEditor.Audio.Viewer
{
	/// <summary>
	/// Interaction logic for AudioViewerView.xaml
	/// </summary>
	public partial class AudioViewerView
	{
		private readonly string _filePath;
		private readonly Timer _timer = new Timer();

		private Microsoft.DirectX.AudioVideoPlayback.Audio _audio;

		private bool _isTimer;
		private bool _isVolumeOn = true;
		private int _volume = Options.Instance.Volume;
//		private DateTime _lastClick = DateTime.MinValue;

		public AudioViewerView(string filePath)
		{
			_filePath = filePath;
			InitializeComponent();

			volume.Value = _volume;

			_timer.Interval = 1000;
			_timer.Tick += OnTimerTick;
			_timer.Start();
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (_audio == null)
				return;

			_isTimer = true;
			progress.Value = _audio.CurrentPosition;
			_isTimer = false;

			time.Content = string.Format("{0:c}/{1:c}", TimeSpan.FromSeconds(Math.Ceiling(_audio.CurrentPosition)), TimeSpan.FromSeconds(Math.Ceiling(_audio.Duration)));
		}

		private void OnPlayClick(object sender, RoutedEventArgs e)
		{
			Play();
		}

		internal void Play()
		{
			if (_audio == null)
			{
				try
				{
					_audio = new Microsoft.DirectX.AudioVideoPlayback.Audio(_filePath);
				}
				catch (Exception exception)
				{
					Exception wrapper = new Exception("This content cannot be opened as an audio data.", exception);
					Logger.Log(wrapper);

					return;
				}

				_audio.Volume = _isVolumeOn ? _volume : -10000;

				progress.Minimum = 0;
				progress.Maximum = _audio.Duration;
				progress.IsEnabled = true;

				_audio.Play();
			}
			else
			{
				_audio.Play();
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
				VolumeUp();
			else VolumeDown();
		}

		private void VolumeUp()
		{
			volume.Value = Math.Min(0, _volume + 100);
		}

		private void VolumeDown()
		{
			volume.Value = Math.Max(-10000, _volume - 100);
		}

		private void AudioPositionUp()
		{
			if (_audio != null)
				progress.Value = _audio.CurrentPosition = Math.Min(_audio.Duration, _audio.CurrentPosition + 15);
		}

		private void AudioPositionDown()
		{
			if (_audio != null)
				progress.Value = _audio.CurrentPosition = Math.Max(0, _audio.CurrentPosition - 15);
		}
		
		private void Pause()
		{
			if (_audio == null)
				return;

			_audio.Pause();
		}

		private void Stop()
		{
			if (_audio == null)
				return;

			_audio.Stop();
		}

		private void OnPauseClick(object sender, RoutedEventArgs e)
		{
			_audio.Pause();
		}

		private void OnStopClick(object sender, RoutedEventArgs e)
		{
			if (_audio != null)
			{
				_audio.Stop();
			}
		}

		internal void Dispose()
		{
			if (_audio != null)
			{
				_audio.Stop();

				_timer.Stop();
				_timer.Dispose();
			}
		}

		private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_volume == (int) e.NewValue)
				return;

			_volume = (int)e.NewValue;
			Options.Instance.Volume = _volume;

			if (_audio != null)
				_audio.Volume = _volume;
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (_audio != null && _audio.State == StateFlags.Running)
			{
				_audio.Pause();
			}
			else
			{
				Play();
			}

			if (e != null)
				e.Handled = true;
		}

		internal void OnKeyDown(object sender, KeyEventArgs e)
		{
//			bool hasAlt = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) > 0;

			if (e.Key == Key.Space)
			{
				if (_audio != null && _audio.State == StateFlags.Running)
					Pause();
				else
					Play();
			}
			else if (e.Key == Key.Up)
				VolumeUp();
			else if (e.Key == Key.Down)
				VolumeDown();
			else if (e.Key == Key.Right)
				AudioPositionUp();
			else if (e.Key == Key.Left)
				AudioPositionDown();
			else return;

			e.Handled = true;
		}

		private void OnProgressChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isTimer)
				return;

			_audio.CurrentPosition = e.NewValue;
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			SetVolumeOn(!_isVolumeOn);
		}

		private void SetVolumeOn(bool value)
		{
			if (!value)
			{
				if (_audio != null)
					_audio.Volume = -10000;

				volumeImage.Template = (ControlTemplate)FindResource("volume-off");
				volume.IsEnabled = false;

				_isVolumeOn = false;
			}
			else
			{
				if (_audio != null)
					_audio.Volume = _volume;

				volumeImage.Template = (ControlTemplate)FindResource("volume-up");
				volume.IsEnabled = true;

				_isVolumeOn = true;
			}
		}

		public void SaveStatus(XElement xElement)
		{
			xElement.Add(new XElement("VolumeOn", _isVolumeOn.ToString(CultureInfo.InvariantCulture)));
		}

		public void LoadStatus(XElement xElement)
		{
			if (xElement == null)
				return;

			XElement xVolumeOff = xElement.Element("VolumeOn");
			if (xVolumeOff != null)
			{
				_isVolumeOn = bool.Parse(xVolumeOff.Value);
				SetVolumeOn(_isVolumeOn);
			}
		}
		
		public IList<Control> GetEditorCommands()
		{
			return new Control[]
				       {
					       new MenuItem {Header = "Play", Command = new SimpleCommand(Play), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Audio;component/Resources/play.png", UriKind.RelativeOrAbsolute))}},
					       new MenuItem {Header = "Pause", Command = new SimpleCommand(Pause), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Audio;component/Resources/pause.png", UriKind.RelativeOrAbsolute))}},
					       new MenuItem {Header = "Stop", Command = new SimpleCommand(Stop), Icon = new Image {Source = new BitmapImage(new Uri(@"/UniversalEditor.Audio;component/Resources/stop.png", UriKind.RelativeOrAbsolute))}},
						   new Separator(), 
						   new MenuItem {Header = "Volume Up", Command = new SimpleCommand(VolumeUp), InputGestureText = "Up"},
						   new MenuItem {Header = "Volume Down", Command = new SimpleCommand(VolumeUp), InputGestureText = "Dowm"},
						   new Separator(), 
						   new MenuItem {Header = "Jump forward", Command = new SimpleCommand(AudioPositionUp), InputGestureText = "Right"},
						   new MenuItem {Header = "Jump backward", Command = new SimpleCommand(AudioPositionDown), InputGestureText = "Left"},
				       };
		}
	}
}
