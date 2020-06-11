using Microsoft.Xna.Framework;

namespace OpenSora.Rendering
{
	public class SceneCharacter
	{
		private Vector3 _position;

		public Animation Chip;
		public Vector3 Position
		{
			get
			{
				return _position;
			}

			set
			{
				_position = new Vector3(value.X, value.Y, value.Z);
			}
		}

		public int AnimationStart;
		public int AnimationStep;
	}
}
