using OpenSora.Dir;
using OpenSora.Tests.Fixtures;
using System.IO;
using System.Text;
using Xunit;

namespace OpenSora.Tests
{
	public class DirProcessorTests
	{
		[Fact]
		public void BuildEntries_ParsesDirDatPair()
		{
			using (var temp = new TempDirectory())
			{
				var data = Encoding.ASCII.GetBytes("HELLO");
				var datPath = SyntheticArchive.Write(temp.Path, "ED6_TEST", ("TEST0001._DS", data));

				var result = DirProcessor.BuildEntries(temp.Path);

				var pair = Assert.Single(result);
				Assert.Equal(datPath, pair.Key);
				Assert.Equal(data, File.ReadAllBytes(datPath));

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
				var data = new byte[] { 1, 2, 3, 4 };
				SyntheticArchive.Write(temp.Path, "ED6_TEST",
					("EMPTY000._DS", new byte[0]),
					("TEST0002._DS", data));

				var result = DirProcessor.BuildEntries(temp.Path);

				var pair = Assert.Single(result);
				var entry = Assert.Single(pair.Value);
				Assert.Equal("TEST0002._DS", entry.Name);
				Assert.Equal(1, entry.Index);
			}
		}
	}
}
