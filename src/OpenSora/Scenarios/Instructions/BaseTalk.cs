using Myra.Graphics2D.TextureAtlases;
using OpenSora.Rendering;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSora.Scenarios.Instructions
{
	public abstract class BaseTalk: BaseInstruction
	{
		private static readonly Regex _portraitRegex = new Regex(@"#(\d+)F(.*)", RegexOptions.Singleline);
		private static readonly Regex _cleaningRegex = new Regex(@"#(\d+)P(.*)", RegexOptions.Singleline);

		public const int SymbolDurationInMs = 30;
		public const int PhraseDurationInMs = 3000;

		private TalkString[] TalkStrings;

		public abstract ScpString[] ScpStrings
		{
			get;
		}

		public override int DurationInMs
		{
			get
			{
				EnsureStringsInternal();
				var result = 0;

				foreach(var str in TalkStrings)
				{
					result += CalculateStringDuration(str.Text);
				}

				return result;
			}
		}

		private static int CalculateStringDuration(string s)
		{
			return s.Length * SymbolDurationInMs + PhraseDurationInMs;
		}

		private void EnsureStringsInternal()
		{
			if (TalkStrings != null)
			{
				return;
			}

			// Merge some strings
			string lastPortraitId = null;
			var internalStrings = new List<TalkString>();
			var sb = new StringBuilder();
			for(var i = 0; i < ScpStrings.Length;)
			{
				var str = ScpStrings[i];

				if (str.Type != ScpStringType.SCPSTR_CODE_STRING)
				{
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

				var text = sb.ToString();

				var newString = new TalkString
				{
					PortraitId = lastPortraitId,
					Text = text
				};

				var match = _portraitRegex.Match(text);
				if (match != null && match.Success)
				{
					newString.PortraitId = match.Groups[1].Value;
					newString.Text = match.Groups[2].Value;

					if (lastPortraitId == null)
					{
						lastPortraitId = newString.PortraitId;
					}
				}

				match = _cleaningRegex.Match(newString.Text);
				if (match != null && match.Success)
				{
					newString.Text = match.Groups[2].Value;
				}

				if (!string.IsNullOrEmpty(newString.Text))
				{
					internalStrings.Add(newString);
				}
			}

			TalkStrings = internalStrings.ToArray();
		}

		public override void Update(ExecutionWorker worker)
		{
			base.Update(worker);

			EnsureStringsInternal();

			var passed = worker.InstructionPassedInMs;
			var i = 0;

			TalkString str;
			for (; i < TalkStrings.Length; ++i)
			{
				str = TalkStrings[i];
				var duration = CalculateStringDuration(str.Text);
				if (passed < duration)
				{
					break;
				}

				passed -= duration;
			}

			if (i >= TalkStrings.Length)
			{
				return;
			}

			str = TalkStrings[i];
			if (str.PortraitId != null && str.Portrait == null)
			{
				str.Portrait = new TextureRegion(worker.Context.ResourceLoader.GetCharacterPortrait(str.PortraitId));
			}

			int symbolsCount = str.Text.Length;
			if (passed < symbolsCount * SymbolDurationInMs)
			{
				symbolsCount = passed / SymbolDurationInMs;
			}

			worker.Context.Scene.ShowTalk(str, symbolsCount);
		}
	}
}
