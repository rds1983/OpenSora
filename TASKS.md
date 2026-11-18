# TASKS.md - OpenSora roadmap and task board

> **This document is alive.** Agents and contributors may split, merge, reorder, add, or remove
> tasks when reality disagrees with the plan. Keep changes visible in the changelog, keep task IDs
> stable when possible, and never silently drop unfinished work.

**Last updated:** 2026-11-18
**Current milestone:** M0 - Foundations, tests, and architecture
**Next task:** M0-06 - Add diagnostics, logging, and crash reporting
**Last completed:** M0-05 - Establish CI (2026-11-18)

## Purpose

The goal is a full-featured engine that runs **The Legend of Heroes: Trails in the Sky FC, SC, and
the 3rd** from unmodified game data: title screen to credits, including field exploration, dialogue
and cutscenes, the tactical battle system, orbments, shops, quests, minigames, audio, movies, menus,
saves, and options.

This file is the single source of truth for **what to do next**. The current-state summary lives in
`README.md`; the working agreement lives in `AGENTS.md`.

## How to use this board

1. Work on the first unchecked task in `Current milestone` whose dependencies are satisfied, unless
   the developer directs otherwise.
2. Mark the task `_in progress_` before starting and update `Last updated`.
3. Implement it, test it thoroughly, and update the status section of `README.md` with the new
   project state.
4. Ask the developer to confirm the task is finished.
5. Only after explicit confirmation: mark the task `[x]` with a completion date, append it to the
   completed log, update `Current focus`, and commit/push.

You may edit this document at any time to reflect new knowledge: add discovery tasks, split a task
that is too large, change acceptance criteria, or revise milestones. Record structural changes in the
changelog with a one-line rationale.

## Status legend

| Marker | Meaning |
|---|---|
| `- [ ]` + `_todo_` | Not started |
| `- [ ]` + `_in progress_` | Actively being worked on |
| `- [ ]` + `_blocked: reason_` | Cannot proceed; reason recorded |
| `- [x]` + `_done YYYY-MM-DD_` | Confirmed by the developer and recorded |
| `_deferred_` | Deliberately pushed out of the current milestone |

Task IDs are stable. New tasks get a new ID in the appropriate milestone, for example `M3-14`.
Discovery work that spans milestones goes to `Cross-cutting backlog` until it is promoted.

## Current focus

- **Milestone:** M0 - make the codebase safe to evolve before adding gameplay.
- **Next task:** M0-06 - add diagnostics, logging, and crash reporting.
- **Why:** every later milestone depends on automated verification and deterministic updates. The VM,
  asset loaders, and battle formulas cannot be validated by "the viewer still opens".
- **Suggested first vertical slice after M0:** FC prologue through the first field map, first
  dialogue, first chest, and first battle (`M3-12`).

## Milestones at a glance

| ID | Milestone | Exit criteria | Status |
|---|---|---|---|
| M0 | Foundations, tests, and architecture | CI builds/tests on Windows and Linux; deterministic engine host; fixtures; format docs | _in progress_ |
| M1 | Asset pipeline parity | Every asset in all three games loads; maps, models, sprites, audio, movies, fonts usable at runtime | _todo_ |
| M2 | Scenario VM and game state | Complete opcode coverage for FC/SC/3rd; stepped VM; flags, party, inventory, events, dialogue, save state | _todo_ |
| M3 | Field gameplay and game shell (FC) | Player can explore, talk, transition maps, open menus, save/load, and reach a battle | _todo_ |
| M4 | Battle system (FC) | Full AT battle loop with attacks, crafts, arts, items, AI, rewards, and presentation | _todo_ |
| M5 | RPG systems and FC content parity | Orbments, equipment, shops, quests, minigames, localization, and a full FC playthrough | _todo_ |
| M6 | SC support | SC assets, scripts, systems, save import, and a full SC playthrough | _todo_ |
| M7 | The 3rd support | 3rd assets, scripts, doors, systems, and a full 3rd playthrough | _todo_ |
| M8 | Hardening, tooling, and release | Cross-platform builds, performance targets, visual regression, modding tools, docs, 1.0 | _todo_ |

## D0 - Documentation bootstrap

- [x] **DOC-01 - Initialize project goal, roadmap, and agent workflow** - _done 2026-11-18_
  - Depends on: none
  - Scope: update `README.md` with the full-engine goal and honest current state, create the
    milestone roadmap in `TASKS.md`, and define the confirmation-gated workflow in `AGENTS.md`.
  - Done when: the three documents exist, agree with each other, and the developer accepts the plan.

## M0 - Foundations, tests, and architecture

**Goal:** make the project safe to evolve. No gameplay milestone should start before the test
harness, deterministic runtime skeleton, and format documentation exist.

**Exit criteria:** CI builds and tests on Windows and Linux; the engine host runs a deterministic
headless update loop; synthetic fixtures cover every core format; format docs exist; asset policy is
enforced.

- [x] **M0-01 - Add automated test infrastructure** - _done 2026-11-18_
  - Depends on: none
  - Scope: create `OpenSora.Tests` (xUnit), add it to the solution, add a `dotnet test` entry point,
    add first loader and decompiler tests, document how to run tests.
  - Done when: `dotnet test OpenSora.MonoGame.sln` runs at least one meaningful test per core area
    (archive, texture, scenario decompiler) and fails the build on regressions.
  - Result: 5 tests covering archive parsing, uncompressed DDS decoding, and FC scenario
    parsing/decompilation; Debug and Release test runs pass. A manual full-corpus FC smoke test
    decompiled 491 scenarios from 549 `_SN` entries with 0 exceptions.

- [x] **M0-02 - Create the engine host and deterministic update loop** - _done 2026-11-18_
  - Depends on: M0-01
  - Scope: introduce an engine host that owns update/render phases and a fixed-timestep game clock;
    make `OpenSora.Viewer` a tool that drives the host; remove `DateTime.Now` from simulation code.
  - ADR: `docs/adr/0001-engine-host.md`
  - Done when: the viewer behaves as before, a headless host can run N fixed updates with identical
    results, and simulation code has no wall-clock access.
  - Result: `EngineHost`/`EngineTime` added with 60 Hz fixed updates and headless stepping; viewer,
    scenario playback, sprite animation, and camera input are host-driven; `DateTime.Now` is gone
    from simulation/animation code. 11 tests pass in Debug and Release; viewer startup smoke test
    passed with the FC installation.

- [x] **M0-03 - Add deterministic RNG and input abstractions** - _done 2026-11-18_
  - Depends on: M0-02
  - Scope: seeded RNG service, input snapshot interface, replayable input trace, time source
    abstraction.
  - ADR: `docs/adr/0002-deterministic-input-and-random.md`
  - Done when: the same seed and input trace produce identical state over 10,000 updates; a test
    proves it.
  - Result: `DeterministicRandom`, `InputSnapshot`/`InputTrace` with recording and replay,
    `IFrameTimeSource`/`FrameTimeSource`, and a 10,000-update deterministic simulation test. 23 tests
    pass in Debug and Release; viewer startup smoke test passed with the FC installation.

- [x] **M0-04 - Build the synthetic fixture toolkit** - _done 2026-11-18_
  - Depends on: M0-01
  - Scope: generators for minimal DIR/DAT archives, DDS textures, CH/CP sprites, X2/X3 models, and
    FC scenario scripts. SC/3rd scenario fixtures are added with their parsers in M2-03/M2-04.
    Fixtures must be generated from code or scripts, never copied from a game installation.
  - Done when: tests can create fixtures in a temp folder, load them, and assert on known values;
    fixture policy is documented.
  - Result: `SyntheticArchive`, `SyntheticDds`, `SyntheticCh`, `SyntheticModel`, and
    `SyntheticScenario` builders plus headless CH/animation decoders. 29 tests pass in Debug and
    Release; real-data CH/CP sample decoding passed; fixture policy documented in
    `docs/testing/fixtures.md`.

- [x] **M0-05 - Establish CI** - _done 2026-11-18_
  - Depends on: M0-01, M0-04
  - Scope: GitHub Actions (or keep AppVeyor) for Windows and Linux: restore, build, test, publish
    viewer/tool artifacts, cache NuGet packages. Keep the workflow runnable on pull requests and
    manually via `workflow_dispatch`.
  - Done when: a pull request runs build + tests on both OSes and artifacts are downloadable.
  - Result: GitHub Actions matrix for Windows x64 and Linux x64 with NuGet caching, Release
    build/test, TRX test results, and viewer/Unpacker/Decompiler artifacts. Workflow commands passed
    locally on Windows and Linux (WSL); removed the unused WinForms reference that blocked clean
    Linux builds. Hosted run green:
    https://github.com/rds1983/OpenSora/actions/runs/44260301782

- [ ] **M0-06 - Add diagnostics, logging, and crash reporting** - _todo_
  - Depends on: M0-02
  - Scope: structured logging categories (archive, asset, VM, render, audio), unknown-opcode and
    unknown-chunk warnings, unhandled-exception report with recent log context, debug overlay.
  - Done when: a forced load failure produces an actionable report without a debugger attached, and
    unknown data is reported once per source instead of spamming.

- [ ] **M0-07 - Document the binary formats** - _todo_
  - Depends on: none
  - Scope: `docs/formats/` pages for DIR/DAT, DDS, CH/CP, X2/X3, SN/SCP + expressions, save files,
    audio containers, movie containers, and effects. Cite existing references and mark unknowns.
  - Done when: every loader file links to its format page, and the docs list open questions.

- [ ] **M0-08 - Record architecture decisions and dependency rules** - _todo_
  - Depends on: M0-02
  - Scope: `docs/adr/` with decisions on engine vs viewer separation, MonoGame boundaries, VM design,
    asset access, save format, and modding. Define allowed project references.
  - Done when: the first ADRs are accepted and CI or review checks the dependency direction.

- [ ] **M0-09 - Establish the asset and legal policy** - _todo_
  - Depends on: none
  - Scope: ignore game folders, extracted assets, saves, and large binaries; document clean-room
    expectations, third-party notices, and how to obtain test data.
  - Done when: `.gitignore` and docs make accidental asset commits unlikely, and CI rejects
    suspiciously large binaries.

- [ ] **M0-10 - Add baseline performance and memory measurements** - _todo_
  - Depends on: M0-02
  - Scope: benchmark CLI for map load, model load, scenario parse, VM update, and frame time; record
    a baseline on a representative machine.
  - Done when: a repeatable benchmark report exists and regressions can be compared.

- [ ] **M0-11 - Clean up build warnings and namespace issues** - _todo_
  - Depends on: M0-01
  - Scope: fix existing compiler warnings, obsolete MonoGame API usage, incorrect `Unpacker`
    namespaces, and dead code; decide whether warnings are errors.
  - Done when: the solution builds with 0 warnings or each remaining warning has a documented,
    intentional suppression.

- [ ] **M0-12 - Add a smoke test for the viewer** - _todo_
  - Depends on: M0-04
  - Scope: headless or off-screen initialization test that constructs the viewer host, loads a
    fixture game folder, and shuts down cleanly.
  - Done when: CI catches startup regressions without requiring a GPU-heavy interactive run.

## M1 - Asset pipeline parity

**Goal:** load every runtime asset from all three games without exceptions, with correct decoding,
names, and performance characteristics.

**Exit criteria:** an asset validation pass over FC, SC, and 3rd reports zero unhandled formats;
maps, models, sprites, effects, audio, movies, and fonts are usable through stable runtime APIs.

- [ ] **M1-01 - Build a virtual filesystem over archives and loose files** - _todo_
  - Depends on: M0-04
  - Scope: `IGameFileSystem` abstraction, DIR/DAT reader with lazy index, case-insensitive lookup,
    loose-file override priority, thread-safe cache, disposal.
  - Done when: the same logical path resolves from archive or override; tests cover duplicates,
    broken entries, and concurrent reads.

- [ ] **M1-02 - Add an asset catalog and game/edition detection** - _todo_
  - Depends on: M1-01
  - Scope: detect FC/SC/3rd and supported PC editions from executables and archive signatures;
    enumerate logical assets; validate expected files; expose a versioned manifest.
  - Done when: all three games are identified correctly, unsupported editions fail with a clear
    message, and `assets verify` reports missing/unknown entries.

- [ ] **M1-03 - Complete texture decoding** - _todo_
  - Depends on: M1-01
  - Scope: cover DXT1/2/3/4/5, uncompressed formats, mipmaps, alpha modes, and CH-embedded images;
    handle malformed data gracefully; add golden image tests.
  - Done when: every texture in the three games decodes and matches a reference hash or visual
    fixture.

- [ ] **M1-04 - Complete the CH/CP sprite pipeline** - _todo_
  - Depends on: M1-03
  - Scope: frames, anchors, palettes, head/body variants, portraits, animation info parsing, all
    directions and steps.
  - Done when: a gallery tool can play every character animation correctly and tests cover frame
    selection and anchor placement.

- [ ] **M1-05 - Complete X2/X3 model loading** - _todo_
  - Depends on: M1-03
  - Scope: node hierarchy, transforms, materials, texture binding, normals, vertex colors, bounds,
    scale conversion, multiple meshes per model.
  - Done when: every map and character model in the three games loads and renders with correct
    materials; no fallback-white textures remain on supported assets.

- [ ] **M1-06 - Implement model and skeletal animation playback** - _todo_
  - Depends on: M1-05
  - Scope: animation clips, keyframes, looping, blending, events, socket/attachment lookup, and
    playback control used by field and battle scenes.
  - Done when: animated map objects and character models play correctly in a viewer test and expose
    a runtime API with unit tests.

- [ ] **M1-07 - Load maps, tilemaps, and walkmeshes** - _todo_
  - Depends on: M1-05
  - Scope: map metadata, tilemap composition, terrain height, walkable flags, collision polygons or
    grid, object placement, map lighting/ambient data.
  - Done when: every field map in FC loads, renders, and exposes a walkability query that matches
    manual spot checks.

- [ ] **M1-08 - Implement the effects system** - _todo_
  - Depends on: M1-07
  - Scope: parse and play 2D/3D effects, screen effects, blur, fade, shake, flash, and effect
    lifetime/looping; integrate with scene rendering.
  - Done when: representative field and battle effects play without leaks and a fixture test covers
    lifecycle and parameter binding.

- [ ] **M1-09 - Implement the audio subsystem** - _todo_
  - Depends on: M1-01
  - Scope: OGG/WAV streaming and decoding, BGM loop points, SE pool, voice playback, 3D attenuation,
    channel groups, volume/pause/fade, device loss handling.
  - Done when: BGM, SE, and voice play from fixtures and real data; audio settings persist; tests
    cover mixer state and loop boundaries.

- [ ] **M1-10 - Implement movie playback** - _todo_
  - Depends on: M1-01
  - Scope: decode the game's movie container/codec (investigate AVI/MJPEG or platform variant),
    audio sync, skip, pause, and fallback behavior.
  - Done when: the intro and an event movie play with audio and can be skipped; unsupported codecs
    fail with a clear message.

- [ ] **M1-11 - Implement the text and font pipeline** - _todo_
  - Depends on: M1-01
  - Scope: Shift-JIS/UTF-8 handling, SCP control codes (line feed, wait, clear, color, ruby, item and
    character names), font atlas generation, wrapping, measurement, and EN/JA font sets.
  - Done when: dialogue samples render identically in structure (line breaks, colors, ruby) and text
    metrics match the original layout within tolerance.

- [ ] **M1-12 - Add full-corpus asset validation** - _todo_
  - Depends on: M1-01 through M1-11
  - Scope: CLI that walks all assets in a game folder, loads each with the real pipeline, records
    failures/unknowns/timings, and writes a report.
  - Done when: reports exist for FC, SC, and 3rd; every remaining failure has a tracked task.

- [ ] **M1-13 - Add an asset cache and conversion pipeline** - _todo_
  - Depends on: M1-12
  - Scope: optional on-disk cache for decoded textures/models/audio, invalidation by source hash,
    parallel warm-up, and a clean-cache command.
  - Done when: cold/warm load times are measured and the cache never returns stale or corrupt data.

## M2 - Scenario VM and game state

**Goal:** replace the preview timeline with a deterministic script VM that can execute the field,
event, and battle scripts of all three games, backed by complete game state.

**Exit criteria:** decompiler coverage is 100% for observed opcodes; unknown opcodes are reported;
the VM supports calls, threads, waits, flags, party, inventory, dialogue, events, and save/restore;
golden traces pass for representative scripts in all three games.

- [ ] **M2-01 - Replace the timeline worker with a stepped VM** - _todo_
  - Depends on: M0-02, M0-03
  - Scope: instruction pointer, call stack, frame/yield model, blocking instructions, concurrent
    workers, wait/sleep semantics, cancellation, deterministic stepping, and seek by replay.
  - Done when: scripts can block on dialogue/movement/audio and resume correctly; a test proves
    nested calls and multiple workers; the viewer preview uses the same VM.

- [ ] **M2-02 - Complete FC opcode and decompiler coverage** - _todo_
  - Depends on: M2-01
  - Scope: replace `Custom` gaps with named instructions, verify operand layouts, add branch target
    validation, and build an opcode discovery report over all FC scripts.
  - Done when: every observed FC opcode has a name and operand decoder; no script fails to
    decompile; unknown opcodes are listed with counts.

- [ ] **M2-03 - Add SC opcode and decompiler coverage** - _todo_
  - Depends on: M2-02
  - Scope: SC-specific table, format differences, expression extensions, a corpus report, and
    extending the synthetic scenario fixtures to SC.
  - Done when: every SC script decompiles and round-trips structurally; differences from FC are
    documented.

- [ ] **M2-04 - Add the 3rd opcode and decompiler coverage** - _todo_
  - Depends on: M2-03
  - Scope: 3rd-specific table, format differences, a corpus report, and extending the synthetic
    scenario fixtures to the 3rd.
  - Done when: every 3rd script decompiles and differences from SC are documented.

- [ ] **M2-05 - Complete the expression VM** - _todo_
  - Depends on: M2-01
  - Scope: arithmetic, comparisons, boolean logic, flags, string/character/item lookups, party and
    inventory queries, random values, and type coercion; match original semantics.
  - Done when: a table-driven test suite covers every expression opcode, including edge cases.

- [ ] **M2-06 - Implement full field instruction semantics** - _todo_
  - Depends on: M2-02, M2-05
  - Scope: character move/turn/chip/flags, camera, fades, effects, messages, sound, scene transitions,
    party/inventory/economy changes, event begin/end, and queue work items.
  - Done when: representative FC field scripts run end-to-end with correct visible and state effects;
    unimplemented instructions fail loudly with opcode and script context.

- [ ] **M2-07 - Build the game state model** - _todo_
  - Depends on: M2-01
  - Scope: scenario flags, global variables, party roster and reserve, inventory, mira, sepith,
    equipment, quartz, quests, playtime, location, chapter, RNG state, and versioning.
  - Done when: state can be cloned, hashed, and compared; tests cover mutation and serialization.

- [ ] **M2-08 - Implement the event and trigger system** - _todo_
  - Depends on: M2-06, M2-07
  - Scope: entry points, map events, talk/range/auto triggers, priorities, event queue, NPC
    enable/disable, one-shot flags, and cutscene input lock.
  - Done when: entering a map or talking to an NPC starts the correct script with correct priority;
    trigger tests cover overlapping events.

- [ ] **M2-09 - Implement the dialogue runtime** - _todo_
  - Depends on: M1-11, M2-05, M2-06
  - Scope: typewriter effect, wait/clear/color/ruby codes, choices, portraits, speaker names, voice
    sync, auto/skip modes, and message history.
  - Done when: a dialogue fixture with all control codes renders and advances correctly; choices
    return the right value to the VM.

- [ ] **M2-10 - Implement VM save/restore** - _todo_
  - Depends on: M2-07
  - Scope: serialize worker stacks, instruction pointers, pending waits, scene/event state, and game
    state into a versioned format; restore deterministically.
  - Done when: save/load at arbitrary safe points reproduces the same subsequent trace.

- [ ] **M2-11 - Add VM trace and disassembly tooling** - _todo_
  - Depends on: M2-02
  - Scope: CLI disassembler for FC/SC/3rd, trace logs with instruction offsets and state diffs,
    opcode coverage reports, and a script search tool.
  - Done when: a developer can explain any script failure from tool output alone.

- [ ] **M2-12 - Harden the VM against malformed and pathological scripts** - _todo_
  - Depends on: M2-01
  - Scope: bounds checks, recursion/loop limits, invalid branch targets, truncated operands, fuzzing,
    and graceful error recovery.
  - Done when: a fuzz corpus produces no crashes, hangs, or unbounded allocations.

- [ ] **M2-13 - Add VM conformance tests** - _todo_
  - Depends on: M2-02 through M2-10
  - Scope: golden traces for representative scripts per game: dialogue, movement, flags, choices,
    events, and save/restore.
  - Done when: traces are checked in as fixtures and CI compares state hashes.

## M3 - Field gameplay and game shell (FC)

**Goal:** turn the engine into a playable field game for FC: walk, talk, interact, transition maps,
open menus, and enter battles.

**Exit criteria:** a clean FC installation can be started from the title screen, played through a
scripted vertical slice, saved, reloaded, and continue to the first battle.

- [ ] **M3-01 - Render field maps with correct depth and presentation** - _todo_
  - Depends on: M1-06, M1-07
  - Scope: y-sorting, transparency, character/map depth interaction, culling, shadows, ambient
    tint, and camera framing that matches the original.
  - Done when: side-by-side screenshots of a reference map match layout and layering.

- [ ] **M3-02 - Implement player movement and collision** - _todo_
  - Depends on: M1-07, M3-01
  - Scope: 8-direction movement, run/walk, walkmesh constraints, collision response, slopes/height,
    animation state, footstep events, and camera follow.
  - Done when: the player can traverse a reference map without clipping, and a scripted route ends
    at the expected coordinates within tolerance.

- [ ] **M3-03 - Implement interaction and map transitions** - _todo_
  - Depends on: M3-02
  - Scope: talk/examine ray or probe, doors, stairs, map exits, chests, save points, shops, and
    transition fades/loading.
  - Done when: interacting with a reference NPC, chest, door, and save point triggers the correct
    script/state.

- [ ] **M3-04 - Implement map metadata and ambience** - _todo_
  - Depends on: M1-07
  - Scope: map name/place name, BGM assignment, default camera, weather/ambient effects, and
    per-map flags.
  - Done when: entering a map applies its name, music, and ambience; tests cover fallback behavior.

- [ ] **M3-05 - Integrate cutscenes with gameplay** - _todo_
  - Depends on: M2-06, M2-08, M3-02
  - Scope: input lock, scripted camera, skip/fast-forward, pause, and clean handoff back to player
    control.
  - Done when: a cutscene fixture plays, can be skipped, and leaves the player in a valid state.

- [ ] **M3-06 - Build the in-game HUD and pause flow** - _todo_
  - Depends on: M3-05
  - Scope: dialogue window, location banner, prompts, loading screen, pause overlay, and focus/input
    routing between game and UI.
  - Done when: HUD and pause work with keyboard and gamepad; input does not leak into the VM.

- [ ] **M3-07 - Implement the field-side encounter system** - _todo_
  - Depends on: M3-02, M3-04
  - Scope: monster symbols, roaming/chase behavior, contact, ambush/advantage states, encounter
    rate/tables, a battle-transition request, and escape return position. The battle scene itself is
    supplied by M4-02.
  - Done when: touching a monster produces the correct encounter and advantage state, the transition
    can be exercised against a stub battle scene, and returning restores the field position.

- [ ] **M3-08 - Build the camp menu** - _todo_
  - Depends on: M2-07, M3-06
  - Scope: status, items, orbment, equipment, recipes, journal, options, save, load, and quit;
    navigation, descriptions, and confirmation dialogs.
  - Done when: every menu screen opens, mutates state correctly, and matches reference layouts.

- [ ] **M3-09 - Build save/load UI and slots** - _todo_
  - Depends on: M2-10, M3-08
  - Scope: slot list, metadata (chapter, location, playtime, party), overwrite confirmation,
    thumbnails if the original has them, and error handling.
  - Done when: saving and loading from the UI round-trips a mid-game state, and corrupt saves are
    rejected safely.

- [ ] **M3-10 - Implement title screen and new game flow** - _todo_
  - Depends on: M1-10, M3-08, M3-09
  - Scope: title, new game, continue, options, credits, intro movie, and initial state setup.
  - Done when: a fresh start reaches the prologue and continue loads the selected save.

- [ ] **M3-11 - Implement persistent settings and input** - _todo_
  - Depends on: M0-03
  - Scope: resolution, window mode, scaling/filtering, vsync/frame limit, volumes, text speed,
    battle speed, language, keybinds, and gamepad mappings with persistence.
  - Done when: settings survive restart and apply live where expected.

- [ ] **M3-12 - Add the scripted vertical-slice test** - _todo_
  - Depends on: M3-01 through M3-11, M4-02
  - Scope: deterministic input/script replay from new game to the first battle, with screenshots and
    state assertions.
  - Done when: the test passes in CI (headless where possible) and catches regressions in field,
    dialogue, save/load, and battle entry.

- [ ] **M3-13 - Polish field presentation** - _todo_
  - Depends on: M3-01
  - Scope: footstep SFX, character shadows, alpha sorting, map animations, screen shake/fade, and
    camera easing.
  - Done when: the vertical slice looks and sounds close to the reference capture.

## M4 - Battle system (FC)

**Goal:** implement the complete FC tactical battle system, driven by original data and scripts.

**Exit criteria:** every battle type in FC can be fought with correct turn order, actions, formulas,
AI, presentation, rewards, and defeat/victory handling.

**Note:** M4-06 and M4-09 depend on M5-01 and M5-02. Those tasks are scheduled at the start of M5 and
may be completed before M4 is closed.

- [ ] **M4-01 - Parse battle data and encounter definitions** - _todo_
  - Depends on: M1-12, M2-06
  - Scope: monsters, stats, skills, formations, encounter tables, battle scripts, drops, and
    battlefield metadata.
  - Done when: every FC battle can be instantiated from data and a report lists unsupported fields.

- [ ] **M4-02 - Implement the battle state machine and AT bar** - _todo_
  - Depends on: M4-01
  - Scope: turn order, delay values, action phases, interrupts, escape, victory/defeat, and
    deterministic battle clock.
  - Done when: turn order matches reference traces for scripted battles.

- [ ] **M4-03 - Implement actions, movement, range, and targeting** - _todo_
  - Depends on: M4-02
  - Scope: move, attack, crafts, arts, items, run, target selection, area shapes, line of sight, and
    action validation.
  - Done when: each action type can be executed and rejected correctly, with tests for range/area.

- [ ] **M4-04 - Implement combat formulas and status effects** - _todo_
  - Depends on: M4-03
  - Scope: damage, hit, critical, element affinity, defense, buffs/debuffs, status ailments, death,
    and turn-delay interactions.
  - Done when: formula tests match reference values within documented tolerances.

- [ ] **M4-05 - Implement crafts, CP, and S-Crafts** - _todo_
  - Depends on: M4-03, M4-04
  - Scope: craft data, CP gain/spend, S-Craft thresholds, S-Break timing, and craft animations.
  - Done when: every FC craft is usable and CP/S-Break behavior matches the reference.

- [ ] **M4-06 - Implement arts, EP, and orbment casting** - _todo_
  - Depends on: M4-03, M4-04, M5-02
  - Scope: art lists, EP cost, cast time, interruption, elemental values, and orbment-derived arts.
  - Done when: casting, interruption, and EP consumption match reference traces.

- [ ] **M4-07 - Implement monster AI and skills** - _todo_
  - Depends on: M4-02, M4-04
  - Scope: AI scripts/behaviors, target choice, skill usage, reinforcements, fleeing, and boss
    phases.
  - Done when: scripted battles against representative monsters produce plausible, deterministic
    behavior and no stalls.

- [ ] **M4-08 - Build battle presentation** - _todo_
  - Depends on: M4-03, M1-06, M1-08
  - Scope: battle camera, model animation, effects, damage numbers, UI panels, voice, victory pose,
    and transitions.
  - Done when: a recorded battle looks structurally close to the reference and runs at target frame
    rate.

- [ ] **M4-09 - Implement rewards and battle-driven progression** - _todo_
  - Depends on: M4-02, M5-01
  - Scope: EXP, level-ups, sepith, drops, chest rewards, quest flags, and defeat/retry flow.
  - Done when: rewards match reference values and persist into field state.

- [ ] **M4-10 - Add a deterministic battle harness** - _todo_
  - Depends on: M4-02
  - Scope: scripted inputs, seeded RNG, state hashes, golden logs, and a CLI to run a battle headless.
  - Done when: the same battle replay produces identical hashes across runs and machines.

- [ ] **M4-11 - Validate battle parity across the game** - _todo_
  - Depends on: M4-01 through M4-10
  - Scope: sample early, mid, and late battles; bosses; ambush/advantage; escape; defeat.
  - Done when: a checklist of representative battles passes and remaining discrepancies are tracked.

## M5 - RPG systems and FC content parity

**Goal:** implement the progression systems and content needed to complete FC, then validate a full
playthrough.

**Exit criteria:** FC can be completed start-to-credits with all systems working; saves survive
chapter boundaries; localization and performance targets are met.

- [ ] **M5-01 - Implement stats, leveling, and growth** - _todo_
  - Depends on: M2-07
  - Scope: character stats, EXP curves, level-ups, stat growth, and derived values.
  - Done when: level/stat tables match reference data and tests cover boundary levels.

- [ ] **M5-02 - Implement the orbment system** - _todo_
  - Depends on: M2-07, M5-01
  - Scope: slots, lines, quartz inventory, sepith costs, slot unlocking, elemental values, and arts
    derivation.
  - Done when: every orbment configuration yields the expected arts and values.

- [ ] **M5-03 - Implement equipment, shops, and economy** - _todo_
  - Depends on: M5-01
  - Scope: weapons, armor, accessories, buy/sell, shop inventories, money, and stat effects.
  - Done when: shopping and equipping update stats and inventory correctly, with UI and tests.

- [ ] **M5-04 - Implement items, consumables, and cooking** - _todo_
  - Depends on: M5-01
  - Scope: item definitions, use in field and battle, recipes, ingredients, and cooking results.
  - Done when: every item/recipe has correct effects and the recipe menu works.

- [ ] **M5-05 - Implement journal, quests, and bestiary** - _todo_
  - Depends on: M2-08
  - Scope: quest state, bracer notebook entries, monster book, and completion tracking.
  - Done when: quests update their journal state at the right script points and the bestiary fills
    from battles.

- [ ] **M5-06 - Implement minigames** - _todo_
  - Depends on: M3-08
  - Scope: fishing and casino games available in FC, with rewards and original rules.
  - Done when: each minigame is playable, winnable/losable, and pays out correctly.

- [ ] **M5-07 - Implement collectibles and optional content** - _todo_
  - Depends on: M5-05
  - Scope: books/newspapers, hidden quests, optional scenes, and reward handoff.
  - Done when: optional content is reachable and state is persisted.

- [ ] **M5-08 - Build the localization pipeline** - _todo_
  - Depends on: M1-11
  - Scope: EN/JA text tables, extraction/repacking, font coverage, encoding validation, and a
    translation-friendly format.
  - Done when: switching language changes all game text and no missing glyphs appear in either
    language.

- [ ] **M5-09 - Validate a full FC playthrough** - _todo_
  - Depends on: M3-12, M4-11, M5-01 through M5-08
  - Scope: chapter-by-chapter checklist, save/load at boundaries, side content, credits, and crash
    sweep.
  - Done when: FC is completed start-to-credits without blockers and the report is archived.

- [ ] **M5-10 - Meet FC performance and load-time targets** - _todo_
  - Depends on: M1-13, M5-09
  - Scope: frame pacing, map transition time, memory ceiling, and startup time on minimum specs.
  - Done when: targets are documented and measured on at least two machines.

## M6 - SC support

**Goal:** make *Trails in the Sky SC* fully playable on the same engine, including its script and
system differences.

**Exit criteria:** SC starts from its title screen, imports or starts a game, and can be completed
start-to-credits.

- [ ] **M6-01 - Support SC detection and assets** - _todo_
  - Depends on: M1-02
  - Scope: `ed6_win2.exe` detection, archive differences, asset names, and edition handling.
  - Done when: SC is detected, its catalog validates, and the viewer/engine opens SC data.

- [ ] **M6-02 - Support SC scripts and VM extensions** - _todo_
  - Depends on: M2-03, M6-01
  - Scope: new opcodes, expression changes, event differences, and script-driven systems.
  - Done when: SC scripts execute with full coverage and golden traces pass.

- [ ] **M6-03 - Implement SC field, battle, and menu differences** - _todo_
  - Depends on: M6-02, M3-12, M4-11
  - Scope: new party members, systems, balance changes, UI differences, and any new minigames.
  - Done when: SC-specific systems are implemented and covered by tests.

- [ ] **M6-04 - Implement FC save import** - _todo_
  - Depends on: M2-10, M6-01
  - Scope: read FC clear data/saves, map the carried state, and handle missing or modded saves.
  - Done when: an FC clear save produces the expected SC starting bonuses.

- [ ] **M6-05 - Validate a full SC playthrough** - _todo_
  - Depends on: M6-01 through M6-04
  - Scope: chapter-by-chapter checklist, side content, credits, and crash sweep.
  - Done when: SC is completed start-to-credits without blockers and the report is archived.

## M7 - The 3rd support

**Goal:** make *Trails in the Sky the 3rd* fully playable, including its door and side-story content.

**Exit criteria:** the 3rd starts from its title screen and can be completed start-to-credits.

- [ ] **M7-01 - Support 3rd detection and assets** - _todo_
  - Depends on: M1-02
  - Scope: executable detection, archive/asset differences, edition handling, and catalog validation.
  - Done when: the 3rd is detected and all assets validate.

- [ ] **M7-02 - Support 3rd scripts and VM extensions** - _todo_
  - Depends on: M2-04, M7-01
  - Scope: new opcodes, expression changes, and script-driven systems.
  - Done when: 3rd scripts execute with full coverage and golden traces pass.

- [ ] **M7-03 - Implement the door system** - _todo_
  - Depends on: M7-02, M3-12
  - Scope: memory doors, side-story doors, minigame doors, quiz/arena content, and their rewards.
  - Done when: every door type opens, plays, completes, and persists its state.

- [ ] **M7-04 - Implement 3rd battle and system differences** - _todo_
  - Depends on: M7-02, M4-11
  - Scope: new characters, balance changes, systems, UI, and any exclusive mechanics.
  - Done when: 3rd-specific systems are implemented and covered by tests.

- [ ] **M7-05 - Validate a full 3rd playthrough** - _todo_
  - Depends on: M7-01 through M7-04
  - Scope: chapter/door checklist, side content, credits, and crash sweep.
  - Done when: the 3rd is completed start-to-credits without blockers and the report is archived.

## M8 - Hardening, tooling, and release

**Goal:** make OpenSora a stable, documented, cross-platform 1.0 release with useful modding and
debug tools.

**Exit criteria:** all three games pass automated and manual validation on Windows, Linux, and
macOS; performance targets are met; tools and docs are published; legal review is complete.

- [ ] **M8-01 - Support Windows, Linux, and macOS builds** - _todo_
  - Depends on: M5-09, M6-05, M7-05
  - Scope: platform-specific asset paths, case sensitivity, audio/video backends, input, packaging,
    and CI runners.
  - Done when: all three OSes build, pass tests, and complete the smoke playthrough.

- [ ] **M8-02 - Optimize performance, streaming, and memory** - _todo_
  - Depends on: M8-01
  - Scope: asset streaming, background loading, batching, texture atlases, GC pressure, and frame
    pacing on minimum specs.
  - Done when: documented targets are met on reference hardware and regressions are caught in CI.

- [ ] **M8-03 - Add visual regression testing** - _todo_
  - Depends on: M3-12, M4-11
  - Scope: deterministic screenshots for reference scenes, battles, menus, and effects with
    tolerance-based comparison.
  - Done when: CI flags unintended visual changes and approved baselines are versioned.

- [ ] **M8-04 - Build modding and authoring tools** - _todo_
  - Depends on: M2-11, M3-08
  - Scope: asset browser, model/map viewer, scenario editor, text editor, and packaging/validation
    tools for mods.
  - Done when: a sample mod can be created, validated, loaded, and distributed using only the tools.

- [ ] **M8-05 - Build the debug console and inspector** - _todo_
  - Depends on: M2-11
  - Scope: state inspector, flag/party/inventory editors, script jump, free camera, speed control,
    and cheats behind an explicit developer mode.
  - Done when: developers can diagnose most script and state bugs without a debugger.

- [ ] **M8-06 - Complete user and contributor documentation** - _todo_
  - Depends on: M8-01
  - Scope: install/build/run guides, troubleshooting, format docs, engine API docs, modding guide,
    and contribution guide.
  - Done when: a new contributor can build, test, and land a task using the docs alone.

- [ ] **M8-07 - Set up packaging and releases** - _todo_
  - Depends on: M8-01
  - Scope: versioning, reproducible builds, CI artifacts, release notes, checksums, and update
    channel decision.
  - Done when: tagged releases are produced automatically for all platforms.

- [ ] **M8-08 - Add accessibility and input polish** - _todo_
  - Depends on: M8-01
  - Scope: remappable keyboard/gamepad, dead zones, text size/speed, colorblind-safe UI where
    possible, and window/scaling options.
  - Done when: the game is fully playable with keyboard or gamepad and settings persist.

- [ ] **M8-09 - Complete legal and asset policy review** - _todo_
  - Depends on: M8-06
  - Scope: GPL compliance, third-party notices, trademark usage, no shipped game assets, and
    distribution guidance.
  - Done when: the release checklist includes legal sign-off and the repo contains no infringing
    content.

- [ ] **M8-10 - Define and verify 1.0 release criteria** - _todo_
  - Depends on: M8-01 through M8-09
  - Scope: final compatibility matrix, known issues list, crash rate target, and playthrough sign-off
    for FC, SC, and 3rd.
  - Done when: all criteria are met and 1.0 is tagged.

## Cross-cutting backlog

These are valuable but should be promoted into a milestone only when they unblock or materially
improve the goal. Keep them here with enough context to act later.

- [ ] **X-01 - Widescreen and HD presentation** - _deferred_
  - Original is 4:3; decide on pillarboxing, widescreen extension, integer scaling, and filtering.
- [ ] **X-02 - Steam Deck and Linux handheld validation** - _deferred_
  - Controller-first UI, suspend/resume, Proton/GL behavior, and performance profile.
- [ ] **X-03 - New Game+ and carry-over rules** - _deferred_
  - Determine which games support it and how clear data is consumed.
- [ ] **X-04 - Gallery, sound test, and movie theater** - _deferred_
  - Optional extras unlocked by completion.
- [ ] **X-05 - Achievements and stats** - _deferred_
  - Only if they can be implemented without external services; keep offline by default.
- [ ] **X-06 - Replay and speedrun tooling** - _deferred_
  - Deterministic replays, input display, split timer, and frame-step.
- [ ] **X-07 - Plugin/mod loader API** - _deferred_
  - Stable extension points for content and UI mods, with versioning and sandboxing rules.
- [ ] **X-08 - Crash report upload (opt-in)** - _deferred_
  - Privacy-preserving, off by default, with local report export first.
- [ ] **X-09 - Engine UI localization** - _deferred_
  - Translate menus/options independently of game text.
- [ ] **X-10 - Additional PC editions** - _deferred_
  - GOG/Japanese releases after the Steam editions are stable.

## Research spikes

These are time-boxed investigations that reduce uncertainty. Promote the result into a task or an
ADR; do not let a spike become an unbounded project.

- [ ] **R-01 - Identify the movie container and codec per edition** - _todo_
  - Needed by M1-10. Determine whether to implement a decoder or use an existing library, and check
    licensing.
- [ ] **R-02 - Map the effects format and opcode surface** - _todo_
  - Needed by M1-08 and M2-06. Produce a field guide and sample fixtures.
- [ ] **R-03 - Map battle scripts and AI data** - _todo_
  - Needed by M4-01 through M4-07. Determine whether scripts are SCP bytecode or a separate format.
- [ ] **R-04 - Map save file formats for FC/SC/3rd** - _todo_
  - Needed by M2-10, M3-09, M6-04. Decide whether original-save import is feasible.
- [ ] **R-05 - Measure asset sizes and loading bottlenecks** - _todo_
  - Needed by M1-13 and M8-02. Establish budgets before optimizing.
- [ ] **R-06 - Confirm edition differences (Steam vs GOG vs JP)** - _todo_
  - Needed by M1-02. Decide the supported matrix and error messages for unsupported installs.

## Open questions and decisions needed

Record answers as ADRs when resolved.

1. **Primary target editions:** Steam English FC/SC/3rd are assumed. Are GOG and Japanese PC
   releases in scope for 1.0 or later?
2. **Original save interoperability:** should the engine import original saves read-only, write its
   own format, or both? Cross-game clear-data import is expected for SC.
3. **Movie playback strategy:** custom decoder vs third-party library, and how to keep GPL
   compliance clean.
4. **Modding scope:** asset replacement only, or script/UI mods and a plugin API?
5. **Engine/viewer split:** what belongs in the reusable engine versus the developer viewer? The
   working assumption is engine = simulation + runtime rendering; viewer = tooling and previews.
6. **Rendering backend:** stay on MonoGame DesktopGL for 1.0, or keep an FNA/DX path? The current
   build is DesktopGL only.
7. **Minimum hardware and OS versions:** needed before M8 performance targets are meaningful.
8. **Localization ownership:** ship engine translations, or only support game-data text tables?

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Unknown formats (effects, battle scripts, movies) | Blocks milestones | Time-boxed spikes; keep loaders tolerant and report unknowns |
| Scope is a full trilogy | Burnout, endless roadmap | Vertical slice first; FC complete before SC/3rd; living task board |
| No game data in CI | Regressions slip through | Synthetic fixtures; local corpus validation scripts; documented manual checklist |
| Naive loaders are slow | Poor experience | Baseline benchmarks early; cache/streaming work in M1/M8 |
| Formula drift from original | Battles feel wrong | Golden traces, reference captures, tolerance-based tests |
| Asset/legal mistakes | Project takedown | Explicit policy, ignore rules, CI checks, no shipped assets |
| MonoGame/platform breakage | Build failures | Pin versions, multi-OS CI, ADR for backend changes |
| Agent overclaims completion | Unreliable history | Developer confirmation gate; evidence-based done criteria |

## Global definition of done

A task is done only when all of the following are true:

- The implementation matches the task's acceptance criteria and the current architecture.
- The solution builds with no new errors or warnings.
- New behavior has automated tests; changed behavior has updated tests.
- The relevant manual verification was performed and recorded (game/version, map/battle, result).
- No regressions in existing viewer/tool behavior.
- `README.md` reflects the new project state and known limitations honestly.
- The developer explicitly confirmed the task is finished.
- `TASKS.md` is updated with status, date, completed-log entry, and any newly discovered work.
- The commit is focused, message references the task ID, and it has been pushed.
- CI is green for the pushed commit, or the failure is understood and tracked.

## Completed log

| Date | Task | Summary |
|---|---|---|
| 2026-11-18 | DOC-01 | Established the project goal, live roadmap, and agent workflow; documented the current viewer state. |
| 2026-11-18 | M0-01 | Added the xUnit test project with archive, DDS, and FC scenario coverage; documented the test workflow. |
| 2026-11-18 | M0-02 | Added the fixed-timestep engine host, removed wall-clock timing from simulation/animation, and recorded ADR 0001. |
| 2026-11-18 | M0-03 | Added deterministic RNG, input snapshots/traces with recording and replay, frame-time sources, and ADR 0002. |
| 2026-11-18 | M0-04 | Added the synthetic fixture toolkit and headless CH/animation decoders; documented fixture policy. |
| 2026-11-18 | M0-05 | Added GitHub Actions CI for Windows and Linux with caching, tests, and downloadable artifacts. |

## Changelog

| Date | Change | Rationale |
|---|---|---|
| 2026-11-18 | Initial roadmap with M0-M8, cross-cutting backlog, spikes, risks, and definition of done | Turn the revived viewer into a full trilogy engine with an actionable, testable plan |
| 2026-11-18 | Added ADR 0001 and completed M0-02; next focus is M0-03 | Establish deterministic fixed-step timing before input abstraction and gameplay work |
| 2026-11-18 | Added ADR 0002 and completed M0-03; next focus is M0-04 | Seal deterministic random/input contracts before building the fixture toolkit |
| 2026-11-18 | Completed M0-04; SC/3rd scenario fixtures moved to M2-03/M2-04 | Build loadable fixtures only for formats the engine can currently parse |
| 2026-11-18 | Completed M0-05; next focus is M0-06 | Establish hosted verification before diagnostics and asset work |
