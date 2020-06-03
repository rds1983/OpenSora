using OpenSora.Scenarios.Expressions;
using System.Collections.Generic;

namespace OpenSora.Scenarios.Instructions
{
	public class Switch : BaseInstruction
	{
		public class Option
		{
			public int Id { get; }
			public int Offset { get; }

			public Option(int id, int offset)
			{
				Id = id;
				Offset = offset;
			}
		}

		public Expression[] Expression
		{
			get
			{
				return (Expression[])Operands[0];
			}
		}

		public Option[] Options
		{
			get
			{
				return (Option[])Operands[1];
			}
		}

		public int DefaultOffset
		{
			get
			{
				return (int)Operands[2];
			}
		}

		internal static void Decompile(DecompilerContext context, ref List<object> operands, ref List<int> branchTargets)
		{
			operands.Add(context.DecompileExpression());

			var optionsCount = (int)context.Reader.ReadUInt16();

			var options = new List<Option>();
			for (var i = 0; i < optionsCount; ++i)
			{
				var option = new Option(context.Reader.ReadUInt16(), context.Reader.ReadUInt16());
				options.Add(option);
				branchTargets.Add(option.Offset);
			}

			operands.Add(options.ToArray());

			var offset = context.Reader.ReadUInt16();
			operands.Add(offset);

			branchTargets.Add(offset);
		}
	}
}