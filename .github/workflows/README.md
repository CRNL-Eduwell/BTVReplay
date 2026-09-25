# CI builds

`build.yml` builds BTVReplay for **Linux, Windows and macOS** whenever a **GitHub Release is
published** (the `develop -> master` version flow ends by publishing `VX.Y.Z`) and on manual
dispatch. Builds use a **free Unity Personal licence** — no Pro/Plus subscription required. On a
release, each platform's build is zipped and **attached to that release** as an asset
(`BTVReplay.X.Y.Z.<os>.zip`); every run also uploads the same archive as a workflow artifact
(Actions run -> Artifacts), which is what a manual `workflow_dispatch` run produces.

Each platform builds on its **own native runner** (`ubuntu-latest`, `windows-latest`,
`macos-latest`). That avoids unsupported cross-platform IL2CPP compilation and — because GitHub's
macOS runners are Apple Silicon — produces a genuine **arm64 `.app`** whose arm64-only native
plugins (`EEGFormat`, `BTVReplayLibraryC++`, `Framework`) actually load. The build itself reuses
`BTVReplayBuilder.Build` (the same code as **Tools -> Build BTVReplay** in the editor), so CI
artifacts match local builds, including the `Assets/Config` and per-platform plugin copy.

## One-time setup: Unity credentials (required)

Builds will fail until a Unity account is available to CI. We activate a **Personal** licence
directly from account credentials (the old manual `.alf -> .ulf` web flow no longer issues
Personal licences). Add two repository secrets:

**Settings -> Secrets and variables -> Actions -> New repository secret**

| Secret | Value |
|---|---|
| `UNITY_USERNAME` | the email of a Unity account (id.unity.com) with a Personal licence |
| `UNITY_PASSWORD` | that account's password |

No serial is needed for Personal. The account currently used is a personal Unity ID; if it is
ever shared or rotated, prefer a dedicated CI-only Unity account (a Personal licence allows only
a couple of simultaneous activations, so a separate account avoids knocking out a local editor).

> **Credential hygiene:** these are real account credentials. They live only in GitHub Actions
> encrypted secrets — never commit them, never echo them in a step, and rotate the password if a
> log ever exposes them.

## Triggering a build

- **Automatic:** publish a GitHub Release (`VX.Y.Z`) — the end of the `/btv-release` flow. The
  builds attach to that release as assets when they finish. (A release with no assets for ~20 min
  is normal: the builds are still running.)
- **Manual:** **Actions -> Build BTVReplay -> Run workflow**. Left empty, it produces workflow
  artifacts only. Given `release_tag` (e.g. `V4.3.0`), it checks out that tag, builds it and
  attaches the archives to that existing release (replacing any with the same name): use it when a
  release build failed and has been fixed. From the CLI:
  `gh workflow run build.yml --ref develop -f release_tag=V4.3.0`.

The first run per target is slow (no `Library` cache + a full Unity editor install); later runs
reuse the cached `Library` and are much faster.

## Cost note (private repo)

GitHub bills runner minutes by OS multiplier on private repos: **Linux 1x, Windows 2x, macOS
10x**. A Unity build is minute-heavy, so the macOS job is the real cost driver. Keeping the
trigger to **release merges + manual dispatch** (rather than every push) keeps this well within
the monthly free-minutes tier for a normal release cadence. If macOS minutes become a concern,
drop the `macos-latest` matrix entry and build the `.app` locally via Tools -> Build BTVReplay.

## Known caveats (expect to iterate on the first runs)

- **Editor version**: `unity-version` in `build.yml` is pinned to `6000.4.10f1` and must match
  `ProjectSettings`. Re-pin it on an editor upgrade. `RageAgainstThePixel/unity-setup` must be
  able to resolve that version via Unity Hub.
- **First arm64 macOS build**: arm64 Unity builds occasionally surface native-plugin / signing
  quirks. Treat the first macOS run as provisional and verify the `.app` launches and loads the
  EEG plugins before trusting the artifact.
- **Linux Hub pin**: the Linux entry pins Unity Hub `3.19.5` (`hubVersion`). Hub 3.20+ installs to
  `/usr/lib/unityhub`, and `unity-setup` v2.6.0 still looks only in `/opt/unityhub`, so an unpinned
  Linux job fails in "Install Unity" with `ENOENT ... /opt/unityhub/unityhub`. Drop the pin once
  `unity-setup` bundles `unity-cli` 3.0.2 or later.
- **Editor path**: the build step invokes `"$UNITY_EDITOR_PATH"` (exported by `unity-setup`). If
  the first run can't find the editor, check that step's output and adjust.
- **Artifact layout**: `BTVReplayBuilder` writes `build/BTVReplay.<version>.<os>/`; the whole
  `build/` folder is uploaded. Adjust the upload path if the layout changes.
