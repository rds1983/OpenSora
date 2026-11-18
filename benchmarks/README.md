# OpenSora benchmarks

`OpenSora.Benchmarks` is a headless benchmark harness for loaders, scenario parsing, the engine host,
and deterministic state updates. It requires a local game installation and never reads or writes game
data into the repository.

## Usage

```bash
# Create a report
dotnet run --project OpenSora.Benchmarks -c Release -- \
  --game "/path/to/Trails in the Sky FC" \
  --label "my machine" \
  --iterations 5 \
  --warmup 1 \
  --output benchmarks/benchmark-result.json

# Compare a new run with a baseline
dotnet run --project OpenSora.Benchmarks -c Release -- \
  --game "/path/to/Trails in the Sky FC" \
  --iterations 5 \
  --warmup 1 \
  --output benchmarks/benchmark-result.json \
  --compare benchmarks/baseline.json
```

Options:

| Option | Meaning |
|---|---|
| `--game <path>` | Game installation folder; required |
| `--output <path>` | JSON report path; default `benchmarks/benchmark-result.json` |
| `--label <text>` | Human-readable machine/report label |
| `--iterations <n>` | Measured iterations, 1-100; default 5 |
| `--warmup <n>` | Warmup iterations, 0-20; default 1 |
| `--model <entry>` | Specific `_X2`/`_X3` entry name |
| `--texture <entry>` | Specific `_DS` entry name |
| `--scenario <entry>` | Specific `_SN` entry name |
| `--compare <path>` | Compare current results with a prior JSON report |

`benchmarks/baseline.json` is checked in as a reference. `benchmarks/benchmark-result.json` is ignored.

## Benchmarks

| Name | Workload |
|---|---|
| `archive-index` | Build the full `.dir`/`.dat` index |
| `map-model-load` | Read, decompress, and parse one X2 model (map proxy) |
| `texture-decode` | Read and decode one `_DS` DDS texture on the CPU |
| `scenario-parse` | Read, decompress, parse, and decompile one `_SN` scenario |
| `scenario-corpus-parse` | Same for all non-empty `_SN` entries |
| `engine-update-100k-steps` | Headless fixed-step `EngineHost` updates |
| `engine-frame-advance-10k` | CPU frame advances with a fixed 60 Hz source |
| `deterministic-state-100k` | Input trace + deterministic RNG state updates |

Skipped workloads are recorded in the JSON report with reasons:

- `vm-update` - the headless scenario VM is not implemented yet (M2-15).
- `full-map-load` - composed map/tilemap/asset loading is not implemented yet (M1-07).
- `render-frame` - requires a graphics device/window; only CPU frame advance is measured.

## Methodology

- Each benchmark runs `warmup` iterations, then `iterations` measured iterations.
- Before each measured iteration: full GC, finalizer wait, full GC.
- Time is measured with `Stopwatch`; allocations with
  `GC.GetAllocatedBytesForCurrentThread`.
- The report uses the median total time, median per-operation time, min/max total time, and median
  allocated bytes.
- `scenario-corpus-parse` runs once because it parses the whole corpus; its per-operation value is
  total time divided by the number of scenarios.
- Results are CPU-only unless a future GPU benchmark is added.

Benchmarks are sensitive to hardware, runtime version, OS, archive cache state, and JIT warmup.
Compare runs on the same machine/runtime; use the baseline as a trend reference, not a hard CI gate.

## Baseline

See [baseline.md](baseline.md) for the recorded reference values and environment.

## Adding a benchmark

1. Add a deterministic workload with a fixed input selection.
2. Record the selected entry names/counts in the report, not absolute paths.
3. Keep game data out of the repository; use the local installation only.
4. Update `baseline.md` only when the benchmark definition or reference hardware changes.
5. For VM execution benchmarks, use M2-15 once the headless VM exists.
