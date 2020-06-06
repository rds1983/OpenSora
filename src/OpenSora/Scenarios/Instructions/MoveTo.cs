using Microsoft.Xna.Framework;
using System.Linq;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveTo: BaseInstruction
	{
		private Vector3? _initialPosition;

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

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			var character = worker.Context.Scene.Characters.Values.First();
			if (_initialPosition == null)
			{
				_initialPosition = character.Position;
			}

			var targetPosition = ExecutionContext.ToPosition(X, Y, -Z);

			var newPosition = new Vector3(_initialPosition.Value.X + (targetPosition.X - _initialPosition.Value.X) * worker.TotalPassedPart,
				_initialPosition.Value.Y + (targetPosition.Y - _initialPosition.Value.Y) * worker.TotalPassedPart,
				_initialPosition.Value.Z + (targetPosition.Z - _initialPosition.Value.Z) * worker.TotalPassedPart);

			character.Position = newPosition;
			var cameraPosition = new Vector3(newPosition.X - 8, newPosition.Y + 6.0f, newPosition.Z + 6.0f);
			worker.Context.Scene.Camera.SetLookAt(cameraPosition, newPosition);
		}
	}
}
