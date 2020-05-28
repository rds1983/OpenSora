using OpenSora.Utility;
using System;
using System.Collections.Generic;
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

		public  ScenarioEntry[] Entries { get; private set; }
		public ScenarioEntry ScenaFunctionTable { get; private set; }

		public int ChipFrameInfoOffset { get; private set; }

		public int PlaceNameOffset { get; private set; }

		public int PlaceNameNumber { get; private set; }

		public int PreInitFunctionIndex { get; private set; }

		public ScenarioEntryPoint[] EntryPoints { get; private set; }
		public ScenarioFunctionInfo[] Functions { get; private set; }

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

				result.Entries = new ScenarioEntry[SCN_INFO_MAXIMUM];
				for(var i = 0; i < result.Entries.Length; ++i)
				{
					result.Entries[i] = new ScenarioEntry(reader.ReadInt16(), reader.ReadInt16());
				}

				result.StringTableOffset = reader.ReadInt16();
				result.HeaderEndOffset = reader.ReadInt32();
				result.ScenaFunctionTable = new ScenarioEntry(reader.ReadInt16(), reader.ReadInt16());

				var entryPointsCount = (result.Entries[0].Offset - stream.Position) / 0x44;
				var entryPoints = new List<ScenarioEntryPoint>();
				for(var i = 0; i < entryPointsCount; ++i)
				{
					entryPoints.Add(ScenarioEntryPoint.FromBinaryReader(reader));
				}
				result.EntryPoints = entryPoints.ToArray();

				for(var i = 0; i < result.Entries.Length; ++i)
				{
					var se = result.Entries[i];

					stream.Seek(se.Offset, SeekOrigin.Begin);

					var infos = new List<ScenarioBaseInfo>();

					for (var j = 0; j < se.Size; ++j)
					{
						ScenarioBaseInfo scenarioBaseInfo = null;
						switch (i)
						{
							case 0:
							case 1:
								scenarioBaseInfo = ScenarioChipInfo.FromBinaryReader(reader);
								break;
							case 2:
								scenarioBaseInfo = ScenarioNpcInfo.FromBinaryReader(reader);
								break;
							case 3:
								scenarioBaseInfo = ScenarioMonsterInfo.FromBinaryReader(reader);
								break;
							case 4:
								scenarioBaseInfo = ScenarioEventInfo.FromBinaryReader(reader);
								break;
							case 5:
								scenarioBaseInfo = ScenarioActorInfo.FromBinaryReader(reader);
								break;
						}

						infos.Add(scenarioBaseInfo);
					}

					se.ScenarioInfo = infos.ToArray();
				}

				stream.Seek(result.ScenaFunctionTable.Offset, SeekOrigin.Begin);
				var functionOffsets = new List<int>();
				for (var i = 0; i < result.ScenaFunctionTable.Size / 2; ++i)
				{
					functionOffsets.Add(reader.ReadInt16());
				}

				var functions = new List<ScenarioFunctionInfo>();
				foreach(var offset in functionOffsets)
				{
					var function = ScenarioFunctionInfo.FromBinaryReader(reader, offset);
					functions.Add(function);
				}

				result.Functions = functions.ToArray();
			}

			return result;
		}
	}
}