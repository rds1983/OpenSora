namespace OpenSora.Scenarios.Instructions
{
	public class ExitThread : BaseInstruction
	{
		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			worker.Finished = true;
		}
	}
}