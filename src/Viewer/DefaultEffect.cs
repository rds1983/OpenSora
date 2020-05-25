using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Viewer
{
	internal class DefaultEffect: Effect
	{
		private Color _diffuseColor = Color.White;
		private Matrix _worldViewProj = Matrix.Identity;
		private Texture2D _texture;
		private bool _dirty = true;

		public Color DiffuseColor
		{
			get
			{
				return _diffuseColor;
			}

			set
			{
				_diffuseColor = value;
				_dirty = true;
			}
		}

		public Matrix WorldViewProjection
		{
			get
			{
				return _worldViewProj;
			}

			set
			{
				_worldViewProj = value;
				_dirty = true;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return _texture;
			}

			set
			{
				_texture = value;
				_dirty = true;
			}
		}

		public DefaultEffect(GraphicsDevice device): base(device, Resources.DefaultEffect)
		{
		}

		protected override void OnApply()
		{
			if (_dirty)
			{
				Parameters["_diffuseColor"].SetValue(_diffuseColor.ToVector4());
				Parameters["_worldViewProj"].SetValue(_worldViewProj);
				Parameters["_texture"].SetValue(_texture);
				_dirty = false;
			}
		}
	}
}
