using System.IO;

namespace OpenSora.Scenarios.Instructions
{
	public class QueueWorkItem2: BaseInstruction
	{
		public int Target { get; private set; }
		public int TargetId { get; private set; }
		public int Length { get; private set; }
		public BaseInstruction[] Block { get; private set; }

		public override void Decompile(DecompilerContext context, out int[] branchTargets)
		{
			base.Decompile(context, out branchTargets);

			Target = context.ReadUInt16();
			TargetId = context.ReadUInt16();
			Length = context.ReadByte();

			Length += 3;

			var pos = context.Reader.BaseStream.Position;
			Block = context.DecompileBlock();
			context.Reader.BaseStream.Seek(pos + Length, SeekOrigin.Begin);
		}
	}
}
