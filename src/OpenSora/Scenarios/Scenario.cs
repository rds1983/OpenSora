using OpenSora.Utility;
using System.IO;

namespace OpenSora.Scenarios
{
	public class Scenario
	{
		public const int SCN_INFO_MAXIMUM = 6;
		public const int NUMBER_OF_INCLUDE_FILE = 8;

		public string MapName { get; private set; }
		public string Location { get; private set; }
		public int MapIndex { get; private set; }
		public int MapDefaultBgm { get; private set; }
		public int Flags { get; private set; }
		public int EntryFunctionIndex { get; private set; }
		public byte[] IncludedScenario { get; private set; }
		public int Reserved { get; private set; }
		public int StringTableOffset { get; private set; }
		public int HeaderEndOffset { get; private set; }

		public  ScenarioEntry[] ScnInfoOffset { get; private set; }
		public ScenarioEntry ScenaFunctionTable { get; private set; }

		public int ChipFrameInfoOffset { get; private set; }

		public int PlaceNameOffset { get; private set; }

		public int PlaceNameNumber { get; private set; }

		public int PreInitFunctionIndex { get; private set; }

		private Scenario()
		{
		}

		public static Scenario FromFCStream(Stream stream)
		{
			var result = new Scenario();

			using (var reader = new BinaryReader(stream))
			{
				result.MapName = reader.LoadSizedString(10);
				result.Location = reader.LoadSizedString(14);
				result.MapIndex = reader.ReadInt16();
				result.MapDefaultBgm = reader.ReadInt16();
				result.Flags = reader.ReadInt16();
				result.EntryFunctionIndex = reader.ReadInt16();
				result.IncludedScenario = reader.ReadBytes(NUMBER_OF_INCLUDE_FILE * 4);
				result.Reserved = reader.ReadInt16();

				result.ScnInfoOffset = new ScenarioEntry[SCN_INFO_MAXIMUM];
				for(var i = 0; i < result.ScnInfoOffset.Length; ++i)
				{
					result.ScnInfoOffset[i] = new ScenarioEntry(reader.ReadInt16(), reader.ReadInt16());
				}

				result.StringTableOffset = reader.ReadInt16();
				result.HeaderEndOffset = reader.ReadInt32();
				result.ScenaFunctionTable = new ScenarioEntry(reader.ReadInt16(), reader.ReadInt16());

			}

			return result;
		}
	}
}