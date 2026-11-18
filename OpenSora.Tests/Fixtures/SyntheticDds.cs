using System;
using System.IO;
using System.Text;

namespace OpenSora.Tests.Fixtures
{
	/// <summary>
	/// Builds minimal uncompressed RGBA DDS images. Fixture data is generated from code and never
	/// copied from a game installation.
	/// </summary>
	public static class SyntheticDds
	{
		/// <summary>
		/// Creates a 32-bit A8R8G8B8 DDS. Pixels are supplied in BGRA byte order, matching the DDS
		/// memory layout.
		/// </summary>
		public static byte[] CreateUncompressedRgba(int width, int height, byte[] bgraPixels)
		{
			if (width <= 0 || height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			if (bgraPixels == null)
			{
				throw new ArgumentNullException(nameof(bgraPixels));
			}

			if (bgraPixels.Length != width * height * 4)
			{
				throw new ArgumentException("Pixel data must contain exactly width * height * 4 bytes.", nameof(bgraPixels));
			}

			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(Encoding.ASCII.GetBytes("DDS "));

				// DDS_HEADER
				writer.Write(124u); // size
				writer.Write(0x00001007u); // caps | height | width | pixel format
				writer.Write((uint)height);
				writer.Write((uint)width);
				writer.Write((uint)(width * 4)); // pitch
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

				writer.Write(bgraPixels);

				return stream.ToArray();
			}
		}
	}
}
