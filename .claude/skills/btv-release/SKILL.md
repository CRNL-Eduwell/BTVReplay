---
name: btv-release
description: Cut a BTVReplay release. Bumps bundleVersion in ProjectSettings.asset, lands the develop→master merge (subject "Merge develop into master: VX.Y.Z"), tags VX.Y.Z (capital V everywhere), and writes operator-facing GitHub release notes per the CLAUDE.md Release Template (theme headings, plain bullets, no file:line / no review-§ citations, NO tool attribution), rolling up the PRs merged since the previous release. Use when the user says "cut a release", "release X.Y.Z", "ship a version", "make the GitHub release", or "bump the version and release".
---

# btv-release — cut a BTVReplay version

Releasing is a fixed multi-step ritual that's easy to fumble (tag casing, merge-subject wording, notes written for the wrong audience). See **CLAUDE.md → Release Template / Release body style** — it is the source of truth; re-read it if it changed. This skill encodes the mechanics + the gotchas observed across 4.0.0 → 4.4.0.

## Hard rules

- **Capital `V` everywhere.** Tag is `VX.Y.Z` (`V4.2.0`) and the develop→master **merge subject is `Merge develop into master: VX.Y.Z`** (also capital V). The published tags (`V4.0.0`–`V4.2.0`) are capital V and immutable, so the merge subject is standardized to match. (Historical note: 4.1.0/4.2.0 merge commits used a lowercase `v` in the subject — that was inconsistent with the tags; do NOT copy it, use capital `V`.)
- **Release name is `BTVReplay X.Y.Z`** (no V).
- Release notes are **operator-facing**: describe behaviour change + effect, not the mechanism. **No** `file.cs:line`, **no** review-section citations (those live in PRs), **no** bold lead-ins on bullets, **no** tool attribution.
- The version lives in `ProjectSettings/ProjectSettings.asset` → `bundleVersion:` (single line). That's the only file the bump touches.

## Steps

1. **Pick the version** X.Y.Z. Confirm with the user if not given.
2. **Bump through a PR to develop.** `develop` and `master` only take PRs (org ruleset, PR + one review; the admin bypass is PR-only since 2026-10-02, so a direct push is rejected). On a branch off develop, change `bundleVersion: <old>` → `bundleVersion: X.Y.Z` in `ProjectSettings/ProjectSettings.asset` and commit `chore: bump version to X.Y.Z`. Ride it on the last feature PR of the release if one is open (4.4.0 did, #71), otherwise open a `chore/bump-version-X.Y.Z` PR with `/btv-pr`. The user merges: `gh pr merge <n> --merge --admin --delete-branch` (the auto-mode classifier blocks Claude from admin-merging). Pull develop afterwards.
3. **Gather the changelog material.** List what shipped since the last release:
   `gh pr list --state merged --base develop --json number,title --search "merged:>=<date of last release>"`, or `git log V<previous>..develop --oneline` (the merge commits name the PRs). Group these into a few **user-facing themes**.
4. **Merge develop → master through a release PR.** Open it from develop itself, with an empty body (the notes go on the GitHub release, not here):
   ```
   gh pr create --base master --head develop --title "Merge develop into master: VX.Y.Z" --body ""
   ```
   The user merges it with the subject set to the same words and **no** `--delete-branch` (the head is develop):
   ```
   gh pr merge <n> --merge --admin --subject "Merge develop into master: VX.Y.Z" --body ""
   ```
   Then `git fetch origin` and check `git log -1 --format=%s origin/master` reads `Merge develop into master: VX.Y.Z`.
5. **Create the GitHub release** (tag on master):
   ```
   gh release create VX.Y.Z --target master --title "BTVReplay X.Y.Z" --notes-file <notes.md>
   ```
   Write the notes to a temp file and use `--notes-file` to preserve formatting.
6. **Check the archives before announcing.** Publishing the release starts the CI build that attaches one archive per platform. When they are attached, run step 3 of the `verify-btv` skill: download the macOS archive, verify its signature, and open it from Finder as a user would. A "damaged and can't be opened" dialog means the release is broken; fix it before announcing (this is what happened to 4.3.0).
7. Report the release URL and which platform archives were opened. Switch back to develop afterward.

## Release notes shape

- Optional **one-line lead** (no heading) only for a **major** version stating the release's arc (4.0.0 did this; minor releases skip it).
- **Theme headings** chosen to fit the work — pick the few that cover it: `## Reliability`, `## Fixes`, `## Performance`, `## Internal`, `## Platform`, `## Security`, `## Data safety`, `## Build`, or a feature-named one like `## Portable patient bases`. Not a fixed set.
- Each section is a **plain bullet list**. One bullet rolls up one or more PRs into a single plain sentence.
- **Name the prior bad behaviour in plain terms** ("used to vanish", "two launches could previously destroy the base entirely") so the fix's value is obvious.
- Bold only a key version/engine fact when it matters (`**Unity 6.4 (6000.4.10f1)**`).

## Gold example (4.2.0, abridged)

```
## Portable patient bases
- Patient-base paths can be stored as machine-independent tokens (`${NAME}/...`)
  that resolve against named roots configured per machine, so the same base can be
  shared across machines without rewriting every path. Legacy absolute paths keep
  working and become portable as bases are re-saved.

## Fixes
- Numbers in patient files and in user input parse the same regardless of the
  machine's locale — a value no longer reads differently on a French machine than
  on an English one.
- Video seeking is frame-accurate: scrubbing lands on the exact frame, and releasing
  the scrollbar where the video already sits no longer hangs on a buffering icon.

## Build
- Native plugin folders are reorganised honestly per platform — `Windows-x86_64`,
  `Linux-x86_64`, `macOS-arm64` — instead of a misleading shared `x86_64` grouping.

## Internal
- Around 20 new edit-mode tests cover the path-token grammar, locale-independent
  number parsing, and the anatomy-availability cache.
```

For a major version, prefix a single lead line, e.g. 4.0.0: *"The foundation release of the modernization effort: …"*.
