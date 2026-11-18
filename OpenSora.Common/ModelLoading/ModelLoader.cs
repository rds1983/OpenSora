using OpenSora.Diagnostics;
using OpenSora.Utility;
using System;
using System.IO;

namespace OpenSora.ModelLoading
{
	public static class ModelLoader
	{
		// Format notes: docs/formats/x2-x3.md
		public static Frame Load(Stream stream, int version)
		{
			try
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
					catch (Exception ex)
					{
						Log.Error(LogCategory.Asset, "Failed to load model.", ex);
					}

					return rootFrame;
				}
			}
			catch (Exception ex)
			{
				Log.Error(LogCategory.Asset, "Failed to load model.", ex);
				throw;
			}
		}
	}
}
