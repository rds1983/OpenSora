using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioFunctionInfo
	{
		public int Offset;

		public static ScenarioFunctionInfo FromBinaryReader(BinaryReader reader, int offset)
		{
			var result = new ScenarioFunctionInfo
			{
				Offset = offset
			};

			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			var decompiler = new Decompiler(reader);

			decompiler.DecompileBlock();

			return result;
		}
	}
}
