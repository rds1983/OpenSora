using OpenSora.Scenarios.Instructions;
using System.Collections.Generic;
using System.Linq;

namespace OpenSora.Scenarios
{
	partial class Decompiler
	{
		private static readonly DecompilerTableEntry[] DecompilerArrayFC = new DecompilerTableEntry[]
		{
			CreateEntry<ExitThread>(),
			CreateEntry<Return>(string.Empty, InstructionFlags.INSTRUCTION_END_BLOCK),
			CreateEntry<Jc>(string.Empty, InstructionFlags.INSTRUCTION_START_BLOCK, customDecompiler: Jc.Decompile),
			CreateEntry<Jump>("O", InstructionFlags.INSTRUCTION_JUMP),
			CreateEntry<Switch>(string.Empty, InstructionFlags.INSTRUCTION_END_BLOCK, customDecompiler: Switch.Decompile),
			CreateEntry<Call>("CH"),
			CreateEntry<NewScene>("LCCC"),
			CreateEntry<IdleLoop>(),
			CreateEntry<Sleep>("I"),
			CreateEntry<SetMapFlags>("L"),
			CreateEntry<ClearMapFlags>("L"),
			CreateEntry<FadeToDark>("iic"),
			CreateEntry<FadeToBright>("ii"),
			CreateCustomEntry("OP_0D"),
			CreateEntry<Fade>("I"),
			CreateEntry<Battle>("LLBWB"),
			CreateCustomEntry("OP_10","BB"),
			CreateCustomEntry("OP_11","BBBLLL"),
			CreateEntry<StopSound>("LLL"),
			CreateEntry<SetPlaceName>("W"),
			CreateEntry<BlurSwitch>(),
			CreateCustomEntry("OP_15"),
			CreateCustomEntry("OP_16", customDecompiler: DecompileOp16),
			CreateEntry<ShowSaveMenu>(),
			CreateCustomEntry("OP_18"),
			CreateEntry<EventBegin>("B"),
			CreateEntry<EventEnd>("B"),
			CreateCustomEntry("OP_1B","BBW"),
			CreateCustomEntry("OP_1C","BBW"),
			CreateCustomEntry("OP_1D","B"),
			CreateCustomEntry("OP_1E"),
			CreateCustomEntry("OP_1F","BL"),
			CreateCustomEntry("OP_20","L"),
			CreateCustomEntry("OP_21"),
			CreateCustomEntry("OP_22","WBB"),
			CreateCustomEntry("OP_23","W"),
			CreateCustomEntry("OP_24","WB"),
			CreateEntry<SoundDistance>("WLLLLLBL"),
			CreateEntry<SoundLoad>("H"),
			CreateEntry<Yield>(),
			CreateCustomEntry("OP_28", customDecompiler: DecompileOp28),
			CreateCustomEntry("OP_29", customDecompiler: DecompileOp29),
			CreateCustomEntry("OP_2A", customDecompiler: DecompileOp2a),
			CreateCustomEntry("OP_2B","WW"),
			CreateCustomEntry("OP_2C","WW"),
			CreateEntry<AddParty>("BB"),
			CreateEntry<RemoveParty>("BB"),
			CreateEntry<ClearParty>(),
			CreateCustomEntry("OP_30","B"),
			CreateCustomEntry("OP_31","BBW"),
			CreateCustomEntry("OP_32","BW"),
			CreateCustomEntry("OP_33","BW"),
			CreateCustomEntry("OP_34","BW"),
			CreateCustomEntry("OP_35","BW"),
			CreateCustomEntry("OP_36","BW"),
			CreateCustomEntry("OP_37","BW"),
			CreateEntry<AddSepith>("BW"),
			CreateEntry<SubSepith>("BW"),
			CreateEntry<AddMira>("H"),
			CreateEntry<SubMira>("H"),
			CreateCustomEntry("OP_3C","H"),
			CreateCustomEntry("OP_3D","H"),
			CreateCustomEntry("OP_3E","Wh"),
			CreateCustomEntry("OP_3F","Wh"),
			CreateCustomEntry("OP_40","W"),
			CreateCustomEntry("OP_41", customDecompiler: DecompileOp41),
			CreateCustomEntry("OP_42","B"),
			CreateCustomEntry("OP_43","WBBW"),
			CreateCustomEntry("OP_44","WB"),
			CreateEntry<QueueWorkItem>(customDecompiler:QueueWorkItem.Decompile),
			CreateEntry<QueueWorkItem2>(customDecompiler:QueueWorkItem2.Decompile2),
			CreateEntry<WaitChrThread>("WW"),
			CreateCustomEntry("OP_48"),
			CreateEntry<Event>("CH"),
			CreateCustomEntry("OP_4A","WC"),
			CreateCustomEntry("OP_4B","WC"),
			CreateCustomEntry("OP_4C"),
			CreateEntry<RunExpression>(customDecompiler:RunExpression.Decompile),
			CreateCustomEntry("OP_4E"),
			CreateCustomEntry("OP_4F", customDecompiler: DecompileOp4f),
			CreateCustomEntry("OP_50"),
			CreateCustomEntry("OP_51", customDecompiler: DecompileOp51),
			CreateEntry<TalkBegin>("W"),
			CreateEntry<TalkEnd>("W"),
			CreateEntry<AnonymousTalk>("S"),
			CreateCustomEntry("OP_55"),
			CreateCustomEntry("OP_56","B"),
			CreateCustomEntry("OP_57"),
			CreateEntry<CloseMessageWindow>(),
			CreateCustomEntry("OP_59"),
			CreateEntry<SetMessageWindowPos>("hhhh"),
			CreateEntry<ChrTalk>("WS"),
			CreateEntry<NpcTalk>("WSS"),
			CreateEntry<Menu>("hhhcS"),
			CreateEntry<MenuEnd>("W"),
			CreateCustomEntry("OP_5F","W"),
			CreateEntry<SetChrName>("S"),
			CreateCustomEntry("OP_61","W"),
			CreateCustomEntry("OP_62","WLIBBLB"),
			CreateCustomEntry("OP_63","W"),
			CreateCustomEntry("OP_64","BW"),
			CreateCustomEntry("OP_65","BW"),
			CreateCustomEntry("OP_66","W"),
			CreateCustomEntry("OP_67","iiii"),
			CreateCustomEntry("OP_68","W"),
			CreateCustomEntry("OP_69","WL"),
			CreateCustomEntry("OP_6A","W"),
			CreateCustomEntry("OP_6B","ii"),
			CreateCustomEntry("OP_6C","ii"),
			CreateEntry<MoveCamera>("iiii"),
			CreateCustomEntry("OP_6E","ii"),
			CreateCustomEntry("OP_6F","Wi"),
			CreateCustomEntry("OP_70","WL"),
			CreateCustomEntry("OP_71","WW"),
			CreateCustomEntry("OP_72","WW"),
			CreateCustomEntry("OP_73","W"),
			CreateCustomEntry("OP_74","WLW"),
			CreateCustomEntry("OP_75","BLB"),
			CreateCustomEntry("OP_76","WLWLLLBB"),
			CreateCustomEntry("OP_77","BBBLB"),
			CreateCustomEntry("OP_78","BBB"),
			CreateCustomEntry("OP_79","BW"),
			CreateCustomEntry("OP_7A","BW"),
			CreateCustomEntry("OP_7B"),
			CreateCustomEntry("OP_7C","LLLL"),
			CreateCustomEntry("OP_7D","B"),
			CreateCustomEntry("OP_7E","WWWBL"),
			CreateEntry<LoadEffect>("BS"),
			CreateEntry<PlayEffect>("BBWiiihhhiiiwiiii"),
			CreateEntry<Play3DEffect>("BBBSLLLWWWLLLL"),
			CreateCustomEntry("OP_82","BB"),
			CreateCustomEntry("OP_83","BB"),
			CreateCustomEntry("OP_84","B"),
			CreateCustomEntry("OP_85","W"),
			CreateEntry<SetChrChipByIndex>("WH"),
			CreateEntry<SetChrSubChip>("WH"),
			CreateEntry<SetChrPos>("Wiiih"),
			CreateCustomEntry("OP_89","Wiiih"),
			CreateEntry<TurnDirection>("WWH"),
			CreateCustomEntry("OP_8B","WLLW"),
			CreateEntry<SetChrDir>("Whh"),
			CreateCustomEntry("OP_8D","Wiiiii"),
			CreateEntry<MoveTo>("WLLLLB"),
			CreateCustomEntry("OP_8F","WLLLLB"),
			CreateCustomEntry("OP_90","WLLLLB"),
			CreateCustomEntry("OP_91","WLLLLB"),
			CreateCustomEntry("OP_92","WWLLB"),
			CreateCustomEntry("OP_93","WWLLB"),
			CreateCustomEntry("OP_94","BWWLLB"),
			CreateCustomEntry("OP_95","WLLLLL"),
			CreateCustomEntry("OP_96","WLLLLL"),
			CreateCustomEntry("OP_97","WLLLLW"),
			CreateCustomEntry("OP_98","WLLLL"),
			CreateCustomEntry("OP_99","WBBL"),
			CreateEntry<SetChrFlags>("WW"),
			CreateEntry<ClearChrFlags>("WW"),
			CreateEntry<SetChrBattleFlags>("WW"),
			CreateEntry<ClearChrBattleFlags>("WW"),
			CreateCustomEntry("OP_9E","WLLLL"),
			CreateCustomEntry("OP_9F","WBBBBL"),
			CreateCustomEntry("OP_A0","WBBBL"),
			CreateCustomEntry("OP_A1","WW"),
			CreateCustomEntry("OP_A2","W"),
			CreateCustomEntry("OP_A3","W"),
			CreateCustomEntry("OP_A4","W"),
			CreateCustomEntry("OP_A5","W"),
			CreateCustomEntry("OP_A6","W"),
			CreateCustomEntry("OP_A7","WW"),
			CreateCustomEntry("OP_A8","BBBBB"),
			CreateCustomEntry("OP_A9","B"),
			CreateCustomEntry("OP_AA"),
			CreateCustomEntry("OP_AB"),
			CreateCustomEntry("OP_AC","W"),
			CreateCustomEntry("OP_AD","LWWL"),
			CreateCustomEntry("OP_AE","L"),
			CreateCustomEntry("OP_AF","BW"),
			CreateCustomEntry("OP_B0","WB"),
			CreateCustomEntry("OP_B1","S"),
			CreateCustomEntry("OP_B2","BBW"),
			CreateEntry<PlayMovie>("BS"),
			CreateCustomEntry("OP_B4","B"),
			CreateCustomEntry("OP_B5","WB"),
			CreateCustomEntry("OP_B6","B"),
			CreateCustomEntry("OP_B7","WB"),
			CreateCustomEntry("OP_B8","B"),
			CreateCustomEntry("OP_B9","WW"),
			CreateCustomEntry("OP_BA","BW"),
			CreateCustomEntry("OP_BB","BB"),
			CreateEntry<SaveClearData>(id: 222),
		};

		private readonly static Dictionary<int, DecompilerTableEntry> DecompilerTableFC = new Dictionary<int, DecompilerTableEntry>();

		static Decompiler()
		{
			var id = 0;
			foreach (var entry in DecompilerArrayFC)
			{
				if (entry.Id != null)
				{
					id = entry.Id.Value;
				}

				DecompilerTableFC[id] = entry;

				++id;
			}
		}

		private static void DecompileOp16(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			var b = context.ReadByte();

			operands.Add(b);
			if (b == 2)
			{
				operands.AddRange(context.DecompileOperands("LLLL"));
			}
		}

		private static void DecompileOp28(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			var oprs = context.DecompileOperands("WB");

			operands.AddRange(oprs);

			var op = (int)oprs[1];
			if (op == 1 || op == 2)
			{
				operands.Add(context.DecompileOperand('W'));
			}
			else if (op == 3 || op == 4)
			{
				operands.Add(context.DecompileOperand('B'));
			}
		}

		private static void DecompileOp29(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			var oprs = context.DecompileOperands("WB");

			operands.AddRange(oprs);

			var op = (int)oprs[1];
			if (op == 1)
			{
				operands.Add(context.DecompileOperand('W'));
			}
			else
			{
				operands.Add(context.DecompileOperand('B'));
			}
		}
		private static void DecompileOp2a(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			for (var i = 0; i < 0xc; ++i)
			{
				var opr = context.ReadUInt16();
				operands.Add(opr);
				if (opr == 0xffff)
				{
					break;
				}
			}
		}

		private static readonly int[] Op41Values =
		{
			0x258, 0x259, 0x25A, 0x25B, 0x25C, 0x25D, 0x25E, 0x25F, 0x260, 0x261, 0x262, 0x263, 0x264, 0x265, 0x266, 0x267, 0x268, 0x269, 0x26A, 0x26B, 0x26C, 0x26D, 0x26E, 0x26F, 0x270, 0x271,
			0x272, 0x273, 0x274, 0x275, 0x276, 0x27D, 0x27E, 0x27F, 0x280, 0x281, 0x282, 0x283, 0x284, 0x285, 0x286, 0x287, 0x28A, 0x28B, 0x28E, 0x28F, 0x291, 0x2C1, 0x2C2, 0x2C3, 0x2C6, 0x2C7,
			0x2C8, 0x2C9, 0x2CA, 0x2D0, 0x2D1, 0x2D2, 0x2D3, 0x2D4, 0x315, 0x316, 0x317, 0x318, 0x319, 0x31A, 0x31B, 0x31C, 0x31D, 0x31E, 0x31F
		};

		private static void DecompileOp41(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			var oprs = context.DecompileOperands("BW");

			operands.AddRange(oprs);

			var op = (int)oprs[1];

			if (Op41Values.Contains(op))
			{
				operands.Add(context.DecompileOperand('B'));
			}
		}

		private static void DecompileOp4f(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			operands.Add(context.ReadByte());
			operands.Add(context.DecompileExpression());
		}

		private static void DecompileOp51(DecompilerContext context, DecompilerTableEntry entry, ref List<object> operands, ref List<int> branchTargets)
		{
			var oprs = context.DecompileOperands("WB");
			operands.AddRange(oprs);
			var exp = context.DecompileExpression();
			operands.Add(exp);
		}
	}
}