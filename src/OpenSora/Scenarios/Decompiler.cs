using OpenSora.Scenarios.Instructions;
using System;
using System.IO;

namespace OpenSora.Scenarios
{
	public partial class Decompiler
	{
		class DecompilerTableEntry
		{
			public Type InstructionType { get; private set; }

			public DecompilerTableEntry(Type instructionType)
			{
				if (instructionType == null)
				{
					throw new ArgumentNullException(nameof(instructionType));
				}

				InstructionType = instructionType;
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

		private static DecompilerTableEntry CreateEntry<T>() where T: BaseInstruction
		{
			return new DecompilerTableEntry(typeof(T));
		}
	}
}
