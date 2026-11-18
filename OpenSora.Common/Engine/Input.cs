using System;
using System.Collections.Generic;

namespace OpenSora.Engine
{
	/// <summary>
	/// Logical game actions. The viewer and game-specific input mappers translate physical keys,
	/// mouse, and gamepad state into these values.
	/// </summary>
	[Flags]
	public enum InputButton : ulong
	{
		None = 0,
		Confirm = 1UL << 0,
		Cancel = 1UL << 1,
		Menu = 1UL << 2,
		Run = 1UL << 3,
		PageLeft = 1UL << 4,
		PageRight = 1UL << 5,
		Up = 1UL << 6,
		Down = 1UL << 7,
		Left = 1UL << 8,
		Right = 1UL << 9,
		CameraLeft = 1UL << 10,
		CameraRight = 1UL << 11,
		CameraUp = 1UL << 12,
		CameraDown = 1UL << 13
	}

	/// <summary>
	/// Immutable input for one fixed update. Analog axes are in the range [-1, 1].
	/// </summary>
	public readonly struct InputSnapshot : IEquatable<InputSnapshot>
	{
		public static readonly InputSnapshot Empty = new InputSnapshot();

		public InputButton Buttons { get; }
		public float MoveX { get; }
		public float MoveY { get; }
		public float LookX { get; }
		public float LookY { get; }

		public InputSnapshot(InputButton buttons, float moveX = 0, float moveY = 0, float lookX = 0, float lookY = 0)
		{
			Buttons = buttons;
			MoveX = moveX;
			MoveY = moveY;
			LookX = lookX;
			LookY = lookY;
		}

		public bool IsDown(InputButton button)
		{
			return button != InputButton.None && (Buttons & button) == button;
		}

		public bool Equals(InputSnapshot other)
		{
			return Buttons == other.Buttons &&
				MoveX.Equals(other.MoveX) &&
				MoveY.Equals(other.MoveY) &&
				LookX.Equals(other.LookX) &&
				LookY.Equals(other.LookY);
		}

		public override bool Equals(object obj)
		{
			return obj is InputSnapshot other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Buttons, MoveX, MoveY, LookX, LookY);
		}

		public override string ToString()
		{
			return string.Format("Buttons={0}, Move=({1}, {2}), Look=({3}, {4})",
				Buttons, MoveX, MoveY, LookX, LookY);
		}
	}

	/// <summary>
	/// Supplies input snapshots to the simulation. Sources are sampled once per fixed update.
	/// </summary>
	public interface IInputSource
	{
		InputSnapshot Sample();
	}

	/// <summary>
	/// Ordered per-tick input snapshots used for replay, regression tests, and debugging.
	/// </summary>
	public sealed class InputTrace
	{
		private readonly List<InputSnapshot> _snapshots = new List<InputSnapshot>();

		public int Count
		{
			get
			{
				return _snapshots.Count;
			}
		}

		public InputSnapshot this[int index]
		{
			get
			{
				return _snapshots[index];
			}
		}

		public InputTrace()
		{
		}

		public InputTrace(IEnumerable<InputSnapshot> snapshots)
		{
			if (snapshots == null)
			{
				throw new ArgumentNullException(nameof(snapshots));
			}

			_snapshots.AddRange(snapshots);
		}

		public void Add(InputSnapshot snapshot)
		{
			_snapshots.Add(snapshot);
		}

		public void Clear()
		{
			_snapshots.Clear();
		}

		public InputTrace Clone()
		{
			return new InputTrace(_snapshots);
		}
	}

	/// <summary>
	/// Wraps another input source and records every sampled snapshot.
	/// </summary>
	public sealed class RecordingInputSource : IInputSource
	{
		private readonly IInputSource _inner;

		public InputTrace Trace { get; } = new InputTrace();

		public RecordingInputSource(IInputSource inner)
		{
			if (inner == null)
			{
				throw new ArgumentNullException(nameof(inner));
			}

			_inner = inner;
		}

		public InputSnapshot Sample()
		{
			var snapshot = _inner.Sample();
			Trace.Add(snapshot);
			return snapshot;
		}
	}

	/// <summary>
	/// Replays a recorded input trace in order. Once the trace is exhausted, empty snapshots are
	/// returned so the simulation can keep running deterministically.
	/// </summary>
	public sealed class ReplayInputSource : IInputSource
	{
		private readonly InputTrace _trace;
		private int _index;

		public int Position
		{
			get
			{
				return _index;
			}
		}

		public bool IsFinished
		{
			get
			{
				return _index >= _trace.Count;
			}
		}

		public ReplayInputSource(InputTrace trace)
		{
			if (trace == null)
			{
				throw new ArgumentNullException(nameof(trace));
			}

			_trace = trace;
		}

		public InputSnapshot Sample()
		{
			if (IsFinished)
			{
				return InputSnapshot.Empty;
			}

			return _trace[_index++];
		}

		public void Rewind()
		{
			_index = 0;
		}
	}
}
