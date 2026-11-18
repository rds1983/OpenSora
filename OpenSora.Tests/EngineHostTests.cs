using OpenSora.Engine;
using System;
using System.Collections.Generic;
using Xunit;

namespace OpenSora.Tests
{
	public class EngineHostTests
	{
		[Fact]
		public void Advance_EmitsFixedSteps()
		{
			var host = new EngineHost(0.25, maxFrameSeconds: 1.0);
			var times = new List<EngineTime>();
			host.Update += times.Add;

			host.Advance(1.0);

			Assert.Equal(4, times.Count);
			Assert.Equal(4, host.Tick);
			Assert.Equal(1.0, host.TotalSeconds, 10);

			for (var i = 0; i < times.Count; ++i)
			{
				Assert.Equal(i + 1, times[i].Tick);
				Assert.Equal(0.25, times[i].DeltaSeconds, 10);
				Assert.Equal(0.25 * (i + 1), times[i].TotalSeconds, 10);
			}
		}

		[Fact]
		public void Advance_ClampsLongFrames()
		{
			var host = new EngineHost(0.1, maxFrameSeconds: 0.25);
			var updateCount = 0;
			host.Update += _ => ++updateCount;

			host.Advance(10.0);

			Assert.Equal(2, updateCount);
			Assert.Equal(2, host.Tick);
		}

		[Fact]
		public void RunFixedSteps_IsDeterministic()
		{
			var first = new EngineHost();
			var second = new EngineHost();
			var firstTrace = new List<double>();
			var secondTrace = new List<double>();
			var firstState = 0.0;
			var secondState = 0.0;

			first.Update += time =>
			{
				firstTrace.Add(time.TotalSeconds);
				firstState = firstState * 1.0001 + time.DeltaSeconds * 3.0;
			};
			second.Update += time =>
			{
				secondTrace.Add(time.TotalSeconds);
				secondState = secondState * 1.0001 + time.DeltaSeconds * 3.0;
			};

			first.RunFixedSteps(10000);
			second.RunFixedSteps(10000);

			Assert.Equal(10000, first.Tick);
			Assert.Equal(10000, second.Tick);
			Assert.Equal(firstTrace, secondTrace);
			Assert.Equal(firstState, secondState);
		}

		[Fact]
		public void Advance_NegativeElapsed_Throws()
		{
			var host = new EngineHost();

			Assert.Throws<ArgumentOutOfRangeException>(() => host.Advance(-0.01));
		}
	}
}
