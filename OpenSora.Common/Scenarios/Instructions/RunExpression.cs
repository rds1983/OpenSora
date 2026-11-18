using OpenSora.Scenarios.Expressions;
using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class RunExpression: BaseInstruction
	{
		public int Address
		{
			get
			{
				return (int)Operands[0];
			}
		}
		public Expression[] Expression
		{
			get
			{
				return (Expression[])Operands[1];
			}
		}

		internal static void Decompile(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			operands.Add(context.ReadUInt16());
			operands.Add(context.DecompileExpression());
		}
	}
}
