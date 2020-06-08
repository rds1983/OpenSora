namespace OpenSora.Scenarios.Instructions
{
	public class ChrTalk: BaseInstruction
	{
		public const int PhraseDurationInMs = 3000;

		public int CharId
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public ScpString[] ScpStrings
		{
			get
			{
				return (ScpString[])Operands[1];
			}
		}

		public override int DurationInMs
		{
			get
			{
				var result = 0;

				foreach(var str in ScpStrings)
				{
					if (str.Type == ScpStringType.SCPSTR_CODE_STRING && !string.IsNullOrEmpty(str.String))
					{
						result += PhraseDurationInMs;
					}
				}

				return result;
			}

		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			var passed = worker.InstructionPassedInMs;
			var i = 0;
			for(; i < ScpStrings.Length; ++i)
			{
				var str = ScpStrings[i];
				if (str.Type == ScpStringType.SCPSTR_CODE_STRING && !string.IsNullOrEmpty(str.String))
				{
					if (passed < PhraseDurationInMs)
					{
						break;
					}

					passed -= PhraseDurationInMs;
				}
			}

			if (i >= ScpStrings.Length)
			{
				return;
			}

			var text = ScpStrings[i];
			worker.Context.Scene.ShowTalk(CharId, text.String);
		}
	}
}
