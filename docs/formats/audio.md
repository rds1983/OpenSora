# Audio

- **Status:** not implemented
- **Research task:** none separate; covered by M1-09
- **Planned work:** M1-09
- **Code:** none yet

## Observed data

In the sampled Steam FC installation:

- Background music lives outside the archives in `BGM/` as Ogg Vorbis `.ogg` files, for example
  `ED6001.ogg`, `ED6100.ogg`. The sample contained 58 `.ogg` files.
- Sound effects are archive entries named `ED6SExxx.WAV` (1496 in the parsed FC entry list). Payloads
  begin with a standard RIFF/WAVE header (`RIFF....WAVEfmt `) and are stored raw in the archive.
- Additional voice/audio entries were not identified in the sampled entry names.

## Needed implementation

M1-09 must define:

- Ogg Vorbis and WAV streaming/decoding;
- BGM loop points (whether stored in metadata or hard-coded per track);
- sound-effect pooling, channel groups, and volume;
- voice playback and mapping to dialogue lines;
- 3D attenuation for positional effects;
- pause/fade behavior and device-loss recovery;
- deterministic timing relative to the fixed engine update and scenario VM.

## Open questions

1. Where are voice tracks stored and how are they referenced by scenarios?
2. Are BGM loop points encoded in Ogg comments, in game data tables, or hard-coded?
3. Exact WAV format (PCM bit depth/sample rate/channels) across all sound effects.
4. SC/3rd audio differences and additional formats.
5. How does the original mix BGM, SE, and voice volumes?
6. Are there streaming size limits or platform-specific containers?
7. How should audio sync with skippable dialogue and fast-forward?
