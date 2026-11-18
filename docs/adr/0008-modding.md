# ADR 0008 - Modding and loose-file overrides

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Implementation:** M1-01, M8-04

## Context

Players and contributors will want to replace textures, scripts, music, UI data, and eventually add
content. The engine also needs a safe way to test modified assets without repacking archives or
mutating a game installation. Supporting arbitrary code execution would expand the security and legal
surface dramatically.

## Decision

1. **Loose-file overrides are the first modding mechanism.** The virtual filesystem checks a mod/override
   directory before archives. Overrides are keyed by logical asset path, not by internal archive index.
2. **The base installation is read-only.** OpenSora never writes into the game folder. Saves, logs,
   caches, and mods live in OpenSora-owned directories.
3. **Mods are data-first.** Asset replacement and data-table overrides are supported before script or
   code extensibility.
4. **No arbitrary code execution in 1.0.** A plugin API is deferred. Any future plugin system requires
   explicit loading rules, versioning, sandboxing expectations, and a security review.
5. **Mods are discoverable and validated.** A manifest identifies name/version/author, game profile,
   and overridden files. A validation command reports missing, duplicate, or unsupported assets.
6. **Diagnostics identify override source.** Logs and unknown-data reports include whether an asset
   came from base archives or a mod.
7. **Legal policy applies.** Mods and test fixtures must not be committed with copyrighted game data.

## Consequences

- Testing and local asset iteration require no repacking.
- Mod conflicts are resolved by a documented priority order.
- Script-level modding waits for a stable VM and asset catalog.
- A future plugin API can be designed without compromising deterministic core behavior.

## Alternatives considered

- **Repack-only modding.** Rejected: slow, error-prone, and destructive to the installation.
- **Immediate plugin/code loading.** Rejected: security, determinism, and compatibility costs.
- **Per-file patching of archives.** Rejected: complex and risks corrupting base data.

## Open questions

- Mod load order and conflict resolution.
- Manifest schema and validation UX.
- Whether script replacement is allowed before a plugin API.
- Save compatibility when mods change item/quest data.
- Sandboxing and trust model for any future code plugins.
