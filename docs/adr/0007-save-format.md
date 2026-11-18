# ADR 0007 - Save format

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Implementation:** M2-10, M3-09, M6-04

## Context

The viewer only persists an XML window-state file. There is no game save implementation. The original
games use proprietary save/clear-data formats that have not been reverse engineered. A full engine
needs reliable, versioned saves that include deterministic state and can survive code changes.

## Decision

1. **OpenSora owns its save format.** The primary format is project-defined and versioned. It is not
   required to match the original game's format.
2. **The format is a versioned binary container.** It starts with a magic value and schema version,
   followed by section metadata. Integers are little-endian. A human-readable dump tool is provided for
   debugging.
3. **Sections are explicit and independently versioned where useful:**
   - game state (flags, variables, party, inventory, economy, quests, location, playtime);
   - VM state (worker stacks, instruction pointers, pending waits, expression stacks);
   - RNG state;
   - metadata (game profile, edition, chapter, timestamp, engine version);
   - integrity/checksum.
4. **Writes are atomic.** Write to a temporary file, flush, then replace the target. Never truncate a
   good save on failure.
5. **Saves are portable.** No absolute host paths, process ids, or machine-specific identifiers in
   simulation state. Paths are represented by logical asset IDs.
6. **Original saves are imported read-only when feasible.** A separate importer maps original data into
   OpenSora state. It must never claim to write the original format. FC clear-data import for SC is the
   first target.
7. **Version migrations are explicit.** Unknown versions fail safely with a diagnostic report. Adding a
   field requires a schema decision and test coverage.
8. **Save data is never committed to the repository.** Tests use synthetic state snapshots.

## Consequences

- Save/load tests can round-trip deterministic snapshots.
- Replays and save files share the same state/serialization contracts.
- Cross-game import is a mapping problem, not a hidden dependency on proprietary files.
- Save compatibility becomes a maintained API surface.
- Some original-game features (thumbnails, exact UI metadata) may need separate presentation data.

## Alternatives considered

- **Write the original save format.** Rejected for now: unknown format, checksum, and compatibility
  risk; read-only import is the safer path.
- **Use JSON only.** Rejected as the primary format: larger, slower, and less explicit for binary VM
  state; JSON remains useful for debug dumps.
- **Serialize object graphs automatically.** Rejected: unstable across refactors and hard to version.

## Open questions

- Exact section encoding and compression (if any).
- Integrity mechanism: checksum vs signature.
- Save-slot thumbnails and metadata ownership.
- Steam Cloud interaction and file naming.
- FC-to-SC clear-data mapping and whether SC-to-3rd carry-over exists.
