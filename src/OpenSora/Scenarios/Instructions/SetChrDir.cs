namespace OpenSora.Scenarios.Instructions
{
	public class SetChrDir: BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int Angle
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			var character = worker.Context.EnsureCharacter(CharId);
			character.AnimationStart = ExecutionContext.DegreesToAnimationStart(Angle);
		}
	}
}
