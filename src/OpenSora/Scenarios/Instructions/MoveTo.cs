using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using System;
using System.Linq;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveTo: BaseInstruction
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

		private SceneCharacterInfo GetCharacter(ExecutionWorker worker)
		{
			return worker.Context.Scene.Characters.Values.First();
		}

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			var character = GetCharacter(worker);
			_initialPosition = character.Position;
			var delta = ExecutionContext.ToPosition(X, Y, Z) - _initialPosition;

			var angle = (int)(Math.Atan2(delta.Z, delta.X) * 360 / (2 * Math.PI)) + 90;
			if (angle < 0)
			{
				angle += 360;
			}

			var rotation = ExecutionContext.DegreesToAnimationStart(angle);
			character.AnimationStart = rotation;
			character.AnimationStep = 8;
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			var character = GetCharacter(worker);

			var targetPosition = ExecutionContext.ToPosition(X, Y, Z);

			var newPosition = new Vector3(_initialPosition.X + (targetPosition.X - _initialPosition.X) * worker.TotalPassedPart,
				_initialPosition.Y + (targetPosition.Y - _initialPosition.Y) * worker.TotalPassedPart,
				_initialPosition.Z + (targetPosition.Z - _initialPosition.Z) * worker.TotalPassedPart);

			character.Position = newPosition;
			var cameraPosition = new Vector3(newPosition.X - 8, newPosition.Y + 6.0f, newPosition.Z + 6.0f);
			worker.Context.Scene.Camera.SetLookAt(cameraPosition, newPosition);
		}

		public override void End(ExecutionWorker worker)
		{
			base.End(worker);

			// End the movement animation
			var character = GetCharacter(worker);
			character.AnimationStep = 0;
		}
	}
}
