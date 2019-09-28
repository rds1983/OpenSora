using System;
using System.Windows.Forms;

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

			try
			{
				using (var game = new ViewerGame())
				{
					game.SoraFolder = args[0];
					game.Run();
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
				Console.WriteLine(ex.ToString());
			}
		}
	}
}