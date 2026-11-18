using OpenSora.Scenarios;
using OpenSora.Scenarios.Instructions;
using OpenSora.Tests.Fixtures;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class ScenarioTests
	{
		[Fact]
		public void FromFCStream_ParsesHeaderAndFunctionTable()
		{
			Scenario scenario;
			using (var stream = new MemoryStream(SyntheticScenario.CreateFc()))
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
