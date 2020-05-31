namespace OpenSora.Scenarios.Instructions
{
	public class Jump: BaseInstruction
	{
		public override void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			base.Decompile(context, out branchTargets);
		}
	}
}
