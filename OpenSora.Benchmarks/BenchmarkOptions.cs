using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace OpenSora.Benchmarks
{
	internal sealed class BenchmarkOptions
	{
		public string GamePath { get; private set; }
		public string OutputPath { get; private set; } = "benchmarks/benchmark-result.json";
		public string Label { get; private set; } = "local";
		public string ComparePath { get; private set; }
		public int Iterations { get; private set; } = 5;
		public int Warmup { get; private set; } = 1;
		public string ModelEntry { get; private set; }
		public string TextureEntry { get; private set; }
		public string ScenarioEntry { get; private set; }
		public bool ShowHelp { get; private set; }

		public static BenchmarkOptions Parse(string[] args)
		{
			var result = new BenchmarkOptions();

			for (var i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				switch (arg)
				{
					case "--help":
					case "-h":
						result.ShowHelp = true;
						break;
					case "--game":
						result.GamePath = NextValue(args, ref i, arg);
						break;
					case "--output":
						result.OutputPath = NextValue(args, ref i, arg);
						break;
					case "--label":
						result.Label = NextValue(args, ref i, arg);
						break;
					case "--compare":
						result.ComparePath = NextValue(args, ref i, arg);
						break;
					case "--iterations":
						result.Iterations = ParseInt(NextValue(args, ref i, arg), arg);
						break;
					case "--warmup":
						result.Warmup = ParseInt(NextValue(args, ref i, arg), arg);
						break;
					case "--model":
						result.ModelEntry = NextValue(args, ref i, arg);
						break;
					case "--texture":
						result.TextureEntry = NextValue(args, ref i, arg);
						break;
					case "--scenario":
						result.ScenarioEntry = NextValue(args, ref i, arg);
						break;
					default:
						throw new ArgumentException("Unknown argument: " + arg);
				}
			}

			if (!result.ShowHelp && string.IsNullOrEmpty(result.GamePath))
			{
				throw new ArgumentException("--game <path> is required.");
			}

			if (result.Iterations <= 0 || result.Iterations > 100)
			{
				throw new ArgumentException("--iterations must be between 1 and 100.");
			}

			if (result.Warmup < 0 || result.Warmup > 20)
			{
				throw new ArgumentException("--warmup must be between 0 and 20.");
			}

			return result;
		}

		public static void PrintUsage()
		{
			Console.WriteLine("OpenSora benchmark harness");
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine("  dotnet run --project OpenSora.Benchmarks -- --game <path> [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("  --game <path>        Game installation folder (required)");
			Console.WriteLine("  --output <path>      JSON report path (default: benchmarks/benchmark-result.json)");
			Console.WriteLine("  --label <text>       Machine/report label (default: local)");
			Console.WriteLine("  --iterations <n>     Measured iterations (default: 5)");
			Console.WriteLine("  --warmup <n>         Warmup iterations (default: 1)");
			Console.WriteLine("  --model <entry>      Specific _X2/_X3 entry name");
			Console.WriteLine("  --texture <entry>    Specific _DS entry name");
			Console.WriteLine("  --scenario <entry>   Specific _SN entry name");
			Console.WriteLine("  --compare <path>     Compare current results with a prior JSON report");
			Console.WriteLine("  --help               Show this help");
		}

		private static string NextValue(string[] args, ref int index, string option)
		{
			++index;
			if (index >= args.Length)
			{
				throw new ArgumentException("Missing value for " + option);
			}

			return args[index];
		}

		private static int ParseInt(string value, string option)
		{
			int result;
			if (!int.TryParse(value, out result))
			{
				throw new ArgumentException("Invalid integer for " + option + ": " + value);
			}

			return result;
		}
	}

	internal sealed class BenchmarkReport
	{
		public int SchemaVersion { get; set; } = 1;
		public string Label { get; set; }
		public string GeneratedAtUtc { get; set; }
		public MachineInfo Machine { get; set; }
		public GameInfo Game { get; set; }
		public List<BenchmarkResult> Benchmarks { get; set; } = new List<BenchmarkResult>();
		public List<SkippedBenchmark> Skipped { get; set; } = new List<SkippedBenchmark>();
	}

	internal sealed class MachineInfo
	{
		public string Os { get; set; }
		public string Runtime { get; set; }
		public string Architecture { get; set; }
		public int ProcessorCount { get; set; }
	}

	internal sealed class GameInfo
	{
		public string Type { get; set; }
		public int EntryCount { get; set; }
		public int ScenarioEntries { get; set; }
		public string ModelEntry { get; set; }
		public string TextureEntry { get; set; }
		public string ScenarioEntry { get; set; }
	}

	internal sealed class BenchmarkResult
	{
		public string Name { get; set; }
		public int Iterations { get; set; }
		public long OperationsPerIteration { get; set; }
		public double MedianTotalMs { get; set; }
		public double MedianPerOperationMs { get; set; }
		public double MinTotalMs { get; set; }
		public double MaxTotalMs { get; set; }
		public long MedianAllocatedBytes { get; set; }
		public long MedianAllocatedBytesPerOperation { get; set; }
		public string Notes { get; set; }
	}

	internal sealed class SkippedBenchmark
	{
		public string Name { get; set; }
		public string Reason { get; set; }
	}

	internal static class BenchmarkJson
	{
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public static void Write(string path, BenchmarkReport report)
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(path));
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(path, JsonSerializer.Serialize(report, Options));
		}

		public static BenchmarkReport Read(string path)
		{
			return JsonSerializer.Deserialize<BenchmarkReport>(File.ReadAllText(path), Options);
		}
	}
}
