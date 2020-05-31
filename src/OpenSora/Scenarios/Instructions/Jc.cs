using OpenSora.Scenarios.Expressions;

namespace OpenSora.Scenarios.Instructions
{
	public class Jc: BaseInstruction
	{
		public Expression[] Expressions { get; private set; }
		public int JcOffset { get; private set; }

		public override void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			base.Decompile(context, out branchTargets);

			Expressions = context.DecompileExpression();
			JcOffset = context.Reader.ReadUInt16();
			branchTargets = new int[] { JcOffset };
		}
	}
}
