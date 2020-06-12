namespace OpenSora.Scenarios.Instructions
{
	public class ChrTalk: BaseTalk
	{
		public override int? CharId => (int)Operands[0];

		public override string CharName => string.Empty;

		public override ScpString[] ScpStrings
		{
			get
			{
				return (ScpString[])Operands[1];
			}
		}
	}
}
