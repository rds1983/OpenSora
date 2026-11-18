using System;

namespace OpenSora.Viewer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				using (var game = new ViewerGame())
				{
					game.Run();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}