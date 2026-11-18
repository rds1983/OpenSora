using System;
using System.IO;
using System.Text;

namespace OpenSora.Tests.Fixtures
{
	/// <summary>
	/// Builds a minimal FC scenario with one function: Sleep(300) followed by Return. Fixture data is
	/// generated from code and never copied from a game installation. SC/3rd scenario fixtures are
	/// added together with their parsers.
	/// </summary>
	public static class SyntheticScenario
	{
		private const int HeaderSize = 100;
		private const int FunctionOffset = 102;

		private static void WriteFixedAscii(BinaryWriter writer, string value, int size)
		{
			var bytes = new byte[size];
			var source = Encoding.ASCII.GetBytes(value);
			Array.Copy(source, bytes, Math.Min(source.Length, size));
			writer.Write(bytes);
		}

		public static byte[] CreateFc()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				WriteFixedAscii(writer, "TESTMAP", 10); // MapName
				WriteFixedAscii(writer, "TEST_LOC", 14); // Location
				writer.Write((ushort)1); // MapIndex
				writer.Write((ushort)2); // MapDefaultBgm
				writer.Write((ushort)3); // Flags
				writer.Write((ushort)0); // EntryFunctionIndex
				writer.Write(new byte[8 * 4]); // IncludedScenario
				writer.Write((ushort)0); // Reserved

				for (var i = 0; i < 6; ++i)
				{
					writer.Write((ushort)HeaderSize); // entry offset
					writer.Write((ushort)0); // entry size
				}

				writer.Write((ushort)HeaderSize); // string table offset
				writer.Write(HeaderSize); // header end offset
				writer.Write((ushort)HeaderSize); // function table offset
				writer.Write((ushort)2); // function table size: one ushort

				if (stream.Position != HeaderSize)
				{
					throw new InvalidDataException("The synthetic scenario header has an unexpected size.");
				}

				writer.Write((ushort)FunctionOffset); // function offset

				// Sleep(300): opcode 0x08, operand I (uint32), then Return (opcode 0x01).
				writer.Write((byte)0x08);
				writer.Write(300u);
				writer.Write((byte)0x01);

				return stream.ToArray();
			}
		}
	}
}
