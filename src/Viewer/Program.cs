using System;
using System.Windows.Forms;

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
				MessageBox.Show(ex.ToString());
				Console.WriteLine(ex.ToString());
			}
		}
	}
}