using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenSora.Utility;
using System.Collections.Generic;
using System.IO;

namespace OpenSora
{
	public static class AnimationLoader
	{
		private const int ImageWidth = 256;
		private const int ImageHeight = 256;
		public const int ChunkSize = 16;
		private const int BytesPerColor = 2;

		private const int TextureSize = 1024;


		public static Texture2D[] LoadCPFile(GraphicsDevice device, Stream chStream, Stream cpStream)
		{
			var colorResult = new List<Color[]>();
			var result = new List<Texture2D>();

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

						colorBuffer[j] = Imaging.Pixel4444To32(val);
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
