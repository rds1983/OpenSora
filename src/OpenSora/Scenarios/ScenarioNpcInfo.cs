using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioNpcInfo : ScenarioBaseInfo
	{
		public int X;
		public int Z;
		public int Y;
		public int Direction;
		public int Unknown2;
		public int Unknown3;
		public int ChipIndex;
		public int NpcIndex;
		public int InitFunctionIndex;
		public int InitScenaIndex;
		public int TalkFunctionIndex;
		public int TalkScenaIndex;

		public static ScenarioNpcInfo FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioNpcInfo
			{
				X = reader.ReadInt32(),
				Z = reader.ReadInt32(),
				Y = reader.ReadInt32(),
				Direction = reader.ReadInt16(),
				Unknown2 = reader.ReadInt16(),
				Unknown3 = reader.ReadInt32(),
				ChipIndex = reader.ReadInt16(),
				NpcIndex = reader.ReadInt16(),
				InitFunctionIndex = reader.ReadInt16(),
				InitScenaIndex = reader.ReadInt16(),
				TalkFunctionIndex = reader.ReadInt16(),
				TalkScenaIndex = reader.ReadInt16(),
			};
		}
	}
}