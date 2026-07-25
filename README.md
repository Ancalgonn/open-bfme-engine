<p align="center">
  <img src="docs/assets/openbfme-readme-banner.png" alt="An original fantasy battlefield reconstructed as a modern game-engine wireframe" width="100%">
</p>

<h1 align="center">OpenBFME Engine</h1>

<p align="center">
  An experimental open-source Godot RTS project targeting BFME2 skirmish play,<br>
  powered by content converted locally from a game installation you own.
</p>

<p align="center">
  <img alt="Status: experimental alpha" src="https://img.shields.io/badge/status-experimental%20alpha-c58b31">
  <img alt="Godot 4.7" src="https://img.shields.io/badge/Godot-4.7-478cbf?logo=godotengine&logoColor=white">
  <img alt="BFME2 1.06" src="https://img.shields.io/badge/compatibility-BFME2%201.06-40513b">
  <img alt="License: GPL v3" src="https://img.shields.io/badge/license-GPLv3-663399">
</p>

> [!IMPORTANT]
> OpenBFME is an experimental engine project, not a finished game or a download
> of BFME2. It does not distribute EA's game assets. The compatibility workflow
> requires a lawfully acquired BFME2 1.06 installation and converts content
> locally on your computer.

## What is OpenBFME?

OpenBFME is rebuilding the skirmish side of *The Battle for Middle-earth II* in
Godot. The project has three goals:

1. Reproduce BFME2 skirmish behavior and presentation through measured
   comparison with the original game.
2. Replace the aging proprietary runtime with an understandable, deterministic,
   self-hostable modern engine.
3. Give RTS developers and BFME modders a practical base for new factions, maps,
   scenarios, presentation packs, and total conversions.

The importer understands BFME2's source formats; the game runtime loads a
versioned pack generated privately on the user's machine. Proprietary retail
content stays outside Git and outside public releases.

## Where the development tree is today

The current target is a polished Men-versus-Men skirmish on Fords of Isen II.
The codebase contains broader systems and experimental faction paths, but they
are not all at the same quality level. This table uses only three statuses:

- **Completed** means the implementation passes its applicable current tests.
- **In progress** means meaningful code and tests exist, but parity or release
  acceptance is incomplete.
- **Not started** means there is no supported implementation.

| Feature | BFME II | Rise of the Witch-king | OpenBFME status |
|---|---|---|---|
| Local retail discovery and fail-closed archive identity | Required source | Future overlay | Completed for BFME II 1.06 |
| Core skirmish loop | Included | Inherited and expanded | In progress |
| Men-versus-Men on Fords of Isen II | Included | Inherited | In progress; primary playable slice |
| Six BFME II factions | Included | Additional units and balance changes | In progress; coverage is uneven |
| Angmar | Not included | New faction | Not started; outside the current release target |
| Building, production, combat, upgrades, powers, and heroes | Included | Expanded | In progress |
| Skirmish AI | Included | Expanded | In progress |
| LAN/online multiplayer | Included | Inherited | In progress |
| Custom fortresses and walls | Included | Inherited | In progress |
| Create-a-Hero | Included | New Troll class, weapons, and armor | Not started |
| Campaigns | Good and Evil campaigns | Angmar campaign | Not started; outside project scope |
| War of the Ring | Included | Expanded persistence and siege rules | Not started; outside project scope |
| Code-only Windows export | Not applicable | Not applicable | Completed |
| Launcher, signed update manifest, updates, and rollback | Not applicable | Not applicable | Completed |
| Repeatable packaged BFME II import | Required source | Not the current release target | Completed |
| Clean Windows release VM and public release | Not applicable | Not applicable | In progress |

EA's original announcements are the reference for the high-level comparison:
[BFME II introduced custom heroes, fortresses, walls, and War of the Ring](https://ir.ea.com/press-releases/press-release-details/2006/EA-Ships-The-Lord-of-the-Rings-The-Battle-for-Middle-earth-II-and-The-Lord-of-the-Rings-The-Battle-for-Middle-earth-II-Collectors-Edition-Highly-Anticipated-PC-Game-Ships-Nationwide-Today/default.aspx);
[Rise of the Witch-king added Angmar, faction units, a campaign, expanded
Create-a-Hero, and an upgraded War of the Ring](https://ir.ea.com/press-releases/press-release-details/2006/EAs-The-Lord-of-the-Rings-The-Battle-for-Middle-earth-II-The-Rise-of-the-Witch-king-Has-Shipped-for-the-PC/default.aspx).
Project status comes from current code and gates, not those marketing pages.
See [STATUS.md](STATUS.md) for current evidence and blockers.

The packaged launcher has produced two byte-identical BFME II Men/Fords packs
from separate empty local states, and the resulting Windows export starts with
that selected pack. Public release publication remains blocked until the same
test passes on a dedicated clean Windows VM.

## Why this project exists

This began as a joke and an AI benchmark: could frontier models turn the raw
pieces of BFME into something usable in Godot? After a few hours of human
direction and a few days of AI-assisted coding, the project owner recalls having
a surprisingly playable prototype. That personal timeline is part of the
project's origin story, not a reproducible benchmark result.

The larger motivation is the community. BFME modders have spent years doing
remarkable work within the limits of an old proprietary engine. OpenBFME aims to
give those developers a modern starting point without redistributing the
original game.

## How it works

```text
Lawfully owned BFME2 1.06 installation
              |
              v
  Python importer + pinned converters
              |
              v
  Private, local, versioned runtime pack
              |
              +--> deterministic authoritative simulation
              |
              +--> Godot rendering, input, UI, and audio
```

- `importer/` discovers, extracts, converts, validates, and records provenance.
- `game/` contains the Godot client and current playable runtime.
- `engine/` contains the path toward a pure deterministic simulation layer.
- `contracts/` contains machine-readable product and modding policies.
- `.private/` contains local retail inputs and converted output and is never a
  public-release input.

Strict private compatibility paths are designed to fail when required retail
evidence is missing. Focused tests enforce that rule on covered paths; complete
repository-wide fallback auditing remains release work.

## Try the development build

The current workflow is Windows-first and intended for developers. You need a
lawfully acquired BFME2 1.06 installation, Godot 4.7, Python 3.12, and the .NET
SDK selected by `global.json`.

The guided onboarding wizard checks prerequisites, validates your install
fail-closed, converts or verifies the Men content pack, and runs the headless
verification gates:

```bat
python tools\onboard.py
```

Non-interactive equivalent (CI or scripted setup):

```bat
python tools\onboard.py --install "D:\Games\BFME2" --godot "C:\Tools\Godot\Godot_v4.7-stable_win64_console.exe" --yes
```

The manual command path still works:

```bat
set OPENBFME_GODOT=C:\Tools\Godot\Godot_v4.7-stable_win64.exe
run_doctor.bat
run_importer.bat "D:\Games\BFME2"
run_retail_slice.bat
```

Use your actual Godot and BFME2 paths. Read the
[onboarding guide](docs/ONBOARDING.md) for the ten-minute walkthrough and the
[getting-started guide](docs/GETTING_STARTED.md) for the full background before
importing.

## Roadmap

1. Finish the Men-versus-Men Fords of Isen II release slice.
2. Ship the code-only Windows launcher, repeatable local importer, updates, and
   rollback.
3. Expand BFME II skirmish coverage to more Men units, maps, and factions.
4. Harden multiplayer and modding after the local skirmish release is stable.

Campaign material and War of the Ring are not part of this roadmap. The stable
scope and non-goals live in [DIRECTION.md](DIRECTION.md).

## Find your way around

| If you want to... | Start here |
|---|---|
| Understand the project in five minutes | [Documentation hub](docs/README.md) |
| Set up a fresh machine in ten minutes | [Onboarding](docs/ONBOARDING.md) |
| Install and run the developer build | [Getting started](docs/GETTING_STARTED.md) |
| Check current passes and failures | [Status](STATUS.md) |
| Understand the engine boundaries | [Architecture](docs/ARCHITECTURE.md) |
| Learn how retail conversion stays private | [Content pipeline](docs/CONTENT_PIPELINE.md) |
| Understand the parity standard | [BFME2 parity](docs/BFME2_PARITY.md) |
| Read the modding direction | [Modding](docs/MODDING.md) |
| Contribute safely | [Contributing](CONTRIBUTING.md) |
| Understand the use of AI | [AI development](docs/AI_DEVELOPMENT.md) |
| Ask a common question | [FAQ](docs/FAQ.md) |

## Built with AI, judged by evidence

OpenBFME has been built with extensive AI assistance under human direction and
testing. The project owner reports that Fable 5, ChatGPT Sol, and Kimi K3
contributed substantial implementation and review work. The current Git history
does not preserve model-level attribution for individual changes, so those
credits are owner testimony rather than repository-verifiable authorship.

That origin is part of the experiment, not proof that the result is correct.
Claims are accepted only when backed by source evidence, focused tests, runtime
behavior, original-game comparison, and human review. See
[docs/AI_DEVELOPMENT.md](docs/AI_DEVELOPMENT.md).

## Contributing

The repository is being prepared for wider collaboration. Good contributions are
narrow, reproducible, and tied either to observed BFME2 behavior or a documented
modern-engine contract.

Do not submit retail assets, converted content, game packs, original-game
captures, secrets, personal configuration, or agent instruction files. Start
with [CONTRIBUTING.md](CONTRIBUTING.md).

## License and legal notice

The proposed public source is distributed under the GNU General Public License
v3.0; the repository now carries its own [LICENSE](LICENSE). That license applies
to code the project is authorized to license, not to *The Lord of the Rings*,
BFME2, or third-party content. Third-party provenance and notice review remains a
publication gate.

OpenBFME is an unofficial fan project. It is not affiliated with, endorsed by,
or sponsored by Electronic Arts, Middle-earth Enterprises, the Tolkien Estate,
Embracer Group, or their licensors. Related names, trademarks, characters, and
original game assets belong to their respective owners.

Users must supply their own lawfully acquired copy of BFME2. Retail and converted
retail assets must never be committed, uploaded, bundled, or redistributed with
this project.
