# Project direction

OpenBFME aims to recreate BFME2 1.06 skirmish play in a modern, open-source,
moddable engine.

Players provide their own legally acquired copy of BFME2. OpenBFME converts the
required content locally and does not distribute original or converted game
assets.

## Scope

| Goal | Status |
|---|---|
| Men versus Men on Fords of Isen II | In progress |
| Full Men faction on the selected maps | In progress |
| All six BFME2 factions and skirmish systems | In progress |
| Self-hosted multiplayer for up to eight players | Not started |
| Replays, observers, Create-a-Hero, and map tools | Not started |
| Accessibility and modern release tools | Not started |

## Compatibility target

OpenBFME targets BFME2 version 1.06. Gameplay is compared with the original game
where possible; having a file or model available does not mean the feature is
finished.

## Not in scope

- The Good and Evil campaigns
- Campaign maps and scripting
- War of the Ring
- Rise of the Witch-king support during the BFME2 phase
- Ranked services or a mandatory online account

## Long-term technical direction

- Godot for presentation, input, interface, audio, and desktop integration
- A deterministic simulation suitable for replays and multiplayer
- Self-hosted local, listen-server, and dedicated-server play
- Up to eight players
- Separate versioning for gameplay mods and presentation mods
- No retail or converted retail assets in the repository or public downloads

See [PLAN.md](PLAN.md) for the development order and [STATUS.md](STATUS.md) for
current progress.
