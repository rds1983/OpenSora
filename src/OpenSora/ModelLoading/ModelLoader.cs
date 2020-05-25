using OpenSora.Utility;
using System;
using System.IO;

namespace OpenSora.ModelLoading
{
	public static class ModelLoader
	{
		public static Frame Load(Stream stream, int version)
		{
			using (var reader = new BinaryReader(stream))
			{
				reader.SkipBytes(version == 2?4:48);

				int length = 0;
				var id = reader.LoadZeroTerminatedString(out length);

				var rootFrame = new Frame
				{
					Id = id
				};

				var context = new ModelLoadContext
				{
					Reader = reader,
					Version = version,
					Parent = rootFrame
				};

				try
				{
					rootFrame.LoadFromStream(context);
				}
				catch (Exception)
				{
				}

				return rootFrame;
			}
		}
	}
}
