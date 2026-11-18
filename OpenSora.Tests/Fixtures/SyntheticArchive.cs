using System;
using System.IO;
using System.Text;

namespace OpenSora.Tests.Fixtures
{
	/// <summary>
	/// Builds minimal LB DIR/DAT archives. Fixture data is generated from code and never copied from
	/// a game installation.
	/// </summary>
	public static class SyntheticArchive
	{
		private static void WriteEntry(BinaryWriter writer, string name, int compressedSize, int decompressedSize, int offset)
		{
			if (name.Length != 12)
			{
				throw new ArgumentException("DIR entry names must be exactly 12 characters.", nameof(name));
			}

			writer.Write(Encoding.ASCII.GetBytes(name));
			writer.Write(0); // Timestamp2
			writer.Write(compressedSize);
			writer.Write(decompressedSize);
			writer.Write(0); // Unused
			writer.Write(0); // Timestamp
			writer.Write(offset);
		}

		/// <summary>
		/// Writes a matching pair of .dir/.dat files into <paramref name="folder"/> and returns the
		/// .dat path. Empty data produces a zero-size entry, which DirProcessor skips.
		/// </summary>
		public static string Write(string folder, string baseName, params (string Name, byte[] Data)[] entries)
		{
			if (folder == null)
			{
				throw new ArgumentNullException(nameof(folder));
			}

			if (baseName == null)
			{
				throw new ArgumentNullException(nameof(baseName));
			}

			if (entries == null)
			{
				throw new ArgumentNullException(nameof(entries));
			}

			var datPath = Path.Combine(folder, baseName + ".dat");
			var dirPath = Path.Combine(folder, baseName + ".dir");
			var offsets = new int[entries.Length];

			using (var datStream = File.Create(datPath))
			{
				for (var i = 0; i < entries.Length; ++i)
				{
					var entry = entries[i];
					offsets[i] = (int)datStream.Position;
					datStream.Write(entry.Data, 0, entry.Data.Length);
				}
			}

			using (var dirStream = File.Create(dirPath))
			using (var writer = new BinaryWriter(dirStream))
			{
				writer.Write(Encoding.ASCII.GetBytes("LB DIR"));
				writer.Write((ushort)0); // Padding
				writer.Write(entries.Length);
				writer.Write(0); // Reserved

				for (var i = 0; i < entries.Length; ++i)
				{
					var entry = entries[i];
					WriteEntry(writer, entry.Name, entry.Data.Length, entry.Data.Length, offsets[i]);
				}
			}

			return datPath;
		}
	}
}
