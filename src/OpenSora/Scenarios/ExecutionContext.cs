using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using System;
using System.Collections.Generic;

namespace OpenSora.Scenarios
{
	public class ExecutionWorker
	{
		private BaseInstruction[] _instructions;

		public int _currentInstructionIndex, _instructionPassed, _totalDurationInMs, _totalPassedInMs;
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
			_instructionPassed = 0;
			_currentInstructionIndex = 0;
			_totalPassedInMs = 0;
			TotalPassedPartChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Update(int passedMs)
		{
			if (_instructions == null || _instructions.Length == 0 || _totalPassedInMs >= _totalDurationInMs)
			{
				return;
			}

			_instructionPassed += passedMs;
			_totalPassedInMs += passedMs;

			if (_totalPassedInMs > _totalDurationInMs)
			{
				_totalPassedInMs = _totalDurationInMs;
			}

			TotalPassedPartChanged?.Invoke(this, EventArgs.Empty);

			for (; _currentInstructionIndex < _instructions.Length; ++_currentInstructionIndex)
			{
				var instruction = _instructions[_currentInstructionIndex];

				instruction.Update(this);

				if (_instructionPassed < instruction.DurationInMs)
				{
					break;
				}

				_instructionPassed -= instruction.DurationInMs;
			}
		}

	}

	public class ExecutionContext
	{
		public const float PositionScale = 1000.0f;

		private Scenario _scenario;
		private ScenarioFunctionInfo _function;
		private DateTime? _lastDt;

		public ExecutionWorker MainWorker { get; }
		public List<ExecutionWorker> AdditionalWorkers { get; } = new List<ExecutionWorker>();

		public ResourceLoader ResourceLoader { get; }
		public Scene Scene { get; set; }

		public Scenario Scenario
		{
			get
			{
				return _scenario;
			}

			set
			{
				_scenario = value;
				Function = null;
			}
		}

		public ScenarioFunctionInfo Function
		{
			get
			{
				return _function;
			}

			set
			{
				_function = value;
				_lastDt = null;
				if (value != null)
				{
					MainWorker.Instructions = value.Instructions;
				} else
				{
					MainWorker.Instructions = null;
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				return _lastDt != null;
			}
		}


		public ExecutionContext(ResourceLoader resourceLoader)
		{
			ResourceLoader = resourceLoader;
			MainWorker = new ExecutionWorker(this);
		}

		public void PlayPause()
		{
			if (_lastDt == null)
			{
				_lastDt = DateTime.Now;
			} else
			{
				_lastDt = null;
			}
		}

		public void Update()
		{
			if (_lastDt == null)
			{
				return;
			}

			var passedMs = (int)(DateTime.Now - _lastDt.Value).TotalMilliseconds;

			MainWorker.Update(passedMs);

			foreach(var worker in AdditionalWorkers)
			{
				worker.Update(passedMs);
			}

			AdditionalWorkers.RemoveAll(w => w.Finished);

			_lastDt = DateTime.Now;
		}

		public ScenarioNpcInfo GetNpcById(int id)
		{
			return Scenario.NpcInfo[id - 8];
		}

		public static Vector3 ToPosition(int x, int y, int z)
		{
			return new Vector3(x / PositionScale, y / PositionScale, z / PositionScale);
		}

		public SceneCharacterInfo EnsureCharacter(int id)
		{
			SceneCharacterInfo result;
			if (Scene.Characters.TryGetValue(id, out result))
			{
				return result;
			}

			var npc = GetNpcById(id);
			var chip = Scenario.ChipInfo[npc.ChipIndex];
			var animationEntry = ResourceLoader.FindByIndex(chip.ChipIndex);
			var animation = ResourceLoader.LoadAnimation(animationEntry);

			result = new SceneCharacterInfo
			{
				Chip = animation,
				Position = ToPosition(npc.X, npc.Y, npc.Z)
			};

			Scene.Characters[id] = result;

			return result;
		}
	}
}
