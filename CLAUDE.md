# BTVReplay (BrainTV Replay) — Claude Code Instructions

Unity desktop app for synchronized replay of intracranial EEG (SEEG) + patient video for
epilepsy monitoring review (CHUV). Loads patient databases, draws EEG traces, renders a 3D
brain (MNI or patient meshes) with electrode activity, computes time-frequency maps and
correlations, all synchronized to a video clock.

- Unity **6.4 (6000.4.10f1)**, C#, uGUI 2.0 (no UI Toolkit, no TMP usage). Upgraded from
  2021.3.16f1 in June 2026.
- Single scene: `Assets/_main.unity`, YAML (Force Text serialization since June 2026).
- No asmdefs. Edit-mode tests live in `Assets/Scripts/Editor/Tests/` (~80 cases, run via
  Unity Test Runner); with no asmdefs they compile into the predefined `Assembly-CSharp-Editor`.
  There is no CI gate and no play-mode coverage.
- Comments and commit messages are a French/English mix; UI strings English.

## Layout

- `Assets/Scripts/Data/` — persistence: subject DBs (`.txt`→`.dbtv`→`.dbtv2` JSON, migration
  chain in `Data/Factory/SubjectsFactory.cs`), EEG file infos (Elan/Micromed/BrainVision/EDF),
  workspaces, events (`.pos`/`.btv`), protocols (`.prov`). EEG samples are read natively via
  the `EEGFormat` C++ library wrapped in `Data/Files/EEG/File.cs`, copied to managed dicts by
  `Data/IEegDataContainer.cs` then disposed.
- `Assets/Scripts/Services/` — `Session.Current` owns mutable state for the loaded patient;
  existing static services are compatibility facades over it (`EegFileService` with 6 EEG
  slots, `EventsService`, `TracesService`, `AnatomicalDataService`, `VideoService`, etc.).
  `CalculationManager` handles FFT/STFT/correlation, `SubjectLoaderService` orchestrates loads,
  and `SubjectRepository` owns DB load/save + `*BU` backup. A patient switch replaces and
  disposes the Session in `SubjectLoaderService` before loading the replacement scene;
  `ApplicationState.ResetAllServices()` then initializes state and publishes UI resets. Runtime
  modules receive that same Session through toolbar initialization, the `BTV3DModule` composition
  root, or `LoaderMessage.PatientSession`. Modules treat Session as an opaque lifetime identity:
  use explicit-session service APIs rather than reading its internal patient-state properties.
  Edit-mode source gates enforce both rules without runtime exceptions.
- `Assets/Scripts/Messenger/` — typed pub/sub singleton. One handler per (recipient,
  MessageContext enum); messages carry typed `TaskToExecute` enum op-codes. Register in
  Awake/Start, Unregister in OnDestroy. Dispatch is registration-order, allocation-free,
  per-handler exception-isolated; duplicate registrations are rejected with a console error.
- `Assets/Scripts/Brain/` — runtime meshes from `.tri`/`.gii`, electrode spheres (`Site`),
  rendered by a dedicated camera at x=-10000 into a RenderTexture shown via `BrainWarden`.
- `Assets/Scripts/Traces/` — LineRenderer-based EEG traces, full redraw on every video tick.
- `Assets/Scripts/VideoPlayer/` — `IVideoPlayer`: `UnityVideoPlayer` (default),
  `GhostVideoPlayer` (fake clock when no video). **The video clock is the master**:
  `CustomVideoPlayer.Update` broadcasts time every frame; all modules redraw from it.
  (`VideoService` additionally drives a system-installed VLC executable for audio
  extraction/recording — an external-tool dependency, not a bundled library.)
- `Assets/Scripts/UI/` — toolbar system (`ToolbarSelector` → `Toolbar` subclasses → `Tool`
  components), windows spawned by name via `Tools/WindowsManager.cs`, dock/drag system in
  `Assets/Scripts/Tools/Window/`. Parts vendored from HiBoP (virtualized list, handlers).
- `Assets/Plugins/` — per-platform natives: in-house `EEGFormat`, `BTVReplayLibraryC++`,
  `AudioFormat`, `Framework` (+ FFTW3/MSVC runtime on Windows). Layout is honest:
  `Windows-x86_64/`, `Linux-x86_64/`, `macOS-arm64/` (no Intel-Mac libs exist), `Managed/`
  (Json.NET). Each native's .meta enables only its own platform + matching editor OS.
- `Assets/Config/` — atlases, MNI meshes, sounds, `.prov` protocols, and test patient bases in
  `PatientBase/` (contain machine-absolute paths; treat as fixtures, never real patient data).
- `Assets/Scripts/Editor/BTVReplayBuilderWindow.cs` — build menu (Win/Linux/macOS-arm64);
  copies `Assets/Config` next to the build. Output paths are hardcoded.

## Gotchas

- **Data safety**: the DB migration chain and backup logic have known data-destroying bugs;
  saves are not atomic; loaders swallow exceptions and substitute empty data. See
  `Docs/code-review-2026-06.md` §1.2–1.3, §2 before touching anything in `Data/` or
  `SubjectRepository`. Never "fix" a load failure by saving over the input file.
- DB JSON uses Json.NET `TypeNameHandling.Auto` — class/namespace renames in
  `Data/EegFileInfo/` **break existing .dbtv2 files** (they embed `Assembly-CSharp` type names).
- Number parsing/writing is culture-sensitive in places (fr locale bugs historically);
  target `CultureInfo.InvariantCulture` for all new persistence code.
- Threading: background work uses async/await — gather inputs on the main thread, compute in
  `Task.Run` returning a result, publish after the `await` (the continuation resumes on the
  Unity main thread). Capture `Session.Current` before starting patient-specific work and
  require `Session.IsCurrent(capturedSession)` before publishing. Never mutate session state
  from inside a `Task.Run` worker; report progress via `IProgress` (see `LoadingManager.Load`).
- Messenger: one handler per (recipient, context) — a duplicate registration is rejected and
  logged as an error; always pair Register/Unregister with the **same** context.
- Many GameObject lookups are by scene-object name string (`GameObject.Find`) — renaming
  scene objects breaks runtime behavior.
- Never add `using UnityEditor;` to runtime scripts without an `#if UNITY_EDITOR` guard —
  it breaks standalone player builds (a whole batch of these was removed in phase 0).
- **Logging**: use `BtvLog.Log(...)` for informational logs (it's `[Conditional]` — compiled
  out of release builds, kept in editor/dev). Keep `Debug.LogWarning/LogError/LogException`
  for things that must always be visible.
- Before deleting a MonoBehaviour script, GUID-check scenes and prefabs and remove its serialized
  components in the same change; edit-mode coverage verifies retained legacy prefabs have no
  missing scripts.

## Git & GitHub rules

- **No Co-Authored-By** in commit messages, and **no "Generated with Claude Code" / tool
  attribution** in commit messages or PR bodies.
- **PR/branch target**: work happens on feature branches off `develop`; PRs target `develop`.
  `master` is the release branch, only updated by merging `develop` (historical flow of this
  repo, kept as-is).
- **Commit style**: `type: short description` (e.g. `feat:`, `fix:`, `docs:`, `chore:`,
  `refactor:`, `build:`, `perf:`).

## PR Template

The `/btv-pr` skill drafts a PR to this spec and opens it against `develop`. This section is the
canonical spec the skill follows — edit it here and the skill re-reads it; keep the two in sync.

Open with a lead paragraph (no heading): one or two sentences saying what the change does
and where it comes from — cite the review section / tier it closes (`review §2 "culture
bugs"`, `Closes the review's §5 …`, `tier E item #1`). Then add only the sections the change
warrants. A small PR (e.g. #30, #32) is just the lead paragraph plus a bullet list; reach for
headings only when the change has genuinely distinct parts.

Common sections, in this order, pick what fits:
- `## What` — summary of the change (refactors).
- `## Design` — the approach and the decisions behind it (features).
- `## Pieces` — the new components / files introduced.
- `## The crash` then `## Fixes` — root cause first, then the fixes (bug PRs).
- `## Collateral` (or an inline `Drive-by:` bullet) — incidental changes ridden along.
- `## Tests` — count + what they pin (`13 new edit-mode tests …`, `Full suite: 83/83 passing`).
- `## Verification` (or `## Manual verification suggested`) — the exact in-editor flows
  exercised, led with `In-editor: …`. Present on every non-trivial PR (the app has no CI gate).
- A closing note for scope: `Known warts left as-is: …`, or what a follow-up PR will land.

### PR body style

- Dense and precise — explain the **why** and the prior broken behaviour, not just the diff.
  Name the old failure mode (`used to …`, `the old implementation …`) before the fix.
- Bullets lead with a bold phrase naming the change, then the explanation:
  `- **Rename = replace**: EditSubjectName now swaps in a renamed copy …`.
- Make it traceable: reference code as `file.cs:line` and cite review sections (`§1.2–1.3`).
- Report tests and verification honestly — counts, pass totals, what each test pins; if a step
  was manual-only, skipped, or deferred to a follow-up, say so plainly.
- Follow the Git & GitHub rules above: no tool attribution, PRs target `develop`.

## Release Template

The `/btv-release` skill runs this whole ritual (version bump → develop→master merge → tag →
notes). This section is the canonical spec the skill follows.

One GitHub release per version: name `BTVReplay X.Y.Z`, tag `VX.Y.Z` (capital `V`). The
develop→master merge that precedes the release uses the subject `Merge develop into master:
VX.Y.Z` — capital `V` too, matching the tag. (Historical note: the 4.1.0/4.2.0 merge subjects
used a lowercase `v`; that was inconsistent with the tags, standardized to capital `V` going
forward.) A release rolls up the PRs since the last one into user-facing themes — it speaks to
operators, not to the code.

- Optional one-line lead stating the release's arc, for a major version (4.0.0: "The
  foundation release of the modernization effort: …"); minor releases skip it.
- Group changes under theme headings chosen to fit the release — e.g. `## Reliability`,
  `## Fixes`, `## Performance`, `## Internal`, `## Platform`, `## Security`, `## Data safety`,
  `## Build`. Not a fixed set; use the few that cover the work.
- Each section is a plain bullet list.

### Release body style

- Outcome-oriented and user-facing: describe the behaviour change and its effect, not the
  mechanism. No bold lead-ins, no `file.cs:line`, no review-section citations (those stay in
  the PRs) — one release bullet rolls up one or more PRs into a single plain sentence.
- Name the prior bad behaviour in plain terms ("used to vanish", "two launches could
  previously destroy the base entirely") so the value of the fix is clear.
- Bold only a key version / engine fact when it matters (`**Unity 6.4 (6000.4.10f1)**`).

## Current modernization effort

Full review with file:line findings: `Docs/code-review-2026-06.md`.
Plan (phased): security triage → repo hygiene → data-safety fixes → Unity LTS upgrade →
perf/architecture cleanup → tests. Keep changes incremental; edit-mode tests now cover parts
of the data/services layer (`Assets/Scripts/Editor/Tests/`) but there's no CI gate and no
play-mode coverage, so also verify in-editor with the test bases in `Assets/Config/PatientBase/`
after each step.
