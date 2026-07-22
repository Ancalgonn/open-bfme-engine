# Development plan

OpenBFME is focused on BFME2 1.06 skirmish play. Campaigns and War of the Ring
are not planned.

| Stage | Status |
|---|---|
| Import BFME2 content locally without redistributing it | Completed |
| Finish Men versus Men on Fords of Isen II | In progress |
| Finish the Men faction across the selected maps | In progress |
| Finish all six factions and the main skirmish systems | In progress |
| Add deterministic multiplayer for up to eight players | Not started |
| Add replays, observers, Create-a-Hero, and map tools | Not started |
| Add accessibility, mod management, and a public installer | Not started |

## Current priorities

1. Complete the new interface.
2. Stabilize the Men/Fords playable slice.
3. Fix gameplay and presentation test failures.
4. Improve the remaining factions and maps.
5. Prepare a developer-friendly public build.

## Technical direction

- Godot handles graphics, input, interface, and audio.
- Python tools convert content from a locally installed copy of BFME2.
- Converted retail content stays private and is never committed.
- The long-term simulation will be deterministic and suitable for self-hosted
  multiplayer.
- Mods and custom content will use versioned formats so incompatible matches can
  be detected before launch.

See [DIRECTION.md](DIRECTION.md) for the project scope and
[STATUS.md](STATUS.md) for current progress.
