using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenSora
{
	public static class AnimationLoader
	{
		private const int BytesPerColor = 2;

		private const int TextureSize = 1024;
		public const int ChunkSize = 16;
		public const int ChunksPerRow = TextureSize / ChunkSize;

		public static Texture2D LoadImage(GraphicsDevice device, Stream chStream)
		{
			var colorBuffer = new Color[TextureSize * TextureSize];
			var chunkBuffer = new Color[ChunkSize * ChunkSize];
			using (var chReader = new BinaryReader(chStream))
			{
				var chunksCount = chReader.ReadUInt16();
				var chunks = new List<byte[]>();
				for (var i = 0; i < chunksCount; ++i)
				{
					chunks.Add(chReader.ReadBytes(ChunkSize * ChunkSize * BytesPerColor));
				}

				var chunksPerSize = TextureSize / ChunkSize;
				for (var i = 0; i < chunks.Count; ++i)
				{
					var tileX = i % chunksPerSize;
					var tileY = i / chunksPerSize;

					var chunk = chunks[i];
					for (var j = 0; j < chunkBuffer.Length; ++j)
					{
						var b1 = chunk[j * BytesPerColor];
						var b2 = chunk[j * BytesPerColor + 1];
						ushort val = (ushort)((b2 << 8) + b1);

						chunkBuffer[j] = Utility.Imaging.Pixel4444To32(val);
					}

					for (var y = 0; y < ChunkSize; ++y)
					{
						for (var x = 0; x < ChunkSize; ++x)
						{
							colorBuffer[(tileY * ChunkSize + y) * TextureSize + tileX * ChunkSize + x] = chunkBuffer[y * ChunkSize + x];
						}
					}
				}
			}
			var texture = new Texture2D(device, TextureSize, TextureSize);
			texture.SetData(colorBuffer);

			return texture;
		}

		public static ushort?[][,] LoadInfo(Stream cpStream)
		{
			ushort?[][,] result = null;

			using (var cpReader = new BinaryReader(cpStream))
			{
				var infoCount = cpReader.ReadUInt16();
				result = new ushort?[infoCount][,];

				for (var i = 0; i < infoCount; ++i)
				{
					result[i] = new ushort?[ChunkSize, ChunkSize];
					for (var y = 0; y < ChunkSize; ++y)
					{
						for (var x = 0; x < ChunkSize; ++x)
						{
							var data = cpReader.ReadUInt16();
							if (data == 0xffff)
							{
								// Skip
								continue;
							}

							result[i][y, x] = data;
						}
					}
				}
			}

			return result;
		}
	}
}
