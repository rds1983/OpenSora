# CH/CP sprites

- **Status:** partial implementation
- **Loaders:** `OpenSora.Common/ChLoader.cs`, `OpenSora.Common/AnimationLoader.cs`
- **Consumer:** `OpenSora.Common/ResourceLoader.cs`
- **Fixtures:** `OpenSora.Tests/Fixtures/SyntheticCh.cs`
- **Tests:** `OpenSora.Tests/SpriteFixtureTests.cs`, `OpenSora.Tests/FixtureToolkitTests.cs`

## Overview

`_CH` entries are shared image banks. They are used in two related ways:

- **Plain images** (UI portraits, menus, faces, backgrounds): a rectangular pixel array decoded by
  `ChLoader`.
- **Animation sheets**: a set of 16x16 pixel chunks assembled into a 1024x1024 texture, paired with a
  `_CP` frame-info entry and driven by scenario animation instructions.

`_CP` entries describe frames for a CH animation sheet. `ResourceLoader.LoadAnimation` pairs a CH name
with the corresponding CP entry; for observed names, a trailing space is replaced with `P` (for
example `CH20000 ._CH` pairs with `CH20000P._CP`).

## Plain CH images

`ChLoader.LoadImageData` selects an image type using:

1. the archive base name (for example `ED6_DT00`),
2. the CH file-name prefix,
3. the stream length.

The current type table is a heuristic list in `ChLoader.cs`. Fallback type is 768x768 pixels with two
bytes per pixel.

Supported pixel formats:

| Format | Bytes/pixel | Layout |
|---|---:|---|
| `BGRA4444` | 2 | little-endian ushort: B bits 0-3, G 4-7, R 8-11, A 12-15 |
| `BGRA1555` | 2 | little-endian ushort: B bits 0-4, G 5-9, R 10-14, A bit 15 |
| `BGRA8888` | 4 | bytes B, G, R, A |

The decoded color array is uploaded as a texture by `ChLoader.LoadImage`. The current upload call
passes `(Height, Width)` as the texture dimensions; this is inherited behavior and is listed as an open
question for visual verification.

## Animation sheets

CH animation-sheet format (current implementation):

```
uint16 chunkCount
repeat chunkCount:
    16 * 16 * uint16 BGRA4444 pixels   // 512 bytes per chunk
```

Chunks are laid out into a 1024x1024 texture with 64 chunks per row (16x16 pixels each). A chunk index
is therefore `tileY * 64 + tileX`.

CP animation-info format:

```
uint16 frameCount
repeat frameCount:
    16 * 16 * uint16 chunk indices
```

`0xFFFF` marks an empty cell. Each frame is a 16x16 grid of chunk indices; it is not a pixel bitmap.
`AnimationLoader.LoadInfo` decodes this into `ushort?[frame][y, x]`.

The viewer preview uses `Animation.Animate(start, step, deltaSeconds)` to advance frames on the fixed
engine clock.

## Observed data

In the FC sample:

- 3052 `_CH` entries and 1331 `_CP` entries.
- Plain image examples: `C_MOUSE ._CH`, `C_NUMBER._CH`.
- Animation-sheet examples: `CH20000 ._CH`.
- CP examples: `CH20000P._CP`.
- CH/CP payloads are typically Falcom-compressed in the archive.

Real-data smoke tests decoded 5 plain CH images, 3 CP files, and 3 animation sheets successfully.

## Testing

- Synthetic CP info with known cells and `0xFFFF` empties.
- Synthetic single-chunk animation sheet and pixel checks.
- Synthetic default-size plain CH image and BGRA4444 color mapping.
- Fixture toolkit writes CH images/CP info to a temp folder and loads them back.

## Open questions

1. Complete image-type table (names, dimensions, formats) for FC/SC/3rd.
2. Palette/alpha semantics and whether any CH variants use external palettes.
3. Anchor points, origin, and head/body composition rules.
4. Whether all animation sheets use exactly 1024x1024 and 64 chunks per row.
5. Correct texture orientation/dimension order for plain images.
6. SC/3rd differences and animation info variants.
7. Character portrait naming and expression selection.
