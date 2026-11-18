using OpenSora.Engine;
using Xunit;

namespace OpenSora.Tests
{
	public class InputTests
	{
		private static InputTrace BuildTrace(int count)
		{
			var trace = new InputTrace();
			for (var i = 0; i < count; ++i)
			{
				var buttons = InputButton.None;
				if ((i & 1) == 0)
				{
					buttons |= InputButton.Confirm;
				}

				if (i % 3 == 0)
				{
					buttons |= InputButton.Run;
				}

				trace.Add(new InputSnapshot(buttons, i / 100.0f, -i / 200.0f, 0.25f, -0.5f));
			}

			return trace;
		}

		[Fact]
		public void RecordingAndReplay_PreserveSnapshots()
		{
			var original = BuildTrace(100);
			var recording = new RecordingInputSource(new ReplayInputSource(original));

			for (var i = 0; i < original.Count; ++i)
			{
				recording.Sample();
			}

			Assert.Equal(original.Count, recording.Trace.Count);

			var replay = new ReplayInputSource(recording.Trace);
			for (var i = 0; i < original.Count; ++i)
			{
				Assert.Equal(original[i], replay.Sample());
			}

			Assert.True(replay.IsFinished);
			Assert.Equal(InputSnapshot.Empty, replay.Sample());
		}

		[Fact]
		public void InputSnapshot_CombinesButtonsAndAxes()
		{
			var snapshot = new InputSnapshot(
				InputButton.Confirm | InputButton.Run,
				0.5f,
				-0.25f,
				0.1f,
				-0.2f);

			Assert.True(snapshot.IsDown(InputButton.Confirm));
			Assert.True(snapshot.IsDown(InputButton.Run));
			Assert.False(snapshot.IsDown(InputButton.Cancel));
			Assert.False(InputSnapshot.Empty.IsDown(InputButton.None));

			Assert.Equal(0.5f, snapshot.MoveX);
			Assert.Equal(-0.25f, snapshot.MoveY);
			Assert.Equal(0.1f, snapshot.LookX);
			Assert.Equal(-0.2f, snapshot.LookY);
		}

		[Fact]
		public void TraceClone_IsIndependent()
		{
			var trace = BuildTrace(10);
			var clone = trace.Clone();

			trace.Add(InputSnapshot.Empty);

			Assert.Equal(11, trace.Count);
			Assert.Equal(10, clone.Count);
		}
	}
}
