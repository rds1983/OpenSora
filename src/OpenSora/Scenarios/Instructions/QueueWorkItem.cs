using System.Collections.Generic;
using System.IO;

namespace OpenSora.Scenarios.Instructions
{
	public class QueueWorkItem : BaseInstruction
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

		internal static void InternalDecompile(DecompilerContext context, ref List<object> operands, int extraLength)
		{
			operands.Add(context.ReadUInt16());
			operands.Add(context.ReadUInt16());
			var length = context.ReadByte();

			length += extraLength;

			var pos = context.Reader.BaseStream.Position;
			operands.Add(context.DecompileBlock(length));
			context.Reader.BaseStream.Seek(pos + length, SeekOrigin.Begin);
		}

		internal static void Decompile(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			InternalDecompile(context, ref operands, 1);
		}
	}
}
