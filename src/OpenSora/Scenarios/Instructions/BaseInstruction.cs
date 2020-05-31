using System.Collections.Generic;

namespace OpenSora.Scenarios
{
	public class BaseInstruction
	{
		public int Offset { get; set; }
		public object[] Operands { get; set; }

		public virtual void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			branchTargets = null;

			var branchTargetsList = new List<int>();
			if (!string.IsNullOrEmpty(context.Entry.Operand))
			{
				var operands = new List<object>();
				for (var i = 0; i < context.Entry.Operand.Length; ++i)
				{
					char c = context.Entry.Operand[i];
					var op = context.DecompileOperand(c);
					if (char.ToLower(c) == 'o')
					{
						branchTargetsList.Add((int)op);
					}

					operands.Add(op);
				}

				Operands = operands.ToArray();
				branchTargets = branchTargetsList.ToArray();
			}
		}
	}
}