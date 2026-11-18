using Microsoft.Xna.Framework;

namespace OpenSora.Rendering
{
	internal class RenderContext
	{
		private Matrix? _viewProjection;
		private BoundingFrustum _frustrum;
		private Matrix _projection = Matrix.Identity, _view = Matrix.Identity;

		public Matrix Projection
		{
			get
			{
				return _projection;
			}

			set
			{
				if (value == _projection)
				{
					return;
				}

				_projection = value;
				ResetView();
			}
		}

		public Matrix View
		{
			get
			{
				return _view;
			}

			set
			{
				if (value == _view)
				{
					return;
				}


				_view = value;
				ResetView();
			}
		}

		public Matrix ViewProjection
		{
			get
			{
				if (_viewProjection == null)
				{
					_viewProjection = View * Projection;
				}

				return _viewProjection.Value;
			}
		}

		public BoundingFrustum Frustrum
		{
			get
			{
				if (_frustrum == null)
				{
					_frustrum = new BoundingFrustum(ViewProjection);
				}

				return _frustrum;
			}
		}

		public Matrix World { get; set; }

		public void ResetView()
		{
			_viewProjection = null;
			_frustrum = null;
		}

		public RenderContext()
		{
			World = Matrix.Identity;
		}
	}
}
