# AGENTS.md - Working agreement for OpenSora

This document defines how AI agents and human contributors work on OpenSora. It is the operating
manual for the project: read it at the start of every session, follow it exactly, and update it only
when the team agrees the process has changed.

## Mission

Turn OpenSora into a full-featured engine that runs **Trails in the Sky FC, SC, and the 3rd** from
unmodified game data, start-to-credits, with field gameplay, dialogue, battles, menus, saves, audio,
movies, and cross-platform support.

The project goal and current state live in `README.md`. The roadmap and task board live in
`TASKS.md`. When documents disagree, the implementation and test evidence win; fix the documents
immediately.

## Non-negotiables

1. **No game data in the repository.** Never commit game installations, extracted archives, assets,
   saves, movies, audio, or other copyrighted content. Do not add test fixtures copied from a game.
   Use synthetic fixtures generated from code or scripts. Read `docs/legal.md` before adding data,
   screenshots, logs, or dependencies.
2. **One task at a time.** Pick one board task, finish it, get confirmation, then move on. Do not
   batch unrelated work into one confirmation request.
3. **No completion claims without evidence.** "It builds" is not testing. State exactly what was run,
   on which game/version, and what passed or failed.
4. **No marking done and no commit/push before explicit developer confirmation.** This is a hard gate.
   If the developer is unavailable, stop and report; do not self-approve.
5. **`TASKS.md` is a living document.** You may split, merge, reorder, add, or rewrite tasks at any
   time to match reality. Record structural changes in the changelog. You may not silently delete
   unfinished work.
6. **Preserve working behavior.** The viewer and tools must keep working while the engine grows. If a
   change intentionally breaks something, call it out and get agreement first.
7. **Ask before destructive or high-impact changes.** Examples: deleting projects, replacing the
   rendering backend, adding a large dependency, rewriting the VM architecture, changing the save
   format incompatibly, or changing the public workflow.
8. **Follow the existing code style.** Tabs, existing naming, file layout, and patterns. Match the
   surrounding code before inventing new conventions.

## Session start checklist

Before touching code:

1. Read `AGENTS.md`, `README.md`, and `TASKS.md`.
2. Run `git status` and `git log --oneline -10` to understand the working tree and recent history.
3. Check the "Current focus", "Next task", and any `_blocked:_` markers in `TASKS.md`.
4. Read the files relevant to the selected task, plus at least one nearby module to learn the local
   patterns.
5. State the chosen task and a short plan in your response.

If the working tree has uncommitted changes you did not make, treat them as the developer's work.
Do not discard or overwrite them without explicit permission.

## The workflow

### Step 1 - Get the next task from `TASKS.md`

Default selection order:

1. The task named in **Current focus / Next task**, if its dependencies are satisfied.
2. The first unchecked task in the current milestone whose dependencies are satisfied.
3. The first unchecked task in the next milestone, if the current milestone is blocked.
4. A cross-cutting task or research spike, if the developer asks for it.

Rules:

- Respect `Depends on:` fields. If a dependency is not done, choose another task or explicitly ask
  the developer whether to proceed out of order.
- If the task is blocked, change its marker to `_blocked: reason_`, record what is needed, and pick
  another task. Do not sit idle.
- If the task is too large for one reviewable change, split it in `TASKS.md` first (for example
  `M3-12a`, `M3-12b`) and do the first part.
- If a task no longer makes sense, rewrite it or replace it. Record why in the changelog.
- If the developer directs you to work on something not on the board, treat that as the task. If it
  is substantial, add it to `TASKS.md` so the work is tracked.
- Before starting, set the task marker to `_in progress_` and update `Last updated` in `TASKS.md`.
  Planning edits do not need confirmation.

Announce the task in one or two sentences, including the task ID and what "done" will mean.

### Step 2 - Do it

Implementation rules:

- Keep the change focused on the task. Do not sneak in unrelated refactors, renames, or formatting
  churn.
- Prefer small, readable changes that fit the existing architecture. If the task requires an
  architectural change, write or update an ADR first (see M0-08). Respect accepted ADRs; supersede
  rather than silently rewriting them.
- Keep the project dependency graph in `docs/adr/0009-project-dependencies.md`; `ArchitectureTests`
  must stay green.
- Add the tests alongside the code, not afterward, when practical.
- Handle failure paths: malformed data, missing assets, unsupported editions, and partial state.
  Error messages must identify the asset/script and the operation that failed.
- Keep the viewer and CLI tools working. If a shared API changes, update every caller.
- Do not add dependencies without a clear reason and developer awareness. Prefer the standard
  library and existing packages first.
- Do not hardcode game-specific paths, editions, or content into engine code. Put data and detection
  behind the asset catalog and game profile abstractions.
- Do not use wall-clock time, unseeded randomness, or platform-specific behavior in simulation code.
  Use the engine clock, RNG, and input abstractions.
- Route diagnostics through `OpenSora.Diagnostics.Log` with an appropriate category. Use
  `UnknownDataReporter.ReportOnce` for unknown opcodes/chunks/fields; do not spam the log per frame.
  Do not add telemetry or automatic log uploads.
- Update comments and docs when behavior or formats become clearer. Unknowns belong in
  `docs/formats/` with a tracked task, not in silent guesses.

If you discover additional work:

- Small, directly related fixes: do them as part of the task and mention them in the report.
- Larger discoveries: add tasks to `TASKS.md` (with dependencies and done criteria) and mention them
  in the confirmation request. Do not expand the current task indefinitely.

### Step 3 - Test thoroughly

Testing is task-specific. At minimum:

1. Build the whole solution and confirm no new errors or warnings:
   ```bash
   dotnet build OpenSora.MonoGame.sln
   ```
2. Run all automated tests:
   ```bash
   dotnet test OpenSora.MonoGame.sln
   ```
3. Run the checks that match the change (see the table below).
4. Re-run the previous task's verification path when the change touches shared code.
5. Record the commands, game/edition/version, map/battle/script used, and the observed result. Include
   failures and how you resolved them.

| Change type | Required verification |
|---|---|
| Archive/asset loader | Load every affected asset type from all available games; report failures; run fixture tests; check memory/leaks |
| Texture/model/sprite | Decode a representative sample; golden hash or screenshot; compare against reference |
| Scenario decompiler/VM | Decompile the full corpus for the affected game; report unknown opcodes; run golden traces; test malformed input |
| Field gameplay | Scripted route or manual run; screenshots; collision/transition checks; save/load mid-run |
| Battle | Deterministic replay with state hashes; compare formulas and turn order; victory, defeat, escape |
| Menu/UI | Keyboard and gamepad navigation; all screens; persistence; edge cases (empty inventory, full party) |
| Save/load | Round-trip at multiple points; corrupt/truncated/old-version saves fail safely; cross-chapter continuity |
| Audio/video | Playback, looping, seek/skip, device changes, volume/pause, missing-file fallback |
| Performance | Benchmark before/after; frame time, load time, allocations; document hardware |
| Documentation only | Link check, command accuracy, no broken references; build still succeeds |

If the change cannot be verified automatically because it needs game data or a GPU:

- Use synthetic fixtures for automated coverage where possible.
- Perform a manual check and describe exactly what you did.
- State clearly what remains unverified. Never present an unverified area as tested.
- Do not ask the developer to verify basic correctness that you can test yourself.

### Step 4 - Update `README.md` with the new project state

After the code and tests are ready, update the status section of `README.md`:

- Update the capabilities table if the change alters what works.
- Update "What works today" and "Known limitations" honestly.
- Update build/run/tool instructions if commands, prerequisites, or project layout changed.
- Update the roadmap summary only if the milestone structure changed.
- Do not overclaim. "Partial", "viewer-only", and "not yet supported" are valid states.
- Keep the README user-facing. Implementation minutiae belong in `TASKS.md`, docs, or commit
  messages.

The README update is part of the task and must be ready **before** asking for confirmation.

### Step 5 - Ask the developer whether the task is finished

Stop and ask for explicit confirmation. Use this template:

> **Task M3-02 - Player movement and collision is ready for review.**
>
> **What changed:** ...
> **How it was tested:** commands, game/edition/version, map/route, results.
> **Automated coverage:** test names and what they assert.
> **Known limitations / unverified areas:** ...
> **Risks:** ...
> **Files changed:** ...
>
> Is this task finished? If yes, I will mark it done in `TASKS.md`, update the completed log, and
> commit/push. If not, tell me what to change.

Rules:

- Ask one clear yes/no question about this task. Do not bundle multiple tasks.
- Do not mark the task `[x]`, do not update the completed log, and do not commit or push yet.
- If the developer says no, or asks for changes: return to Step 2, implement, re-test, update the
  README, and ask again.
- If the developer asks a question instead of confirming, answer it. Do not treat it as approval.
- Only an explicit confirmation such as "yes", "finished", "approved", "ship it", or "commit" counts.
  Silence, ambiguity, or "looks fine" without a clear instruction to finish is not confirmation.
- If the developer wants to defer the task, mark it `_deferred_` or leave it `_in progress_` with a
  note, and do not commit unless asked.

### Step 6 - Only after explicit confirmation: update `TASKS.md`, commit, and push

After confirmation:

1. Mark the task `[x]` and `_done YYYY-MM-DD_`.
2. Append an entry to the **Completed log** with the date, task ID, and a one-line summary.
3. Update **Current focus**, **Next task**, and **Last updated**.
4. Add any discovered tasks, adjust dependencies, and record structural changes in the **Changelog**.
5. Review the full diff before committing:
   ```bash
   git status
   git diff --stat
   git diff
   ```
6. Verify no game assets, saves, secrets, build outputs, or large binaries are staged.
7. Commit with a focused message that references the task ID, for example:
   ```
   M3-02: implement player movement and collision

   - Add walkmesh-constrained movement and 8-direction animation state
   - Add collision response and camera follow
   - Cover movement and transition edge cases with tests
   - Update README status
   ```
8. Push to the current branch:
   ```bash
   git push
   ```
   If the branch has no upstream, ask the developer which remote/branch to use before setting one.
9. Check the CI run for the pushed commit. If it fails, fix the failure before claiming the task is
   complete or starting the next task.
10. Report the commit hash, push result, CI result, and the next recommended task.

If the push fails (permissions, conflicts, protected branch), stop and report the exact error. Do not
force-push, rewrite history, or create a pull request unless the developer asks.

## Editing `TASKS.md`

`TASKS.md` is allowed and expected to change. Follow these rules:

- **Planning edits** (splitting, adding, reordering, clarifying, changing dependencies or acceptance
  criteria) may be made at any time. No confirmation needed. Update the changelog and `Last updated`.
- **Completion edits** (marking `[x]`, completed log, moving "Next task") require explicit developer
  confirmation.
- Keep task IDs stable. If a task is replaced, leave a short note or give the replacement a new ID.
  Do not recycle IDs for different work.
- Keep the format consistent: status marker, task ID and title, `Depends on`, `Scope`, `Done when`.
- When adding a task, include enough context that another agent can execute it without re-deriving
  the plan.
- If a task grows beyond one reviewable change, split it rather than letting it sprawl.
- If a task is obsolete, explain why in the changelog instead of deleting it silently.
- Keep "Current focus", "Next task", and "Last completed" accurate at all times.

## Git conventions

- One logical change per commit. Do not mix documentation-only, refactor, and feature work unless
  they are inseparable.
- Do not commit generated build output, IDE state, extracted assets, or local game paths.
- Do not amend, rebase, force-push, or rewrite shared history without explicit permission.
- Do not create tags or releases unless the task calls for it and the developer confirms.
- If pre-existing uncommitted changes are present, do not include them in your commit. Stage only your
  files and mention the others.
- Commit messages use the task ID prefix and an imperative summary, as shown above.

## Communication

- Be concise and factual. Report what changed, what was tested, what is uncertain, and what is next.
- Surface blockers early. Do not silently work around a broken dependency or missing game data.
- Use file paths when referring to code.
- When a test fails, include the failing command and the relevant output.
- Do not claim "all tests pass" unless you actually ran the full suite.
- If you cannot test something, say so and explain what would be needed.

## Quick reference commands

```bash
# Build everything
dotnet build OpenSora.MonoGame.sln

# Run tests
dotnet test OpenSora.MonoGame.sln

# Run the viewer
dotnet run --project OpenSora.Viewer/OpenSora.Viewer.MonoGame.csproj

# Unpack a game folder
dotnet run --project OpenSora.Unpacker/OpenSora.Unpacker.csproj -- <game-folder> <output-folder>

# Disassemble FC scenarios
dotnet run --project OpenSora.Decompiler/OpenSora.Decompiler.csproj -- <game-folder> <output-folder>
```

## Current environment notes

- Solution: `OpenSora.MonoGame.sln`
- Runtime: .NET 8 (`net8.0`), MonoGame DesktopGL 3.8.6.1, Myra 1.6.8
- Primary OS used by the developer: Windows; CI should also cover Linux
- The solution currently builds with warnings; treat new warnings as regressions and fix existing
  ones under M0-11
- Automated tests live in `OpenSora.Tests`; run them with `dotnet test OpenSora.MonoGame.sln`
- CI runs on GitHub Actions for Windows and Linux; validate locally on Windows and, when available,
  WSL/Linux before pushing
- Diagnostics write local logs and crash reports under the platform's application-data directory; see
  `docs/diagnostics.md`. Nothing is uploaded
- Binary format notes live in `docs/formats/`; when changing a loader, update its format page and mark
  statements as verified, inferred, or unknown
- Architecture decisions live in `docs/adr/`; dependency direction is enforced by
  `OpenSora.Tests/ArchitectureTests.cs` and CI
- Asset/legal policy lives in `docs/legal.md`; `RepositoryPolicyTests` rejects tracked game data and
  large binaries in CI, and `.gitignore` blocks common asset/save/archive extensions
- Simulation time comes from `OpenSora.Engine.EngineHost`; do not use `DateTime.Now` or other
  wall-clock sources in simulation or animation code
- Simulation randomness must use `DeterministicRandom`; do not use `System.Random` in simulation code
- Simulation input must come from `InputSnapshot`/`InputTrace`; do not read keyboard, mouse, or
  gamepad state inside simulation code
- Test fixtures must be generated from code in `OpenSora.Tests/Fixtures`; see
  `docs/testing/fixtures.md`. Never copy game data into the repository
- Game data is not present in the repository and must never be added

## When in doubt

Prefer the smallest change that advances the current task, preserves working behavior, and can be
verified. Ask the developer before irreversible or architectural decisions. Put the answer in
`TASKS.md` or an ADR so the next agent does not have to rediscover it.
