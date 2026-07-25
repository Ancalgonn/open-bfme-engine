# Launcher and Windows releases

OpenBFME releases are code-only. They contain the engine, launcher, importer,
and pinned conversion runtime, but never a retail game pack. A player supplies
a locally installed copy of BFME II 1.06. RotWK is a separate future overlay
and is not part of the current release target.

## Player workflow

1. Download `OpenBFME-Launcher-<version>-windows-x64.zip` from a GitHub Release.
2. Extract it to a new folder and run `OpenBFME.Launcher.exe`.
3. Select the local BFME II installation and choose **Import BFME II (Men)**.
4. Wait for tool verification, conversion, pack audit, and local selection to
   complete. Retail files and converted output stay on that PC.
5. Choose **Play OpenBFME**.

The first import downloads hash-pinned Blender, OpenSAGE, and FFmpeg archives.
The launcher includes its own pinned Python runtime. Later imports reuse the
attested tools and cache.

The stable channel checks
`https://github.com/Ancalgonn/open-bfme-engine/releases/latest/download/release-manifest.json`.
The launcher verifies the manifest's detached RSA signature before reading it.
The signed manifest binds the repository, version, channel, full commit,
package name, compressed and expanded byte sizes, SHA-256, and approved HTTPS
download URL. The updater downloads into a new immutable version directory,
verifies every installed file before selection, and retains only the current
and previous verified versions.

## Launcher flags

```text
--channel stable|playtest|nightly
--manifest-url <approved GitHub HTTPS URL>
--install-root <directory>
--no-update
--verify-only
--headless
--import-bfme2 --bfme2-path <directory>
```

Stable has a default update feed. Playtest and nightly builds require an
explicit immutable manifest URL until those channels have dedicated feeds.

## Maintainer release flow

The `windows release` GitHub workflow has two entry points:

- `workflow_dispatch` builds and uploads a temporary Actions artifact without
  publishing a GitHub Release.
- A tag matching `v*` builds, verifies, uploads, and publishes an immutable
  GitHub Release.

The build job:

1. runs engine, launcher, reproducibility-comparator, export-firewall, and raw
   launcher-protocol tests;
2. downloads Godot 4.7 and its templates and verifies fixed SHA-512 digests;
3. stages `game/` outside the checkout while excluding `.private`, generated
   caches, captures, and `game/data/base`;
4. imports and exports with Godot and fails on logged warnings or errors even
   when Godot exits zero;
5. launches the produced executable headlessly as a smoke test;
6. publishes a self-contained Windows launcher with the importer and Python
   runtime;
7. scans both ZIPs for unsafe paths, retail formats, private paths, game packs,
   and agent instructions;
8. creates and signs `release-manifest.json`, writes `SHA256SUMS.txt`, and
   records GitHub build provenance; and
9. grants GitHub write permission only to the separate tag publication job.

For a release tag, a second job must run on an isolated runner labeled
`self-hosted`, `windows`, `x64`, and `openbfme-release-vm`. It verifies the
manifest signature and both ZIPs, runs the packaged launcher twice against a
lawfully installed BFME II copy from empty state, compares the complete packs
byte-for-byte, and smoke-launches the packaged game with the selected pack.
The publication job cannot start unless this VM gate passes.

| Release component | Status |
|---|---|
| Code-only Windows export and smoke test | Completed |
| Launcher updates, self-update, integrity checks, and rollback | Completed |
| Signed manifests and build provenance | Completed |
| Two independent local BFME II imports with identical packs | Completed |
| Dedicated clean Windows acceptance runner | In progress |
| Authenticode signing for the Windows executables | Not started |
| First public tagged release | Not started |

Create a stable release:

```powershell
git tag -s v0.1.0 -m "OpenBFME 0.1.0"
git push origin v0.1.0
```

Use a suffix such as `v0.1.0-playtest.1` for a prerelease. Do not tag a dirty
tree, a commit that lacks the required milestone evidence, or a revision whose
code-only export manifest has not been reviewed.

## Local release validation

```powershell
powershell -ExecutionPolicy Bypass -File tools/release/Test-ReleaseTools.ps1
dotnet run --project launcher/OpenBFME.Launcher.Tests/OpenBFME.Launcher.Tests.csproj -c Release
python -m unittest tools.release.test_compare_import_bundles -v
```

The GitHub workflow is the source of truth for packaging. Local runs are a
preflight and do not authorize publication.

The manifest signature protects the update feed. Authenticode is a separate
Windows publisher-identity requirement and needs a trusted external
code-signing certificate or service; the release should remain a playtest until
that is configured.

## Failure and recovery

- A package hash, size, URL, or archive-path mismatch stops installation before
  the current version changes.
- A failed import does not select an incomplete pack.
- **Roll back** swaps to the previous verified engine version.
- Diagnostics shown by the launcher redact absolute retail paths.

Campaigns and War of the Ring are not supported by this release system.
