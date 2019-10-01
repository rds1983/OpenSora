using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.AtlasAnimations
{
	public static class AtlasAnimationLoader
	{
		private const int ImageWidth = 256;
		private const int ImageHeight = 256;
		public const int ChunkSize = 16;
		private const int BytesPerColor = 2;

		private const int TextureSize = 1024;

		private static byte GetByte(ushort val, int startPosition, int length)
		{
			var andFlag = (1 << (16 - startPosition)) - 1;

			val &= (ushort)andFlag;

			val >>= 16 - startPosition - length;

			return (byte)(val << (8 - length));
		}

		public static Texture2D[] LoadCPFile(GraphicsDevice device, Stream cpStream, Stream chStream)
		{
			var result = new List<Texture2D>();

			using (var cpReader = new BinaryReader(cpStream))
			using (var chReader = new BinaryReader(chStream))
			{
				var chunksCount = chReader.ReadUInt16();
				var chunks = new List<byte[]>();
				for(var i = 0; i < chunksCount; ++i)
				{
					chunks.Add(chReader.ReadBytes(ChunkSize * ChunkSize * BytesPerColor));
				}

				var chunksPerSize = TextureSize / ChunkSize;
				Texture2D texture = null;
				var colorBuffer = new Color[ChunkSize * ChunkSize];
				for(var i = 0; i < chunks.Count; ++i)
				{
					var textureChunkIndex = i % (chunksPerSize * chunksPerSize);
					if (textureChunkIndex == 0)
					{
						// New texture
						texture = new Texture2D(device, TextureSize, TextureSize);
						result.Add(texture);
					}


					var tileX = textureChunkIndex % chunksPerSize;
					var tileY = textureChunkIndex / chunksPerSize;

					var chunk = chunks[i];
					for (var j = 0; j < colorBuffer.Length; ++j)
					{
						ushort val = (ushort)((chunk[j * BytesPerColor] << 8) +
							chunk[j * BytesPerColor + 1]);

						if (val > 0)
						{
							var k = 5;
						}
						var c = new Color
						{
							R = GetByte(val, 0, 4),
							G = GetByte(val, 4, 4),
							B = GetByte(val, 8, 4),
							A = GetByte(val, 12, 4)
						};

						colorBuffer[j] = c;
					}

					texture.SetData(0,
						new Rectangle(tileX * ChunkSize, tileY * ChunkSize, ChunkSize, ChunkSize),
						colorBuffer,
						0,
						colorBuffer.Length);
				}
			}

			return result.ToArray();
		}
	}
}
