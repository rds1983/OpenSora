using OpenSora.Scenarios.Instructions;

namespace OpenSora.Scenarios
{
	partial class Decompiler
	{
		private static readonly DecompilerTableEntry[] DecompilerTableFC = new DecompilerTableEntry[]
		{
			CreateEntry<ExitThread>(),
			CreateEntry<Return>(),
			CreateEntry<Jc>(),
		};
	}
}
