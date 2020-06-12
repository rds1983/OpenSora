namespace OpenSora.Scenarios.Instructions
{
	public class CameraToChar: BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
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

			worker.Context.Scene.Camera.SetLookAt(ch.Position);
		}
	}
}
