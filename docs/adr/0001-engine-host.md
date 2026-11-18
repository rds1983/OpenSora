# ADR 0001 - Engine host and fixed-timestep clock

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-02

## Context

The viewer previously drove scenario playback from `ViewerGame.Draw()` and advanced the scenario
with `ExecutionContext.Update()`. That method, sprite animation, and camera movement all read
`DateTime.Now`. As a result:

- simulation speed depended on wall-clock time and frame rate;
- the same input could produce different results on different runs;
- headless tests and replays were impossible;
- rendering mutated animation state, making update/render separation unclear.

The engine needs a deterministic clock before scenario execution, battles, saves, and replays can be
validated.

## Decision

1. Add `OpenSora.Engine.EngineHost` and `EngineTime` in `OpenSora.Common/Engine`.
2. `EngineHost` owns a fixed-timestep accumulator. The default step is 60 Hz, and long frames are
   clamped to 0.25 seconds to prevent catch-up spirals.
3. `EngineHost` emits `EngineTime` (`DeltaSeconds`, `TotalSeconds`, `Tick`) once per fixed update.
   `RunFixedSteps(count)` provides a deterministic path for headless tests and tooling.
4. `OpenSora.Viewer` is the host application: it supplies elapsed frame time, input, UI, and
   rendering, and subscribes to engine updates. It no longer calls simulation or animation updates
   from `Draw()`.
5. Simulation code receives delta time through APIs such as `ExecutionContext.Update(double)` and
   `Scene.Update(double)`. `ExecutionContext` accumulates fractional milliseconds so integer
   scenario timing does not drift at 60 Hz.
6. Sprite animation and camera movement are advanced from engine updates, not while rendering.
7. `DateTime.Now` must not appear in simulation or animation timing code.

The engine host does not yet own input, rendering, or a separate engine project. Input abstraction is
M0-03; the engine/viewer project split and dependency rules are M0-08.

## Consequences

### Positive

- Simulation is deterministic for a fixed delta and input trace.
- The same update path works for the viewer, headless runs, and future replay tests.
- Rendering is separated from state mutation.
- Engine timing is testable without a graphics device or wall clock.

### Negative / trade-offs

- The viewer still owns input polling and UI state; some of that state is read inside the engine
  update callback until M0-03.
- A fixed 60 Hz step adds a small amount of latency to high-refresh displays compared with a
  variable-delta simulation.
- `EngineHost` currently lives in `OpenSora.Common`; it may move to a dedicated engine project once
  M0-08 defines the project boundaries.

## Alternatives considered

- **Rely on MonoGame's fixed timestep only.** Rejected: it does not provide a headless deterministic
  path and keeps wall-clock access in simulation code.
- **Use variable delta directly.** Rejected: frame-rate-dependent physics and scenario movement make
  replays and golden tests unreliable.
- **Keep `DateTime.Now` and fake it in tests.** Rejected: hidden global state is hard to audit and
  easy to reintroduce.
