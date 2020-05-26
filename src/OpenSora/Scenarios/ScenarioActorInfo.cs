using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioActorInfo: ScenarioBaseInfo
	{
		public int TriggerX;
		public int TriggerZ;
		public int TriggerY;
		public int TriggerRange;
		public int ActorX;
		public int ActorZ;
		public int ActorY;
		public int Flags;
		public int TalkScenaIndex;
		public int TalkFunctionIndex;
		public int Unknown_22;

		public static ScenarioActorInfo FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioActorInfo
			{
				TriggerX = reader.ReadInt32(),
				TriggerZ = reader.ReadInt32(),
				TriggerY = reader.ReadInt32(),
				TriggerRange = reader.ReadInt32(),
				ActorX = reader.ReadInt32(),
				ActorZ = reader.ReadInt32(),
				ActorY = reader.ReadInt32(),
				Flags = reader.ReadInt16(),
				TalkScenaIndex = reader.ReadInt16(),
				TalkFunctionIndex = reader.ReadInt16(),
				Unknown_22 = reader.ReadInt16()
			};
		}
	}
}
