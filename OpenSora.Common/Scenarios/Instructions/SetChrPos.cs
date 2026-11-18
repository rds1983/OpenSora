namespace OpenSora.Scenarios.Instructions
{
	public class SetChrPos : BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int X
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public int Y
		{
			get
			{
				return (int)Operands[2];
			}
		}

		public int Z
		{
			get
			{
				return (int)Operands[3];
			}
		}

		public int Angle
		{
			get
			{
				return (int)Operands[4];
			}
		}

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			var target = ExecutionContext.ToPosition(X, Y, Z);

			var ch = worker.Context.GetCharacter(CharId);
			if (ch == null)
			{
				return;
			}

			ch.Position = target;
			ch.AnimationStart = ExecutionContext.DegreesToAnimationStart(Angle);
			ch.AnimationStep = 0;
		}
	}
}
