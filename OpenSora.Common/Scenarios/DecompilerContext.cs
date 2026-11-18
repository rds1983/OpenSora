using OpenSora.Scenarios.Expressions;
using OpenSora.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using OpenSora.Scenarios.Instructions;

namespace OpenSora.Scenarios
{
	public class DecompilerContext
	{
		private readonly HashSet<int> _disasmTable = new HashSet<int>();
		private readonly Dictionary<int, DecompilerTableEntry> _entriesTable;
		private readonly HashSet<int> _globalLabelTable = new HashSet<int>();

		public BinaryReader Reader { get; }

		public DecompilerContext(BinaryReader reader, Dictionary<int, DecompilerTableEntry> entriesTable)
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

		public BaseInstruction DecompileInstruction(out int[] branchTargets)
		{
			int offset = (int)Reader.BaseStream.Position;
			var op = Reader.ReadByte();

			var entry = _entriesTable[op];
			var instruction = (BaseInstruction)Activator.CreateInstance(entry.InstructionType);
			instruction.Offset = offset;
			instruction.Entry = entry;

			branchTargets = null;

			var asCustom = instruction as Custom;
			if (asCustom != null)
			{
				asCustom.InstructionName = entry.Name;
			}

			var decompiler = entry.CustomDecompiler ?? BaseInstruction.DecompileDefault;

			var targetsList = new List<int>();
			var operandsList = new List<object>();
			decompiler.Invoke(this, entry, ref operandsList, ref targetsList);

			branchTargets = targetsList.ToArray();
			instruction.Operands = operandsList.ToArray();

			return instruction;
		}

		public BaseInstruction[] DecompileBlock(int? length = null)
		{
			var result = new List<BaseInstruction>();
			var blockRef = new Dictionary<int, BaseInstruction>();

			var start = (int)Reader.BaseStream.Position;

			while (!Reader.IsEOF())
			{
				if (_disasmTable.Contains((int)Reader.BaseStream.Position))
				{
					break;
				}

				if (length != null && Reader.BaseStream.Position - start >= length.Value)
				{
					break;
				}

				int[] branchTargets = null;
				var instruction = DecompileInstruction(out branchTargets);

				_disasmTable.Add(instruction.Offset);

				result.Add(instruction);

				var entry = instruction.Entry;
				if (!entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_END_BLOCK) && !entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_START_BLOCK))
				{
					continue;
				}

				if (branchTargets != null)
				{
					foreach (var target in branchTargets)
					{
						blockRef[target] = instruction;
					}
				}

				if (entry.Flags.HasFlag(InstructionFlags.INSTRUCTION_END_BLOCK))
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

				if (offset >= result[result.Count - 1].Offset || offset < result[0].Offset)
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
						sb.Clear();
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
					sb.Append((char)b);
				}
			}

			return result.ToArray();
		}

		public int ReadSByte()
		{
			return Reader.ReadSByte();
		}

		public int ReadByte()
		{
			return Reader.ReadByte();
		}

		public int ReadInt16()
		{
			return Reader.ReadInt16();
		}

		public int ReadUInt16()
		{
			return Reader.ReadUInt16();
		}

		public int ReadInt32()
		{
			return Reader.ReadInt32();
		}

		public long ReadUInt32()
		{
			return Reader.ReadUInt32();
		}

		public object DecompileOperand(char operand)
		{
			object op;
			switch (operand)
			{

				case 'c':
				case 'b':
					op = ReadSByte();
					break;
				case 'C':
				case 'B':
					op = ReadByte();
					break;
				case 'h':
				case 'w':
					op = ReadInt16();
					break;

				case 'H':
				case 'W':
				case 'o':
					op = ReadUInt16();
					break;

				case 'i':
				case 'l':
					op = ReadInt32();
					break;

				case 'I':
				case 'L':
					op = ReadUInt32();
					break;

				case 'S':
					op = ReadString();
					break;
				case 'M':
					op = ReadInt16();
					break;
				case 'T':
				case 'O':
					op = ReadUInt16();
					break;
				default:
					throw new Exception(string.Format("Unknown operand '{0}'", operand));
			}

			return op;
		}

		public object[] DecompileOperands(string operands)
		{
			var result = new List<object>();

			if (!string.IsNullOrEmpty(operands))
			{
				for(var i = 0; i < operands.Length; ++i)
				{
					result.Add(DecompileOperand(operands[i]));
				}
			}

			return result.ToArray();
		}

		public Expression[] DecompileExpression()
		{
			return Expression.Decompile(this);
		}
	}
}
