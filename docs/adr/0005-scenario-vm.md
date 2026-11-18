# ADR 0005 - Scenario VM design

- **Status:** Accepted
- **Date:** 2026-11-18
- **Task:** M0-08
- **Implementation:** M2-01 through M2-13

## Context

The current `ExecutionWorker` is a preview timeline: it advances by total duration, rewinds by replay,
has no call stack, executes only a subset of instructions, and cannot represent game flags, workers, or
save state. It is useful for the viewer but not suitable for field or battle execution.

## Decision

1. **The VM is stepped, not time-rewound.** Execution advances instruction by instruction with a
   deterministic fixed delta. Seek is implemented by replaying to a target from a snapshot, not by
   mutating partial state.
2. **VM state is explicit.** The VM owns:
   - instruction pointer and current instruction progress;
   - call stack;
   - worker/thread list and scheduling order;
   - blocking wait state (sleep, dialogue, movement, audio, scene transition);
   - expression stack;
   - pending branches/queued work items.
3. **Game state is separate from VM state.** Flags, variables, party, inventory, mira, sepith,
   equipment, quests, playtime, RNG state, and versioning live in a game-state model. VM and game state
   are saved together in one versioned snapshot.
4. **Instructions interact through services, not globals.** Character/camera/scene/audio/message
   operations go through context services. Instructions must not touch `GraphicsDevice`, Myra, or the
   viewer directly.
5. **Failure policy is explicit.** Unknown opcodes are reported once per source and raise a contextual
   error with source/offset. Silent no-ops are not allowed for instructions required by a running
   script; stubs must be tracked.
6. **Determinism is mandatory.** VM scheduling, RNG use, and input sampling are deterministic. RNG uses
   `DeterministicRandom`; input uses `InputSnapshot`; time uses `EngineHost`.
7. **SC/3rd are separate opcode/format profiles.** FC assumptions must not be baked into the VM core;
   format tables and version-specific behavior are selected by a game profile.

## Consequences

- The viewer preview must migrate to the new VM (M2-01).
- All blocking operations need explicit resume semantics.
- Save/restore and golden traces become possible (M2-10, M2-13).
- Instruction implementations must be audited instead of being silent stubs.
- Some current timeline-oriented APIs (`TotalPassedPart`, duration sums) will be replaced.

## Alternatives considered

- **Keep the timeline worker and add flags.** Rejected: cannot model calls, blocking, or save state.
- **Interpret scripts every frame from scratch.** Rejected: expensive and hard to make deterministic.
- **Use a third-party VM.** Rejected: the bytecode and expression semantics are project-specific and
  must match the original game.

## Open questions

- Exact thread/worker scheduling order and preemption rules.
- Blocking-operation cancellation and skip behavior.
- Which instructions need side-effect commands versus direct state mutation.
- SC/3rd opcode extensions and expression differences.
