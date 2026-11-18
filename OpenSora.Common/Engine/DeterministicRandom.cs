using System;

namespace OpenSora.Engine
{
	/// <summary>
	/// Deterministic pseudo-random number generator based on SplitMix64. The state is explicit and
	/// serializable so saves and replays can reproduce the exact sequence.
	/// </summary>
	public sealed class DeterministicRandom
	{
		private const ulong GoldenGamma = 0x9E3779B97F4A7C15UL;

		public ulong Seed { get; }

		/// <summary>
		/// Current generator state. Assign a previously captured value to restore a sequence.
		/// </summary>
		public ulong State { get; set; }

		public DeterministicRandom(ulong seed)
		{
			Seed = seed;
			State = seed;
		}

		/// <summary>
		/// Rewinds the generator to its initial seed.
		/// </summary>
		public void Reset()
		{
			State = Seed;
		}

		public ulong NextUInt64()
		{
			State += GoldenGamma;

			var value = State;
			value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
			value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
			return value ^ (value >> 31);
		}

		public uint NextUInt()
		{
			return (uint)(NextUInt64() >> 32);
		}

		public int NextInt(int maxExclusive)
		{
			if (maxExclusive <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxExclusive));
			}

			return (int)(NextUInt64() % (ulong)maxExclusive);
		}

		public int NextInt(int minInclusive, int maxExclusive)
		{
			if (maxExclusive <= minInclusive)
			{
				throw new ArgumentOutOfRangeException(nameof(maxExclusive));
			}

			var range = (ulong)((long)maxExclusive - minInclusive);
			return (int)(minInclusive + (long)(NextUInt64() % range));
		}

		/// <summary>
		/// Returns a value in the range [0.0, 1.0).
		/// </summary>
		public double NextDouble()
		{
			return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
		}

		/// <summary>
		/// Returns a value in the range [0.0, 1.0).
		/// </summary>
		public float NextFloat()
		{
			return (NextUInt64() >> 40) * (1.0f / 16777216.0f);
		}

		public bool NextBool()
		{
			return (NextUInt64() & 1) != 0;
		}
	}
}
