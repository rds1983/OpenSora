# Synthetic test fixtures

OpenSora tests must never depend on a game installation. All binary test data is generated from code
and written to a temporary folder at test time.

## Rules

1. **Never copy game data into the repository.** This includes archives, textures, models, scripts,
   saves, audio, and movies, even in truncated form.
2. **Generate fixtures from code or scripts.** The builders live in `OpenSora.Tests/Fixtures`.
3. **Keep fixtures minimal.** Each fixture should contain only the bytes needed to exercise the
   loader and a known assertion.
4. **Use deterministic values.** Avoid random bytes unless the test is specifically about malformed
   input; seed any randomness explicitly.
5. **Write to a temporary directory.** Use `TempDirectory` so test runs do not pollute the working
   tree.
6. **Assert on decoded values, not just "no exception".** A fixture should prove dimensions, counts,
   colors, names, or offsets.
7. **Do not use a fixture to claim full format coverage.** A minimal fixture proves the loader path;
   full-corpus validation is separate (see M1-12).

## Builders

| Format | Builder | Headless loading API |
|---|---|---|
| LB DIR/DAT archive | `SyntheticArchive.Write` | `DirProcessor.BuildEntries` |
| Uncompressed RGBA DDS | `SyntheticDds.CreateUncompressedRgba` | `DDS.LoadImage` |
| CH animation sheet | `SyntheticCh.CreateAnimationImage` | `AnimationLoader.LoadImageData` |
| CP animation info | `SyntheticCh.CreateAnimationInfo` | `AnimationLoader.LoadInfo` |
| CH plain image | `SyntheticCh.CreateImage` | `ChLoader.LoadImageData` |
| X2 model | `SyntheticModel.CreateX2` | `ModelLoader.Load(stream, 2)` |
| X3 model | `SyntheticModel.CreateX3` | `ModelLoader.Load(stream, 3)` |
| FC scenario | `SyntheticScenario.CreateFc` | `Scenario.FromFCStream` |

SC and 3rd scenario fixtures are added with their respective parsers in M2-03 and M2-04. The current
engine cannot parse those script formats, so generating them now would not produce a loadable fixture.

## Example

```csharp
using OpenSora.Tests.Fixtures;

using (var temp = new TempDirectory())
{
    var data = Encoding.ASCII.GetBytes("HELLO");
    var datPath = SyntheticArchive.Write(temp.Path, "ED6_TEST", ("TEST0001._DS", data));

    var entries = DirProcessor.BuildEntries(temp.Path);

    var pair = Assert.Single(entries);
    var entry = Assert.Single(pair.Value);
    Assert.Equal("TEST0001._DS", entry.Name);
    Assert.Equal(data.Length, entry.CompressedSize);
}
```

Tests that need a `GraphicsDevice` should test the headless data-decoding API instead, for example
`ChLoader.LoadImageData` or `AnimationLoader.LoadImageData`. GPU upload is covered by manual viewer
smoke tests and future visual regression work.

## Adding a new fixture

1. Add a builder to `OpenSora.Tests/Fixtures`.
2. Document the layout with comments and constants.
3. Add a test to `OpenSora.Tests` that creates the fixture in `TempDirectory`.
4. Load it through the production API and assert on decoded values.
5. If a headless decoder does not exist, extract one from the GPU upload path rather than weakening
   the test.
6. Add the format to the table above.

## Related work

- M0-01: test infrastructure.
- M0-04: this toolkit.
- M1-03/M1-04: complete texture and sprite coverage.
- M1-05: complete model coverage.
- M1-12: full-corpus validation against real installations.
- M2-03/M2-04: SC and 3rd scenario fixtures.
