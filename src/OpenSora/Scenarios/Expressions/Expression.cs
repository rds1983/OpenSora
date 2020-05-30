using OpenSora.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.Scenarios.Expressions
{
	public class Expression
	{
		private static readonly ExpressionType[] _passTypes = new ExpressionType[]
		{
			ExpressionType.EXPR_EQU,
			ExpressionType.EXPR_NEQ,
			ExpressionType.EXPR_LSS,
			ExpressionType.EXPR_GTR,
			ExpressionType.EXPR_LEQ,
			ExpressionType.EXPR_GE,
			ExpressionType.EXPR_EQUZ,
			ExpressionType.EXPR_NEQUZ_I64,
			ExpressionType.EXPR_AND,
			ExpressionType.EXPR_OR,
			ExpressionType.EXPR_ADD,
			ExpressionType.EXPR_SUB,
			ExpressionType.EXPR_NEG,
			ExpressionType.EXPR_XOR,
			ExpressionType.EXPR_IMUL,
			ExpressionType.EXPR_IDIV,
			ExpressionType.EXPR_IMOD,
			ExpressionType.EXPR_STUB,
			ExpressionType.EXPR_IMUL_SAVE,
			ExpressionType.EXPR_IDIV_SAVE,
			ExpressionType.EXPR_IMOD_SAVE,
			ExpressionType.EXPR_ADD_SAVE,
			ExpressionType.EXPR_SUB_SAVE,
			ExpressionType.EXPR_AND_SAVE,
			ExpressionType.EXPR_XOR_SAVE,
			ExpressionType.EXPR_OR_SAVE,
			ExpressionType.EXPR_NOT
		};

		public ExpressionType Type { get; private set; }
		public object[] Operands { get; private set; }

		private Expression(ExpressionType type, object[] operands)
		{
			Type = type;
			Operands = operands;
		}

		public static Expression[] Decompile(BinaryReader reader)
		{
			var result = new List<Expression>();
			while (!reader.IsEOF())
			{
				var type = (ExpressionType)reader.ReadByte();
				var operands = new List<object>();

				switch (type)
				{
					case ExpressionType.EXPR_PUSH_LONG:
						operands.Add(reader.ReadInt32());
						break;
					case ExpressionType.EXPR_END:
						goto end;
					case ExpressionType.EXPR_EQU:
					case ExpressionType.EXPR_NEQ:
					case ExpressionType.EXPR_LSS:
					case ExpressionType.EXPR_GTR:
					case ExpressionType.EXPR_LEQ:
					case ExpressionType.EXPR_GE:
					case ExpressionType.EXPR_EQUZ:
					case ExpressionType.EXPR_NEQUZ_I64:
					case ExpressionType.EXPR_AND:
					case ExpressionType.EXPR_OR:
					case ExpressionType.EXPR_ADD:
					case ExpressionType.EXPR_SUB:
					case ExpressionType.EXPR_NEG:
					case ExpressionType.EXPR_XOR:
					case ExpressionType.EXPR_IMUL:
					case ExpressionType.EXPR_IDIV:
					case ExpressionType.EXPR_IMOD:
					case ExpressionType.EXPR_STUB:
					case ExpressionType.EXPR_IMUL_SAVE:
					case ExpressionType.EXPR_IDIV_SAVE:
					case ExpressionType.EXPR_IMOD_SAVE:
					case ExpressionType.EXPR_ADD_SAVE:
					case ExpressionType.EXPR_SUB_SAVE:
					case ExpressionType.EXPR_AND_SAVE:
					case ExpressionType.EXPR_XOR_SAVE:
					case ExpressionType.EXPR_OR_SAVE:
					case ExpressionType.EXPR_NOT:
						// pass
						break;
					case ExpressionType.EXPR_EXEC_OP:
						throw new NotImplementedException();
					case ExpressionType.EXPR_TEST_SCENA_FLAGS:
					case ExpressionType.EXPR_GET_RESULT:
						operands.Add(reader.ReadUInt16());
						break;
					case ExpressionType.EXPR_PUSH_VALUE_INDEX:
						operands.Add(reader.ReadByte());
						break;
					case ExpressionType.EXPR_GET_CHR_WORK:
						operands.Add(reader.ReadUInt16());
						operands.Add(reader.ReadByte());
						break;
					case ExpressionType.EXPR_RAND:
						// pass
						break;
					case ExpressionType.EXPR_23:
						operands.Add(reader.ReadByte());
						break;
				}

				var expr = new Expression(type, operands.ToArray());
				result.Add(expr);
			}
		end:;

			return result.ToArray();
		}
	}
}
