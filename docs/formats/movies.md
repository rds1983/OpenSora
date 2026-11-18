# Movies

- **Status:** not implemented
- **Research task:** R-01
- **Planned work:** M1-10
- **Code:** `PlayMovie` instruction is a stub

## Observed data

In the sampled Steam FC installation, movies are loose files in the game root:

- `ed6_logo.avi` (~0.6 MB)
- `ed6_op.avi` (~453 MB)

The container is AVI. The video/audio codecs, resolution, frame rate, and audio stream layout have not
been identified.

## Needed implementation

M1-10 must define:

- AVI demuxing and codec support (native decoder or an existing library with acceptable licensing);
- audio/video synchronization;
- skip, pause, and seek behavior;
- fallback/error behavior for unsupported or missing codecs;
- interaction with the `PlayMovie` scenario instruction and input lock.

## Open questions

1. Which video codec(s) and audio codec(s) are used per edition?
2. Are files plain AVI or wrapped/encrypted on some stores?
3. Frame rate, resolution, color space, and audio channel layout.
4. How does the original handle skipping and returning to the scenario?
5. Are there SC/3rd movie files with different codecs or naming?
6. Licensing constraints for any third-party decoder.
