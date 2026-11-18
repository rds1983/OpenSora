# Falcom block compression

- **Status:** partial implementation
- **Loader:** `OpenSora.Common/FalcomDecompressor.cs`
- **Tests/fixtures:** none dedicated yet
- **Evidence:** current implementation plus successful full-corpus FC decompression in M0-01

## Overview

Many archive entries are stored in a Falcom-specific compressed block stream. The current
implementation exposes a single `FalcomDecompressor.Decompress(byte[])` API and is used by the
scenario, model, CH/CP, and generic asset paths.

The exact format is only partially documented. The notes below describe what the current
implementation does; they are **inferred from code** unless stated otherwise.

## Block envelope (inferred)

The decompressor processes a sequence of blocks:

1. Read a `uint16` value (`size`).
2. Peek the next byte (`method`).
3. If `method == 0`, decode with the non-zero/LZ-style decoder.
4. Otherwise, decode the next `size` bytes with the zero/repetition decoder.
5. Read a trailing flag byte. `0` terminates the stream; non-zero continues with another block.

Open questions remain about whether `size` includes the method byte, how block boundaries are chosen,
and whether the trailing flag is part of the compressed payload. The current code seeks back one byte
after reading `method`, so the exact accounting is implementation-specific.

## Non-zero decoder (`method == 0`)

This is a flag-driven LZ-style stream:

- A 16-bit flag word is read in groups; a flag byte is consumed initially.
- `0` copies a literal byte.
- `10` performs a short look-back copy with an 8-bit distance.
- `11` performs a long look-back with a 13-bit distance, or handles stream end/repeating-byte runs.
- Run lengths are encoded with variable-length flag bits and optional extra bytes.

The decoder writes into a fixed 64 KiB output buffer per call. The caller concatenates block outputs.

## Zero/repetition decoder (`method != 0`)

This decoder walks control bytes:

- Top bit clear: literal copy or fill; a secondary bit selects a 5-bit or 13-bit length.
- Top bit set: look-back copy; the low bits encode distance and the next control byte extends long
  runs.
- Fill operations repeat a byte for a length.

The implementation includes several unusual details (including writing into the input array in one
path). Treat the current code as the behavioral specification until a formal spec and fixtures exist.

## Error handling

`Decompress` currently catches all exceptions and returns whatever was decoded. This is tolerant, but
it can hide malformed data. Loaders should log failures and unknown data through
`OpenSora.Diagnostics` and treat output as untrusted.

## Testing gaps

There are no synthetic compression fixtures yet. The M0-01 full-corpus smoke test decompressed FC
scenario data with zero reported exceptions, and real DDS/CH/CP/model samples have been decoded, but
there is no byte-exact round-trip test or malformed-input test.

## Open questions

1. Exact block header layout, `size` semantics, and method-byte accounting.
2. Whether all block types can appear in every asset type.
3. Maximum decompressed size and whether multiple blocks are always required above 64 KiB.
4. Behavior for truncated or adversarial streams; current catch-all behavior is not a validation
   strategy.
5. Whether any SC/3rd entries use a different compression variant.
6. An encoder or fixture generator for regression tests.
