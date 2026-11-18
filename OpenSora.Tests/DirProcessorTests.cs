using OpenSora.Dir;
using System.IO;
using System.Text;
using Xunit;

namespace OpenSora.Tests
{
	public class DirProcessorTests
	{
		private static void WriteEntry(BinaryWriter writer, string name, int compressedSize, int decompressedSize, int offset)
		{
			if (name.Length != 12)
			{
				throw new System.ArgumentException("DIR entry names must be exactly 12 characters.", nameof(name));
			}

			writer.Write(Encoding.ASCII.GetBytes(name));
			writer.Write(0); // Timestamp2
			writer.Write(compressedSize);
			writer.Write(decompressedSize);
			writer.Write(0); // Unused
			writer.Write(0); // Timestamp
			writer.Write(offset);
		}

		private static void WriteDirFile(string path, params (string Name, int CompressedSize, int DecompressedSize, int Offset)[] entries)
		{
			using (var stream = File.Create(path))
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(Encoding.ASCII.GetBytes("LB DIR"));
				writer.Write((ushort)0); // Padding
				writer.Write(entries.Length);
				writer.Write(0); // Reserved

				foreach (var entry in entries)
				{
					WriteEntry(writer, entry.Name, entry.CompressedSize, entry.DecompressedSize, entry.Offset);
				}
			}
		}

		[Fact]
		public void BuildEntries_ParsesDirDatPair()
		{
			using (var temp = new TempDirectory())
			{
				var datPath = Path.Combine(temp.Path, "ED6_TEST.dat");
				var dirPath = Path.Combine(temp.Path, "ED6_TEST.dir");

				File.WriteAllBytes(datPath, Encoding.ASCII.GetBytes("HELLO"));
				WriteDirFile(dirPath, ("TEST0001._DS", 5, 5, 0));

				var result = DirProcessor.BuildEntries(temp.Path);

				var pair = Assert.Single(result);
				Assert.Equal(datPath, pair.Key);

				var entry = Assert.Single(pair.Value);
				Assert.Equal("TEST0001._DS", entry.Name);
				Assert.Equal(0, entry.Index);
				Assert.Equal(5, entry.CompressedSize);
				Assert.Equal(5, entry.DecompressedSize);
				Assert.Equal(0, entry.Offset);
				Assert.Equal(datPath, entry.DatFilePath);
			}
		}

		[Fact]
		public void BuildEntries_SkipsEntriesWithoutCompressedData()
		{
			using (var temp = new TempDirectory())
			{
				var datPath = Path.Combine(temp.Path, "ED6_TEST.dat");
				var dirPath = Path.Combine(temp.Path, "ED6_TEST.dir");

				File.WriteAllBytes(datPath, new byte[] { 1, 2, 3, 4 });
				WriteDirFile(dirPath,
					("EMPTY000._DS", 0, 0, 0),
					("TEST0002._DS", 4, 4, 0));

				var result = DirProcessor.BuildEntries(temp.Path);

				var pair = Assert.Single(result);
				var entry = Assert.Single(pair.Value);
				Assert.Equal("TEST0002._DS", entry.Name);
				Assert.Equal(1, entry.Index);
			}
		}
	}
}
