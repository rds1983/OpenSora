using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioMonsterInfo : ScenarioBaseInfo
	{
		public int X;
		public int Z;
		public int Y;
		public int Unknown_0C;
		public int Unknown_0E;
		public int Unknown_10;
		public int Unknown_11;
		public int Unknown_12;
		public int BattleIndex;
		public int Unknown_18;
		public int Unknown_1A;

		public static ScenarioMonsterInfo FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioMonsterInfo
			{
				X = reader.ReadInt32(),
				Z = reader.ReadInt32(),
				Y = reader.ReadInt32(),
				Unknown_0C = reader.ReadUInt16(),
				Unknown_0E = reader.ReadUInt16(),
				Unknown_10 = reader.ReadByte(),
				Unknown_11 = reader.ReadByte(),
				Unknown_12 = reader.ReadInt32(),
				BattleIndex = reader.ReadUInt16(),
				Unknown_18 = reader.ReadUInt16(),
				Unknown_1A = reader.ReadUInt16(),
			};
		}
	}
}