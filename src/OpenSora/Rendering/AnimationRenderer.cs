using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenSora.Rendering
{
	public class AnimationRenderer
	{
		public static void DrawAnimation(SpriteBatch batch, Point location, Texture2D texture, ushort?[,]  data)
		{
			for (var y = 0; y < data.GetLength(0); ++y)
			{
				for (var x = 0; x < data.GetLength(1); ++x)
				{
					var val = data[y, x];
					if (val == null)
					{
						continue;
					}

					var tileX = val.Value % AnimationLoader.ChunksPerRow;
					var tileY = val.Value / AnimationLoader.ChunksPerRow;

					var loc = new Vector2(location.X + (x * AnimationLoader.ChunkSize),
										  location.Y + (y * AnimationLoader.ChunkSize));
					var rect = new Rectangle(tileX * AnimationLoader.ChunkSize,
						tileY * AnimationLoader.ChunkSize,
						AnimationLoader.ChunkSize, AnimationLoader.ChunkSize);
					batch.Draw(texture, loc, rect, Color.White);
				}
			}
		}
	}
}
