using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Video.Viewer;

namespace UniversalEditor.Video
{
	public class VideoEditor : EditorBase
	{
		private readonly Lazy<VideoViewerView> _media;
		private XElement _xStatusElement;

		public VideoEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{
			_media = new Lazy<VideoViewerView>(CreateVideoViewerView);
		}

		private VideoViewerView CreateVideoViewerView()
		{
			VideoViewerView item = new VideoViewerView(_file.FilePath);
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
	}
}
