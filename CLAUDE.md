# BTVReplay (BrainTV Replay)

Unity desktop app for synchronized replay of intracranial EEG (SEEG) + patient video for
epilepsy monitoring review (CHUV). Loads patient databases, draws EEG traces, renders a 3D
brain (MNI or patient meshes) with electrode activity, computes time-frequency maps and
correlations, all synchronized to a video clock.

- Unity **6.4 (6000.4.10f1)**, C#, uGUI 2.0 (no UI Toolkit, no TMP usage). Upgraded from
  2021.3.16f1 in June 2026.
- Single scene: `Assets/_main.unity`, YAML (Force Text serialization since June 2026).
- No asmdefs, no tests.
- Comments and commit messages are a French/English mix; UI strings English.

## Layout

- `Assets/Scripts/Data/` — persistence: subject DBs (`.txt`→`.dbtv`→`.dbtv2` JSON, migration
  chain in `Data/Factory/SubjectsFactory.cs`), EEG file infos (Elan/Micromed/BrainVision/EDF),
  workspaces, events (`.pos`/`.btv`), protocols (`.prov`). EEG samples are read natively via
  the `EEGFormat` C++ library wrapped in `Data/Files/EEG/File.cs`, copied to managed dicts by
  `Data/IEegDataContainer.cs` then disposed.
- `Assets/Scripts/Services/` — static classes with global state: `EegFileService` (6 EEG
  slots, montage generation), `EventsService`, `TracesService`, `AnatomicalDataService`,
  `CalculationManager` (FFT/STFT/correlation), `SubjectLoaderService` (load orchestrator),
  `SubjectRepository` (DB load/save + `*BU` backup). Reset via `ApplicationState.ResetAllServices()`.
- `Assets/Scripts/Messenger/` — typed pub/sub singleton. Key = (recipient, MessageContext
  enum); messages carry `TaskToExecute` int op-codes. Register in Awake/Start, Unregister in
  OnDestroy (several known leaks — see review).
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
  `AudioFormat`, `Framework` + FFTW3. NOTE: `x86_64/MacOS` actually contains **arm64** in-house
  libs (folder name is wrong).
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
- Threading: ThreadNinja background coroutines mutate static service state off the main
  thread in several places; don't add new cross-thread mutation, marshal results back instead.
- Messenger: one handler per (recipient, context) — duplicates are silently dropped; always
  pair Register/Unregister with the **same** context.
- Many GameObject lookups are by scene-object name string (`GameObject.Find`) — renaming
  scene objects breaks runtime behavior.
- Never add `using UnityEditor;` to runtime scripts without an `#if UNITY_EDITOR` guard —
  it breaks standalone player builds (a whole batch of these was removed in phase 0).

## Git & GitHub rules

- **No Co-Authored-By** in commit messages, and **no "Generated with Claude Code" / tool
  attribution** in commit messages or PR bodies.
- **PR/branch target**: work happens on feature branches off `develop`; PRs target `develop`.
  `master` is the release branch, only updated by merging `develop` (historical flow of this
  repo, kept as-is).
- **Commit style**: `type: short description` (e.g. `feat:`, `fix:`, `docs:`, `chore:`,
  `refactor:`, `build:`, `perf:`).

## Current modernization effort

Full review with file:line findings: `Docs/code-review-2026-06.md`.
Plan (phased): security triage → repo hygiene → data-safety fixes → Unity LTS upgrade →
perf/architecture cleanup → tests. Keep changes incremental; the app has no test coverage,
so verify in-editor with the test bases in `Assets/Config/PatientBase/` after each step.
