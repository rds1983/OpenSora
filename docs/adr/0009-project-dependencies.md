# ADR 0009 - Project dependency rules

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Enforcement:** `OpenSora.Tests/ArchitectureTests.cs`, CI

## Context

The solution contains a shared library, a viewer, two CLI tools, and a test project. Without explicit
rules, shared code tends to accumulate viewer/UI dependencies, tools import each other, and simulation
becomes untestable.

## Current project graph

```
OpenSora.Common.MonoGame      (no project references)
    ^
    ├── OpenSora.Viewer.MonoGame
    ├── OpenSora.Unpacker
    ├── OpenSora.Decompiler
    └── OpenSora.Tests
```

Allowed project references today:

| Project | May reference |
|---|---|
| `OpenSora.Common.MonoGame` | none |
| `OpenSora.Viewer.MonoGame` | `OpenSora.Common.MonoGame` |
| `OpenSora.Unpacker` | `OpenSora.Common.MonoGame` |
| `OpenSora.Decompiler` | `OpenSora.Common.MonoGame` |
| `OpenSora.Tests` | `OpenSora.Common.MonoGame` |

No production project may reference the viewer, tools, or tests. Test projects are leaves.

## Decision

1. **Dependencies point inward.** Shared/engine code must not reference host applications or tools.
2. **The viewer is a composition root.** It may reference engine/common libraries and UI packages; it
   must not be imported by them.
3. **Tools are leaves.** `Unpacker` and `Decompiler` may share common libraries but not each other.
4. **Tests are leaves.** Tests may reference production projects to verify them; production projects
   must never reference tests.
5. **No cycles.** Any proposed cycle is rejected; move shared contracts into a lower-level project.
6. **Future engine runtime:** when `OpenSora.Engine` is extracted (M2-14), the expected graph becomes:

   ```
   OpenSora.Common (formats/services)
       ^
       ├── OpenSora.Engine (simulation/runtime)
       │       ^
       │       └── OpenSora.Viewer
       ├── OpenSora.Unpacker
       ├── OpenSora.Decompiler
       └── OpenSora.Tests
   ```

   The viewer may reference both Common and Engine; Engine may reference Common but not Viewer/UI.
7. **Package policy:** add NuGet dependencies only with a concrete need and version pinning where
   shared. Prefer the standard library and existing packages. Large or platform-specific dependencies
   require an ADR update.
8. **Architecture tests are part of the build.** `dotnet test` parses project references and source
   boundaries; CI fails on violations.

## Consequences

- New projects must update this ADR and `ArchitectureTests` in the same change.
- Common cannot absorb viewer convenience code without a boundary review.
- Tools stay independently runnable and testable.
- The future engine split is guided rather than ad hoc.

## Alternatives considered

- **One monolithic project.** Rejected: prevents headless testing and clean host separation.
- **Rely on code review only.** Rejected: dependency drift is easy to miss; tests make it explicit.
- **Use solution folders as the only boundary.** Rejected: solution folders do not enforce references.

## Open questions

- Exact assembly name and ownership of the future `OpenSora.Engine` project.
- Whether rendering should be a separate platform project or remain in Engine.
- Whether tool-specific tests warrant separate test projects.
- How to enforce package/license policy automatically.
