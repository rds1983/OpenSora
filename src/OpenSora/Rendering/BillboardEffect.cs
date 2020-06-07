using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenSora.Rendering
{
	internal class BillboardEffect : Effect
	{
		private Color _diffuseColor = Color.White;
		private Matrix _worldView = Matrix.Identity;
		private Matrix _projection = Matrix.Identity;
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

		public Matrix WorldView
		{
			get
			{
				return _worldView;
			}

			set
			{
				_worldView = value;
				_dirty = true;
			}
		}

		public Matrix Projection
		{
			get
			{
				return _projection;
			}

			set
			{
				_projection = value;
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

		public BillboardEffect(GraphicsDevice device) : base(device, Resources.BillboardEffect)
		{
		}

		protected override void OnApply()
		{
			if (_dirty)
			{
				Parameters["_diffuseColor"].SetValue(_diffuseColor.ToVector4());
				Parameters["_worldView"].SetValue(_worldView);
				Parameters["_projection"].SetValue(_projection);
				Parameters["_texture"].SetValue(_texture);
				_dirty = false;
			}
		}
	}
}
