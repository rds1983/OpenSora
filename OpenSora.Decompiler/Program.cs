using OpenSora;
using OpenSora.Diagnostics;
using OpenSora.Dir;
using OpenSora.Scenarios;
using System;
using System.IO;

namespace Unpacker
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var diagnostics = DiagnosticsBootstrap.Start("OpenSora.Decompiler"))
			{
				if (args.Length < 2)
				{
					Console.WriteLine("Usage: Unpacker <path/to/sora> <output/path>");
					return;
				}

				try
				{
					var entries = DirProcessor.BuildEntries(args[0]);

					foreach (var pair in entries)
					{
						Console.WriteLine("Processing '{0}'", pair.Key);

						foreach (var entry in pair.Value)
						{
							if (entry.CompressedSize == 0 || entry.DecompressedSize == 0)
							{
								continue;
							}

							if (!entry.Name.EndsWith("_SN"))
							{
								continue;
							}

							Console.WriteLine("Processing script '{0}'", entry.Name);

							try
							{
								byte[] bytes = new byte[entry.CompressedSize];
								using (var stream = File.OpenRead(pair.Key))
								{
									stream.Seek(entry.Offset, SeekOrigin.Begin);
									stream.Read(bytes, 0, bytes.Length);
								}

								bytes = FalcomDecompressor.Decompress(bytes);

								var output = string.Empty;
								using (var stream = new MemoryStream(bytes))
								{
									var scenario = Scenario.FromFCStream(stream);
									output = scenario.ToString();
								}

								var fileName = Path.Combine(args[1], Path.ChangeExtension(entry.Name, "txt"));
								File.WriteAllText(fileName, output);
							}
							catch (Exception ex)
							{
								Log.Error(LogCategory.Scenario, "Failed to decompile '" + entry.Name + "'.", ex);
								Console.WriteLine(ex);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Critical(LogCategory.Archive, "Decompiler terminated with an error.", ex);
					Console.WriteLine(ex);
				}
			}
		}
	}
}
