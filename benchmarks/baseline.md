# Baseline benchmark report

- **Report:** [baseline.json](baseline.json)
- **Generated:** 2026-11-18
- **Label:** Reference Windows workstation (12 logical CPUs)
- **OS:** Microsoft Windows 10.0.26200
- **Runtime:** .NET 8.0.31 (X64)
- **Logical CPUs:** 12
- **Game:** Steam FC
- **Entries:** 12,821
- **Non-empty scenarios:** 491
- **Selected entries:** `ATARI00 ._X2`, `AMARI   ._DS`, `C0100   ._SN`

Command:

```bash
dotnet run --project OpenSora.Benchmarks -c Release -- \
  --game "D:\SteamLibrary\steamapps\common\Trails in the Sky FC" \
  --iterations 5 --warmup 1 \
  --label "Reference Windows workstation (12 logical CPUs)" \
  --output benchmarks/baseline.json
```

| Benchmark | Median total | Per operation | Allocated |
|---|---:|---:|---:|
| `archive-index` | 8.81 ms | 8.81 ms | 5.6 MB |
| `map-model-load` | 0.257 ms | 0.257 ms | 82 KB |
| `texture-decode` | 0.742 ms | 0.742 ms | 770 KB |
| `scenario-parse` | 1.87 ms | 1.87 ms | 1.34 MB |
| `scenario-corpus-parse` | 500.23 ms | 1.019 ms / scenario | 248 MB |
| `engine-update-100k-steps` | 0.357 ms | 0.000004 ms / step | 0 |
| `engine-frame-advance-10k` | 0.113 ms | 0.000011 ms / frame | 0 |
| `deterministic-state-100k` | 1.062 ms | 0.000011 ms / step | ~0.1 KB |

## Caveats

- `map-model-load` is a **proxy** for map loading: the engine does not yet compose tilemaps,
  walkmeshes, textures, and placed objects. That benchmark will change when M1-07 lands.
- `scenario-corpus-parse` parses and decompiles all 491 non-empty FC scenarios; all 491 succeeded in
  this run.
- `vm-update` and `render-frame` are skipped. The headless VM is M2-15; render timing needs a graphics
  device and a dedicated presentation benchmark.
- Single-run numbers can be dominated by JIT/startup; the harness uses warmup plus measured iterations
  and reports medians.
- These values are hardware- and runtime-specific. Compare on the same machine and runtime.

## Updating the baseline

Regenerate only when:

- a benchmark workload changes;
- the reference machine/runtime changes;
- a deliberate performance change makes the old baseline misleading.

Record the reason in the task log and update both `baseline.json` and this summary.
