using Imaging.DDSReader;
using OpenSora.Tests.Fixtures;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class TextureTests
	{
		[Fact]
		public void LoadImage_DecodesUncompressedRgbaDds()
		{
			var dds = SyntheticDds.CreateUncompressedRgba(2, 2, new byte[]
			{
				0x11, 0x22, 0x33, 0x44, // B G R A -> R=0x33, G=0x22, B=0x11, A=0x44
				0xAA, 0xBB, 0xCC, 0xDD, // B G R A -> R=0xCC, G=0xBB, B=0xAA, A=0xDD
				0x00, 0xFF, 0x00, 0xFF, // B G R A -> R=0x00, G=0xFF, B=0x00, A=0xFF
				0x00, 0x00, 0x00, 0x00  // transparent black
			});

			DDSResult image;
			using (var stream = new MemoryStream(dds))
			{
				image = DDS.LoadImage(stream);
			}

			Assert.Equal(2, image.Width);
			Assert.Equal(2, image.Height);
			Assert.Equal(4, image.Data.Length);

			Assert.Equal(0x33, (int)image.Data[0].R);
			Assert.Equal(0x22, (int)image.Data[0].G);
			Assert.Equal(0x11, (int)image.Data[0].B);
			Assert.Equal(0x44, (int)image.Data[0].A);

			Assert.Equal(0xCC, (int)image.Data[1].R);
			Assert.Equal(0xBB, (int)image.Data[1].G);
			Assert.Equal(0xAA, (int)image.Data[1].B);
			Assert.Equal(0xDD, (int)image.Data[1].A);

			Assert.Equal(0x00, (int)image.Data[2].R);
			Assert.Equal(0xFF, (int)image.Data[2].G);
			Assert.Equal(0x00, (int)image.Data[2].B);
			Assert.Equal(0xFF, (int)image.Data[2].A);
		}
	}
}
