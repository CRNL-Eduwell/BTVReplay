# CI builds

`build.yml` builds BTVReplay for Windows, Linux and macOS whenever `master` is updated
(i.e. a `develop -> master` release merge) and on manual dispatch. Builds use
[GameCI](https://game.ci) and are uploaded as workflow artifacts (Actions run -> Artifacts).

## One-time setup: Unity license (required)

The builds will fail until a Unity license is available to CI. This project uses a **Personal**
license, activated via an offline file:

1. **Actions** tab -> **Acquire Unity activation file** -> **Run workflow**. When it finishes,
   download the **Manual Activation File** artifact (a `Unity_v6000.x.alf`).
2. Go to <https://license.unity3d.com/manual>, upload the `.alf`, and download the resulting
   **`.ulf`** license file.
3. Repo **Settings -> Secrets and variables -> Actions -> New repository secret**:
   - `UNITY_LICENSE` — paste the **entire contents** of the `.ulf` file.
   - (Only if you use a serial/Plus/Pro license instead: add `UNITY_EMAIL` and `UNITY_PASSWORD`
     and a `UNITY_SERIAL` secret; Personal needs only `UNITY_LICENSE`.)

The `.ulf` is tied to this Unity version; if you upgrade the editor, re-run the activation flow.

## Triggering a build

- Automatic: merge `develop -> master` (the release flow).
- Manual: **Actions -> Build BTVReplay -> Run workflow**.

The first run for each target is slow (no `Library` cache, ~full asset import); later runs reuse
the cached `Library`.

## Known caveats (expect to iterate on the first runs)

- **GameCI editor image**: GameCI must publish an `unityci/editor` image for `6000.4.10f1`. If
  the build fails immediately with an image-not-found error, pin `unityVersion` to the nearest
  available `6000.4.x` image (check the [unityci/editor tags](https://hub.docker.com/r/unityci/editor/tags))
  or wait for the image to be published.
- **macOS / Apple Silicon**: GameCI builds run in Linux containers and produce an **x86_64 (Intel)**
  macOS build with the Mono backend. This project's in-house native plugins (`EEGFormat`,
  `BTVReplayLibraryC++`, `Framework`) are **arm64-only** on macOS, so the CI macOS artifact will
  **not load those plugins on Apple Silicon**. Treat the CI macOS build as provisional — for a
  real arm64 `.app`, build locally (Tools -> Build BTVReplay) or add a dedicated `macos-14`
  runner job. Windows and Linux (what the distribution catalog needs) are the solid targets.
- **Artifact layout**: the macOS `.app` path in the Config-copy / upload steps assumes
  `build/StandaloneOSX/BTVReplay.app`. Adjust if the first run shows a different name.
