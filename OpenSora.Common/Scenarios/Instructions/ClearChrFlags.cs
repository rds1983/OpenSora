namespace OpenSora.Scenarios.Instructions
{
	public class ClearChrFlags: BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}
	}
}
