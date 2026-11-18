# OpenSora

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
| Tests | xUnit suite for archives, DDS, scenarios, timing, RNG, and input | Unit, integration, golden-trace, and visual regression suites |
| CI | AppVeyor build | Windows and Linux build + test on every change |

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

### Known limitations

* SC scenario scripts are not decompiled or executed; the viewer hides scenario playback for SC.
* The 3rd is not detected or supported.
* Most scenario instructions are decompiler-only stubs with no runtime behavior.
* The execution worker is a duration-based preview timeline, not a game VM: no call stack, threads,
  game flags, inventory, or save state.
* There is no player movement, collision, interaction, encounter, battle, menu, audio, movie, or
  save/load system.
* Test coverage is limited to archive parsing, DDS decoding, FC scenario parsing/decompilation, engine
  timing, deterministic RNG, and input traces. There are no integration, golden-trace, or visual
  regression tests yet.
* The engine host still lives in `OpenSora.Common`, and the viewer owns input and UI. Input
  abstraction and the engine/viewer project split are later M0 tasks.
* The viewer still polls MonoGame devices directly; logical input snapshots are not yet wired into
  the viewer or scenario VM.

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

### Testing

Tests live in `OpenSora.Tests` and run with:

```bash
dotnet test OpenSora.MonoGame.sln
```

The suite must not depend on game installations. Use small synthetic fixtures generated by the test
code; never commit extracted game data. The initial suite covers:

* `.dir`/`.dat` archive parsing, including the zero-size entry rule.
* Uncompressed RGBA DDS decoding and channel order.
* FC scenario header/function-table parsing and instruction decompilation.
* Deterministic fixed-timestep host updates and long-frame clamping.
* Delta-based camera movement.
* Seeded RNG sequence/state restoration and range checks.
* Input snapshot recording, replay, and trace independence.
* A 10,000-update simulation with the same seed and input trace producing identical state.

Add a test for every behavioral change. A change that cannot be covered automatically needs a
recorded manual verification step in the task report.

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
| `OpenSora.Tests` | xUnit tests for loaders, decompilation, engine timing, and future game systems |
| `docs/adr` | Architecture decision records |

The engine host currently lives in `OpenSora.Common`. The roadmap separates the simulation and
runtime rendering core into a dedicated engine project while keeping the viewer as a developer tool.

## Architecture decisions

Design decisions are recorded in [docs/adr](docs/adr). See
[ADR 0001 - Engine host and fixed-timestep clock](docs/adr/0001-engine-host.md) for the deterministic
update model and
[ADR 0002 - Deterministic random numbers and input traces](docs/adr/0002-deterministic-input-and-random.md)
for reproducible randomness and input.

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
