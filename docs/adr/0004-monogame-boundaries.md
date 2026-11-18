# ADR 0004 - MonoGame boundaries

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Follow-up:** M2-14

## Context

MonoGame supplies math types, graphics devices, textures, input devices, and the `Game` loop. The
project also uses Myra for UI. Allowing these types throughout simulation would make headless runs and
deterministic replays impossible.

## Decision

1. **MonoGame math value types are allowed in engine code.** `Vector2`, `Vector3`, `Vector4`, `Matrix`,
   `Color`, and `Rectangle` may be used where they represent data, not devices.
2. **Device and host types are confined to presentation layers.** `Game`, `GameTime`,
   `GraphicsDevice`, `Texture2D`, `SpriteBatch`, `Keyboard`, `Mouse`, `GamePad`, and `Effect` are not
   allowed in simulation projects (`OpenSora.Common/Engine`, `OpenSora.Common/Scenarios`, or the
   future engine runtime).
3. **Myra is viewer-only.** Engine simulation must not reference Myra widgets, `Desktop`, file dialogs,
   or viewer panels.
4. **Platform input is mapped at the host boundary.** The host converts keyboard/mouse/gamepad state
   into `InputSnapshot`; simulation consumes only snapshots.
5. **Time and randomness are engine services.** Simulation uses `EngineHost`/`EngineTime` and
   `DeterministicRandom`, never `DateTime`, `Stopwatch`, or `System.Random`.
6. **Rendering receives immutable data.** Render code may read scene/animation data and mutate
   GPU-side caches, but must not advance simulation state while drawing.

## Enforcement

`ArchitectureTests` scans `OpenSora.Common/Engine` and `OpenSora.Common/Scenarios` for forbidden
graphics/input/Myra identifiers. Project dependency tests enforce assembly direction. Code review must
reject new violations outside temporary, documented exceptions.

## Current exceptions

- `Scene` contains Myra dialogue/debug UI.
- `Scene`, `Animation`, and `CameraInputController` live in `OpenSora.Common/Rendering` and use
  MonoGame graphics/input types. This is allowed for presentation/rendering code but should move out of
  simulation paths.
- `ExecutionContext` uses `Vector3` only, which is allowed.

## Consequences

- Headless tests and replays remain possible.
- Some engine code needs plain-data DTOs instead of textures/devices.
- A few transitional violations must be removed in M2-14.
- Renderer upgrades can happen without changing simulation contracts.

## Alternatives considered

- **Allow MonoGame everywhere for convenience.** Rejected: destroys headless testing and portability.
- **Write custom math types.** Rejected for now: MonoGame math structs are stable and broadly useful.
- **Move all MonoGame code to one project immediately.** Deferred to M2-14 to avoid premature churn.
