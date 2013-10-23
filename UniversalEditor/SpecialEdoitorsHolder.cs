using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UniversalEditor.Base.Utils.LoggerModule;

namespace UniversalEditor
{
	public class SpecialEdoitorsHolder
	{
		private const string FileName = "recent.editors.txt";

		private readonly Dictionary<string, byte> _files = new Dictionary<string, byte>();

		internal void RegisterPath(string path, byte editorId)
		{
			byte cachedId;
			if (_files.TryGetValue(path, out cachedId) && cachedId == editorId)
				return;

			_files[path] = editorId;
			Save();
		}

		internal bool ContainsInfo(string path)
		{
			return _files.ContainsKey(path);
		}

		internal byte GetInfo(string path)
		{
			return _files[path];
		}

		private SpecialEdoitorsHolder()
		{}

		internal static SpecialEdoitorsHolder Load()
		{
			try
			{
				string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
				string filePath = Path.Combine(optionsDir, FileName);

				SpecialEdoitorsHolder instance = new SpecialEdoitorsHolder();

				if (!File.Exists(filePath))
					return instance;

				XDocument doc = XDocument.Load(filePath);
				foreach (XElement element in doc.Root.Elements("File"))
					instance._files[element.Element("Path").Value] = byte.Parse(element.Element("EditorId").Value);

				return instance;
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
				return new SpecialEdoitorsHolder();
			}
		}

		internal void Save()
		{
			string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string filePath = Path.Combine(optionsDir, FileName);

			if (!Directory.Exists(optionsDir))
				Directory.CreateDirectory(optionsDir);

			XDocument doc = new XDocument(
				new XElement("Root"));

			foreach (KeyValuePair<string, byte> file in _files)
			{
				doc.Root.Add(
					new XElement("File",
						new XElement("Path", file.Key),
						new XElement("EditorId", file.Value)));
			}

			doc.Save(filePath);
		}

		internal void UnregisterPath(string filePath)
		{
			if (!_files.ContainsKey(filePath)) 
				return;

			_files.Remove(filePath);
			Save();
		}
	}
}
