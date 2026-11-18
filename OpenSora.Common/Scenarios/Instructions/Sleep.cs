namespace OpenSora.Scenarios.Instructions
{
	public class Sleep: BaseInstruction
	{
		public override int DurationInMs => (int)(long)Operands[0];
	}
}
