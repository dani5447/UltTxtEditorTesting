using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace UniversalEditor.Base.Options
{
	public class Options
	{
		private static Options _instance;

		public static Options Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Load();
				}

				return _instance;
			}
		}

		private bool _isStrongSearch = false;
		private bool _showLineNumbers = true;
		private bool _checkUpdate = true;
		private int _volume;

		public bool IsStrongSearch
		{
			get { return _isStrongSearch; }
			set { _isStrongSearch = value; }
		}

		public int Volume
		{
			get { return _volume; }
			set { _volume = value; }
		}

		public bool ShowLineNumbers
		{
			get { return _showLineNumbers; }
			set { _showLineNumbers = value; }
		}

		public bool CheckUpdate
		{
			get { return _checkUpdate; }
			set { _checkUpdate = value; }
		}

		private static Options Load()
		{
			string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string optionsFile = Path.Combine(optionsDir, "options.xml");

			if (!File.Exists(optionsFile))
				return new Options();

			Options options = new Options();
			XDocument doc = XDocument.Load(optionsFile);

			XElement isStrongSearchElement = doc.Root.Element("IsStrongSearch");
			if (isStrongSearchElement != null)
				options.IsStrongSearch = bool.Parse(isStrongSearchElement.Value);

			XElement showLineNumbersElement = doc.Root.Element("ShowLineNumbers");
			if (showLineNumbersElement != null)
				options.ShowLineNumbers = bool.Parse(showLineNumbersElement.Value);

			XElement checkUpdateElement = doc.Root.Element("CheckUpdate");
			if (checkUpdateElement != null)
				options.CheckUpdate = bool.Parse(checkUpdateElement.Value);

			XElement volumeElement = doc.Root.Element("Volume");
			if (volumeElement != null)
				options.Volume = int.Parse(volumeElement.Value, CultureInfo.InvariantCulture);

			return options;
		}

		public void Save()
		{
			string optionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UniversalFileEditor");
			string optionsFile = Path.Combine(optionsDir, "options.xml");

			if (!Directory.Exists(optionsDir))
				Directory.CreateDirectory(optionsDir);

			XDocument document = new XDocument(
				new XElement("Options",
					new XElement("IsStrongSearch", IsStrongSearch),
					new XElement("ShowLineNumbers", ShowLineNumbers),
					new XElement("CheckUpdate", CheckUpdate),
					new XElement("Volume", Volume)));
			
			document.Save(optionsFile);
		}
	}
}
