# Architecture decision records

ADRs capture decisions that are expensive to rediscover or reverse later. Accepted ADRs are not
silently rewritten; supersede them with a new ADR and update the index.

| ADR | Title | Status | Task |
|---|---|---|---|
| [0001](0001-engine-host.md) | Engine host and fixed-timestep clock | Accepted | M0-02 |
| [0002](0002-deterministic-input-and-random.md) | Deterministic random numbers and input traces | Accepted | M0-03 |
| [0003](0003-engine-viewer-boundaries.md) | Engine runtime and viewer boundaries | Accepted | M0-08 |
| [0004](0004-monogame-boundaries.md) | MonoGame boundaries | Accepted | M0-08 |
| [0005](0005-scenario-vm.md) | Scenario VM design | Accepted | M0-08 |
| [0006](0006-asset-access.md) | Asset access and virtual filesystem | Accepted | M0-08 |
| [0007](0007-save-format.md) | Save format | Accepted | M0-08 |
| [0008](0008-modding.md) | Modding and loose-file overrides | Accepted | M0-08 |
| [0009](0009-project-dependencies.md) | Project dependency rules | Accepted | M0-08 |

## Conventions

- One decision per ADR.
- Include status, date, task, context, decision, consequences, alternatives, and open questions.
- Link the implementation tasks and enforcement mechanism.
- Mark assumptions as inferred or unknown; do not present guesses as facts.
- When a decision changes, add a new ADR that supersedes the old one and update both statuses.

## Enforcement

- `OpenSora.Tests/ArchitectureTests.cs` checks project references and simulation source boundaries.
- CI runs the tests on Windows and Linux.
- Reviewers reject changes that violate an accepted ADR without a superseding decision.
