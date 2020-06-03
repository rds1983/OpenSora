using OpenSora.Scenarios.Expressions;
using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class Jc: BaseInstruction
	{
		public Expression[] Expressions
		{
			get
			{
				return (Expression[])Operands[0];
			}
		}

		public int JcOffset
		{
			get
			{
				return (int)Operands[1];
			}
		}

		internal static void Decompile(DecompilerContext context, ref List<object> operands, ref List<int> branchTargets)
		{
			operands.Add(context.DecompileExpression());

			var offset = context.ReadUInt16();
			operands.Add(offset);

			branchTargets.Add(offset);
		}
	}
}
