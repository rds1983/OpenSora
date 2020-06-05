using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using OpenSora.Scenarios.Instructions;

namespace OpenSora.Scenarios
{
	public class ExecutionContext
	{
		private const float PositionScale = 1000.0f;

		public ResourceLoader ResourceLoader { get; }
		public Scene Scene { get; set; }
		public ScenarioFunctionInfo Function { get; set; }

		public ExecutionContext(ResourceLoader resourceLoader)
		{
			ResourceLoader = resourceLoader;
		}

		public void Execute()
		{
			if (Function == null || Function.Instructions == null || Function.Instructions.Length == 0)
			{
				return;
			}

			foreach (var instruction in Function.Instructions)
			{
				var chr = instruction as SetChrPos;
				if (chr != null)
				{
					// var target = new Vector3(chr.X / PositionScale, chr.Y / PositionScale, chr.Z / PositionScale);
					// var target = new Vector3(-1f, 1.0f, -1.5f);
					var position = new Vector3(target.X - 8, target.Y + 6.0f, -target.Z + 6.0f);
					Scene.Camera.SetLookAt(position, target);
					break;
				}
			}
		}
	}
}
