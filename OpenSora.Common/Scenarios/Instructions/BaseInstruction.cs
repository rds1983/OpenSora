using OpenSora.Scenarios.Instructions;
using System.Collections.Generic;
using System.Text;

namespace OpenSora.Scenarios
{
	public class BaseInstruction
	{
		public QueueWorkItem Queue;

		public DecompilerTableEntry Entry;
		public int Offset { get; set; }
		public object[] Operands { get; set; }

		public virtual int DurationInMs
		{
			get
			{
				return 0;
			}
		}

		public virtual string Name
		{
			get
			{
				return GetType().Name;
			}
		}

		internal static void DecompileDefault(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			if (string.IsNullOrEmpty(entry.Operand))
			{
				return;
			}

			for (var i = 0; i < entry.Operand.Length; ++i)
			{
				char c = entry.Operand[i];
				var op = context.DecompileOperand(c);
				if (char.ToLower(c) == 'o')
				{
					branchTargets.Add((int)op);
				}

				operands.Add(op);
			}
		}

		private static string ToString(ScpString[] strings)
		{
			var sb = new StringBuilder();

			sb.Append("(");
			for (var i = 0; i < strings.Length; ++i)
			{
				var str = strings[i];

				if (str.Type != ScpStringType.SCPSTR_CODE_STRING)
				{
					sb.Append("scpexpr(");
					sb.Append(str.Type);

					sb.Append(", ");
					sb.AppendFormat("{0:X}", str.Value);
					sb.Append(")");
				}
				else
				{
					sb.AppendFormat("\"{0}\"", str.String);
				}

				if (i < strings.Length - 1)
				{
					sb.Append(",\n\t\t");
				}
			}

			sb.Append(")");
			return sb.ToString();
		}

		private static string ToString(BaseInstruction[] block)
		{
			var sb = new StringBuilder();

			sb.Append("{\n");

			foreach(var instruction in block)
			{
				sb.Append("\t");
				sb.AppendLine(instruction.ToString());
			}

			sb.Append("\t}");
			return sb.ToString();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			var name = GetType().Name;
			var asCustom = this as Custom;
			if (asCustom != null)
			{
				name = asCustom.Name;
			}

			sb.AppendFormat("\t{0}(", name);

			for (var i = 0; i < Operands.Length; ++i)
			{
				var op = Operands[i];
				var str = string.Empty;
				switch (op)
				{
					case ScpString[] scp:
						str = ToString(scp);
						break;
					case BaseInstruction[] block:
						str = ToString(block);
						break;
					default:
						str = op.ToString();
						break;

				}

				sb.Append(str);
				if (i < Operands.Length - 1)
				{
					sb.Append(", ");
				}
			}

			sb.Append(");");


			return sb.ToString();
		}

		public virtual void Begin(ExecutionWorker worker)
		{
			//Debug.WriteLine(Name + ".Begin");
		}

		public virtual void Update(ExecutionWorker worker)
		{
		}

		public virtual void End(ExecutionWorker worker)
		{
			//Debug.WriteLine(Name + ".End");
		}
	}
}
