using OpenSora.Scenarios;
using OpenSora.Scenarios.Instructions;
using System.IO;
using System.Text;
using Xunit;

namespace OpenSora.Tests
{
	public class ScenarioTests
	{
		private static void WriteFixedAscii(BinaryWriter writer, string value, int size)
		{
			var bytes = new byte[size];
			var source = Encoding.ASCII.GetBytes(value);
			System.Array.Copy(source, bytes, System.Math.Min(source.Length, size));
			writer.Write(bytes);
		}

		/// <summary>
		/// Builds the smallest FC scenario that the parser accepts: a 100-byte header,
		/// one function-table entry, and a function containing Sleep(300) + Return.
		/// </summary>
		private static byte[] BuildFcScenario()
		{
			const int headerSize = 100;
			const int functionOffset = 102;

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
					writer.Write((ushort)headerSize); // entry offset
					writer.Write((ushort)0); // entry size
				}

				writer.Write((ushort)headerSize); // string table offset
				writer.Write(headerSize); // header end offset
				writer.Write((ushort)headerSize); // function table offset
				writer.Write((ushort)2); // function table size: one ushort

				if (stream.Position != headerSize)
				{
					throw new InvalidDataException("The synthetic scenario header has an unexpected size.");
				}

				writer.Write((ushort)functionOffset); // function offset

				// Sleep(300): opcode 0x08, operand I (uint32), then Return (opcode 0x01).
				writer.Write((byte)0x08);
				writer.Write(300u);
				writer.Write((byte)0x01);

				return stream.ToArray();
			}
		}

		[Fact]
		public void FromFCStream_ParsesHeaderAndFunctionTable()
		{
			Scenario scenario;
			using (var stream = new MemoryStream(BuildFcScenario()))
			{
				scenario = Scenario.FromFCStream(stream);
			}

			Assert.Equal("TESTMAP", scenario.MapName);
			Assert.Equal("TEST_LOC", scenario.Location);
			Assert.Equal(1, scenario.MapIndex);
			Assert.Equal(2, scenario.MapDefaultBgm);
			Assert.Equal(3, scenario.Flags);
			Assert.Empty(scenario.ChipInfo);
			Assert.Empty(scenario.NpcInfo);
			Assert.Empty(scenario.MonsterInfo);
			Assert.Empty(scenario.EventInfo);
			Assert.Empty(scenario.ActorInfo);

			var function = Assert.Single(scenario.Functions);
			Assert.Equal(102, function.Offset);
			Assert.Equal(2, function.Instructions.Length);

			var sleep = Assert.IsType<Sleep>(function.Instructions[0]);
			Assert.Equal(300, sleep.DurationInMs);
			Assert.IsType<Return>(function.Instructions[1]);

			Assert.False(scenario.HasTalk);
			Assert.Contains("function_66", scenario.ToString());
		}

		[Fact]
		public void Decompiler_DecompilesInstructionOperands()
		{
			// Sleep(300); Return;
			var data = new byte[]
			{
				0x08,
				0x2C, 0x01, 0x00, 0x00,
				0x01
			};

			using (var stream = new MemoryStream(data))
			using (var reader = new BinaryReader(stream))
			{
				var decompiler = new Decompiler(reader);
				var instructions = decompiler.DecompileBlock();

				Assert.Equal(2, instructions.Length);
				var sleep = Assert.IsType<Sleep>(instructions[0]);
				Assert.Equal(300L, sleep.Operands[0]);
				Assert.Equal(300, sleep.DurationInMs);
				Assert.IsType<Return>(instructions[1]);
			}
		}
	}
}
