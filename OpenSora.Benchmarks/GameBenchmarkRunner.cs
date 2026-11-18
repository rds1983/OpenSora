using Imaging.DDSReader;
using OpenSora.Dir;
using OpenSora.Engine;
using OpenSora.ModelLoading;
using OpenSora.Scenarios;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenSora.Benchmarks
{
	internal sealed class GameBenchmarkRunner
	{
		private const long EngineUpdateSteps = 100000;
		private const int FrameAdvanceCount = 10000;
		private const int DeterministicStateSteps = 100000;

		private readonly BenchmarkOptions _options;

		public GameBenchmarkRunner(BenchmarkOptions options)
		{
			_options = options;
		}

		public BenchmarkReport Run()
		{
			var gamePath = Path.GetFullPath(_options.GamePath);
			var gameType = ResourceLoader.DetermineGameType(gamePath);
			if (gameType == null)
			{
				throw new InvalidOperationException("Unsupported or unrecognized game folder: " + gamePath);
			}

			var entriesMap = DirProcessor.BuildEntries(gamePath);
			var entries = entriesMap.SelectMany(pair => pair.Value).ToList();
			var scenarioEntries = entries
				.Where(entry => entry.Name.TrimEnd().EndsWith("_SN", StringComparison.Ordinal))
				.OrderBy(entry => entry.Name.TrimEnd(), StringComparer.Ordinal)
				.ToList();

			var modelEntry = SelectEntry(entries, new[] { "_X2", "_X3" }, _options.ModelEntry);
			var textureEntry = SelectEntry(entries, new[] { "_DS" }, _options.TextureEntry);
			var scenarioEntry = SelectEntry(entries, new[] { "_SN" }, _options.ScenarioEntry);
			var modelVersion = modelEntry.Name.TrimEnd().EndsWith("_X3", StringComparison.Ordinal) ? 3 : 2;

			var report = new BenchmarkReport
			{
				Label = _options.Label,
				GeneratedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
				Machine = new MachineInfo
				{
					Os = RuntimeInformation.OSDescription,
					Runtime = RuntimeInformation.FrameworkDescription,
					Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
					ProcessorCount = Environment.ProcessorCount
				},
				Game = new GameInfo
				{
					Type = gameType.Value.ToString(),
					EntryCount = entries.Count,
					ScenarioEntries = scenarioEntries.Count,
					ModelEntry = modelEntry.Name.TrimEnd(),
					TextureEntry = textureEntry.Name.TrimEnd(),
					ScenarioEntry = scenarioEntry.Name.TrimEnd()
				}
			};

			report.Benchmarks.Add(RunBenchmark(
				"archive-index",
				() => DirProcessor.BuildEntries(gamePath),
				_options.Iterations,
				_options.Warmup,
				1,
				"Full .dir/.dat index build."));

			report.Benchmarks.Add(RunBenchmark(
				"map-model-load",
				() =>
				{
					var data = Decompress(ReadEntry(modelEntry));
					ModelLoader.Load(new MemoryStream(data), modelVersion);
				},
				_options.Iterations,
				_options.Warmup,
				1,
				"Map proxy: read, decompress, and parse one X2/X3 model. Full map composition is not implemented yet."));

			report.Benchmarks.Add(RunBenchmark(
				"texture-decode",
				() =>
				{
					var data = ReadTextureData(textureEntry);
					DDS.LoadImage(new MemoryStream(data));
				},
				_options.Iterations,
				_options.Warmup,
				1,
				"Read and decode one _DS DDS texture (CPU only; no GPU upload)."));

			report.Benchmarks.Add(RunBenchmark(
				"scenario-parse",
				() =>
				{
					ParseScenario(scenarioEntry);
				},
				_options.Iterations,
				_options.Warmup,
				1,
				"Read, decompress, parse, and decompile one _SN scenario."));

			var corpusParsed = 0;
			var corpusFailed = 0;
			var corpus = RunBenchmark(
				"scenario-corpus-parse",
				() =>
				{
					corpusParsed = 0;
					corpusFailed = 0;
					foreach (var entry in scenarioEntries)
					{
						try
						{
							ParseScenario(entry);
							++corpusParsed;
						}
						catch
						{
							++corpusFailed;
						}
					}
				},
				1,
				0,
				scenarioEntries.Count,
				"All non-empty _SN entries: read, decompress, parse, and decompile.");
			corpus.Notes = "Parsed " + corpusParsed + ", failed " + corpusFailed + ".";
			report.Benchmarks.Add(corpus);

			var engineState = 0.0;
			var engineHost = new EngineHost();
			engineHost.Update += time => engineState += time.DeltaSeconds;
			report.Benchmarks.Add(RunBenchmark(
				"engine-update-100k-steps",
				() => engineHost.RunFixedSteps((int)EngineUpdateSteps),
				_options.Iterations,
				_options.Warmup,
				EngineUpdateSteps,
				"Headless fixed-step host update; no rendering or scenario VM."));

			var frameHost = new EngineHost();
			var frameSource = new FrameTimeSource(1.0 / 60.0);
			var frameState = 0.0;
			frameHost.Update += time => frameState += time.DeltaSeconds;
			report.Benchmarks.Add(RunBenchmark(
				"engine-frame-advance-10k",
				() =>
				{
					for (var i = 0; i < FrameAdvanceCount; ++i)
					{
						frameHost.Advance(frameSource);
					}
				},
				_options.Iterations,
				_options.Warmup,
				FrameAdvanceCount,
				"10,000 CPU frame advances with a fixed 60 Hz source; no rendering."));

			var trace = BuildInputTrace(DeterministicStateSteps);
			report.Benchmarks.Add(RunBenchmark(
				"deterministic-state-100k",
				() =>
				{
					var replay = new ReplayInputSource(trace);
					var random = new DeterministicRandom(0x123456789ABCDEFUL);
					var state = 0.0;
					for (var i = 0; i < DeterministicStateSteps; ++i)
					{
						var input = replay.Sample();
						state += input.MoveX;
						if (input.IsDown(InputButton.Confirm))
						{
							state += random.NextInt(1, 10);
						}
					}
				},
				_options.Iterations,
				_options.Warmup,
				DeterministicStateSteps,
				"Input trace + deterministic RNG state update."));

			// Keep these values alive so the measured work is not optimized away.
			if (engineState < 0 || frameState < 0)
			{
				throw new InvalidOperationException("Unexpected benchmark state.");
			}

			report.Skipped.Add(new SkippedBenchmark
			{
				Name = "vm-update",
				Reason = "Headless scenario VM is not implemented yet; tracked as M2-15."
			});
			report.Skipped.Add(new SkippedBenchmark
			{
				Name = "full-map-load",
				Reason = "Composed map/tilemap/asset loading is not implemented; map-model-load is the current proxy (M1-07)."
			});
			report.Skipped.Add(new SkippedBenchmark
			{
				Name = "render-frame",
				Reason = "Requires a graphics device/window; CPU frame advance is measured instead."
			});

			return report;
		}

		private static DirEntry SelectEntry(List<DirEntry> entries, string[] suffixes, string requested)
		{
			var candidates = entries
				.Where(entry => suffixes.Any(suffix =>
					entry.Name.TrimEnd().EndsWith(suffix, StringComparison.Ordinal)))
				.OrderBy(entry => entry.Name.TrimEnd(), StringComparer.Ordinal)
				.ToList();

			if (candidates.Count == 0)
			{
				throw new InvalidOperationException(
					"No archive entry with suffix " + string.Join(" or ", suffixes) + " was found.");
			}

			if (string.IsNullOrEmpty(requested))
			{
				return candidates[0];
			}

			var match = candidates.FirstOrDefault(entry =>
				string.Equals(entry.Name.TrimEnd(), requested.Trim(), StringComparison.OrdinalIgnoreCase));
			if (match == null)
			{
				throw new InvalidOperationException(
					"Entry '" + requested + "' with suffix " + string.Join(" or ", suffixes) + " was not found.");
			}

			return match;
		}

		private static byte[] ReadEntry(DirEntry entry)
		{
			using (var stream = File.OpenRead(entry.DatFilePath))
			using (var reader = new BinaryReader(stream))
			{
				stream.Seek(entry.Offset, SeekOrigin.Begin);
				var size = entry.CompressedSize > 0 ? entry.CompressedSize : entry.DecompressedSize;
				return reader.ReadBytes(size);
			}
		}

		private static byte[] Decompress(byte[] data)
		{
			var result = FalcomDecompressor.Decompress(data);
			return result.Length > 0 ? result : data;
		}

		private static byte[] ReadTextureData(DirEntry entry)
		{
			var data = ReadEntry(entry);
			if (data.Length < 4 || data[0] != 'D' || data[1] != 'D' || data[2] != 'S' || data[3] != ' ')
			{
				data = Decompress(data);
			}

			return data;
		}

		private static void ParseScenario(DirEntry entry)
		{
			var data = Decompress(ReadEntry(entry));
			using (var stream = new MemoryStream(data))
			{
				Scenario.FromFCStream(stream);
			}
		}

		private static InputTrace BuildInputTrace(int count)
		{
			var trace = new InputTrace();
			for (var i = 0; i < count; ++i)
			{
				var buttons = InputButton.None;
				if ((i & 1) == 0)
				{
					buttons |= InputButton.Confirm;
				}

				trace.Add(new InputSnapshot(
					buttons,
					((i % 11) - 5) / 5.0f,
					((i % 13) - 6) / 6.0f));
			}

			return trace;
		}

		private BenchmarkResult RunBenchmark(string name, Action action, int iterations, int warmup,
			long operationsPerIteration, string notes)
		{
			for (var i = 0; i < warmup; ++i)
			{
				action();
			}

			var times = new List<double>();
			var allocations = new List<long>();

			for (var i = 0; i < iterations; ++i)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				var allocationBefore = GC.GetAllocatedBytesForCurrentThread();
				var stopwatch = Stopwatch.StartNew();
				action();
				stopwatch.Stop();
				var allocationAfter = GC.GetAllocatedBytesForCurrentThread();

				times.Add(stopwatch.Elapsed.TotalMilliseconds);
				allocations.Add(Math.Max(0, allocationAfter - allocationBefore));
			}

			times.Sort();
			allocations.Sort();

			var median = Median(times);
			var medianAllocation = (long)Median(allocations.Select(value => (double)value).ToList());

			return new BenchmarkResult
			{
				Name = name,
				Iterations = iterations,
				OperationsPerIteration = operationsPerIteration,
				MedianTotalMs = median,
				MedianPerOperationMs = operationsPerIteration > 0 ? median / operationsPerIteration : median,
				MinTotalMs = times[0],
				MaxTotalMs = times[times.Count - 1],
				MedianAllocatedBytes = medianAllocation,
				MedianAllocatedBytesPerOperation = operationsPerIteration > 0
					? medianAllocation / operationsPerIteration
					: medianAllocation,
				Notes = notes
			};
		}

		private static double Median(List<double> values)
		{
			if (values.Count == 0)
			{
				return 0;
			}

			var middle = values.Count / 2;
			return values.Count % 2 == 0
				? (values[middle - 1] + values[middle]) / 2.0
				: values[middle];
		}
	}
}
