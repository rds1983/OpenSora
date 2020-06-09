namespace OpenSora.Scenarios.Instructions
{
	public class Fade: BaseInstruction
	{
		public override int DurationInMs => (int)(long)Operands[0];
	}
}
