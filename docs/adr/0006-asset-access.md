# ADR 0006 - Asset access and virtual filesystem

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Implementation:** M1-01, M1-02, M1-12

## Context

Current loaders open `.dir`/`.dat` files directly through `ResourceLoader`/`DirProcessor`. The viewer
resolves resources by scanning lists and file names. There is no single place for game-edition
detection, loose-file overrides, caching, case-insensitive lookup, or mod support.

Simulation and tools must not depend on absolute Windows paths or on the viewer's current folder.

## Decision

1. **All game data access goes through a virtual filesystem.** `IGameFileSystem` (M1-01) resolves
   logical paths to archive entries or loose files. No simulation code calls `File.OpenRead` on game
   paths directly.
2. **The asset catalog owns identity and metadata.** Game/edition detection, logical names, expected
   assets, and manifests live in the catalog (M1-02), not in scenario or viewer code.
3. **Loaders accept streams or asset handles.** Format readers do not own file discovery. This keeps
   them testable with synthetic fixtures.
4. **Override priority is explicit.** Loose files override archive entries so modding and debugging do
   not require repacking. The base installation is read-only.
5. **Caching is an implementation detail behind the filesystem.** Decoded textures/models/audio may be
   cached on disk or in memory, keyed by source hash; callers still see the same logical API.
6. **Case and path normalization are handled centrally.** Lookups are case-insensitive where the
   original data requires it, independent of host OS.
7. **Diagnostics report missing/unknown assets once per logical source** via `UnknownDataReporter` and
   structured logs.

## Consequences

- The viewer's `ResourceLoader` becomes an asset service or adapter, not a global file scanner.
- Synthetic fixtures can target the same loader APIs as real archives.
- Modding gets a clean insertion point (loose-file overrides).
- Asset validation can walk the catalog rather than duplicate path logic.
- Existing direct-file code is transitional and will be replaced in M1.

## Alternatives considered

- **Keep direct file access and add helpers.** Rejected: path logic spreads across loaders and tools.
- **Require extraction to a loose folder.** Rejected: duplicates data, breaks large installs, and
  complicates mods.
- **Use a third-party VFS package.** Not selected for now; the format is small and project-specific.

## Open questions

- Cache format and invalidation rules.
- Whether archives can be memory-mapped or must be streamed.
- How to expose asset IDs to mods without leaking internal indices.
- Edition-specific manifest differences (Steam/GOG/JP).
