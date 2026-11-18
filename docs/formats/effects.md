# Effects

- **Status:** not implemented
- **Research task:** R-02
- **Planned work:** M1-08
- **Code:** `LoadEffect`, `PlayEffect`, and `Play3DEffect` instructions are stubs

## Observed data

In the sampled FC installation:

- 487 `_EF` entries, for example `BIGFIRE ._EF` and `00000   ._EF`.
- 142 `_EP` entries, for example `BC0300  ._EP`.
- Decompressed `_EF` payloads can begin with texture filename strings such as `cr100_00.png` and
  `tx0000.bmp`, followed by binary data.
- Decompressed `_EP` payloads can contain effect filenames such as `mpC03.eff` and `mpfire4.eff`
  alongside numeric fields.
- `_OP` entries are probably object placement, but this is **inferred/unknown** and not confirmed.

The `_EF` data appears to reference external texture files, so effect parsing and asset resolution are
linked. The `.eff` strings in `_EP` suggest a placement or binding layer distinct from `_EF`.

## Needed implementation

M1-08 must define:

- effect chunk structure, timing, looping, and lifetime;
- texture/sprite references and blending modes;
- 2D screen effects (fade, blur, flash, shake);
- 3D effects and their transform/binding rules;
- how `LoadEffect`, `PlayEffect`, and `Play3DEffect` map to `_EF`/`_EP` data;
- effect cancellation and interaction with skipped cutscenes.

## Open questions

1. What are the `_EF` chunk types and their binary layouts?
2. What is the relationship between `_EF`, `_EP`, and `_OP`?
3. How are effect textures resolved and packed?
4. Which effects are 2D screen effects versus 3D world effects?
5. How do effect parameters bind to scenario instruction operands?
6. SC/3rd effect differences.
7. How should effects behave during pause, fast-forward, and skip?
