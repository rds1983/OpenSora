using OpenSora.Diagnostics;
using System;

namespace OpenSora.Viewer
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var diagnostics = DiagnosticsBootstrap.Start("OpenSora.Viewer"))
			{
				try
				{
					using (var game = new ViewerGame())
					{
						game.Run();
					}
				}
				catch (Exception ex)
				{
					Log.Critical(LogCategory.Diagnostics, "Viewer terminated with an unhandled exception.", ex);
					Console.WriteLine(ex.ToString());

					try
					{
						var path = CrashReporter.WriteReport(ex);
						Console.WriteLine("Crash report: " + path);
					}
					catch (Exception reportException)
					{
						Console.Error.WriteLine("Failed to write crash report: " + reportException.Message);
					}
				}
			}
		}
	}
}