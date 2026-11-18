# ADR 0002 - Deterministic random numbers and input traces

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-03

## Context

Deterministic fixed updates (ADR 0001) are necessary but not sufficient for reproducible simulation.
Gameplay also needs:

- random values that can be restored from a save or replayed exactly;
- input that can be recorded and replayed without device state;
- a way to feed frame time into the host without coupling simulation to wall-clock time.

`System.Random` is not a suitable long-term save/replay format because its algorithm and internal
state can change between runtimes. Reading keyboard, mouse, or gamepad state directly inside
simulation code makes tests and replays impossible.

## Decision

1. **Randomness:** add `DeterministicRandom`, a SplitMix64 generator with an explicit 64-bit state.
   - All simulation randomness must go through this service.
   - `State` can be captured and restored; `Seed` is retained for resets.
   - Convenience methods cover integers, doubles, floats, and booleans.
2. **Input snapshots:** add an immutable `InputSnapshot` containing logical buttons and analog axes.
   - `InputButton` is a `[Flags]` enum of game-level actions, not physical keys.
   - The viewer/platform layer is responsible for mapping devices into snapshots.
3. **Input sources and traces:** add `IInputSource`, `InputTrace`, `RecordingInputSource`, and
   `ReplayInputSource`.
   - A source is sampled once per fixed update.
   - Recording appends the sampled snapshot to a trace; replay returns snapshots in order.
   - Once a replay trace is exhausted, it returns `InputSnapshot.Empty` so the simulation remains
     deterministic.
4. **Frame time:** add `IFrameTimeSource` and `FrameTimeSource`. `EngineHost.Advance(IFrameTimeSource)`
   converts the frame delta into fixed steps. Simulation never reads the source directly.
5. Input mapping, VM integration, and viewer gameplay controls are deferred to the milestones that
   introduce them (M2 and M3). This task establishes the contracts and proves them with tests.

## Consequences

### Positive

- Randomness and input can be captured in saves, replayed, and compared in tests.
- The same seed and input trace produce identical simulation state.
- Simulation code remains independent of MonoGame and physical devices.
- Input traces provide a foundation for bug reports, regression tests, and speedrun tooling.

### Negative / trade-offs

- The input model must be maintained as new actions are added.
- The RNG is project-owned and must stay stable; changing the algorithm or seeding is a save-format
  breaking change.
- `InputTrace` stores one snapshot per fixed update; long recordings need size limits or compression
  later.
- The viewer still polls MonoGame devices directly until gameplay input mapping lands.

## Alternatives considered

- **Use `System.Random`.** Rejected: hidden internal state, runtime-dependent algorithm, weak
  save/replay guarantees.
- **Let simulation read MonoGame input.** Rejected: untestable, non-deterministic, and coupled to the
  viewer.
- **Record raw device events.** Rejected for now: platform-specific, larger, and harder to validate
  than logical per-tick snapshots.
