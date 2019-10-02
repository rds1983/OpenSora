using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.AtlasAnimations
{
	public static class AtlasAnimationLoader
	{
		private static readonly byte[] g_by32to256 = new byte[]
		{
			0x00, 0x08, 0x10, 0x19, 0x21, 0x29, 0x31, 0x3A,
			0x42, 0x4A, 0x52, 0x5A, 0x63, 0x6B, 0x73, 0x7B,
			0x84, 0x8C, 0x94, 0x9C, 0xA5, 0xAD, 0xB5, 0xBD,
			0xC5, 0xCE, 0xD6, 0xDE, 0xE6, 0xEF, 0xF7, 0xFF
		};

		private static readonly byte[] g_by16to256 = new byte[]
		{
			0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
			0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
		};

		private static readonly byte[] g_by02to256 = new byte[]
		{
			0x00, 0xFF
		};

		private const int ImageWidth = 256;
		private const int ImageHeight = 256;
		public const int ChunkSize = 16;
		private const int BytesPerColor = 2;

		private const int TextureSize = 1024;

		private static Color Pixel1555To32(ushort src)
		{
			var c = new Color
			{
				B = g_by32to256[src & 0x1F],
				G = g_by32to256[(src >> 5) & 0x1F],
				R = g_by32to256[(src >> 10) & 0x1F],
				A = g_by02to256[(src >> 15) & 0x01]
			};

			return c;
		}

		private static Color Pixel4444To32(ushort src)
		{
			var c = new Color
			{
				B = g_by16to256[src & 0x0F],
				G = g_by16to256[(src >> 4) & 0x0F],
				R = g_by16to256[(src >> 8) & 0x0F],
				A = g_by16to256[(src >> 12) & 0x0F]
			};

			return c;
		}

		public static Texture2D[] LoadCPFile(GraphicsDevice device, Stream chStream, Stream cpStream)
		{
			var colorResult = new List<Color[]>();
			var result = new List<Texture2D>();

			if (cpStream == null)
			{
				// Plain image
				using (var chReader = new BinaryReader(chStream))
				{
					var length = chStream.Length / BytesPerColor;

					var textureWidth = 2048;
					var textureHeight = 2048;

					if (length < 20000)
					{
						textureWidth = textureHeight = 128;
					} else if (length < 140000)
					{
						textureWidth = 512;
						textureHeight = 256;
					} else if (length < 530000)
					{
						textureWidth = 768;
						textureHeight = 768;
					} else if (length < 1100000)
					{
						textureWidth = 2048;
						textureHeight = 2048;
					}

					var texture = new Texture2D(device, textureWidth, textureHeight);
					var data = chReader.ReadBytes((int)chStream.Length);

					var colorBuffer = new Color[textureWidth * textureHeight];

					for (var i = 0; i < colorBuffer.Length; ++i)
					{
						if (i * 2 >= data.Length)
						{
							break;
						}

						var b1 = data[i * BytesPerColor];
						var b2 = data[i * BytesPerColor + 1];
						ushort val = (ushort)((b2 << 8) + b1);

						colorBuffer[i] = Pixel1555To32(val);
					}

					texture.SetData(colorBuffer);
					result.Add(texture);
				}

				return result.ToArray();
			}

			using (var chReader = new BinaryReader(chStream))
			{
				var chunksCount = chReader.ReadUInt16();
				var chunks = new List<byte[]>();
				for (var i = 0; i < chunksCount; ++i)
				{
					chunks.Add(chReader.ReadBytes(ChunkSize * ChunkSize * BytesPerColor));
				}

				var chunksPerSize = TextureSize / ChunkSize;
				Color[] texture = null;
				var colorBuffer = new Color[ChunkSize * ChunkSize];
				for (var i = 0; i < chunks.Count; ++i)
				{
					var textureChunkIndex = i % (chunksPerSize * chunksPerSize);
					if (textureChunkIndex == 0)
					{
						// New texture
						texture = new Color[TextureSize * TextureSize];
						colorResult.Add(texture);
					}

					var tileX = textureChunkIndex % chunksPerSize;
					var tileY = textureChunkIndex / chunksPerSize;

					var chunk = chunks[i];
					for (var j = 0; j < colorBuffer.Length; ++j)
					{
						var b1 = chunk[j * BytesPerColor];
						var b2 = chunk[j * BytesPerColor + 1];
						ushort val = (ushort)((b2 << 8) + b1);

						colorBuffer[j] = Pixel4444To32(val);
					}

					for (var y = 0; y < ChunkSize; ++y)
					{
						for (var x = 0; x < ChunkSize; ++x)
						{
							texture[(tileY * ChunkSize + y) * TextureSize + tileX * ChunkSize + x] = colorBuffer[y * ChunkSize + x];
						}
					}
				}
			}

			foreach (var colorBuffer in colorResult)
			{
				var texture = new Texture2D(device, TextureSize, TextureSize);
				texture.SetData(colorBuffer);
				result.Add(texture);
			}

			return result.ToArray();
		}
	}
}