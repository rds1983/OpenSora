namespace OpenSora.Scenarios.Instructions
{
	public class CloseMessageWindow: BaseInstruction
	{
		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			worker.Context.Scene.CloseMessageWindow();
		}
	}
}
