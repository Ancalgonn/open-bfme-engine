# Windows VM release acceptance

This is the end-to-end release gate for a clean Windows guest. It must use a
disposable VM or a checkpoint that can be restored. It must not reuse a
developer content pack or the paused reverse-BFME worker fleet.

## Required guest

- Windows 11 or Windows Server 2022 x64
- at least 4 CPU cores, 16 GB RAM, and 80 GB free disk
- working QEMU guest agent or another bounded remote-execution channel
- network access to GitHub, Blender, and the pinned FFmpeg source
- a lawfully installed BFME II 1.06 copy
- RotWK 2.01 only when the optional Angmar test is requested

The GitHub runner must have the labels `self-hosted`, `windows`, `x64`, and
`openbfme-release-vm`. Configure `BFME2_RETAIL_PATH` as a machine-level
environment variable on the guest; do not store the retail path or any retail
payload in GitHub variables, secrets, caches, or artifacts. The runner should
accept only release-tag jobs after the build job has verified the signed tag
and its ancestry on `main`.

Record the VM identity, base snapshot, Windows build, release commit, release
manifest SHA-256, and retail installation identity before testing. Never copy
retail data into CI artifacts or logs.

## Acceptance sequence

1. Restore the clean checkpoint.
2. Download the release ZIPs and `release-manifest.json` from the tested GitHub
   Actions run or release.
3. Verify `SHA256SUMS.txt`.
4. Extract and start the launcher.
5. Import BFME II Men into empty local launcher state.
6. Verify that the selected pack is valid and that no absolute retail path is
   present in its public receipt.
7. Start OpenBFME, reach the menu, open skirmish setup, and begin the supported
   Men-versus-Men Fords of Isen II slice.
8. Close the game, install the next synthetic engine version, verify selection,
   then roll back and verify the prior commit identity.
9. Restore the checkpoint and repeat the BFME II import.
10. Compare the two complete pack trees with
    `tools/release/compare_import_bundles.py`.

The checked-in `tools/release/Invoke-WindowsVmAcceptance.ps1` performs the
package, signature, double-import, full-tree comparison, and exported-game
smoke checks. Snapshot restoration and ephemeral runner registration remain
infrastructure responsibilities so that every tag starts from the approved
clean guest image.

The comparison is whole-tree and byte-exact. It inventories every regular file,
streams SHA-256, rejects links and case-colliding Windows paths, checks required
asset-family counts, and writes a payload-free receipt. A different texture,
model, animation, skeleton, audio file, map, rule, path, size, or byte fails the
gate.

Example:

```powershell
python tools/release/compare_import_bundles.py `
  C:\OpenBFME-A\content-packs\<pack> `
  C:\OpenBFME-B\content-packs\<pack> `
  --game bfme2 `
    --profile men-fords-v0 `
  --release-commit <40-character-commit> `
  --require-family textures=1 `
  --require-family models=1 `
  --require-family animations=1 `
  --receipt C:\OpenBFME-Proof\bfme2-men-repro.json
```

Do not record the retail path, asset names, payload bytes, screenshots containing
private material, or private pack manifests in a public artifact. Preserve only
the sanitized receipt, launcher logs, process exit codes, version identities,
and pass/fail summary.

## Pass condition

The VM gate passes only when the packaged launcher starts on the clean guest,
the importer builds and selects a pack without manual developer dependencies,
the game launches with that pack, update and rollback preserve the correct
commit identities, and the two clean imports have the same canonical digest.
