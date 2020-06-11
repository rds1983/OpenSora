using Microsoft.Xna.Framework;
using OpenSora.Rendering;
using System;
using System.Collections.Generic;

namespace OpenSora.Scenarios
{
	public class ExecutionContext
	{
		public const float PositionScale = 1000.0f;

		private static readonly Dictionary<int, SceneCharacterInfo> _hardcodedNpcs = new Dictionary<int, SceneCharacterInfo>();

		private Scenario _scenario;
		private ScenarioFunctionInfo _function;
		private DateTime? _lastDt;

		public ExecutionWorker MainWorker { get; }
		public List<ExecutionWorker> AdditionalWorkers { get; } = new List<ExecutionWorker>();

		public ResourceLoader ResourceLoader
		{
			get
			{
				return Scene.ResourceLoader;
			}
		}

		public Scene Scene { get; }

		public float PlayedPart
		{
			get
			{
				return MainWorker.TotalPassedPart;
			}

			set
			{
				SetPlayedPart(value);
			}
		}

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

		static ExecutionContext()
		{
			_hardcodedNpcs[254] = new SceneCharacterInfo
			{
				ChipId = 1
			};


			_hardcodedNpcs[257] = new SceneCharacterInfo
			{
				ChipId = 8
			};

			_hardcodedNpcs[258] = new SceneCharacterInfo
			{
				ChipId = 9
			};

			_hardcodedNpcs[259] = new SceneCharacterInfo
			{
				ChipId = 11
			};
		}
		public ExecutionContext(ResourceLoader resourceLoader)
		{
			Scene = new Scene(resourceLoader);
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
			return new Vector3(x / PositionScale, y / PositionScale, -z / PositionScale);
		}

		public SceneCharacter EnsureCharacter(int id)
		{
			SceneCharacter result;
			if (Scene.Characters.TryGetValue(id, out result))
			{
				return result;
			}

			SceneCharacterInfo characterInfo;

			int chipIndex;
			var position = Vector3.Zero;
			if (_hardcodedNpcs.TryGetValue(id, out characterInfo))
			{
				chipIndex = characterInfo.ChipId;
			}
			else
			{
				var npc = GetNpcById(id);
				chipIndex = npc.ChipIndex;
				position = ToPosition(npc.X, npc.Y, npc.Z);
			}

			var chip = Scenario.ChipInfo[chipIndex];
			var animationEntry = ResourceLoader.FindByIndex(chip.ChipIndex);
			var animation = ResourceLoader.LoadAnimation(animationEntry);

			result = new SceneCharacter
			{
				Chip = animation,
				Position = position
			};

			Scene.Characters[id] = result;

			return result;
		}

		private void SetPlayedPart(float part)
		{
			// Rewind and remove additional workers
			MainWorker.Rewind();
			AdditionalWorkers.Clear();

			// Now execute everything up to the part
			int passedMs = (int)(part * MainWorker.TotalDurationInMs);
			MainWorker.Update(passedMs);
		}

		public static int DegreesToAnimationStart(int degrees)
		{
			var result = (int)Math.Round(degrees / 45.0f) + 1;

			while (result >= 8)
			{
				result -= 8;
			}

			return result;
		}
	}
}
