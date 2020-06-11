namespace OpenSora.Scenarios.Instructions
{
	public class SetChrChipByIndex: BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int ChipId
		{
			get
			{
				return (int)Operands[1];
			}
		}
	}
}
