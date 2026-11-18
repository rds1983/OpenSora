using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class QueueWorkItem2 : QueueWorkItem
	{
		internal static void Decompile2(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			InternalDecompile(context, ref operands, 4);
		}
	}
}
