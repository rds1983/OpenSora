using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenSora.Rendering
{
	public class Animation
	{
		private const int ApplicationFrameChangeDelayInMs = 100;

		public ushort?[][,] Info { get; private set; }
		public Texture2D Texture { get; private set; }

		public DateTime? LastFrameRendered;
		public int FrameIndex;

		public ushort?[,] FrameData
		{
			get
			{
				return Info[FrameIndex];
			}
		}

		public Animation(ushort?[][,] info, Texture2D texture)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Info = info;
			Texture = texture;
		}

		public void Animate(int start, int step)
		{
			var now = DateTime.Now;
			if (LastFrameRendered == null ||
				(LastFrameRendered != null &&
				(now - LastFrameRendered.Value).TotalMilliseconds >= ApplicationFrameChangeDelayInMs))
			{
				FrameIndex += step;
				LastFrameRendered = now;
			}

			if (FrameIndex >= Info.Length)
			{
				FrameIndex = Math.Min(start, Info.Length - 1);
			}
		}

		public void Render(SpriteBatch batch, Point location)
		{
			var data = FrameData;
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
					batch.Draw(Texture, loc, rect, Color.White);
				}
			}
		}
	}
}
