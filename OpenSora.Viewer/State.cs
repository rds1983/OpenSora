using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace OpenSora.Viewer
{
	public class State
	{
		public const string StateFileName = "OpenSora.Viewer.config";

		public static string StateFilePath
		{
			get
			{
				var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), StateFileName);
				return result;
			}
		}

		public Point Size { get; set; }
		public string LastFolder { get; set; }

		public void Save()
		{
			using (var stream = new StreamWriter(StateFilePath, false))
			{
				var serializer = new XmlSerializer(typeof(State));
				serializer.Serialize(stream, this);
			}
		}

		public static State Load()
		{
			if (!File.Exists(StateFilePath))
			{
				return null;
			}

			try
			{
				State state;
				using (var stream = new StreamReader(StateFilePath))
				{
					var serializer = new XmlSerializer(typeof(State));
					state = (State)serializer.Deserialize(stream);
				}

				return state;
			}
			catch(Exception)
			{
				return null;
			}
		}

		public override string ToString()
		{
			return string.Format("Size = {0}\n" +
								 "LastFolder = {1}\n",
				Size,
				LastFolder);
		}
	}
}