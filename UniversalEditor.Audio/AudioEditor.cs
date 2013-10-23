using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using UniversalEditor.Audio.Viewer;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Audio
{
    public class AudioEditor : EditorBase
    {
		private readonly Lazy<AudioViewerView> _media;
		private XElement _xStatusElement;

	    public AudioEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
	    {
			_media = new Lazy<AudioViewerView>(CreateAudioViewerView);
	    }

		private AudioViewerView CreateAudioViewerView()
		{
			AudioViewerView item = new AudioViewerView(_file.FilePath);
			item.LoadStatus(_xStatusElement);

			return item;
		}

		public override FrameworkElement EditorControl
		{
			get { return _media.Value; }
		}

		public override void Dispose()
		{
			if (_media.IsValueCreated)
				_media.Value.Dispose();

			base.Dispose();
		}

		public override bool HasEditorCommands
		{
			get { return true; }
		}

		public override IList<Control> EditorCommands
		{
			get { return _media.Value.GetEditorCommands(); }
		}

		public override void SaveStatus(XElement file)
		{
			if (_media.IsValueCreated)
				_media.Value.SaveStatus(file);
			else
			{
				if (_xStatusElement != null)
				{
					foreach (XElement item in _xStatusElement.Elements())
					{
						if (file.Element(item.Name) == null)
							file.Add(new XElement(item));
					}
				}
			}
		}

		public override void LoadStatus(XElement file)
		{
			_xStatusElement = file;
		}

		public override void OnFirstView()
		{
			if (_xStatusElement == null)
				_media.Value.Play();
		}

		public override void OnKeyDown(System.Windows.Input.KeyEventArgs keyEventArgs)
		{
			_media.Value.OnKeyDown(null, keyEventArgs);
		}
    }
}
