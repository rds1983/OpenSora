using OpenSora.Scenarios.Expressions;
using OpenSora.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace OpenSora.Scenarios
{
	public class DecompilerTableEntry
	{
		public Type InstructionType { get; }
		public string Name { get; }
		public string Operand { get; }
		public InstructionFlags Flags { get; }

		public DecompilerTableEntry(Type instructionType, string name, string operand, InstructionFlags flags)
		{
			if (instructionType == null)
			{
				throw new ArgumentNullException(nameof(instructionType));
			}

			InstructionType = instructionType;
			Name = name;
			Operand = operand;
			Flags = flags;
		}
	}

	public class DecompilerContext
	{
		private readonly HashSet<int> _disasmTable = new HashSet<int>();
		private readonly DecompilerTableEntry[] _entriesTable;
		private readonly HashSet<int> _globalLabelTable = new HashSet<int>();

		public BinaryReader Reader { get; }
		public DecompilerTableEntry Entry { get; private set; }

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
			var result = new List<BaseInstruction>();
			var blockRef = new Dictionary<int, BaseInstruction>();

			BaseInstruction lastInstruction = null;
			while (!Reader.IsEOF())
			{
				if (_disasmTable.Contains((int)Reader.BaseStream.Position))
				{
					break;
				}

				var op = Reader.ReadByte();

				Entry = _entriesTable[op];

				lastInstruction = (BaseInstruction)Activator.CreateInstance(Entry.InstructionType);

				int[] branchTargets;
				lastInstruction.Decompile(this, out branchTargets);
				_disasmTable.Add(lastInstruction.Offset);

				result.Add(lastInstruction);

				if (!Entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_END_BLOCK) && !Entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_START_BLOCK))
				{
					continue;
				}

				if (branchTargets != null)
				{
					foreach (var target in branchTargets)
					{
						blockRef[target] = lastInstruction;
					}
				}

				if (Entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_END_BLOCK))
				{
					break;
				}
			}

			var sortedKeys = (from k in blockRef.Keys orderby k select k).ToArray();

			foreach(var offset in sortedKeys)
			{
				var inst = blockRef[offset];

				Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				var newBlock = DecompileBlock();
				if (newBlock.Length == 0)
				{
					continue;
				}

				_globalLabelTable.Add(newBlock[0].Offset);

				if (offset >= lastInstruction.Offset || offset < result[0].Offset)
				{
					result.AddRange(newBlock);
				} else
				{
					for(var i = 0; i < result.Count; ++i)
					{
						var ins = result[i];
						if (offset >= ins.Offset)
						{
							continue;
						}

						result.InsertRange(i, newBlock);
						break;
					}
				}
			}


			return result.ToArray();
		}

		public ScpString[] ReadString()
		{
			var result = new List<ScpString>();
			var sb = new StringBuilder();

			while (!Reader.IsEOF())
			{
				var b = Reader.ReadByte();

				if (b < (byte)' ')
				{
					if (sb.Length != 0)
					{
						result.Add(new ScpString(sb.ToString()));
					}

					if (b == 0)
					{
						break;
					}

					ScpString str;

					switch (b)
					{
						case (int)ScpStringType.SCPSTR_CODE_COLOR:
							str = new ScpString(ScpStringType.SCPSTR_CODE_COLOR, Reader.ReadByte());
							break;
						case (int)ScpStringType.SCPSTR_CODE_ITEM:
							str = new ScpString(ScpStringType.SCPSTR_CODE_ITEM, Reader.ReadUInt16());
							break;
						default:
							str = new ScpString((ScpStringType)b);
							break;
					}

					result.Add(str);
				}
				else
				{
					sb.Append(b);
				}
			}

			return result.ToArray();
		}

		public object DecompileOperand(char operand)
		{
			object op;
			switch (operand)
			{

				case 'c':
				case 'b':
					op = (int)Reader.ReadSByte();
					break;
				case 'C':
				case 'B':
					op = (int)Reader.ReadByte();
					break;
				case 'h':
				case 'w':
					op = (int)Reader.ReadInt16();
					break;

				case 'H':
				case 'W':
				case 'o':
					op = (int)Reader.ReadUInt16();
					break;

				case 'i':
				case 'l':
					op = Reader.ReadInt32();
					break;

				case 'I':
				case 'L':
					op = (long)Reader.ReadUInt32();
					break;

				case 'S':
					op = ReadString();
					break;
				case 'M':
					op = (int)Reader.ReadInt16();
					break;
				case 'T':
				case 'O':
					op = (int)Reader.ReadUInt16();
					break;
				default:
					throw new Exception(string.Format("Unknown operand '{0}'", operand));
			}

			return op;
		}

		public Expression[] DecompileExpression()
		{
			return Expression.Decompile(Reader);
		}
	}
}
