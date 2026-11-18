# Third-party notices

OpenSora is licensed under GPL-3.0. The following third-party packages and native libraries are used
by the project. They retain their own licenses; this file is an attribution index, not a replacement
for the full license texts shipped with each package.

Resolved versions below were taken from `dotnet list package --include-transitive` on 2026-11-18. Update
this file when dependencies change.

## Runtime dependencies

| Package | Resolved version | License | Project / license reference |
|---|---:|---|---|
| MonoGame.Framework.DesktopGL | 3.8.6.1 | MS-PL | https://github.com/MonoGame/MonoGame |
| Myra | 1.6.8 | MIT | https://github.com/rds1983/Myra |
| FontStashSharp.MonoGame | 1.6.2 | Zlib | https://github.com/FontStashSharp/FontStashSharp |
| FontStashSharp.Base | 1.2.9 | Zlib | https://github.com/FontStashSharp/FontStashSharp |
| FontStashSharp.Rasterizers.StbTrueTypeSharp | 1.2.9 | Zlib | https://github.com/FontStashSharp/FontStashSharp |
| XNAssets.MonoGame | 0.8.9 | MIT | https://github.com/rds1983/XNAssets |
| XNAssets.FontStashSharp.MonoGame | 0.8.9 | MIT | https://github.com/rds1983/XNAssets |
| AssetManagementBase | 0.7.3 | MIT | https://github.com/rds1983/AssetManagementBase |
| Cyotek.Drawing.BitmapFont | 2.0.4 | MIT | https://github.com/cyotek/Cyotek.Drawing.BitmapFont |
| NVorbis | 0.10.4 | MIT | https://github.com/NVorbis/NVorbis |
| StbImageSharp | 2.30.16 | Unlicense OR MIT | https://github.com/StbSharp/StbImageSharp |
| StbTrueTypeSharp | 1.26.13 | Unlicense OR MIT | https://github.com/StbSharp/StbTrueTypeSharp |
| System.Memory | 4.5.3 | MIT | https://github.com/dotnet/runtime |
| System.ValueTuple | 4.5.0 | MIT | https://github.com/dotnet/runtime |

## Native components

| Component | Resolved version | License | Notes |
|---|---:|---|---|
| MonoGame.Library.SDL | 2.32.10.2 | zlib (package `LICENSE.txt`) | SDL2 native library for DesktopGL |
| MonoGame.Library.OpenAL | 1.24.3.4 | BSD-3-Clause (package `LICENSE`) | Native OpenAL package; upstream components may carry additional licenses. Verify before release. |

## Test dependencies

| Package | Resolved version | License |
|---|---:|---|
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT |
| xunit | 2.9.3 | Apache-2.0 |
| xunit.runner.visualstudio | 2.8.2 | Apache-2.0 |

## Game content and trademarks

The games, their names, story, characters, audio, video, art, scripts, and other content are **not**
licensed under GPL-3.0 and are not distributed by OpenSora. See [docs/legal.md](docs/legal.md).

## Compliance notes

- Full license texts are included in the corresponding NuGet packages and their upstream projects.
- GPL-3.0 compatibility should be confirmed during the legal review in M8-09.
- Any new dependency requires an update to this file and, if it is large or platform-specific, an ADR
  decision.
- Do not add game assets or extracted content to this repository or to release artifacts.
