namespace OpenSora.Engine
{
	/// <summary>
	/// Supplies the elapsed time for one rendered frame. The engine converts frame time into fixed
	/// simulation steps; implementations must not be read by simulation code directly.
	/// </summary>
	public interface IFrameTimeSource
	{
		double DeltaSeconds { get; }
	}

	/// <summary>
	/// Mutable frame-time source for host applications, tests, and recorded frame timings.
	/// </summary>
	public sealed class FrameTimeSource : IFrameTimeSource
	{
		public double DeltaSeconds { get; set; }

		public FrameTimeSource(double deltaSeconds = 0)
		{
			DeltaSeconds = deltaSeconds;
		}
	}
}
