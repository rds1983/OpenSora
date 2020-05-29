using OpenSora.Scenarios.Instructions;
using System;
using System.IO;

namespace OpenSora.Scenarios
{
	[Flags]
	public enum InstructionFlags
	{
		INSTRUCTION_END_BLOCK = 1 << 0,
		INSTRUCTION_START_BLOCK = 1 << 1,
		INSTRUCTION_CALL = (1 << 2) | INSTRUCTION_START_BLOCK,
		INSTRUCTION_JUMP = (1 << 3) | INSTRUCTION_END_BLOCK,
		INSTRUCTION_SWITCH = 0,
		INSTRUCTION_NONE = 0,
	}

	public partial class Decompiler
	{
		class DecompilerTableEntry
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


		private readonly DecompilerContext _context;

		public BinaryReader Reader
		{
			get
			{
				return _context.Reader;
			}
		}

		public Decompiler(BinaryReader reader)
		{
			_context = new DecompilerContext(reader);
		}

		public void DecompileBlock()
		{
			var op = Reader.ReadByte();

			var entry = DecompilerTableFC[op];
		}

		private static DecompilerTableEntry CreateEntry<T>(string operand = "", InstructionFlags flags = InstructionFlags.INSTRUCTION_SWITCH) where T : BaseInstruction
		{
			return new DecompilerTableEntry(typeof(T), operand, flags);
		}

		private static DecompilerTableEntry CreateCustomEntry(string name, string operand = "", InstructionFlags flags = InstructionFlags.INSTRUCTION_SWITCH)
		{
			return new DecompilerTableEntry(name, operand, flags);
		}
	}
}
