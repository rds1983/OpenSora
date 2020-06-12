namespace OpenSora.Scenarios.Instructions
{
	public class Fade: BaseInstruction
	{
		public override int DurationInMs => (int)(long)Operands[0];

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			worker.Context.Scene.Opacity = 0;
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			worker.Context.Scene.Opacity = worker.InstructionPassedPart;
		}

		public override void End(ExecutionWorker worker)
		{
			base.End(worker);

			worker.Context.Scene.Opacity = 1;
		}
	}
}
