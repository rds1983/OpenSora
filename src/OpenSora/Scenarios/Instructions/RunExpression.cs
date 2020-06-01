using OpenSora.Scenarios.Expressions;

namespace OpenSora.Scenarios.Instructions
{
	public class RunExpression: BaseInstruction
	{
		public int Address { get; private set; }
		public Expression[] Expression { get; private set; }

		public override void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			base.Decompile(context, out branchTargets);

			Address = context.ReadUInt16();
			Expression = context.DecompileExpression();
		}
	}
}
