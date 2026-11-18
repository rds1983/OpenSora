# OpenSora

[![CI](https://github.com/rds1983/OpenSora/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/rds1983/OpenSora/actions/workflows/ci.yml)

OpenSora is an open-source engine for running the **Trails in the Sky** trilogy
(*FC*, *SC*, and *the 3rd*) from the original game data, written in C# on MonoGame. Cross-platform
support for Windows, Linux, and macOS is a 1.0 goal.

> **Project goal:** point OpenSora at an unmodified installation of any of the three games and play it
> from the title screen to the credits - field exploration, NPCs and dialogue, cutscenes, quests,
> the tactical battle system, orbments, shops, minigames, audio, movies, menus, and saves.
>
> OpenSora ships **no game data**. You must own the games. Do not commit extracted assets, saves,
> or other copyrighted content to this repository.

See [TASKS.md](TASKS.md) for the living roadmap and [AGENTS.md](AGENTS.md) for the development
workflow used by contributors and AI agents.

## Project status

OpenSora is in an early stage. The current codebase is primarily a **resource viewer** with partial
scenario preview support. It is **not yet a playable engine**. The table below is the honest
current-state snapshot; it is updated whenever a task lands.

| Area | Today | Goal |
|---|---|---|
| Engine loop | Fixed 60 Hz engine host; viewer owns input and UI | Replayable deterministic simulation across platforms |
| Determinism | Seeded RNG and recordable/replayable input traces | Full save/replay coverage for all game systems |
| Diagnostics | Structured logs, local crash reports, unknown-data dedup, F12 overlay | Log retention policy and developer analysis tooling |
| Format docs | Working notes for archives, compression, DDS, CH/CP, X2/X3, and FC SCP | Complete FC/SC/3rd references for every asset and save format |
| Game detection | FC and SC folders recognized | FC, SC, and the 3rd, all supported PC editions |
| Archives (`.dir`/`.dat`) | Read-only browsing | Full virtual filesystem with loose-file overrides |
| Textures | DDS and CH images | Complete format coverage, mipmaps, alpha |
| Models | X2/X3 static models | Full hierarchy, materials, skinning, animation |
| Character sprites | CH/CP animation preview | Field and battle parity |
| Scenario scripts | FC decompilation and timeline preview | Complete deterministic VM for FC/SC/3rd |
| Field gameplay | None | Walk, collide, interact, map transitions |
| Dialogue | Simple message preview | Full SCP text, portraits, choices, voice |
| Battle | None | Complete AT-based battle system |
| Menus / RPG systems | None | Orbments, equipment, items, journal, options |
| Audio | None | BGM, SE, voice, 3D audio, mixing |
| Movies | None | Intro and event movie playback with skip |
| Save/load | Viewer window state only | Full game saves, slots, FC -> SC import |
| Platforms | Windows (MonoGame DesktopGL) | Windows, Linux, macOS |
| Tests | xUnit suite with synthetic fixtures for archives, DDS, CH/CP, X2/X3, and FC scenarios | Unit, integration, golden-trace, and visual regression suites |
| CI | GitHub Actions: Windows + Linux build, test, and artifact publishing | Hosted PR checks and release automation |

### What works today

* Browsing `.dir`/`.dat` archives from a game folder.
* Viewing `_DS` textures and `_CH` character images/animations.
* Loading and rendering `_X2`/`_X3` static models with textures.
* Loading FC `_SN` scenario files and previewing functions on a timeline.
* Camera controls, resource filtering, and a Myra-based viewer UI.
* Command-line tools for unpacking archives and disassembling FC scenarios.
* An xUnit test suite covering `.dir`/`.dat` parsing, uncompressed DDS decoding, FC scenario
  header/function decompilation, deterministic engine timing, and delta-based camera movement.
* A fixed-timestep engine host (60 Hz by default) drives scenario playback, sprite animation, and
  camera movement. Simulation no longer reads wall-clock time.
* Seeded SplitMix64 randomness, logical input snapshots, frame-time sources, and recordable/replayable
  input traces for deterministic simulation tests.
* A synthetic fixture toolkit for DIR/DAT archives, uncompressed DDS textures, CH/CP sprite data,
  X2/X3 models, and FC scenarios. Fixtures are generated from code, never copied from game data.
* Structured diagnostic logs, local crash reports with recent log context, unknown-data deduplication,
  and an F12 diagnostics overlay in the viewer.
* Working format notes for DIR/DAT, Falcom compression, DDS, CH/CP, X2/X3, FC scenarios, and SCP
  expressions.
* Architecture decision records and an automated project-dependency/architecture test.

### Known limitations

* SC scenario scripts are not decompiled or executed; the viewer hides scenario playback for SC.
* The 3rd is not detected or supported.
* Most scenario instructions are decompiler-only stubs with no runtime behavior.
* The execution worker is a duration-based preview timeline, not a game VM: no call stack, threads,
  game flags, inventory, or save state.
* There is no player movement, collision, interaction, encounter, battle, menu, audio, movie, or
  save/load system.
* Test coverage is limited to archive parsing, DDS decoding, FC scenario parsing/decompilation, engine
  timing, deterministic RNG, input traces, and minimal CH/CP/model fixtures. There are no integration,
  golden-trace, or visual regression tests yet.
* SC and 3rd scenario fixtures are pending their parsers (M2-03/M2-04).
* The engine host still lives in `OpenSora.Common`, and the viewer owns input and UI. Input
  abstraction and the engine/viewer project split are later M0 tasks.
* The viewer still polls MonoGame devices directly; logical input snapshots are not yet wired into
  the viewer or scenario VM.
* Diagnostics are local only. There is no telemetry, automatic upload, or log retention policy yet.
* Save, audio, movie, and effect format pages are open research notes; those loaders are not
  implemented. SC/3rd scenario and sidecar formats remain largely undocumented.
* The engine runtime still lives in `OpenSora.Common`; the dedicated engine assembly and UI-boundary
  cleanup are tracked as M2-14.

## Running the current viewer

1. Build from source (see below) or download a binary release from
   https://github.com/rds1983/OpenSora/releases.
2. Run `OpenSora.Viewer`.
3. Click `Change...` and choose the folder containing the game (for example
   `D:\Games\Steam\steamapps\common\Trails in the Sky SC`).
4. Pick a resource type (`Textures`, `Models`, `Images`, `Animations`, or `Scenes`).
5. Enjoy the preview.

### Camera controls

* `W`, `A`, `S`, `D` - move the camera.
* Hold the right mouse button and move the mouse - rotate the camera.
* `I` / `O` - increase / decrease the view angle.
* `~` - toggle render debug info.
* `F12` - toggle the diagnostics log overlay.

## Building from source

Prerequisites:

* .NET 8 SDK
* Windows, Linux, or macOS (the viewer currently targets MonoGame DesktopGL)
* A local installation of the games for manual testing (not included)

```bash
git clone https://github.com/rds1983/OpenSora.git
cd OpenSora
dotnet build OpenSora.MonoGame.sln
dotnet test OpenSora.MonoGame.sln
dotnet run --project OpenSora.Viewer/OpenSora.Viewer.MonoGame.csproj
```

The solution builds with 0 errors as of 2026-11-18 (warnings are tracked as cleanup work).

### Continuous integration

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs on pushes to `master`, pull requests, and
manual dispatch. The matrix covers Windows x64 and Linux x64 and performs:

1. NuGet restore with package caching.
2. Release build.
3. Release test run with downloadable TRX results.
4. Publishing of the viewer, `Unpacker`, and `Decompiler` for the target runtime.
5. Upload of per-platform artifacts.

### Testing

Tests live in `OpenSora.Tests` and run with:

```bash
dotnet test OpenSora.MonoGame.sln
```

The suite must not depend on game installations. Use small synthetic fixtures generated by the test
code; never commit extracted game data. See [docs/testing/fixtures.md](docs/testing/fixtures.md) for
the fixture policy and builders. The initial suite covers:

* `.dir`/`.dat` archive parsing, including the zero-size entry rule.
* Uncompressed RGBA DDS decoding and channel order.
* CH animation sheets and CP animation info, including empty 0xFFFF cells.
* Default-size CH image decoding and BGRA4444 channel mapping.
* X2 and X3 model structure, transforms, vertices, materials, and indices.
* FC scenario header/function-table parsing and instruction decompilation.
* Deterministic fixed-timestep host updates and long-frame clamping.
* Delta-based camera movement.
* Seeded RNG sequence/state restoration and range checks.
* Input snapshot recording, replay, and trace independence.
* A 10,000-update simulation with the same seed and input trace producing identical state.
* Structured log sinks, crash report contents, unknown-data deduplication, and forced load failures.

Add a test for every behavioral change. A change that cannot be covered automatically needs a
recorded manual verification step in the task report.

## Diagnostics

Diagnostics are local: a per-session log, crash reports with recent log context, and an F12 overlay in
the viewer. See [docs/diagnostics.md](docs/diagnostics.md) for file locations, categories, and
unknown-data deduplication.

Logs and crash reports are never uploaded.

### Command-line tools

```bash
# Extract files from a game folder's .dir/.dat archives
dotnet run --project OpenSora.Unpacker/OpenSora.Unpacker.csproj -- <path/to/game> <output/path>

# Disassemble FC scenario files
dotnet run --project OpenSora.Decompiler/OpenSora.Decompiler.csproj -- <path/to/game> <output/path>
```

## Repository layout

| Project | Purpose |
|---|---|
| `OpenSora.Common` | Format readers, loaders, rendering, engine host, deterministic RNG/input, scenario decompiler/VM |
| `OpenSora.Viewer` | Myra-based resource viewer and scenario preview, acting as the engine host application |
| `OpenSora.Unpacker` | CLI tool that extracts archive entries |
| `OpenSora.Decompiler` | CLI tool that disassembles scenario scripts |
| `OpenSora.Tests` | xUnit tests, synthetic fixtures, deterministic simulation checks, and future game systems |
| `docs/adr` | Architecture decision records |
| `docs/testing` | Test and fixture policy |
| `docs/diagnostics.md` | Logging, crash reports, and debug-overlay guide |
| `docs/formats` | Binary format notes and open questions |

The engine host currently lives in `OpenSora.Common`. The roadmap separates the simulation and
runtime rendering core into a dedicated engine project while keeping the viewer as a developer tool.

## Architecture decisions

Design decisions are recorded in [docs/adr](docs/adr). The index links all accepted ADRs, including
the engine/viewer boundary, MonoGame boundaries, scenario VM, asset access, save format, modding, and
project dependency rules. `ArchitectureTests` enforces the current project graph and simulation
boundaries in CI.

## Format documentation

Binary format notes live in [docs/formats](docs/formats). The index lists implemented, partial, and
unimplemented formats, the observed archive entry catalog, references, and open questions. Statements
are marked verified, inferred, or unknown; loader source files link to their format page.

## Roadmap

The goal is reached when a clean installation of each game can be completed start-to-credits with
all gameplay systems, audio, and presentation intact, on Windows, Linux, and macOS, with automated
regression coverage. The concrete milestones and task board live in [TASKS.md](TASKS.md).

## Contributing

Read [AGENTS.md](AGENTS.md) for the workflow. In short: take the next task from `TASKS.md`, implement
it, test it thoroughly, update the status section of this README, ask the developer for confirmation,
and only then update `TASKS.md` and commit/push.

* Keep changes consistent with the existing structure, naming, and style (tabs, .NET 8 C#).
* Never commit game assets, extracted files, saves, or copyrighted data.
* Add tests for every behavioral change. "It builds" is not a test.
* Do not rewrite the roadmap silently - `TASKS.md` is a living document and changes must be visible.

## Credits

### Games

* [Sora No Kiseki FC](https://store.steampowered.com/app/251150/The_Legend_of_Heroes_Trails_in_the_Sky/)
* [Sora No Kiseki SC](https://store.steampowered.com/app/251290/The_Legend_of_Heroes_Trails_in_the_Sky_SC/)
* [Sora No Kiseki 3rd](https://store.steampowered.com/app/436670/The_Legend_of_Heroes_Trails_in_the_Sky_the_3rd/)

### People

* [uyjulian](https://github.com/uyjulian)

### Libraries

* [JuusanKoubou](https://github.com/Ouroboros/JuusanKoubou)
* [Kiseki-Texture-Tool](https://github.com/Sewer56/Kiseki-Texture-Tool)
* [EDDecompiler](https://github.com/ZhenjianYang/EDDecompiler)
* [MonoGame](http://www.monogame.net/)
* [Myra](https://github.com/rds1983/Myra)

## License

GPL-3.0. See [LICENSE](LICENSE). The games, their names, and their assets belong to their
respective rights holders.
