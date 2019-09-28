using System.Collections.Generic;
using System.IO;

namespace OpenSora.Viewer.ModelLoading
{
	public static class LoaderUtils
	{
		public static string LoadZeroTerminatedString(this BinaryReader reader, out int length)
		{
			var result = new List<char>();
			length = 0;
			while(true)
			{
				var ch = reader.ReadByte();
				++length;
				if(ch == 0)
				{
					break;
				}

				result.Add((char)ch);
			}

			return new string(result.ToArray());
		}

		public static float[] LoadTransform(this BinaryReader reader)
		{
			var result = new float[16];
			for(var i = 0; i < result.Length; ++i)
			{
				result[i] = reader.ReadSingle();
			}

			return result;
		}

		public static void SkipBytes(this BinaryReader reader, int size)
		{
			reader.BaseStream.Seek(size, SeekOrigin.Current);
		}

		public static int ReadInt16Be(this BinaryReader reader)
		{
			var b1 = reader.ReadByte();
			var b2 = reader.ReadByte();

			return (b1 << 8) + b2;
		}
	}
}
