# Asset and legal policy

OpenSora is an independent, clean-room-style engine implementation. It is licensed under GPL-3.0 and
does not include or redistribute game data.

This document is a project policy, not legal advice. Contributors are responsible for complying with
the laws and license terms that apply to them.

## Project license

Source code and original project documentation are licensed under **GPL-3.0**; see [LICENSE](../LICENSE).
Third-party dependencies retain their own licenses; see [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).

## No game data in the repository

Never commit, attach to an issue, or publish through this repository:

- game installations, `.dir`/`.dat` archives, or extracted archive contents;
- textures, models, sprites, tilesets, effects, fonts, or other art assets;
- scenario scripts, decompiled scripts, text dumps, or translation files derived from game data;
- audio, music, voice, movies, or screenshots captured from the games;
- save files, clear data, configuration files, registry exports, or Steam userdata;
- game executables, DLLs, key files, or DRM-related files;
- logs or crash reports containing local paths, machine names, or other user data.

The repository-level `.gitignore` blocks common archive, asset, save, audio, movie, and binary
extensions. `RepositoryPolicyTests` scans tracked files and fails CI when it finds game-data-like
extensions, local save paths, or files larger than 5 MB.

## Test data

- Use a legally obtained local installation of the games for manual testing.
- Keep test-game paths in local configuration or environment variables, never in committed files.
- Do not run CI against game data. CI uses synthetic fixtures only.
- Synthetic fixtures live in `OpenSora.Tests/Fixtures` and are generated from code; see
  [testing/fixtures.md](testing/fixtures.md).
- Format documentation may describe structures and observed values, but must not reproduce
  copyrighted content. Avoid long verbatim text, scripts, or asset dumps.

## Clean-room and reverse engineering

- OpenSora is an independent C# implementation. Do not copy original game source code or proprietary
  SDK code into the repository.
- Format research is performed for interoperability: understanding file layouts, opcodes, and runtime
  behavior needed to run legally owned games.
- Public open-source references (EDDecompiler, JuusanKoubou, Kiseki-Texture-Tool, etc.) are credited in
  [README.md](../README.md) and format pages. Their licenses and attribution requirements still apply.
- Do not contribute decompiled proprietary code as project source. Small factual format notes and
  independently written parsers are acceptable.
- Contributors must comply with local law regarding reverse engineering and interoperability.

## Screenshots and media

- New screenshots in documentation should be asset-free or use synthetic fixtures.
- Do not add gameplay footage, music, voice clips, or extracted art to the repository.
- The existing `images/OpenSora.png` predates this policy. Treat it as a legacy exception and replace
  it with an asset-free capture before a 1.0 release.
- README screenshots are not a license to reuse game assets elsewhere.

## Mods

- Mods are data-first and must be distributed separately from OpenSora.
- Do not commit mod payloads that contain copyrighted game data.
- The base game installation is read-only; OpenSora writes only to its own data directories.
- A future plugin/code-loading API requires an explicit security and legal review.

## Trademarks and attribution

*The Legend of Heroes*, *Trails in the Sky*, *Sora no Kiseki*, and related names and assets are
trademarks or copyrighted works of their respective rights holders, including Nihon Falcom and its
licensors. OpenSora is not affiliated with or endorsed by them.

The project name and original code are independent of the games. Do not use game logos or artwork to
promote OpenSora.

## Reporting and contributions

- Report suspected policy violations privately to the maintainers; do not attach the offending game
  data.
- Pull requests must not add game data, extracted assets, or machine-specific paths.
- By contributing, you agree that your original contribution is licensed under GPL-3.0 and that you
  have the right to submit it.
- When in doubt, ask before adding a binary, screenshot, log, save, or large file.

## Enforcement

- `dotnet test` runs `RepositoryPolicyTests` on Windows and Linux CI.
- Reviewers must reject changes that violate this policy even if the automated checks do not catch
  them.
- M8-09 performs a final legal and asset-policy review before release.
