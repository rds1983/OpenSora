namespace OpenSora.Scenarios
{
	public class ScenarioEntry
	{
		public int Offset { get; private set; }
		public int Size { get; private set; }

		public ScenarioBaseInfo[] ScenarioInfo;

		public ScenarioEntry(int offset, int size)
		{
			Offset = offset;
			Size = size;
		}
	}
}
