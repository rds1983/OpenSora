using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioChipInfo : ScenarioBaseInfo
	{
		public int ChipIndex;

		public static ScenarioChipInfo FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioChipInfo
			{
				ChipIndex = reader.ReadInt32()
			};
		}
	}
}
