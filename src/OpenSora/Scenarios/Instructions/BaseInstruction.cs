using System.Collections.Generic;

namespace OpenSora.Scenarios
{
	public class BaseInstruction
	{
		public int Offset { get; set; }
		public object[] Operands { get; set; }

		internal static void DecompileDefault(DecompilerContext context, ref List<object> operands, ref List<int> branchTargets)
		{
			if (string.IsNullOrEmpty(context.Entry.Operand))
			{
				return;
			}

			for (var i = 0; i < context.Entry.Operand.Length; ++i)
			{
				char c = context.Entry.Operand[i];
				var op = context.DecompileOperand(c);
				if (char.ToLower(c) == 'o')
				{
					branchTargets.Add((int)op);
				}

				operands.Add(op);
			}
		}
	}
}
