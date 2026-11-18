using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using Xunit;

namespace OpenSora.Tests
{
	public class CameraInputControllerTests
	{
		[Fact]
		public void Update_MovesCameraBySuppliedDelta()
		{
			var camera = new Camera();
			camera.SetLookAt(new Vector3(100, 100, 100), Vector3.Zero);

			var controller = new CameraInputController(camera);
			var start = camera.Position;

			controller.SetControlKeyState(CameraInputController.ControlKeys.Right, true);
			controller.Update(1.0f);

			Assert.NotEqual(start, camera.Position);
		}

		[Fact]
		public void Update_ZeroDelta_DoesNotMoveCamera()
		{
			var camera = new Camera();
			camera.SetLookAt(new Vector3(100, 100, 100), Vector3.Zero);

			var controller = new CameraInputController(camera);
			var start = camera.Position;

			controller.SetControlKeyState(CameraInputController.ControlKeys.Right, true);
			controller.Update(0.0f);

			Assert.Equal(start, camera.Position);
		}
	}
}
