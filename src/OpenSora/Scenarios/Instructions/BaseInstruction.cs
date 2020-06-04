using OpenSora.Scenarios.Instructions;
using System.Collections.Generic;
using System.Text;

namespace OpenSora.Scenarios
{
	public class BaseInstruction
	{
		public DecompilerTableEntry Entry;
		public int Offset { get; set; }
		public object[] Operands { get; set; }

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
			for(var i = 0; i < strings.Length; ++i)
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

			for(var i = 0; i < Operands.Length; ++i)
			{
				var op = Operands[i];
				var asString = op as ScpString[];
				string str;
				if (asString != null)
				{
					str = ToString(asString);
				}
				else
				{
					str = op.ToString();
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
	}
}
