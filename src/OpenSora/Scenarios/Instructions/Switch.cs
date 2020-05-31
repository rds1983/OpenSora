using OpenSora.Scenarios.Expressions;
using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class Switch: BaseInstruction
	{
		public class Option
		{
			public int Id { get; }
			public int Offset { get; }

			public Option(int id, int offset)
			{
				Id = id;
				Offset = offset;
			}
		}

		public Expression[] Expression { get; private set; }
		public Option[] Options { get; private set; }
		public int DefaultOffset { get; private set; }


		public override void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			base.Decompile(context, out branchTargets);

			Expression = context.DecompileExpression();

			var optionsCount = (int)context.Reader.ReadUInt16();

			var options = new List<Option>();
			var targets = new List<int>();
			for(var i = 0; i < optionsCount; ++i)
			{
				var option = new Option(context.Reader.ReadUInt16(), context.Reader.ReadUInt16());
				options.Add(option);
				targets.Add(option.Offset);
			}

			Options = options.ToArray();
			DefaultOffset = context.Reader.ReadUInt16();
			targets.Add(DefaultOffset);

			branchTargets = targets.ToArray();
		}
	}
}
