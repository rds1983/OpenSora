# SCP expressions

- **Status:** partial decompilation only
- **Loader:** `OpenSora.Common/Scenarios/Expressions/Expression.cs`, `ExpressionType.cs`
- **Tests:** none dedicated yet
- **Related:** [sn-scp.md](sn-scp.md)

## Overview

Scenario instructions can carry expressions (for example `Jc` conditions). Expressions are a byte
stream of expression opcodes terminated by `EXPR_END`. The current implementation decodes operand
shapes but does not evaluate expression semantics.

## Opcodes

The `ExpressionType` enum currently defines:

| Value | Name | Operand decoding in current code |
|---:|---|---|
| 0 | `EXPR_PUSH_LONG` | uint32 |
| 1 | `EXPR_END` | none; ends expression |
| 2 | `EXPR_EQU` | none |
| 3 | `EXPR_NEQ` | none |
| 4 | `EXPR_LSS` | none |
| 5 | `EXPR_GTR` | none |
| 6 | `EXPR_LEQ` | none |
| 7 | `EXPR_GE` | none |
| 8 | `EXPR_EQUZ` | none |
| 9 | `EXPR_NEQUZ_I64` | none |
| 10 | `EXPR_AND` | none |
| 11 | `EXPR_OR` | none |
| 12 | `EXPR_ADD` | none |
| 13 | `EXPR_SUB` | none |
| 14 | `EXPR_NEG` | none |
| 15 | `EXPR_XOR` | none |
| 16 | `EXPR_IMUL` | none |
| 17 | `EXPR_IDIV` | none |
| 18 | `EXPR_IMOD` | none |
| 19 | `EXPR_STUB` | none |
| 20 | `EXPR_IMUL_SAVE` | none |
| 21 | `EXPR_IDIV_SAVE` | none |
| 22 | `EXPR_IMOD_SAVE` | none |
| 23 | `EXPR_ADD_SAVE` | none |
| 24 | `EXPR_SUB_SAVE` | none |
| 25 | `EXPR_AND_SAVE` | none |
| 26 | `EXPR_XOR_SAVE` | none |
| 27 | `EXPR_OR_SAVE` | none |
| 28 | `EXPR_EXEC_OP` | embedded instruction |
| 29 | `EXPR_NOT` | none |
| 30 | `EXPR_TEST_SCENA_FLAGS` | uint16 |
| 31 | `EXPR_GET_RESULT` | uint16 |
| 32 | `EXPR_PUSH_VALUE_INDEX` | byte |
| 33 | `EXPR_GET_CHR_WORK` | uint16 + byte |
| 34 | `EXPR_RAND` | none |
| 35 | `EXPR_23` | byte |

These value assignments and operand shapes are **inferred from the current parser** and need
confirmation against a reference decompiler.

## Semantics

The expression VM is not implemented. Future work (M2-05) must define:

- stack behavior and value types (integer width, booleans, flags);
- comparison and zero-check semantics;
- save variants (`*_SAVE`) and their side effects;
- flag testing (`EXPR_TEST_SCENA_FLAGS`), character work reads, and result access;
- random value range and interaction with the deterministic RNG;
- embedded instruction execution (`EXPR_EXEC_OP`);
- undefined/unknown opcode handling.

All randomness in expression evaluation must use `DeterministicRandom` (ADR 0002), not
`System.Random`.

## Unknown-data handling

The decompiler currently reads any byte as an `ExpressionType` and creates an expression even for
values outside the known enum. This should become an `UnknownDataReporter.ReportOnce` warning plus a
contextual failure or a tracked stub, similar to unknown scenario opcodes.

## Open questions

1. Confirm opcode numbering and operand shapes against FC/SC/3rd scripts.
2. Define the expression VM stack/value model and comparison semantics.
3. Determine which expressions mutate game state or read character work.
4. Define SC/3rd expression extensions.
5. Add synthetic expression fixtures and golden evaluation tests.
