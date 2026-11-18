using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioEventInfo : ScenarioBaseInfo
	{
		public int X;
		public int Y;
		public int Z;
		public int Range;
		public int Unknown_10;
		public int Unknown_14;
		public int Unknown_18;
		public int Unknown_1C;

		public static ScenarioEventInfo FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioEventInfo
			{
				X = reader.ReadInt32(),
				Z = reader.ReadInt32(),
				Y = reader.ReadInt32(),
				Range = reader.ReadInt32(),
				Unknown_10 = reader.ReadInt32(),
				Unknown_14 = reader.ReadInt32(),
				Unknown_18 = reader.ReadInt32(),
				Unknown_1C = reader.ReadInt32(),
			};
		}
	}
}
