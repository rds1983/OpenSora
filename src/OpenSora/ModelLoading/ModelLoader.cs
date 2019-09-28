using System;
using System.IO;

namespace OpenSora.ModelLoading
{
	public static class ModelLoader
	{
		public static Frame Load(Stream stream)
		{
			using (var reader = new BinaryReader(stream))
			{
				reader.SkipBytes(4);

				int length = 0;
				var id = reader.LoadZeroTerminatedString(out length);

				var rootFrame = new Frame
				{
					Id = id
				};

				try
				{
					rootFrame.LoadFromStream(reader);
				}
				catch (Exception)
				{
				}

				return rootFrame;
			}
		}
	}
}
