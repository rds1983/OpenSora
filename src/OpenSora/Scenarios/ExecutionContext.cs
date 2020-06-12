using Microsoft.Xna.Framework;
using OpenSora.Dir;
using OpenSora.Rendering;
using OpenSora.Scenarios.Instructions;
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
					if (value.Instructions != null)
					{
						var toAdd = new HashSet<int>();
						var toIgnore = new HashSet<int>();
						// Place characters
						foreach (var ins in value.Instructions)
						{
							var asSetCharPosition = ins as SetChrPos;
							if (asSetCharPosition != null)
							{
								toIgnore.Add(asSetCharPosition.CharId);
							}

							var asTalk = ins as BaseTalk;
							if (asTalk != null && asTalk.CharId != null)
							{
								toAdd.Add(asTalk.CharId.Value);
							}
						}

						foreach(var id in toAdd)
						{
							if (!toIgnore.Contains(id))
							{
								EnsureCharacter(id);
							}
						}
					}
				}
				else
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
			_hardcodedNpcs[257] = new SceneCharacterInfo
			{
				ChipId = "00000"
			};

			_hardcodedNpcs[258] = new SceneCharacterInfo
			{
				ChipId = "00010"
			};

			_hardcodedNpcs[259] = new SceneCharacterInfo
			{
				ChipId = "00020"
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
			}
			else
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

			foreach (var worker in AdditionalWorkers)
			{
				worker.Update(passedMs);
			}

			AdditionalWorkers.RemoveAll(w => w.Finished);

			_lastDt = DateTime.Now;
		}

		public ScenarioNpcInfo GetNpcById(int id)
		{
			id -= 8;
			if (id < 0 || id >= Scenario.NpcInfo.Length)
			{
				return null;
			}

			return Scenario.NpcInfo[id];
		}

		public static Vector3 ToPosition(int x, int y, int z)
		{
			return new Vector3(x / PositionScale, y / PositionScale, -z / PositionScale);
		}

		private Animation GetCharacterInfo(int id, out Vector3 position)
		{
			int chipIndex;
			position = Vector3.Zero;

			DirEntry animationEntry;
			SceneCharacterInfo characterInfo;
			if (_hardcodedNpcs.TryGetValue(id, out characterInfo))
			{
				animationEntry = ResourceLoader.FindByName("CH" + characterInfo.ChipId);
			}
			else
			{
				var npc = GetNpcById(id);
				chipIndex = npc.ChipIndex;
				position = ToPosition(npc.X, npc.Y, npc.Z);
				var chip = Scenario.ChipInfo[chipIndex];
				animationEntry = ResourceLoader.FindByIndex(chip.ChipIndex);
			}

			return ResourceLoader.LoadAnimation(animationEntry);
		}

		public SceneCharacter EnsureCharacter(int id)
		{
			SceneCharacter result;
			if (Scene.Characters.TryGetValue(id, out result))
			{
				return result;
			}

			Vector3 position;
			var animation = GetCharacterInfo(id, out position);

			result = new SceneCharacter
			{
				Chip = animation,
				Position = position
			};

			Scene.Characters[id] = result;

			return result;
		}

		public void ResetCharacterChip(int id)
		{
			var ch = EnsureCharacter(id);

			Vector3 position;
			var animation = GetCharacterInfo(id, out position);

			ch.Chip = animation;
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
