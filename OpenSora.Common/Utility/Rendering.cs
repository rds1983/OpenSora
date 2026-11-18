using Microsoft.Xna.Framework;

namespace OpenSora.Utility
{
	public static class Rendering
	{
		private const float Distance = 30.0f;

		public static Vector3 ToCameraPosition(this Vector3 pos)
		{
			return new Vector3(pos.X - Distance, pos.Y + Distance, pos.Z + Distance);
		}
	}
}
