using System;
using System.IO;
using System.Linq;

namespace OpenSora.Benchmarks
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					BenchmarkOptions.PrintUsage();
					return 1;
				}

				var options = BenchmarkOptions.Parse(args);
				if (options.ShowHelp)
				{
					BenchmarkOptions.PrintUsage();
					return 0;
				}

				var report = new GameBenchmarkRunner(options).Run();
				BenchmarkJson.Write(options.OutputPath, report);
				PrintReport(report);

				if (!string.IsNullOrEmpty(options.ComparePath))
				{
					Compare(report, options.ComparePath);
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return 1;
			}
		}

		private static void PrintReport(BenchmarkReport report)
		{
			Console.WriteLine();
			Console.WriteLine("OpenSora benchmark - {0}", report.Label);
			Console.WriteLine("Game: {0}; entries: {1}; scenarios: {2}",
				report.Game.Type, report.Game.EntryCount, report.Game.ScenarioEntries);
			Console.WriteLine("Machine: {0}; {1}; {2} CPUs",
				report.Machine.Os, report.Machine.Runtime, report.Machine.ProcessorCount);
			Console.WriteLine();
			Console.WriteLine("{0,-30} {1,12} {2,14} {3,14} {4,14}",
				"Benchmark", "Median ms", "Per op ms", "Alloc KB", "Alloc B/op");
			Console.WriteLine(new string('-', 92));

			foreach (var benchmark in report.Benchmarks)
			{
				Console.WriteLine("{0,-30} {1,12:0.###} {2,14:0.######} {3,14:0.0} {4,14:0}",
					benchmark.Name,
					benchmark.MedianTotalMs,
					benchmark.MedianPerOperationMs,
					benchmark.MedianAllocatedBytes / 1024.0,
					benchmark.MedianAllocatedBytesPerOperation);
			}

			if (report.Skipped.Count > 0)
			{
				Console.WriteLine();
				Console.WriteLine("Skipped:");
				foreach (var skipped in report.Skipped)
				{
					Console.WriteLine("  {0}: {1}", skipped.Name, skipped.Reason);
				}
			}

			Console.WriteLine();
		}

		private static void Compare(BenchmarkReport current, string baselinePath)
		{
			if (!File.Exists(baselinePath))
			{
				Console.Error.WriteLine("Baseline report not found: " + baselinePath);
				return;
			}

			var baseline = BenchmarkJson.Read(baselinePath);
			Console.WriteLine("Comparison against {0}:", baselinePath);
			Console.WriteLine("{0,-30} {1,14} {2,14} {3,10}",
				"Benchmark", "Baseline/op", "Current/op", "Delta");

			foreach (var benchmark in current.Benchmarks)
			{
				var old = baseline.Benchmarks.FirstOrDefault(candidate =>
					string.Equals(candidate.Name, benchmark.Name, StringComparison.Ordinal));
				if (old == null)
				{
					Console.WriteLine("{0,-30} {1,14} {2,14} {3,10}",
						benchmark.Name, "-", benchmark.MedianPerOperationMs, "new");
					continue;
				}

				var delta = old.MedianPerOperationMs == 0
					? 0
					: (benchmark.MedianPerOperationMs - old.MedianPerOperationMs) / old.MedianPerOperationMs * 100.0;

				Console.WriteLine("{0,-30} {1,14:0.######} {2,14:0.######} {3,9:+0.0;-0.0;0.0}%",
					benchmark.Name,
					old.MedianPerOperationMs,
					benchmark.MedianPerOperationMs,
					delta);
			}

			Console.WriteLine();
		}
	}
}
