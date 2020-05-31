using OpenSora.Scenarios.Instructions;
using System;
using System.Collections.Generic;
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

	public delegate void CustomDecompilerDelegate(DecompilerContext context, ref List<object> operands, ref List<int> branchTargets);

	public class DecompilerTableEntry
	{
		public Type InstructionType { get; }
		public string Name { get; }
		public string Operand { get; }
		public InstructionFlags Flags { get; }

		public CustomDecompilerDelegate CustomDecompiler { get; }

		public DecompilerTableEntry(Type instructionType, string name, string operand, InstructionFlags flags, CustomDecompilerDelegate customDecompiler)
		{
			if (instructionType == null)
			{
				throw new ArgumentNullException(nameof(instructionType));
			}

			InstructionType = instructionType;
			Name = name;
			Operand = operand;
			Flags = flags;
			CustomDecompiler = customDecompiler;
		}
	}

	public partial class Decompiler
	{
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
			_context = new DecompilerContext(reader, DecompilerTableFC);
		}

		public BaseInstruction[] DecompileBlock()
		{
			return _context.DecompileBlock();
		}

		private static DecompilerTableEntry CreateEntry<T>(string operand = "", InstructionFlags flags = InstructionFlags.INSTRUCTION_SWITCH) where T : BaseInstruction
		{
			return new DecompilerTableEntry(typeof(T), string.Empty, operand, flags, null);
		}

		private static DecompilerTableEntry CreateCustomEntry(string name, string operand = "", InstructionFlags flags = InstructionFlags.INSTRUCTION_NONE, CustomDecompilerDelegate customDecompiler = null)
		{
			return new DecompilerTableEntry(typeof(Custom), name, operand, flags, customDecompiler);
		}
	}
}
