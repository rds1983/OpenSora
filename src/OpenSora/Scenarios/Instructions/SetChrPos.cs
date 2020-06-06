using Microsoft.Xna.Framework;

namespace OpenSora.Scenarios.Instructions
{
	public class SetChrPos : BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int X
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public int Y
		{
			get
			{
				return (int)Operands[2];
			}
		}

		public int Z
		{
			get
			{
				return (int)Operands[3];
			}
		}

		public int Angle
		{
			get
			{
				return (int)Operands[4];
			}
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			var target = ExecutionContext.ToPosition(X, Y, -Z);
			// var target = new Vector3(-1f, 1.0f, -1.5f);
			var position = new Vector3(target.X - 8, target.Y + 6.0f, target.Z + 6.0f);
			worker.Context.Scene.Camera.SetLookAt(position, target);

			var character = worker.Context.EnsureCharacter(CharId);
			character.Position = target;
		}
	}
}
