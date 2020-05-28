using System;
using System.IO;

namespace OpenSora.Scenarios.Instructions
{
	public class DecompilerContext
	{
		public BinaryReader Reader { get; }

		public DecompilerContext(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			Reader = reader;
		}
	}
}
