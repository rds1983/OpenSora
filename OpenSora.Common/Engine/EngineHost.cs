using System;

namespace OpenSora.Engine
{
	/// <summary>
	/// Owns the fixed-timestep simulation clock. Rendering and input remain the host application's
	/// responsibility; the engine only emits deterministic update ticks.
	/// </summary>
	public sealed class EngineHost
	{
		public const double DefaultFixedDeltaSeconds = 1.0 / 60.0;
		public const double DefaultMaxFrameSeconds = 0.25;

		private double _accumulatorSeconds;
		private double _totalSeconds;
		private long _tick;

		public double FixedDeltaSeconds { get; }
		public double MaxFrameSeconds { get; }

		public long Tick
		{
			get
			{
				return _tick;
			}
		}

		public double TotalSeconds
		{
			get
			{
				return _totalSeconds;
			}
		}

		/// <summary>
		/// Raised once per fixed update. Handlers must be deterministic and must not read wall-clock
		/// time or device state directly.
		/// </summary>
		public event Action<EngineTime> Update;

		public EngineHost(double fixedDeltaSeconds = DefaultFixedDeltaSeconds, double maxFrameSeconds = DefaultMaxFrameSeconds)
		{
			if (fixedDeltaSeconds <= 0 || double.IsNaN(fixedDeltaSeconds) || double.IsInfinity(fixedDeltaSeconds))
			{
				throw new ArgumentOutOfRangeException(nameof(fixedDeltaSeconds));
			}

			if (maxFrameSeconds <= 0 || double.IsNaN(maxFrameSeconds) || double.IsInfinity(maxFrameSeconds))
			{
				throw new ArgumentOutOfRangeException(nameof(maxFrameSeconds));
			}

			FixedDeltaSeconds = fixedDeltaSeconds;
			MaxFrameSeconds = maxFrameSeconds;
		}

		/// <summary>
		/// Adds elapsed frame time and runs as many fixed updates as the accumulator allows. Long
		/// frames are clamped to <see cref="MaxFrameSeconds"/> to prevent catch-up spirals.
		/// </summary>
		public void Advance(TimeSpan elapsed)
		{
			Advance(elapsed.TotalSeconds);
		}

		/// <summary>
		/// Adds the delta supplied by a frame-time source. The source is sampled once per frame; the
		/// host still owns the fixed-step accumulator.
		/// </summary>
		public void Advance(IFrameTimeSource timeSource)
		{
			if (timeSource == null)
			{
				throw new ArgumentNullException(nameof(timeSource));
			}

			Advance(timeSource.DeltaSeconds);
		}

		/// <summary>
		/// Adds elapsed frame time and runs as many fixed updates as the accumulator allows. Long
		/// frames are clamped to <see cref="MaxFrameSeconds"/> to prevent catch-up spirals.
		/// </summary>
		public void Advance(double elapsedSeconds)
		{
			if (elapsedSeconds < 0 || double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds))
			{
				throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
			}

			_accumulatorSeconds += Math.Min(elapsedSeconds, MaxFrameSeconds);
			PumpFixedUpdates();
		}

		/// <summary>
		/// Runs exactly <paramref name="count"/> fixed updates. Intended for headless runs, tests, and
		/// replay tooling where no frame-time source is involved.
		/// </summary>
		public void RunFixedSteps(int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			for (var i = 0; i < count; ++i)
			{
				Step();
			}
		}

		public void Reset()
		{
			_accumulatorSeconds = 0;
			_totalSeconds = 0;
			_tick = 0;
		}

		private void PumpFixedUpdates()
		{
			while (_accumulatorSeconds >= FixedDeltaSeconds)
			{
				_accumulatorSeconds -= FixedDeltaSeconds;
				Step();
			}
		}

		private void Step()
		{
			++_tick;
			_totalSeconds += FixedDeltaSeconds;
			Update?.Invoke(new EngineTime(FixedDeltaSeconds, _totalSeconds, _tick));
		}
	}
}
