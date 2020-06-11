namespace OpenSora.Scenarios.Instructions
{
	public class Custom: BaseInstruction
	{
		public string InstructionName { get; set; }

		public override string Name
		{
			get
			{
				return InstructionName;
			}
		}
	}
}
