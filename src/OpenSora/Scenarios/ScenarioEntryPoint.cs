using System.IO;

namespace OpenSora.Scenarios
{
	public class ScenarioEntryPoint
	{
		public int Unknown_00;
		public int Unknown_04;
		public int Unknown_08;
		public int Unknown_0C;
		public int Unknown_0E;
		public int Unknown_10;
		public int Unknown_14;
		public int Unknown_18;
		public int Unknown_1C;
		public int Unknown_20;
		public int Unknown_24;
		public int Unknown_28;
		public int Unknown_2C;
		public int Unknown_30;
		public int Unknown_32;
		public int Unknown_34;
		public int Unknown_36;
		public int Unknown_38;
		public int Unknown_3A;
		public int InitScenaIndex;
		public int InitFunctionIndex;
		public int EntryScenaIndex;
		public int EntryFunctionIndex;

		public static ScenarioEntryPoint FromBinaryReader(BinaryReader reader)
		{
			return new ScenarioEntryPoint
			{
				Unknown_00 = reader.ReadInt32(),
				Unknown_04 = reader.ReadInt32(),
				Unknown_08 = reader.ReadInt32(),
				Unknown_0C = reader.ReadInt16(),
				Unknown_0E = reader.ReadInt16(),
				Unknown_10 = reader.ReadInt32(),
				Unknown_14 = reader.ReadInt32(),
				Unknown_18 = reader.ReadInt32(),
				Unknown_1C = reader.ReadInt32(),
				Unknown_20 = reader.ReadInt32(),
				Unknown_24 = reader.ReadInt32(),
				Unknown_28 = reader.ReadInt32(),
				Unknown_2C = reader.ReadInt32(),
				Unknown_30 = reader.ReadInt16(),
				Unknown_32 = reader.ReadInt16(),
				Unknown_34 = reader.ReadInt16(),
				Unknown_36 = reader.ReadInt16(),
				Unknown_38 = reader.ReadInt16(),
				Unknown_3A = reader.ReadInt16(),
				InitScenaIndex = reader.ReadInt16(),
				InitFunctionIndex = reader.ReadInt16(),
				EntryScenaIndex = reader.ReadInt16(),
				EntryFunctionIndex = reader.ReadInt16()
			};
		}
	}
}