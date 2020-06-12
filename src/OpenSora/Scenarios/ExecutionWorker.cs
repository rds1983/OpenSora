using OpenSora.Scenarios.Instructions;
using System;

namespace OpenSora.Scenarios
{
	public class ExecutionWorker
	{
		private BaseInstruction[] _instructions;

		public int _currentInstructionIndex, _instructionPassedInMs, _totalDurationInMs, _totalPassedInMs;
		public bool _callBegin = true;
		public ExecutionContext Context { get; }

		public BaseInstruction[] Instructions
		{
			get
			{
				return _instructions;
			}

			set
			{
				_instructions = value;
				_totalDurationInMs = 0;
				if (_instructions != null)
				{
					for (var i = 0; i < _instructions.Length; ++i)
					{
						_totalDurationInMs += _instructions[i].DurationInMs;
					}
				}

				Rewind();
			}
		}

		public int InstructionPassedInMs
		{
			get
			{
				return _instructionPassedInMs;
			}
		}

		public int InstructionTotalInMs
		{
			get
			{
				if (_instructions == null || _currentInstructionIndex >= _instructions.Length)
				{
					return 0;
				}

				return _instructions[_currentInstructionIndex].DurationInMs;
			}
		}

		public float InstructionPassedPart
		{
			get
			{
				var total = InstructionTotalInMs;
				if (total == 0)
				{
					return 0;
				}

				return (float)InstructionPassedInMs / total;
			}
		}

		public int TotalPassedInMs
		{
			get
			{
				return _totalPassedInMs;
			}
		}

		public int TotalDurationInMs
		{
			get
			{
				return _totalDurationInMs;
			}
		}

		public float TotalPassedPart
		{
			get
			{
				if (_totalDurationInMs == 0)
				{
					return 0;
				}

				return (float)_totalPassedInMs / _totalDurationInMs;
			}
		}

		public bool Finished
		{
			get
			{
				return _totalPassedInMs >= _totalDurationInMs;
			}
		}

		public event EventHandler TotalPassedPartChanged;

		public ExecutionWorker(ExecutionContext context)
		{
			if (context == null)
			{
				throw new ArgumentOutOfRangeException(nameof(context));
			}

			Context = context;
		}

		public void Rewind()
		{
			_instructionPassedInMs = 0;
			_callBegin = true;
			_currentInstructionIndex = 0;
			_totalPassedInMs = 0;
			TotalPassedPartChanged?.Invoke(this, EventArgs.Empty);
			Context.Scene.Characters.Clear();
			Context.Scene.CloseMessageWindow();
		}

		public void Update(int passedMs)
		{
			if (_instructions == null || _instructions.Length == 0 || _totalPassedInMs >= _totalDurationInMs)
			{
				return;
			}

			_instructionPassedInMs += passedMs;
			_totalPassedInMs += passedMs;

			if (_totalPassedInMs > _totalDurationInMs)
			{
				_totalPassedInMs = _totalDurationInMs;
			}

			TotalPassedPartChanged?.Invoke(this, EventArgs.Empty);

			for (; _currentInstructionIndex < _instructions.Length; ++_currentInstructionIndex)
			{
				var instruction = _instructions[_currentInstructionIndex];

				if (_callBegin)
				{
					instruction.Begin(this);
					_callBegin = false;
				}

				if (_instructionPassedInMs < instruction.DurationInMs)
				{
					instruction.Update(this);
					break;
				}

				instruction.End(this);
				_callBegin = true;

				_instructionPassedInMs -= instruction.DurationInMs;
			}
		}

	}
}
