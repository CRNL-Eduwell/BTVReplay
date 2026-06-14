---
name: btv-release
description: Cut a BTVReplay release. Bumps bundleVersion in ProjectSettings.asset, lands the develop→master merge (subject "Merge develop into master: VX.Y.Z"), tags VX.Y.Z (capital V everywhere), and writes operator-facing GitHub release notes per the CLAUDE.md Release Template (theme headings, plain bullets, no file:line / no review-§ citations, NO tool attribution), rolling up the PRs merged since the previous release. Use when the user says "cut a release", "release X.Y.Z", "ship a version", "make the GitHub release", or "bump the version and release".
---

# btv-release — cut a BTVReplay version

Releasing is a fixed multi-step ritual that's easy to fumble (tag casing, merge-subject wording, notes written for the wrong audience). See **CLAUDE.md → Release Template / Release body style** — it is the source of truth; re-read it if it changed. This skill encodes the mechanics + the gotchas observed across 4.0.0 → 4.2.0.

## Hard rules

- **Capital `V` everywhere.** Tag is `VX.Y.Z` (`V4.2.0`) and the develop→master **merge subject is `Merge develop into master: VX.Y.Z`** (also capital V). The published tags (`V4.0.0`–`V4.2.0`) are capital V and immutable, so the merge subject is standardized to match. (Historical note: 4.1.0/4.2.0 merge commits used a lowercase `v` in the subject — that was inconsistent with the tags; do NOT copy it, use capital `V`.)
- **Release name is `BTVReplay X.Y.Z`** (no V).
- Release notes are **operator-facing**: describe behaviour change + effect, not the mechanism. **No** `file.cs:line`, **no** review-section citations (those live in PRs), **no** bold lead-ins on bullets, **no** tool attribution.
- The version lives in `ProjectSettings/ProjectSettings.asset` → `bundleVersion:` (single line). That's the only file the bump touches.

## Steps

1. **Pick the version** X.Y.Z. Confirm with the user if not given.
2. **Bump on develop.** On `develop`, change `bundleVersion: <old>` → `bundleVersion: X.Y.Z` in `ProjectSettings/ProjectSettings.asset`. Commit `chore: bump version to X.Y.Z`. (Historically done via a `chore/bump-version-X.Y.Z` branch → PR to develop, e.g. #29/#9 — use `/btv-pr` if you want the PR; a direct commit on develop is also in the history.) Ensure develop is pushed/up to date.
3. **Gather the changelog material.** List what shipped since the last release:
   `gh pr list --state merged --base develop --json number,title --search "merged:>=<date of last release>"`, or `git log V<previous>..develop --oneline` (the merge commits name the PRs). Group these into a few **user-facing themes**.
4. **Merge develop → master.**
   ```
   git switch master && git pull
   git merge --no-ff develop -m "Merge develop into master: VX.Y.Z"
   git push origin master
   ```
5. **Create the GitHub release** (tag on master):
   ```
   gh release create VX.Y.Z --target master --title "BTVReplay X.Y.Z" --notes-file <notes.md>
   ```
   Write the notes to a temp file and use `--notes-file` to preserve formatting.
6. Report the release URL. Switch back to develop afterward.

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
