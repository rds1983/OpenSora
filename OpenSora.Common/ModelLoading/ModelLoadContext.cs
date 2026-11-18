using System.IO;

namespace OpenSora.ModelLoading
{
	public class ModelLoadContext
	{
		public BinaryReader Reader;
		public int Version;
		public Frame Parent;
	}
}
