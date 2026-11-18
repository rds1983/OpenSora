using Microsoft.Xna.Framework;
using System;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveTo: BaseInstruction
	{
		private Vector3 _initialPosition;

		public int CharId
		{
			get
			{
				if (Queue != null)
				{
					return Queue.Target;
				}

				return (int)Operands[0];
			}
		}

		public int X
		{
			get
			{
				return (int)((long)Operands[1]);
			}
		}

		public int Y
		{
			get
			{
				return (int)((long)Operands[2]);
			}
		}

		public int Z
		{
			get
			{
				return (int)((long)Operands[3]);
			}
		}

		public override int DurationInMs
		{
			get
			{
				return (int)(long)Operands[4];
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

			var character = worker.Context.GetCharacter(CharId);
			if (character == null)
			{
				return;
			}

			_initialPosition = character.Position;

			var delta = TargetPosition - _initialPosition;

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

			var character = worker.Context.GetCharacter(CharId);
			if (character == null)
			{
				return;
			}

			var targetPosition = TargetPosition;

			var part = worker.InstructionPassedPart;
			var newPosition = new Vector3(_initialPosition.X + (targetPosition.X - _initialPosition.X) * part,
				_initialPosition.Y + (targetPosition.Y - _initialPosition.Y) * part,
				_initialPosition.Z + (targetPosition.Z - _initialPosition.Z) * part);

			character.Position = newPosition;
			worker.Context.Scene.Camera.SetLookAt(newPosition);
		}

		public override void End(ExecutionWorker worker)
		{
			base.End(worker);

			var character = worker.Context.GetCharacter(CharId);
			if (character == null)
			{
				return;
			}

			// Set target position
			var targetPosition = TargetPosition;
			character.Position = targetPosition;
			worker.Context.Scene.Camera.SetLookAt(targetPosition);

			// End the movement animation
			character.AnimationStep = 0;
		}
	}
}
