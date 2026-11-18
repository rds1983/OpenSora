using System;
using System.IO;

namespace OpenSora.Tests.Fixtures
{
	/// <summary>
	/// Builds minimal CH/CP sprite data. Fixture data is generated from code and never copied from a
	/// game installation.
	/// </summary>
	public static class SyntheticCh
	{
		public const int DefaultChWidth = 768;
		public const int DefaultChHeight = 768;
		public const ushort Default4444Color = 0xF80F;

		/// <summary>
		/// Builds a CH animation sheet: a chunk count followed by 16x16 BGRA4444 chunks.
		/// </summary>
		public static byte[] CreateAnimationImage(int chunks = 1, ushort color = Default4444Color)
		{
			if (chunks <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunks));
			}

			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write((ushort)chunks);

				for (var chunk = 0; chunk < chunks; ++chunk)
				{
					for (var i = 0; i < 16 * 16; ++i)
					{
						writer.Write(color);
					}
				}

				return stream.ToArray();
			}
		}

		/// <summary>
		/// Builds CP animation info: a frame count followed by 16x16 frame grids. 0xffff means empty.
		/// </summary>
		public static byte[] CreateAnimationInfo(int frames = 2)
		{
			if (frames <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(frames));
			}

			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write((ushort)frames);

				for (var frame = 0; frame < frames; ++frame)
				{
					for (var y = 0; y < 16; ++y)
					{
						for (var x = 0; x < 16; ++x)
						{
							ushort value = 0xFFFF;

							if (frame == 0 && x == 0 && y == 0)
							{
								value = Default4444Color;
							}
							else if (frame == 0 && x == 1 && y == 0)
							{
								value = 0x1234;
							}
							else if (frame == 1 && x == 0 && y == 0)
							{
								value = 0xAAAA;
							}

							writer.Write(value);
						}
					}
				}

				return stream.ToArray();
			}
		}

		/// <summary>
		/// Builds a default-size (768x768, BGRA4444) CH image. The first pixel is
		/// <see cref="Default4444Color"/>; the rest are transparent.
		/// </summary>
		public static byte[] CreateImage()
		{
			var data = new byte[DefaultChWidth * DefaultChHeight * 2];
			data[0] = (byte)(Default4444Color & 0xFF);
			data[1] = (byte)(Default4444Color >> 8);
			return data;
		}
	}
}
