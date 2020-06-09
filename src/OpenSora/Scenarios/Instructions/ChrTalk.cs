using System.Collections.Generic;
using System.Text;

namespace OpenSora.Scenarios.Instructions
{
	public class ChrTalk: BaseInstruction
	{
		public const int PhraseDurationInMs = 3000;

		private ScpString[] ScpStringsInternal;

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
				EnsureStringsInternal();
				var result = 0;

				foreach(var str in ScpStringsInternal)
				{
					if (str.Type == ScpStringType.SCPSTR_CODE_STRING && !string.IsNullOrEmpty(str.String))
					{
						result += PhraseDurationInMs;
					}
				}

				return result;
			}
		}

		private void EnsureStringsInternal()
		{
			if (ScpStringsInternal != null)
			{
				return;
			}

			// Merge some strings
			var internalStrings = new List<ScpString>();
			var sb = new StringBuilder();
			for(var i = 0; i < ScpStrings.Length;)
			{
				var str = ScpStrings[i];

				if (str.Type != ScpStringType.SCPSTR_CODE_STRING)
				{
					internalStrings.Add(str);
					++i;
					continue;
				}

				sb.Clear();
				var j = i;
				for (;j < ScpStrings.Length; ++j)
				{
					str = ScpStrings[j];
					if (str.Type == ScpStringType.SCPSTR_CODE_STRING)
					{
						if (!string.IsNullOrEmpty(str.String))
						{
							sb.Append(str.String);
						}
					} else if (str.Type == ScpStringType.SCPSTR_CODE_LINE_FEED)
					{
						sb.Append("\n");
					} else
					{
						break;
					}
				}

				i = j;
				internalStrings.Add(new ScpString(sb.ToString()));
			}

			ScpStringsInternal = internalStrings.ToArray();
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			EnsureStringsInternal();

			var passed = worker.InstructionPassedInMs;
			var i = 0;
			for(; i < ScpStringsInternal.Length; ++i)
			{
				var str = ScpStringsInternal[i];
				if (str.Type == ScpStringType.SCPSTR_CODE_STRING && !string.IsNullOrEmpty(str.String))
				{
					if (passed < PhraseDurationInMs)
					{
						break;
					}

					passed -= PhraseDurationInMs;
				}
			}

			if (i >= ScpStringsInternal.Length)
			{
				return;
			}

			var text = ScpStringsInternal[i];
			worker.Context.Scene.ShowTalk(CharId, text.String);
		}
	}
}
