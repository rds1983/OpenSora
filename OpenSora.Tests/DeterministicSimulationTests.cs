using OpenSora.Engine;
using Xunit;

namespace OpenSora.Tests
{
	public class DeterministicSimulationTests
	{
		private static InputTrace BuildInputTrace(int count)
		{
			var trace = new InputTrace();
			for (var i = 0; i < count; ++i)
			{
				var buttons = InputButton.None;
				if ((i & 1) == 0)
				{
					buttons |= InputButton.Confirm;
				}

				if (i % 5 == 0)
				{
					buttons |= InputButton.Run;
				}

				if (i % 7 == 0)
				{
					buttons |= InputButton.Right;
				}

				trace.Add(new InputSnapshot(
					buttons,
					((i % 11) - 5) / 5.0f,
					((i % 13) - 6) / 6.0f));
			}

			return trace;
		}

		private sealed class TestSimulation
		{
			private readonly DeterministicRandom _random;
			private readonly ReplayInputSource _input;

			public double PositionX;
			public double PositionY;
			public long Score;

			public int InputPosition
			{
				get
				{
					return _input.Position;
				}
			}

			public TestSimulation(ulong seed, InputTrace trace)
			{
				_random = new DeterministicRandom(seed);
				_input = new ReplayInputSource(trace);
			}

			public void Update(EngineTime time)
			{
				var input = _input.Sample();

				PositionX += input.MoveX * time.DeltaSeconds;
				PositionY += input.MoveY * time.DeltaSeconds;

				if (input.IsDown(InputButton.Confirm))
				{
					Score += _random.NextInt(1, 10);
				}

				if (input.IsDown(InputButton.Run))
				{
					PositionX += _random.NextDouble() * time.DeltaSeconds;
				}

				if (input.IsDown(InputButton.Right))
				{
					PositionY += _random.NextFloat() * time.DeltaSeconds;
				}
			}

			public (double PositionX, double PositionY, long Score, ulong RandomState, int InputPosition) Snapshot()
			{
				return (PositionX, PositionY, Score, _random.State, _input.Position);
			}
		}

		[Fact]
		public void SameSeedAndInputTrace_ProduceIdenticalStateOver10000Updates()
		{
			var trace = BuildInputTrace(10000);

			var first = new TestSimulation(0x123456789ABCDEFUL, trace);
			var second = new TestSimulation(0x123456789ABCDEFUL, trace);

			var firstHost = new EngineHost();
			var secondHost = new EngineHost();
			firstHost.Update += first.Update;
			secondHost.Update += second.Update;

			firstHost.RunFixedSteps(10000);
			secondHost.RunFixedSteps(10000);

			Assert.Equal(first.Snapshot(), second.Snapshot());
			Assert.Equal(10000, first.InputPosition);
			Assert.Equal(10000, second.InputPosition);
		}

		[Fact]
		public void DifferentSeeds_ProduceDifferentState()
		{
			var trace = BuildInputTrace(10000);

			var first = new TestSimulation(1UL, trace);
			var second = new TestSimulation(2UL, trace);

			var firstHost = new EngineHost();
			var secondHost = new EngineHost();
			firstHost.Update += first.Update;
			secondHost.Update += second.Update;

			firstHost.RunFixedSteps(10000);
			secondHost.RunFixedSteps(10000);

			Assert.NotEqual(first.Snapshot(), second.Snapshot());
		}
	}
}
