using OpenSora.Scenarios.Expressions;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.Scenarios
{
	public class DecompilerTableEntry
	{
		public Type InstructionType { get; private set; }
		public string Name { get; private set; }
		public string Operand { get; private set; }
		public InstructionFlags Flags { get; private set; }

		public DecompilerTableEntry(Type instructionType, string operand, InstructionFlags flags)
		{
			if (instructionType == null)
			{
				throw new ArgumentNullException(nameof(instructionType));
			}

			InstructionType = instructionType;
			Operand = operand;
			Flags = flags;
		}

		public DecompilerTableEntry(string name, string operand, InstructionFlags flags)
		{
			Name = name;
			Operand = operand;
			Flags = flags;
		}
	}

	public class DecompilerContext
	{
		private readonly List<int> _disasmTable = new List<int>();
		private readonly DecompilerTableEntry[] _entriesTable;
		public BinaryReader Reader { get; }

		public DecompilerContext(BinaryReader reader, DecompilerTableEntry[] entriesTable)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			if (entriesTable == null)
			{
				throw new ArgumentNullException(nameof(entriesTable));
			}

			Reader = reader;
			_entriesTable = entriesTable;
		}

		public BaseInstruction[] DecompileBlock()
		{
			_disasmTable.Clear();

			var result = new List<BaseInstruction>();
			while(true)
			{
				var op = Reader.ReadByte();

				var entry = _entriesTable[op];

				var instruction = (BaseInstruction)Activator.CreateInstance(entry.InstructionType);

				instruction.Decompile(this);
				result.Add(instruction);

				if (entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_END_BLOCK))
				{
					break;
				}
			}

			return result.ToArray();
		}

		public Expression[] DecompileExpression()
		{
			return Expression.Decompile(Reader);
		}
	}
}
