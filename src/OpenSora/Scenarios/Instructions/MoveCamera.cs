using Microsoft.Xna.Framework;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveCamera : BaseInstruction
	{
		public int X
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int Y
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public int Z
		{
			get
			{
				return (int)Operands[2];
			}
		}

		public override int DurationInMs
		{
			get
			{
				return 0;
			}
		}

		private Vector3 TargetPosition
		{
			get
			{
				return ExecutionContext.ToPosition(X, Y, Z);
			}
		}

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);
			// Set target position
			var targetPosition = TargetPosition;
			worker.Context.Scene.Camera.SetLookAt(targetPosition);
		}
	}
}
