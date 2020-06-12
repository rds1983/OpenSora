using Microsoft.Xna.Framework;

namespace OpenSora.Utility
{
	public static class Rendering
	{
		public static Vector3 ToCameraPosition(this Vector3 pos)
		{
			return new Vector3(pos.X - 8, pos.Y + 6.0f, pos.Z + 6.0f);
		}
	}
}
