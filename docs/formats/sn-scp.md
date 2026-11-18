# SN/SCP scenarios

- **Status:** FC partial; SC/3rd parsers not implemented
- **Loader:** `OpenSora.Common/Scenarios/Scenario.cs`, `ScenarioFunctionInfo.cs`, `Decompiler.cs`,
  `DecompilerContext.cs`
- **Tests:** `OpenSora.Tests/ScenarioTests.cs`, `OpenSora.Tests/Fixtures/SyntheticScenario.cs`
- **Tool:** `OpenSora.Decompiler`

## Overview

`_SN` entries contain scenario scripts (SCP bytecode) plus map metadata, NPC/monster/event tables, and
a function table. In the FC sample the payload is Falcom-compressed and most entries are non-empty.

The current parser handles the **FC layout only**. SC and 3rd use different layouts/opcodes and are
tracked in M2-03/M2-04.

## FC header

The parser reads 100 bytes of header before entry data:

| Offset | Size | Field |
|---:|---:|---|
| 0 | 10 | Map name (ASCII, zero-padded) |
| 10 | 14 | Location (ASCII, zero-padded) |
| 24 | 2 | Map index |
| 26 | 2 | Default BGM |
| 28 | 2 | Flags |
| 30 | 2 | Entry function index |
| 32 | 32 | Eight included-scenario IDs (4 bytes each) |
| 64 | 2 | Reserved |
| 66 | 24 | Six info-table entries: `(uint16 offset, uint16 count)` |
| 90 | 2 | String table offset |
| 92 | 4 | Header end offset |
| 96 | 4 | Function table entry: `(uint16 offset, uint16 count)` |

Entry points are read between the header and the first info-table offset. The loader computes their
count as `(entries[0].offset - 100) / 0x44`; each entry point is 68 bytes. Most entry-point fields are
currently named `Unknown_*`; the final four fields are init/entry scene and function indices.

## Info tables

The six `(offset, count)` entries point to arrays:

| Index | Content | Element size / known fields |
|---:|---|---|
| 0, 1 | Chip info | int32 chip index |
| 2 | NPC info | X/Y/Z, direction, unknown fields, chip/NPC indices, init/talk scene/function indices |
| 3 | Monster info | X/Y/Z, unknown fields, battle index |
| 4 | Event info | X/Y/Z, range, unknown fields |
| 5 | Actor info | trigger X/Y/Z/range, actor X/Y/Z, flags, talk scene/function indices |

The chip/monster/event/actor layouts are only partially understood; fields named `Unknown_*` in code
are intentionally not assigned semantics.

## Function table and instructions

The function table contains absolute `uint16` offsets into the script. Each function is decompiled into
`BaseInstruction[]` using the FC opcode table in `Decompiler/Decompiler.FC.cs`.

Instruction decoding:

- instruction opcode is one byte;
- operands use the format characters in the table (`B`, `H`, `I`, `S`, etc.);
- branch targets are absolute offsets;
- calls, jumps, switches, and queued work items are represented separately;
- unknown opcodes are reported once per source and raise `InvalidDataException` with offset and source.

The decompiler is a static/disassembly view. The execution VM is still a viewer timeline with many
stub instructions; see M2.

## Strings

Talk instructions hold inline SCP strings. Control codes currently known:

| Code | Meaning |
|---:|---|
| `0x01` | Line feed |
| `0x02` | Enter/wait |
| `0x03` | Clear |
| `0x05` | Unknown |
| `0x07` | Color (followed by one byte) |
| `0x09` | Unknown |
| `0x1F` | Item name (followed by uint16) |

`0x00` terminates a string. Text encoding is currently treated as byte/ASCII; Japanese text requires
Shift-JIS handling and is an open item.

## Testing and observed data

- Synthetic FC scenario fixture: header, one function, `Sleep(300)` + `Return`.
- Decompiler test for operand decoding.
- Real FC smoke test decompiled 491 non-empty `_SN` entries with no reported exceptions in M0-01.

## Open questions

1. SC and 3rd scenario layouts, opcode tables, and new instruction semantics.
2. Meaning of entry-point fields and the string table offset.
3. Complete NPC/monster/event/actor/chip field semantics.
4. Function scena/function index pair semantics and cross-file calls.
5. Full SCP expression semantics; see [scp-expressions.md](scp-expressions.md).
6. Text encoding (Shift-JIS), ruby, and voice markers.
7. 3rd door scripts and their entry points.
