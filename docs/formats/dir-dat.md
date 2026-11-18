# DIR/DAT archives

- **Status:** implemented reader
- **Loader:** `OpenSora.Common/Dir/DirProcessor.cs`, `OpenSora.Common/Dir/DirEntry.cs`
- **Fixture:** `OpenSora.Tests/Fixtures/SyntheticArchive.cs`
- **Tests:** `OpenSora.Tests/DirProcessorTests.cs`

## Overview

Games in the trilogy store most resources in pairs of files:

- `ED6_DTxx.dir` - directory with 36-byte entries.
- `ED6_DTxx.dat` - concatenated payload data.

The directory is read entirely into memory; data is loaded lazily by offset. The `.dat` file has no
header observed by the loader. Entry offsets are absolute file offsets into the `.dat`.

## `.dir` header

| Offset | Size | Type | Meaning |
|---:|---:|---|---|
| 0 | 6 | ASCII | Signature `LB DIR` |
| 6 | 2 | bytes | Padding; value not validated |
| 8 | 4 | int32 | Entry count |
| 12 | 4 | int32 | Reserved; expected zero |
| 16 | 36 * count | entries | Directory entries |

The loader validates the signature and the reserved zero, then reads exactly `count` entries.

## Entry layout

Each entry is 36 bytes:

| Offset | Size | Type | Meaning |
|---:|---:|---|---|
| 0 | 12 | ASCII | Name; space-padded before the extension, e.g. `C_MOUSE ._CH` |
| 12 | 4 | int32 | Unknown timestamp/id (`Timestamp2` in code) |
| 16 | 4 | int32 | Compressed size in bytes |
| 20 | 4 | int32 | Decompressed size in bytes |
| 24 | 4 | int32 | Unused/unknown |
| 28 | 4 | int32 | Unknown timestamp |
| 32 | 4 | int32 | Absolute offset into the matching `.dat` |

Entries with `CompressedSize == 0` are skipped by the loader. They appear to be unused or broken
slots. For raw payloads, compressed and decompressed sizes are equal.

The name field is not stored as a C string in observed data. The loader decodes all 12 bytes and does
not trim padding itself; callers rely on names ending with the expected extension.

## Payload encoding

An entry payload is either raw or wrapped in Falcom block compression. Observed behavior:

- `_DS` and `WAV` payloads often begin with their own magic (`DDS `, `RIFF`) and are stored raw.
- `_SN`, `_X2`, `_CH`, and `_CP` payloads in the FC sample are typically compressed.
- `ResourceLoader.LoadTexture` probes the DDS signature before decompressing.
- Other loaders call `FalcomDecompressor.Decompress` unconditionally.

See [falcom-compression.md](falcom-compression.md).

## Testing

`SyntheticArchive.Write` creates a matching `.dir`/`.dat` pair from generated entries. Tests verify:

- header and entry parsing;
- offsets and sizes;
- zero-size entry skipping;
- that the `.dat` bytes match the generated payload.

## Open questions

1. Meaning of the two timestamp/id fields and the unused field.
2. Whether names are always ASCII or can contain Shift-JIS bytes in Japanese releases.
3. Whether entries are ever aligned inside `.dat` or share payloads.
4. Exact behavior for duplicate names and zero-length-but-present entries.
5. SC/3rd archive pair naming and any header changes.
6. Compression method selection per entry type and per edition.
