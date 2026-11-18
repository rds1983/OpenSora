using OpenSora.Engine;
using System;
using System.Collections.Generic;
using Xunit;

namespace OpenSora.Tests
{
	public class DeterministicRandomTests
	{
		[Fact]
		public void SameSeed_ProducesSameSequence()
		{
			var first = new DeterministicRandom(123456789UL);
			var second = new DeterministicRandom(123456789UL);

			for (var i = 0; i < 1000; ++i)
			{
				Assert.Equal(first.NextUInt64(), second.NextUInt64());
			}
		}

		[Fact]
		public void DifferentSeeds_ProduceDifferentSequences()
		{
			var first = new DeterministicRandom(1UL);
			var second = new DeterministicRandom(2UL);

			Assert.NotEqual(first.NextUInt64(), second.NextUInt64());
		}

		[Fact]
		public void StateRestore_ContinuesSequence()
		{
			var random = new DeterministicRandom(42UL);
			for (var i = 0; i < 10; ++i)
			{
				random.NextUInt64();
			}

			var state = random.State;
			var expected = new List<ulong>();
			for (var i = 0; i < 10; ++i)
			{
				expected.Add(random.NextUInt64());
			}

			random.State = state;

			for (var i = 0; i < expected.Count; ++i)
			{
				Assert.Equal(expected[i], random.NextUInt64());
			}
		}

		[Fact]
		public void NextInt_StaysInRange()
		{
			var random = new DeterministicRandom(7UL);

			for (var i = 0; i < 10000; ++i)
			{
				var value = random.NextInt(-5, 6);
				Assert.InRange(value, -5, 5);
			}
		}

		[Fact]
		public void NextDoubleAndFloat_StayInUnitInterval()
		{
			var random = new DeterministicRandom(99UL);

			for (var i = 0; i < 10000; ++i)
			{
				var doubleValue = random.NextDouble();
				var floatValue = random.NextFloat();

				Assert.True(doubleValue >= 0.0 && doubleValue < 1.0);
				Assert.True(floatValue >= 0.0f && floatValue < 1.0f);
			}
		}

		[Fact]
		public void InvalidRanges_Throw()
		{
			var random = new DeterministicRandom(1UL);

			Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(5, 5));
			Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(6, 5));
		}
	}
}
