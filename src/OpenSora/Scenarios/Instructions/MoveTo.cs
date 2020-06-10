using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using System;
using System.Linq;

namespace OpenSora.Scenarios.Instructions
{
	public class MoveTo: BaseInstruction
	{
		private Vector3 _initialPosition;

		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public long X
		{
			get
			{
				return (long)Operands[1];
			}
		}

		public int Y
		{
			get
			{
				return (int)(long)Operands[2];
			}
		}

		public int Z
		{
			get
			{
				return (int)(long)Operands[3];
			}
		}

		public override int DurationInMs
		{
			get
			{
				return (int)(long)Operands[4];
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

			var x = (int)(X & 0xffffffff);
			var delta = ExecutionContext.ToPosition(x, Y, Z) - _initialPosition;

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

			var x = (int)(X & 0xffffffff);
			var targetPosition = ExecutionContext.ToPosition(x, Y, Z);

			var part = worker.InstructionPassedPart;
			var newPosition = new Vector3(_initialPosition.X + (targetPosition.X - _initialPosition.X) * part,
				_initialPosition.Y + (targetPosition.Y - _initialPosition.Y) * part,
				_initialPosition.Z + (targetPosition.Z - _initialPosition.Z) * part);

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
