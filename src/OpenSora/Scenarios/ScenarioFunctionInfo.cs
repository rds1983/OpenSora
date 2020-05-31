using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioFunctionInfo
	{
		public int Offset { get; }
		public BaseInstruction[] Instructions { get; }

		private ScenarioFunctionInfo(int offset, BaseInstruction[] instructions)
		{
			Offset = offset;
			Instructions = instructions;
		}


		public static ScenarioFunctionInfo FromBinaryReader(BinaryReader reader, int offset)
		{
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			var decompiler = new Decompiler(reader);

			var instructions = decompiler.DecompileBlock();

			return new ScenarioFunctionInfo(offset, instructions);
		}
	}
}
