using Microsoft.Xna.Framework;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveCamera : BaseInstruction
	{
		private Vector3 _initialPosition;
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
				return (int)Operands[3];
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

			_initialPosition = worker.Context.Scene.Camera.Position;
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			var targetPosition = TargetPosition;

			var part = worker.InstructionPassedPart;
			var newPosition = new Vector3(_initialPosition.X + (targetPosition.X - _initialPosition.X) * part,
				_initialPosition.Y + (targetPosition.Y - _initialPosition.Y) * part,
				_initialPosition.Z + (targetPosition.Z - _initialPosition.Z) * part);
			worker.Context.Scene.Camera.SetLookAt(newPosition);
		}

		public override void End(ExecutionWorker worker)
		{
			base.End(worker);

			// Set target position
			var targetPosition = TargetPosition;
			worker.Context.Scene.Camera.SetLookAt(targetPosition);
		}
	}
}
