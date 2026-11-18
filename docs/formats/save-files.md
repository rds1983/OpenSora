# Save files

- **Status:** not implemented
- **Research task:** R-04
- **Planned work:** M2-10 (VM save/restore), M3-09 (save UI), M6-04 (FC-to-SC import)
- **Code:** none yet

## Known

No save-file reader or writer exists in the current codebase. The viewer only persists a small XML
configuration in the user's Documents folder; that is not a game save.

Trails in the Sky stores clear data and save slots differently from game to game and edition to
edition. Any implementation must be based on inspected files, not assumptions.

## Required state

A complete save needs at least:

- party roster, active party, reserve members, and character stats/levels;
- equipment, quartz, orbment slots/lines, inventory, recipes, books/quests;
- scenario flags, global variables, event/NPC state, map/location, chapter, playtime;
- mira, sepith, casino tokens, and minigame/quest state;
- RNG state, VM worker/instruction state, and pending scene/event state;
- versioned format metadata and integrity checks.

## Research questions

1. Save file names, locations, and directory layout per edition (Steam/GOG/JP).
2. Slot header: magic, version, timestamp, chapter/location, playtime, party summary, thumbnail.
3. Encoding/compression/checksums and integrity rules.
4. Which runtime state is stored and which is reconstructed on load.
5. FC clear-data layout and the SC import bonuses it carries.
6. SC-to-3rd carry-over rules, if any.
7. Steam Cloud behavior and whether files are encrypted/wrapped.
8. Compatibility policy: import original saves read-only, write our own format, or both.

## Approach

1. Implement a versioned OpenSora save format for our own VM/state first (M2-10).
2. Add slot UI and metadata without depending on the original format (M3-09).
3. Study original save samples and add a read-only importer where feasible.
4. Document exact fields and mark verified vs inferred.
5. Never commit original save files; use synthetic fixtures.

## Open questions

- Can original saves be parsed without reverse-engineering a checksum/signature?
- Are clear-data files separate from normal slots?
- How does the original handle saves made in different chapters or with modded data?
- What is the minimum safe set of fields for a faithful FC-to-SC import?
