# ADR 0003 - Engine runtime and viewer boundaries

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Follow-up:** M2-14

## Context

`OpenSora.Common` currently contains format readers, rendering, the engine host, deterministic
services, the scenario preview timeline, and a Myra dialogue widget used by `Scene`. The viewer is a
MonoGame/Myra tool that also acts as the host application.

A full engine must let field, battle, menus, saves, and replay run without depending on the viewer
window or its UI. Viewer-specific code must never become required by simulation code.

## Decision

1. **Viewer is a host/tool, not the engine.** It owns the window, MonoGame `Game` loop adaptation,
   platform input mapping, Myra UI composition, file dialogs, camera tooling, and diagnostic display.
   It may depend on engine/common libraries; they must not depend on the viewer.
2. **Engine runtime owns simulation.** It owns the fixed clock, VM, game state, event/scene state,
   asset catalog, save/restore, and deterministic replay. It may emit commands/events; it must not
   call viewer UI directly.
3. **Rendering is a service consumed by the host.** Scene rendering stays behind an engine-facing
   scene/asset model. The viewer supplies a render surface and platform services, but simulation code
   does not know about Myra, `Desktop`, or viewer panels.
4. **Transitional state is allowed but tracked.** The current `Scene` dialogue widget and Myra debug
   text are known boundary violations. They are accepted temporarily and must move to the viewer/UI
   layer in M2-14.
5. **New features choose the correct side.** If a feature needs a window, file dialog, device, or
   viewer widget, it belongs in the viewer/tool. If it affects simulation state or replay, it belongs
   in the engine runtime.

## Consequences

### Positive

- The engine can run headless for tests, replays, and corpus validation.
- Viewer tools can evolve without destabilizing simulation.
- Field/battle/menu systems can share one runtime and one save model.
- CI can enforce dependency direction automatically.

### Negative / trade-offs

- The current `OpenSora.Common` assembly mixes concerns; extracting the runtime is deferred work.
- Some rendering helpers will need interfaces or callbacks instead of direct viewer access.
- The viewer will become a composition root rather than the home of game logic.

## Alternatives considered

- **Keep the viewer as the engine.** Rejected: blocks headless tests, replay, and future frontends.
- **Split everything immediately.** Rejected: too much churn before the VM/state model exists. M2-14
  performs the extraction once those contracts are stable.
- **Put simulation in a platform-specific project.** Rejected: simulation must stay portable and
  deterministic.

## Follow-ups

- M2-14: extract the engine runtime assembly and move Myra dialogue UI out of `Scene`.
- M0-09: project dependency rules and architecture tests.
- Field/battle/menu work must respect the boundary from its first commit.
