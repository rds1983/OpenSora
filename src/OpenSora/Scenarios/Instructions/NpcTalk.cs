namespace OpenSora.Scenarios.Instructions
{
	public class NpcTalk: BaseTalk
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public override ScpString[] ScpStrings
		{
			get
			{
				return (ScpString[])Operands[2];
			}
		}
	}
}
