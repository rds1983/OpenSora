namespace OpenSora.Scenarios.Instructions
{
	public class CloseMessageWindow: BaseInstruction
	{
		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			worker.Context.Scene.CloseMessageWindow();
		}
	}
}
