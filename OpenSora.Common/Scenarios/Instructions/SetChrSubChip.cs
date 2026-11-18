namespace OpenSora.Scenarios.Instructions
{
	public class SetChrSubChip: BaseInstruction
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

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			var ch = worker.Context.GetCharacter(CharId);
			if (ch == null)
			{
				return;
			}

			ch.AnimationStart = ChipId;
		}
	}
}
