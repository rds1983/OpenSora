﻿using OpenSora.Scenarios.Instructions;

namespace OpenSora.Scenarios
{
	partial class Decompiler
	{
		private static readonly DecompilerTableEntry[] DecompilerTableFC = new DecompilerTableEntry[]
		{
			CreateEntry<ExitThread>(),
			CreateEntry<Return>(string.Empty,InstructionFlags.INSTRUCTION_END_BLOCK),
			CreateEntry<Jc>(string.Empty,InstructionFlags.INSTRUCTION_START_BLOCK),
			CreateEntry<Jump>("O",InstructionFlags.INSTRUCTION_JUMP),
			CreateEntry<Switch>(string.Empty,InstructionFlags.INSTRUCTION_END_BLOCK),
			CreateEntry<Call>("CH"),
			CreateEntry<NewScene>(string.Empty),
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
			CreateCustomEntry("OP_16"),
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
			CreateCustomEntry("OP_28"),
			CreateCustomEntry("OP_29"),
			CreateCustomEntry("OP_2A"),
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
			CreateCustomEntry("OP_41"),
			CreateCustomEntry("OP_42","B"),
			CreateCustomEntry("OP_43","WBBW"),
			CreateCustomEntry("OP_44","WB"),
			CreateEntry<QueueWorkItem>(string.Empty),
			CreateEntry<QueueWorkItem2>(string.Empty),
			CreateEntry<WaitChrThread>("WW"),
			CreateCustomEntry("OP_48"),
			CreateEntry<Event>("CH"),
			CreateCustomEntry("OP_4A","WC"),
			CreateCustomEntry("OP_4B","WC"),
			CreateCustomEntry("OP_4C"),
			CreateEntry<RunExpression>(string.Empty),
			CreateCustomEntry("OP_4E"),
			CreateCustomEntry("OP_4F"),
			CreateCustomEntry("OP_50"),
			CreateCustomEntry("OP_51"),
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
			CreateCustomEntry("OP_6D","iiii"),
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
			CreateCustomEntry("OP_8C","Whh"),
			CreateCustomEntry("OP_8D","Wiiiii"),
			CreateCustomEntry("OP_8E","WLLLLB"),
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
			CreateEntry<SaveClearData>(),
		};
	}
}