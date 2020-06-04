using OpenSora;
using OpenSora.Dir;
using System;
using System.IO;

namespace Unpacker
{
	class Program
	{
		static void Main(string[] args)
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

						var unpack = !entry.Name.EndsWith("_DT");
						if (unpack)
						{
							Console.WriteLine("Unpacking '{0}'", entry.Name);
						}
						else
						{
							Console.WriteLine("Saving '{0}'", entry.Name);
						}

						try
						{
							byte[] bytes = new byte[entry.CompressedSize];
							using (var stream = File.OpenRead(pair.Key))
							{
								stream.Seek(entry.Offset, SeekOrigin.Begin);
								stream.Read(bytes, 0, bytes.Length);
							}
							if (unpack)
							{
								bytes = FalcomDecompressor.Decompress(bytes);
							}

							var outputPath = Path.Combine(args[1], entry.Name);
							File.WriteAllBytes(outputPath, bytes);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
