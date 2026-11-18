using Imaging.DDSReader;
using System.IO;
using System.Text;
using Xunit;

namespace OpenSora.Tests
{
	public class TextureTests
	{
		private static byte[] BuildUncompressedRgbaDds()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				// Signature
				writer.Write(Encoding.ASCII.GetBytes("DDS "));

				// DDS_HEADER
				writer.Write(124u); // size
				writer.Write(0x00001007u); // caps | height | width | pixel format
				writer.Write(2u); // height
				writer.Write(2u); // width
				writer.Write(8u); // pitch
				writer.Write(0u); // depth
				writer.Write(0u); // mipmap count
				writer.Write(0u); // alpha bit depth
				for (var i = 0; i < 10; ++i)
				{
					writer.Write(0u); // reserved
				}

				// DDS_PIXELFORMAT
				writer.Write(32u); // size
				writer.Write(0x00000041u); // RGB | ALPHAPIXELS
				writer.Write(0u); // fourcc
				writer.Write(32u); // rgb bit count
				writer.Write(0x00FF0000u); // r mask
				writer.Write(0x0000FF00u); // g mask
				writer.Write(0x000000FFu); // b mask
				writer.Write(0xFF000000u); // a mask

				// DDS_CAPS
				writer.Write(0x00001000u); // caps1: texture
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u); // texture stage

				// Pixels are stored as BGRA for A8R8G8B8.
				writer.Write(new byte[]
				{
					0x11, 0x22, 0x33, 0x44, // B G R A -> R=0x33, G=0x22, B=0x11, A=0x44
					0xAA, 0xBB, 0xCC, 0xDD, // B G R A -> R=0xCC, G=0xBB, B=0xAA, A=0xDD
					0x00, 0xFF, 0x00, 0xFF, // B G R A -> R=0x00, G=0xFF, B=0x00, A=0xFF
					0x00, 0x00, 0x00, 0x00  // transparent black
				});

				return stream.ToArray();
			}
		}

		[Fact]
		public void LoadImage_DecodesUncompressedRgbaDds()
		{
			var dds = BuildUncompressedRgbaDds();

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
