# DDS textures

- **Status:** partial implementation
- **Loader:** `OpenSora.Common/DDSReader/DDS.cs`, `DDSImage.cs`, `Decompressor.cs`, `Utils.cs`
- **Consumer:** `OpenSora.Common/ResourceLoader.cs`
- **Fixture:** `OpenSora.Tests/Fixtures/SyntheticDds.cs`
- **Tests:** `OpenSora.Tests/TextureTests.cs`, `OpenSora.Tests/FixtureToolkitTests.cs`

## Overview

`_DS` archive entries are DDS images. In the observed FC sample they are usually stored raw and begin
with the four-byte signature `DDS `. `ResourceLoader.LoadTexture` probes that signature and tries
Falcom block decompression when it is absent.

## Header layout

The loader follows the standard DirectDraw Surface layout:

| Offset | Size | Field |
|---:|---:|---|
| 0 | 4 | Signature `DDS ` |
| 4 | 4 | Header size; must be 124 |
| 8 | 4 | Flags |
| 12 | 4 | Height |
| 16 | 4 | Width |
| 20 | 4 | Pitch or linear size |
| 24 | 4 | Depth |
| 28 | 4 | Mipmap count |
| 32 | 4 | Alpha bit depth |
| 36 | 40 | Reserved (10 x uint32) |
| 76 | 32 | Pixel format |
| 108 | 16 | Caps |
| 124 | 4 | Texture stage |

Pixel format fields at offset 76: size, flags, FourCC, RGB bit count, R/G/B/A masks (eight uint32
values total).

## Data and decoding

- If the `DDSD_LINEARSIZE` flag is set, the loader reads `sizeorpitch` bytes.
- Otherwise it reads `width * rgbaBitCount / 8` bytes per row for `height * depth` rows.
- Raw pixels are converted to `Color` values in R, G, B, A order. For 32-bit A8R8G8B8 data the bytes
  are stored as B, G, R, A.
- The loader currently treats a fully opaque pure red pixel (`255, 0, 0, 255`) as transparent. This is
  an implementation quirk that should be validated against real assets before relying on it.
- Only the base image is uploaded to the GPU. Mipmaps, cubemaps, and volume slices are not modeled in
  the runtime texture path.

### Supported pixel formats

The decoder switch currently covers:

- uncompressed RGBA/RGB and luminance/luminance-alpha;
- DXT1, DXT2, DXT3, DXT4, DXT5;
- ATI1N, ATI2/3Dc, RXGB;
- R16F/G16R16F/A16B16G16R16(F), R32F/G32R32F/A32B32G32R32F.

Not every path has dedicated tests. The synthetic fixture currently covers uncompressed 32-bit RGBA.

## Observed data

In the FC sample, `_DS` entries are raw DDS payloads; the first sampled file was 256x256 with a
standard 124-byte header. Real entries commonly use compressed DXT variants, so full texture coverage
depends on the decoder paths above.

## Testing

- Synthetic 2x2 uncompressed RGBA fixture verifies dimensions and channel order.
- Fixture toolkit writes a DDS to a temp folder and loads it back.
- Real-data viewer smoke tests exercise actual `_DS` assets.

## Open questions

1. Exact mask/normalization behavior for uncommon bit depths and luminance formats.
2. Mipmap, cubemap, and volume texture support.
3. Whether the pure-red transparency rule is intentional or asset-specific.
4. Color-space handling (sRGB vs linear) and alpha premultiplication.
5. Complete DXT/ATI/RXGB coverage with golden tests.
6. SC/3rd texture variants and any non-DDS `_DS` entries.
