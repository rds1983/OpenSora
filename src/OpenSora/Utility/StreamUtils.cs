using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSora.Utility
{
	internal static class StreamUtils
	{
		public static string LoadZeroTerminatedString(this BinaryReader reader, out int length)
		{
			var result = new List<char>();
			length = 0;

			var sb = new StringBuilder();
			while(true)
			{
				var ch = reader.ReadByte();
				++length;
				if (ch == 0)
				{
					break;
				}


				sb.Append((char)ch);
			}

			return sb.ToString();
		}

		public static float[] LoadTransform(this BinaryReader reader)
		{
			var result = new float[16];
			for (var i = 0; i < result.Length; ++i)
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

		public static string LoadSizedString(this BinaryReader reader, int size)
		{
			var bytes = reader.ReadBytes(size);

			var sb = new StringBuilder();
			for(var i = 0; i < size; ++i)
			{
				if (bytes[i] == 0)
				{
					break;
				}

				sb.Append((char)bytes[i]);
			}

			return sb.ToString();
		}
	}
}
