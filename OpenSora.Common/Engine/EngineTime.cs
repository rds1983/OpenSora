using System;

namespace OpenSora.Engine
{
	/// <summary>
	/// Timing information for one fixed engine update.
	/// </summary>
	public readonly struct EngineTime
	{
		public double DeltaSeconds { get; }
		public double TotalSeconds { get; }
		public long Tick { get; }

		public double DeltaMilliseconds
		{
			get
			{
				return DeltaSeconds * 1000.0;
			}
		}

		public EngineTime(double deltaSeconds, double totalSeconds, long tick)
		{
			if (deltaSeconds <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
			}

			DeltaSeconds = deltaSeconds;
			TotalSeconds = totalSeconds;
			Tick = tick;
		}
	}
}
