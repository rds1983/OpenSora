using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenSora.Utility;
using System.IO;

namespace OpenSora
{
	public enum ChFormat
	{
		BGRA4444,
		BGRA1555,
		BGRA8888
	}

	public static class ChLoader
	{
		private class ImageTypeInfo
		{
			public string Dir;
			public string Prefix;
			public int Bytes;
			public int Width;
			public int Height;
			public ChFormat Format;
		}

		private static readonly ImageTypeInfo[] ImageTypes = new ImageTypeInfo[]
		{
			new ImageTypeInfo {                Prefix="C_STCHR",     Bytes=4, Width=512,  Height=512,    Format= ChFormat.BGRA8888 },
			new ImageTypeInfo {                Prefix="H_STCHR",     Bytes=4, Width=1024, Height=1024,   Format= ChFormat.BGRA8888 },
			new ImageTypeInfo {                Prefix="C_STCH",    Bytes=4, Width=512,  Height=512,    Format= ChFormat.BGRA8888 },
			new ImageTypeInfo {                Prefix="H_STCH",    Bytes=4, Width=1024, Height=1024,   Format= ChFormat.BGRA8888},
			new ImageTypeInfo {                Prefix="C_SUBTI",       Bytes=4, Width=256,  Height=256,    Format= ChFormat.BGRA8888},
			new ImageTypeInfo {                Prefix="H_SUBTI",       Bytes=4, Width=512,  Height=512,    Format= ChFormat.BGRA8888},
			new ImageTypeInfo {                Prefix="C_PASELE",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                Prefix="H_PASELE",      Bytes=2, Width=1024, Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                Prefix="C_CAMP02",      Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="C_CAMP03",      Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT00", Prefix="C_CAMP04",      Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT00", Prefix="H_CAMP04",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="BFACE",   Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="HFACE",   Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="CTI", Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="H_CAMP02",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="H_CAMP03",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="C_MNBG01",      Bytes=2, Width=128,  Height=128,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                Prefix="CA",  Bytes=2, Width=128,  Height=128,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                Prefix="CA",  Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS419",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS438",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS439",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS448",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS478",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS530",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS531",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS532",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS533",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS534",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS535",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS536",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS537",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS538",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS539",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS540",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS541",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_VIS542",      Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_MAP010",      Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_MAP011",      Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_PLAC",    Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_PLAC",    Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_PLAC",    Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_TITLE1",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_TITLE2",      Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_TITLE3",      Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="C_TITLE4",      Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_MAP010",      Bytes=2, Width=1024, Height=2048,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_MAP011",      Bytes=2, Width=1024, Height=2048,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_MAP012",      Bytes=2, Width=1024, Height=2048,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_TITLE1",      Bytes=2, Width=1024, Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_VIS419",      Bytes=2, Width=1024, Height=1536,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_VIS438",      Bytes=2, Width=1024, Height=1536,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT24", Prefix="H_VIS439",      Bytes=2, Width=1024, Height=1536,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT05",                             Bytes=2, Width=128,  Height=128,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT25",                             Bytes=2, Width=128,  Height=128,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT05",                             Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT25",                             Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=1024, Height=1536,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=512,  Height=768,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=1024, Height=1536,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=1024, Height=2048,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=1024, Height=2048,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=512,  Height=1024,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT04",                             Bytes=2, Width=1024, Height=1024,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=1024, Height=1024,   Format=ChFormat.BGRA1555},
			new ImageTypeInfo {Dir="ED6_DT24",                             Bytes=2, Width=256,  Height=512,    Format=ChFormat.BGRA1555},
			new ImageTypeInfo {                                            Bytes=2, Width=1024, Height=1024,   Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                                            Bytes=2, Width=176,  Height=208,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                                            Bytes=2, Width=352,  Height=416,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                                            Bytes=2, Width=256,  Height=256,    Format=ChFormat.BGRA4444},
			new ImageTypeInfo {                                            Bytes=2, Width=512,  Height=512,    Format=ChFormat.BGRA4444}
		};

		private static readonly ImageTypeInfo DefaultImageType = new ImageTypeInfo { Bytes = 2, Width = 768, Height = 768 };

		public static Texture2D LoadImage(GraphicsDevice device, string dataFilePath, string chFileName, Stream chStream)
		{
			dataFilePath = Path.GetFileNameWithoutExtension(dataFilePath);

			// Determine image type
			ImageTypeInfo imageType = DefaultImageType;
			for (var i = 0; i < ImageTypes.Length; ++i)
			{
				var type = ImageTypes[i];
				if (!string.IsNullOrEmpty(type.Dir) && !string.Equals(dataFilePath, type.Dir, System.StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				if (!string.IsNullOrEmpty(type.Prefix) && !chFileName.StartsWith(type.Prefix))
				{
					continue;
				}

				if (chStream.Length != type.Width * type.Height * type.Bytes)
				{
					continue;
				}

				// Found
				imageType = type;
				break;
			}

			// Plain image
			using (var chReader = new BinaryReader(chStream))
			{
				var texture = new Texture2D(device, imageType.Height, imageType.Width);
				var data = chReader.ReadBytes((int)chStream.Length);

				var colorBuffer = new Color[imageType.Width * imageType.Height];

				var bytesPerColor = 2;
				if (imageType.Format == ChFormat.BGRA8888)
				{
					bytesPerColor = 4;
				}
				for (var i = 0; i < colorBuffer.Length; ++i)
				{
					if (i * bytesPerColor >= data.Length)
					{
						break;
					}

					switch (imageType.Format)
					{
						case ChFormat.BGRA4444:
						{
							var b1 = data[i * 2];
							var b2 = data[i * 2 + 1];
							ushort val = (ushort)((b2 << 8) + b1);
							colorBuffer[i] = Utility.Imaging.Pixel4444To32(val);
						}
						break;
						case ChFormat.BGRA1555:
						{
							var b1 = data[i * 2];
							var b2 = data[i * 2 + 1];
							ushort val = (ushort)((b2 << 8) + b1);
							colorBuffer[i] = Utility.Imaging.Pixel1555To32(val);
						}
						break;
						case ChFormat.BGRA8888:
						{
							colorBuffer[i] = new Color
							{
								B = data[i * 4],
								G = data[i * 4 + 1],
								R = data[i * 4 + 2],
								A = data[i * 4 + 3],
							};
						}
						break;
					}
				}

				texture.SetData(colorBuffer);

				return texture;
			}
		}
	}
}