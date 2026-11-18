using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace OpenSora.Rendering
{
	public class Animation
	{
		private const int ApplicationFrameChangeDelayInMs = 100;
		private const double FrameChangeDelayEpsilonMs = 1e-6;

		private int _frameIndex;
		private VertexPositionNormalTexture[] _vertices;
		private short[] _indices;
		private bool _animationStarted;
		private double _frameAccumulatorMs;

		public ushort?[][,] Info { get; private set; }
		public Texture2D Texture { get; private set; }

		public int FrameIndex
		{
			get
			{
				return _frameIndex;
			}

			set
			{
				if (value == _frameIndex)
				{
					return;
				}

				_frameIndex = value;
				_vertices = null;
				_indices = null;
			}
		}

		public ushort?[,] FrameData
		{
			get
			{
				return Info[FrameIndex];
			}
		}

		public VertexPositionNormalTexture[] Vertices
		{
			get
			{
				Update();
				return _vertices;
			}
		}

		public short[] Indices
		{
			get
			{
				Update();
				return _indices;
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

		public void Animate(int start, int step, double deltaSeconds)
		{
			if (step == 0)
			{
				FrameIndex = Math.Min(start, Info.Length - 1);
				ResetAnimationClock();
				return;
			}

			if (!_animationStarted)
			{
				FrameIndex = Math.Min(start, Info.Length - 1);
				_animationStarted = true;
				_frameAccumulatorMs = 0;
				return;
			}

			if (deltaSeconds <= 0)
			{
				return;
			}

			_frameAccumulatorMs += deltaSeconds * 1000.0;

			while (_frameAccumulatorMs + FrameChangeDelayEpsilonMs >= ApplicationFrameChangeDelayInMs)
			{
				_frameAccumulatorMs -= ApplicationFrameChangeDelayInMs;
				FrameIndex += step;
			}

			if (FrameIndex >= Info.Length)
			{
				FrameIndex = Math.Min(start, Info.Length - 1);
			}
		}

		/// <summary>
		/// Clears accumulated frame time so the next animation call starts from the current frame.
		/// </summary>
		public void ResetAnimationClock()
		{
			_animationStarted = false;
			_frameAccumulatorMs = 0;
		}

		private void Update()
		{
			if (_vertices != null)
			{
				return;
			}

			var vertices = new List<VertexPositionNormalTexture>();
			var indices = new List<short>();
			short idx = 0;
			var texture = Texture;
			var data = FrameData;

			var width = data.GetLength(1);
			var height = data.GetLength(0);

			float dx = (float)0.30f;
			float dy = (float)0.35f;

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

					var vx = ((float)x - width / 2) * dx;
					var vy = (3.0f + (float)height / 2 - y) * dy;

					var tx = tileX * AnimationLoader.ChunkSize;
					var ty = tileY * AnimationLoader.ChunkSize;

					// Left Top
					vertices.Add(new VertexPositionNormalTexture(new Vector3(vx, vy, 0),
						Vector3.One,
						new Vector2(tx / (float)texture.Width, ty / (float)texture.Height)));

					// Left Bottom
					vertices.Add(new VertexPositionNormalTexture(new Vector3(vx, vy - dy, 0),
						Vector3.One,
						new Vector2(tx / (float)texture.Width, (ty + AnimationLoader.ChunkSize) / (float)texture.Height)));

					// Right Top
					vertices.Add(new VertexPositionNormalTexture(new Vector3(vx + dx, vy, 0),
						Vector3.One,
						new Vector2((tx + AnimationLoader.ChunkSize) / (float)texture.Width, ty / (float)texture.Height)));

					// Right Bottom
					vertices.Add(new VertexPositionNormalTexture(new Vector3(vx + dx, vy - dy, 0),
						Vector3.One,
						new Vector2((tx + AnimationLoader.ChunkSize) / (float)texture.Width, (ty + AnimationLoader.ChunkSize) / (float)texture.Height)));

					indices.Add(idx);
					indices.Add((short)(idx + 2));
					indices.Add((short)(idx + 1));
					indices.Add((short)(idx + 3));
					indices.Add((short)(idx + 1));
					indices.Add((short)(idx + 2));

					idx += 4;
				}
			}

			_vertices = vertices.ToArray();
			_indices = indices.ToArray();
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
