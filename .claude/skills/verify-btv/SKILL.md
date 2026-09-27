---
name: verify-btv
description: Prove BTVReplay works beyond "it compiles". Runs the edit-mode suite headless, builds and launches a player, and checks a release archive the way a user receives it. Use before a PR's verification section, after a fix, and before publishing a release.
---

# Verify BTVReplay

Three checks, from cheapest to closest to the user:

1. **Edit-mode suite, headless.** The tests in `Assets/Scripts/Editor/Tests/` run from the command line, so a PR can quote a real pass count instead of "tested in the Test Runner".
2. **Player build and launch.** Build the macOS player the way CI does and launch it, so a change that only breaks the standalone player (an unguarded `using UnityEditor;`, a missing plugin, a Config copy) shows up before review.
3. **Release archive.** Download the archive attached to a GitHub release and open it as a user would. This is the check that would have caught the 4.3.0 macOS archive refused as damaged.

The in-editor flows against the `Assets/Config/PatientBase/` fixtures stay manual: the project has no play-mode tests, so there is no scripted way to drive the UI yet. Record them as the `In-editor:` line of the PR, per `btv-pr`.

---

## Before you start

- **Close the project in the Unity editor**, or run against a separate checkout (a git worktree). Batch mode refuses a project another editor has open (`Temp/UnityLockfile`).
- **The first run in a fresh checkout imports every asset** into `Library/` before it runs anything. Later runs reuse it. A fresh worktree of `develop` on 2026-09-27 took 23 s end to end on this Mac, import included, for 167 passing tests.
- The editor version is pinned in `ProjectSettings/ProjectVersion.txt`. On this machine:

```sh
U=/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity
P="$(git rev-parse --show-toplevel)"        # or the path of a worktree
RUN="/tmp/verify-btv/$(date -u +%Y%m%dT%H%M%SZ)"; mkdir -p "$RUN"
```

Evidence goes under `/tmp/verify-btv/`, outside the repository.

---

## 1. Edit-mode suite

```sh
"$U" -batchmode -nographics -projectPath "$P" \
  -runTests -testPlatform EditMode \
  -testResults "$RUN/editmode-results.xml" -logFile "$RUN/unity.log"
echo "exit=$?"
```

Do not add `-quit`: with `-runTests` the editor exits by itself when the run ends. Exit code 0 means every test passed; 2 means at least one failed; any other code means the run did not complete (compile error, licence, lock), so read `unity.log`.

Read the totals from the results file:

```sh
grep -m1 -o '<test-run [^>]*>' "$RUN/editmode-results.xml"   # total, passed, failed, skipped
grep -B1 -A6 'result="Failed"' "$RUN/editmode-results.xml" | grep -E 'test-case|message' | head -40
```

Quote the totals in the PR's Tests line (`Full suite: N/N passing`).

---

## 2. Player build and launch (macOS)

The same entry point CI uses (`.github/workflows/build.yml`):

```sh
"$U" -batchmode -quit -nographics -projectPath "$P" \
  -buildTarget OSXUniversal -executeMethod BTVReplayBuilder.Build \
  -buildOutput "$RUN/build" -logFile "$RUN/build.log"
echo "exit=$?"
APP="$(find "$RUN/build" -maxdepth 2 -name '*.app' | head -1)"
/usr/bin/codesign --verify --deep --strict --verbose=2 "$APP"   # must print "valid on disk" and "satisfies its Designated Requirement"

# launch the player you just built, keep its PID, check it is still up after 25 s, then stop that PID
"$APP/Contents/MacOS/BTVReplay" -logFile "$RUN/player.log" & PID=$!
for i in $(seq 1 25); do kill -0 "$PID" 2>/dev/null || break; perl -e 'select(undef,undef,undef,1)'; done
kill -0 "$PID" 2>/dev/null && { echo "alive after 25 s"; kill "$PID"; } || echo "exited early"
```

Proof: the build exits 0 and logs `Build Finished, Result: Success.`, `codesign` verifies, the player is still running after 25 s, and `player.log` shows no exception. Add a screenshot (`screencapture -x "$RUN/player.png"`) when the PR claims something visible.

Build the other platforms only on their own OS, as CI does: the arm64-only native plugins do not cross-build.

---

## 3. Release archive (after CI attaches it)

CI builds on the release's publication and uploads one archive per platform, named after the build folder (`BTVReplay.X.Y.Z.<os>.zip`). Before announcing a release:

```sh
gh release view VX.Y.Z --json assets --jq '.assets[].name'
gh release download VX.Y.Z --pattern '<the macOS asset name>' --dir "$RUN/release"
ditto -x -k "$RUN/release/"*.zip "$RUN/release/unzipped"
APP="$(find "$RUN/release/unzipped" -maxdepth 3 -name '*.app' | head -1)"
/usr/bin/codesign --verify --deep --strict --verbose=2 "$APP"
```

Then open the app as a user would: from Finder, after downloading the archive through a browser, so the quarantine flag is set. An ad-hoc signed build gets macOS's unverified-developer prompt (Privacy & Security, Open Anyway). A "damaged and can't be opened" dialog is a failed release: do not announce it.

Repeat the launch for the Windows and Linux archives on those machines when one is available, and say in the release PR which platforms were checked.

---

## Evidence

Keep `editmode-results.xml`, `unity.log`, `build.log`, `player.log`, the `codesign` output and any screenshots in `$RUN`. A PR's verification quotes the totals and names the folder; the folder itself stays local.

A result is verified, not verified, or inconclusive. A run that did not complete is inconclusive, never a pass.

---

## Cleanup

Quit any player or editor process this run started, by the PID you launched, not by name: the developer may have their own editor open. Delete `$RUN/build` and `$RUN/release/unzipped` when done; keep the logs, results and screenshots.

---

## Gotchas

- Call `/usr/bin/codesign` by its full path. On machines with FSL installed, `/usr/local/fsl/bin/codesign` comes first on the `PATH`, is a different tool, and answers `--sign is required`.
- A batch-mode run that exits at once with a non-zero code and a short log is usually the project lock or the licence, not the tests.
- A new test file only runs once Unity has compiled it. A compile error anywhere in `Assembly-CSharp-Editor` stops the whole suite, and the exit code is not 0 or 2.
- The player build copies `Assets/Config` into the bundle after Unity signs it, which is why the builder re-signs the bundle. Any new post-build copy needs the same re-sign, and step 2's `codesign` line catches a missing one.

---

Built on the method of the create-verification-skill skill of pstack (https://github.com/cursor/plugins/tree/main/pstack), MIT licence, (c) 2026 Lauren Tan.
