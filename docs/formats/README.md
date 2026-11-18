# Binary format notes

These pages document the on-disk formats used by the Trails in the Sky PC releases. They are working
notes, not a claim of complete reverse engineering.

## Status matrix

| Format | Page | Status |
|---|---|---|
| LB DIR/DAT archives | [dir-dat.md](dir-dat.md) | Implemented reader |
| Falcom block compression | [falcom-compression.md](falcom-compression.md) | Partial; needs fixtures and edge-case tests |
| DDS textures | [dds.md](dds.md) | Partial; uncompressed and several compressed formats |
| CH/CP sprites | [ch-cp.md](ch-cp.md) | Partial; plain images, animation sheets, animation info |
| X2/X3 models | [x2-x3.md](x2-x3.md) | Partial; static frame/mesh data |
| SN/SCP scenarios | [sn-scp.md](sn-scp.md) | FC partial; SC/3rd parsers not implemented |
| SCP expressions | [scp-expressions.md](scp-expressions.md) | Partial; operand decoding only |
| Save files | [save-files.md](save-files.md) | Not implemented; research |
| Audio | [audio.md](audio.md) | Not implemented; observed container types |
| Movies | [movies.md](movies.md) | Not implemented; observed AVI files |
| Effects | [effects.md](effects.md) | Not implemented; observed `_EF`/`_EP` entries |

## Conventions

- All multi-byte integer fields are little-endian.
- Archive entry names are fixed 12-byte ASCII fields. Short names are usually padded with spaces before
  the extension, for example `C_MOUSE ._CH`.
- Archive entries may be stored raw or wrapped in Falcom block compression. Loaders either probe the
  payload signature (DDS, WAV) or know that the type is compressed (CH, CP, SN, X2).
- File names and extension meanings are strongly correlated, but the extension alone is not a formal
  type tag. Treat cross-game differences carefully.
- Anything marked **inferred** comes from current code and sample inspection and needs confirmation
  before it is relied on for save compatibility or modding.

## Observed entry catalog

The following counts and names were observed in a Steam English **FC** installation (12,821 non-empty
archive entries). Counts will differ between games and editions. The meanings marked **unknown** are
not implemented yet.

| Extension | Count | Observed example | Current interpretation |
|---|---:|---|---|
| `_DS` | 2056 | `C_WIPE00._DS` | DDS texture; raw DDS payload |
| `_CH` | 3052 | `C_MOUSE ._CH` | Character/UI sprite bank or animation sheet |
| `_CP` | 1331 | `CH20000P._CP` | CH animation frame info |
| `_SN` | 491 | `T0001   ._SN` | Scenario/SCP script |
| `_X2` | 972 | `BC0100  ._X2` | Map/field model, version 2 |
| `_X3` | 0 | - | Character model, version 3 (not observed in FC sample) |
| `WAV` | 1496 | `ED6SE000.WAV` | RIFF/WAVE sound effect |
| `_DT` | 466 | `T_NAME  ._DT` | Data table; **unknown** |
| `_OP` | 524 | `BC0100  ._OP` | Map/object placement data; **inferred/unknown** |
| `_EF` | 487 | `BIGFIRE ._EF` | Effect definition; **unknown** |
| `_CT` | 444 | `BC0100  ._CT` | Map collision/terrain data; **inferred/unknown** |
| `_HD` | 439 | `BC0100  ._HD` | Map height/geometry data; **inferred/unknown** |
| `_EN` | 373 | `C0100   ._EN` | Map/entity data; **unknown** |
| `_SY` | 186 | `SY00100 ._SY` | Map/system data; **unknown** |
| `_CL` | 154 | `C0100   ._CL` | Map collision data; **inferred/unknown** |
| `_EP` | 142 | `BC0300  ._EP` | Effect placement data; **inferred** |
| `_LM` | 139 | `C0100   ._LM` | Map light/geometry data; **inferred/unknown** |
| `_DA` | 24 | `FONT8   ._DA` | Font/bitmap data; **inferred/unknown** |
| `_MH` | 23 | `BC0500  ._MH` | Map metadata; **unknown** |
| `_VS` | 22 | `CHR     ._VS` | Text/script data with Shift-JIS text; **unknown** |

Loose files observed outside the archives:

- `BGM/*.ogg` - background music.
- `ed6_logo.avi`, `ed6_op.avi` - movies.
- `ed6_win.exe`, `ed6_win_DX9.exe` - game executables used for edition detection.
- `Config.exe`, `Config_DX9.exe` - original configuration tools.

## References

- [EDDecompiler](https://github.com/ZhenjianYang/EDDecompiler) - scenario/SCP tooling and opcode
  research.
- [JuusanKoubou](https://github.com/Ouroboros/JuusanKoubou) - file format tooling.
- [Kiseki-Texture-Tool](https://github.com/Sewer56/Kiseki-Texture-Tool) - texture/CH tooling.
- Current OpenSora loaders and tests are the source of truth for fields marked **implemented**.
- Synthetic fixtures in `OpenSora.Tests/Fixtures` describe the minimum accepted structure for each
  implemented loader.

## Open questions

1. Falcom block compression: exact block framing, method byte semantics, maximum sizes, and malformed
   input behavior.
2. DDS: mipmap/cubemap/volume handling, complete format mask coverage, and color-key rules.
3. CH: complete image-type table, palette/alpha semantics, anchor data, and head/body variants.
4. X2/X3: version differences, material semantics, animation data, and skinning.
5. SN/SCP: SC and 3rd layout/opcode differences; 3rd door scripts.
6. SCP expressions: exact stack semantics, flags, character work, random behavior, and save effects.
7. Map sidecars (`_OP`, `_CT`, `_HD`, `_EN`, `_SY`, `_CL`, `_LM`, `_MH`, `_VS`): chunk layouts and
   runtime roles.
8. Save files: location, naming, header/checksum, slot structure, and FC-to-SC clear data.
9. Audio: loop metadata, voice routing, WAV codec details, and SC/3rd differences.
10. Movies: codec, audio stream layout, and platform/edition differences.
11. Effects: `_EF` chunk structure, `_EP` binding, texture references, timing, and 3D effects.

## Updating these pages

1. Mark whether each statement is **verified**, **inferred**, or **unknown**.
2. Link the loader and test/fixture that proves it.
3. Never paste or commit copyrighted game data; describe structures and use synthetic fixtures.
4. If a format changes between games, document the differences instead of overwriting one page.
