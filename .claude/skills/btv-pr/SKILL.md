---
name: btv-pr
description: Open a pull request for BTVReplay in this repo's exact house style. Diffs the current feature branch against develop, drafts a title (conventional-commit style) and a body following the CLAUDE.md PR Template (lead paragraph citing the review §/tier, the right subset of sections, bold-led bullets, file.cs:line refs, honest test/verification reporting, NO tool attribution), and opens it with `gh pr create --base develop`. Use whenever the user asks to open/draft/update a PR, "make a PR", "PR this", or after finishing a feature/fix branch.
---

# btv-pr — author a BTVReplay pull request

BTVReplay has no CI gate and a dense, specific PR convention (see **CLAUDE.md → PR Template / PR body style**, which is the source of truth — re-read it if it has changed). This skill mechanizes turning a finished branch into a PR that already matches that convention, so review time goes to the code, not the formatting.

## Hard rules (non-negotiable, from CLAUDE.md)

- **Target `develop`.** Always `--base develop`. `master` is release-only. PRs never target master.
- **No tool attribution.** No "Generated with Claude Code", no Co-Authored-By, in the title or body. (See memory `no-tool-attribution`.)
- **Title = conventional commit:** `type: short description` — `feat:`/`fix:`/`docs:`/`chore:`/`refactor:`/`build:`/`perf:`. Reuse the lead commit's subject when it already captures the change.
- **Traceable:** reference code as `file.cs:line` and cite review sections (`§1.2–1.3`) / tiers / the prior PR (`Follow-up to #35`).

## Steps

1. **Confirm the branch.** `git branch --show-current`. If it's `develop` or `master`, STOP and warn — work should be on a feature branch off develop (see memory `branch-before-editing`). Offer to cut one and move the commits.
2. **Read the change.** `git fetch origin develop` then `git log origin/develop..HEAD --oneline` and `git diff origin/develop...HEAD --stat` (plus full diff as needed). Understand *what changed and the prior broken behaviour* — the body must explain the **why**, not echo the diff.
3. **Classify** the PR so you pick the right sections:
   - **bug** → `## The crash` (root cause first) then `## Fixes`
   - **feature** → `## Design` (approach + decisions), optionally `## Pieces` (new files/components)
   - **refactor** → `## What`
   - small PR (one concern) → **no headings**: just the lead paragraph + a bullet list.
4. **Draft the body** (structure below). Reach for headings only when the change has genuinely distinct parts.
5. **Push & open:** `git push -u origin HEAD` then `gh pr create --base develop --title "…" --body "…"`. Write the body to a temp file and pass `--body-file` to preserve formatting/backticks. Return the PR URL.
6. **Updating an existing PR:** if a PR already exists for the branch (`gh pr view`), edit it with `gh pr edit --body-file …` instead of opening a new one.

## Body structure

- **Lead paragraph, no heading** — one or two sentences: what the change does and where it comes from. Cite the review section/tier or prior PR (`review §2 "culture bugs"`, `Closes the review's §5 …`, `Follow-up to #35`).
- Then only the sections the change warrants, in this order: `## What` · `## Design` · `## Pieces` · `## The crash` → `## Fixes` · `## Collateral` (or inline `Drive-by:`) · `## Tests` · `## Verification` / `## Manual verification suggested`.
- **Every non-trivial PR ends with verification**, led with `In-editor: …` — the exact flows exercised against the `Assets/Config/PatientBase/` fixtures (no CI exists, so this is the only proof it works).
- **Tests section only if tests were actually added** — give the count + what they pin and the full-suite pass total (`8 new edit-mode tests …; Full suite: 36/36 passing`). Don't invent it.
- Optional closing scope note: `Known warts left as-is: …` or what a follow-up PR will land.

### Style

- Dense and precise. Name the old failure mode (`used to …`, `the old implementation …`) **before** the fix.
- Bullets lead with a **bold phrase** naming the change, then the explanation: `- **Rename = replace**: EditSubjectName now swaps in a renamed copy …`.
- Report tests/verification honestly — if a step was manual-only, skipped, or deferred, say so.

## Gold example (bug PR — PR #22, abridged)

```
fix: patient-DB edit bugs (cross-DB copy/move crash, rename desync)

Fixes the patient-DB manager bugs around copying/moving subjects between
databases (deferred from the typed-protocol verification, review §1.2–1.3 area).

## The crash
`SelectableList` keys selection on `Subject` instances, and `Subject.GetHashCode()`
depends on `PatientName`. `EditSubjectName` renamed subjects **in place**, stranding
the dictionary entry under the old hash; the next `Refresh()` threw
`KeyNotFoundException` (SelectableList.cs:276) and broke the whole window.

## Fixes
- **Rename = replace**: `EditSubjectName` now swaps in a renamed copy via
  `SubjectRepository.Update`, which raises `Replace` so lists re-key cleanly.
- **Copy deep-copies**: copy used to insert the *same* `Subject` into both DBs …

## Tests
New `DatabaseServiceTests` (edit mode, 8 tests) … Full suite: **36/36 passing**.

## Manual verification suggested
In-editor with the fixture bases (`Assets/Config/PatientBase/`): rename subject →
copy/move to another DB → switch DBs → rename DB → reopen the manager …

Known warts left as-is: two open databases with identical file names still collide …
```

A feature PR (PR #36) instead leads with the same kind of paragraph (`Follow-up to #35 …`), then `## Design` with bold-led bullets and an `In-editor (tested): …` verification line — no `## The crash`.
