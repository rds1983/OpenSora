using OpenSora.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

		public ScenarioChipInfo[] ChipInfo { get; private set; }
		public ScenarioNpcInfo[] NpcInfo { get; private set; }
		public ScenarioMonsterInfo[] MonsterInfo { get; private set; }
		public ScenarioEventInfo[] EventInfo { get; private set; }
		public ScenarioActorInfo[] ActorInfo { get; private set; }

		public bool HasTalk
		{
			get
			{
				if (Functions == null)
				{
					return false;
				}

				return (from f in Functions where f.HasTalk select f).Count() > 0;
			}
		}

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
				result.MapIndex = reader.ReadUInt16();
				result.MapDefaultBgm = reader.ReadUInt16();
				result.Flags = reader.ReadUInt16();
				result.EntryFunctionIndex = reader.ReadUInt16();
				result.IncludedScenario = reader.ReadBytes(NUMBER_OF_INCLUDE_FILE * 4);
				result.Reserved = reader.ReadUInt16();

				result.Entries = new ScenarioEntry[SCN_INFO_MAXIMUM];
				for(var i = 0; i < result.Entries.Length; ++i)
				{
					result.Entries[i] = new ScenarioEntry(reader.ReadUInt16(), reader.ReadUInt16());
				}

				result.StringTableOffset = reader.ReadUInt16();
				result.HeaderEndOffset = reader.ReadInt32();
				result.ScenaFunctionTable = new ScenarioEntry(reader.ReadUInt16(), reader.ReadUInt16());

				var entryPointsCount = (result.Entries[0].Offset - stream.Position) / 0x44;
				var entryPoints = new List<ScenarioEntryPoint>();
				for(var i = 0; i < entryPointsCount; ++i)
				{
					entryPoints.Add(ScenarioEntryPoint.FromBinaryReader(reader));
				}
				result.EntryPoints = entryPoints.ToArray();

				var chipInfo = new List<ScenarioChipInfo>();
				var npcInfo = new List<ScenarioNpcInfo>();
				var monsterInfo = new List<ScenarioMonsterInfo>();
				var eventInfo = new List<ScenarioEventInfo>();
				var actorInfo = new List<ScenarioActorInfo>();

				for(var i = 0; i < result.Entries.Length; ++i)
				{
					var se = result.Entries[i];

					stream.Seek(se.Offset, SeekOrigin.Begin);

					for (var j = 0; j < se.Size; ++j)
					{
						switch (i)
						{
							case 0:
							case 1:
								chipInfo.Add(ScenarioChipInfo.FromBinaryReader(reader));
								break;
							case 2:
								npcInfo.Add(ScenarioNpcInfo.FromBinaryReader(reader));
								break;
							case 3:
								monsterInfo.Add(ScenarioMonsterInfo.FromBinaryReader(reader));
								break;
							case 4:
								eventInfo.Add(ScenarioEventInfo.FromBinaryReader(reader));
								break;
							case 5:
								actorInfo.Add(ScenarioActorInfo.FromBinaryReader(reader));
								break;
						}
					}
				}

				result.ChipInfo = chipInfo.ToArray();
				result.NpcInfo = npcInfo.ToArray();
				result.MonsterInfo = monsterInfo.ToArray();
				result.EventInfo = eventInfo.ToArray();
				result.ActorInfo = actorInfo.ToArray();

				stream.Seek(result.ScenaFunctionTable.Offset, SeekOrigin.Begin);
				var functionOffsets = new List<int>();
				for (var i = 0; i < result.ScenaFunctionTable.Size / 2; ++i)
				{
					functionOffsets.Add(reader.ReadUInt16());
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

		public override string ToString()
		{
			var sb = new StringBuilder();
			for(var i = 0; i < Functions.Length; ++i)
			{
				sb.Append(Functions[i].ToString());
				if (i < Functions.Length - 1)
				{
					sb.Append("\n");
				}
			}

			return sb.ToString();
		}
	}
}