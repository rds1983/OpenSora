namespace OpenSora.Scenarios.Instructions
{
	public class SetChrChipByIndex: BaseInstruction
	{
		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int ChipId
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public override void Begin(ExecutionWorker worker)
		{
			base.Begin(worker);

			var ch = worker.Context.GetCharacter(CharId);
			if (ch == null)
			{
				return;
			}

			if (ChipId == 65535)
			{
				worker.Context.ResetCharacterChip(CharId);
			}
			else
			{
				if (ChipId >= worker.Context.Scenario.ChipInfo.Length)
				{
					return;
				}

				var chip = worker.Context.Scenario.ChipInfo[ChipId];
				var entry = worker.Context.ResourceLoader.FindByIndex(chip.ChipIndex);
				ch.Chip = worker.Context.ResourceLoader.LoadAnimation(entry);
			}
		}
	}
}
