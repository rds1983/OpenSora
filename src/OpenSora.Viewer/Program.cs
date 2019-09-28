using System;

namespace OpenSora.Viewer
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: OpenSora.Viewer <folder_to_sora_fc>");
				return;
			}

			using (var game = new ViewerGame())
			{
				game.SoraFolder = args[0];
				game.Run();
			}
		}
	}
}