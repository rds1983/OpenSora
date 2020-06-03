using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class QueueWorkItem2 : BaseInstruction
	{
		public int Target
		{
			get
			{
				return (int)Operands[0];
			}
		}

		public int TargetId
		{
			get
			{
				return (int)Operands[1];
			}
		}

		public BaseInstruction[] Block
		{
			get
			{
				return (BaseInstruction[])Operands[2];
			}
		}
		internal static void Decompile(DecompilerContext context, ref List<object> operands, ref List<int> branchTargets)
		{
			QueueWorkItem.InternalDecompile(context, ref operands, 3);
		}
	}
}
