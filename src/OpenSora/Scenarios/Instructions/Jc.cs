using OpenSora.Scenarios.Expressions;

namespace OpenSora.Scenarios.Instructions
{
	public class Jc: BaseInstruction
	{
		public Expression[] Expressions { get; private set; }
		public int Offset { get; private set; }

		public override void Decompile(DecompilerContext context)
		{
			base.Decompile(context);

			Expressions = context.DecompileExpression();
			Offset = context.Reader.ReadUInt16();
		}
	}
}
