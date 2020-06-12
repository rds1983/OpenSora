namespace OpenSora.Scenarios.Instructions
{
	public class NpcTalk: BaseTalk
	{
		public override int? CharId => (int)Operands[0];

		public override string CharName
		{
			get
			{
				var strings = (ScpString[])Operands[1];
				if (strings == null || strings.Length == 0)
				{
					return string.Empty;
				}

				return strings[0].String;
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
