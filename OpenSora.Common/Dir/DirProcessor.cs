using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSora.Dir
{
	public static class DirProcessor
	{
		const string LbDirSignature = "LB DIR";

		static void Log(string message, params object[] args)
		{
		}

		static void GenerateError(string message, params object[] args)
		{
			throw new Exception(string.Format(message, args));
		}

		static string ToString(byte[] data)
		{
			return Encoding.UTF8.GetString(data);
		}

		static List<DirEntry> ProcessDirFile(string dirFile, string datFile)
		{
			Log("Processing file '{0}'", dirFile);

			var result = new List<DirEntry>();

			using (var stream = File.OpenRead(dirFile))
			using (var reader = new BinaryReader(stream))
			{
				var sig = ToString(reader.ReadBytes(LbDirSignature.Length));
				if (sig != LbDirSignature)
				{
					GenerateError("Wrong signature.");
				}

				stream.Seek(2, SeekOrigin.Current);

				var numEntries = reader.ReadInt32();
				Log("Number of entries: {0}", numEntries);

				var shouldBeZero = reader.ReadInt32();
				if (shouldBeZero != 0)
				{
					GenerateError("shouldBeZero is not zero: {0}", shouldBeZero);
				}

				for (var i = 0; i < numEntries; ++i)
				{
					var entry = new DirEntry
					{
						DatFilePath = datFile,
						Index = i,
						Name = ToString(reader.ReadBytes(12)),
						Timestamp2 = reader.ReadInt32(),
						CompressedSize = reader.ReadInt32(),
						DecompressedSize = reader.ReadInt32(),
						Unused = reader.ReadInt32(),
						Timestamp = reader.ReadInt32(),
						Offset = reader.ReadInt32()
					};

					if (entry.CompressedSize == 0)
					{
						// Broken file
						continue;
					}

					result.Add(entry);
				}
			}

			return result;
		}

		public static Dictionary<string, List<DirEntry>> BuildEntries(string inputFolder)
		{
			var result = new Dictionary<string, List<DirEntry>>();

			var dirFiles = Directory.GetFiles(inputFolder, "*.dir");
			foreach (var dirFile in dirFiles)
			{
				try
				{
					var datFile = dirFile.Substring(0, dirFile.Length - 4) + ".dat";
					var entries = ProcessDirFile(dirFile, datFile);
					if (!File.Exists(datFile))
					{
						GenerateError("Could not find file '{0}'", datFile);
					}
					result[datFile] = entries;
				}
				catch (Exception ex)
				{
					Log("Error: " + ex);
				}
			}

			return result;
		}
	}
}
