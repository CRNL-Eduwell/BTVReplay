# BTVReplay — Full Code Review (June 2026)

Scope: entire repository at commit `9e1c787` (master). Four parallel deep-dives: data layer,
UI layer, core systems (services/messenger/brain/traces), video/plugins/tools — plus a
project-level health pass.

**The verdict in one line:** the architecture (static services → messenger bus → MonoBehaviour
views, native EEG parsing isolated behind `IEegDataContainer`, toolbar/tool composition) is
sound and worth keeping; the danger is concentrated in persistence plumbing (can silently
destroy patient bases), security hygiene (committed credentials), and per-frame hot paths
(leaks + GC pressure).

---

## 1. Critical — fix before anything else

### 1.1 Hardcoded SMTP credentials + process-wide TLS bypass
`Assets/Scripts/UI/SceneUI/Windows/BugReporter/BugReporterWindow.cs:78` embeds the login and
password of `btvreplayhelp@outlook.fr` in source (pushed to gitlab.com:Chewbaflo/BTVReplay).
Line 82 sets `ServicePointManager.ServerCertificateValidationCallback = ... return true;`
disabling certificate validation **for the whole process**. The mail is sent synchronously on
the UI thread. → Rotate/kill the account, remove the code, replace with GitLab issues API or
just a "copy report to clipboard" flow.

### 1.2 Patient-base migration can destroy data (two launches = total loss)
`Assets/Scripts/Data/Factory/SubjectsFactory.cs:25` passes the original `.txt` path to
`DBFile3.ConvertOldDbFiles`; `DBFile3.cs:81` does `FilePath.Replace(".dbtv", ".dbtv2")` which
no-ops on `.txt` → the v3 JSON **overwrites the legacy .txt**. Next launch parses JSON as v1,
finds no `[----------]` separator, yields zero patients, and re-saves an **empty DB** over
everything. Only the `*BU` backup survives.

### 1.3 Backup logic can truncate the only copy
`Assets/Scripts/Services/SubjectRepository.cs:109-129`: when the main file exists but the
backup doesn't, `File.Create(path).Dispose()` truncates the current DB *before* saving, and
the backup is written with the *new* data — old data never preserved. No atomic write anywhere.
Fix: serialize to `path + ".tmp"` then `File.Replace(tmp, path, backupPath)`.

### 1.4 Committed patient data + hospital internals
`Assets/Config/PatientBase/ben.dbtv2` contains hospital-internal UNC paths (internal IP +
clinical DB layout) and patient pseudonyms; other bases carry old dev-machine paths.
**Status: handled during the June 2026 GitHub migration** — `ben*.dbtv2` and the SMTP password
were scrubbed from the rewritten history; those files are now local-only (git-ignored).
Synthetic fixtures still to be created in Phase 2. The old GitLab repo retains the
pre-rewrite history and should be made private/archived or deleted.

### 1.5 Standalone builds should not even compile
Unguarded `using UnityEditor;` in runtime scripts (CS0246 in player builds), all dead usings:
`UI/Tools/FolderSelector.cs:2`, `UI/SceneUI/Columns/ColumnGUIManager.cs:2`,
`UI/SceneUI/Windows/PatientBaseManager/UserPreferencesGUIManager.cs:2`, `.../Menu/DbSubMenu.cs:2`,
`Tools/Window/WindowLayout.cs:2`, `Tools/Window/WindowGrid.cs:2`, plus in the data layer:
`WorkspaceFile.cs`, `UserPreferencesFile.cs`, `Workspace.cs`, `UserPreferences.cs`,
`DbPreferences.cs`, `WorkspaceFactory.cs`, `Tools/Extensions/EnumExtensions.cs`,
`Services/UserPreferencesService.cs`.

---

## 2. Data layer

**Architecture:** three DB generations versioned by extension — `.txt` (hand-rolled key:value,
`DBFile.cs`) → `.dbtv` (JSON `List<OldSubject>`, `DBFile2.cs`) → `.dbtv2` (JSON `List<Subject>`
with Experiments, `DBFile3.cs`), chained migration at load in `SubjectsFactory.cs:15-41`.
EEG polymorphism via `IEegFileInfo` (Elan/Micromed TRC/BrainVision/EDF) persisted with Json.NET
`TypeNameHandling.Auto`. EEG samples read natively through the `EEGFormat` C++ DLL
(`Data/Files/EEG/File.cs`), copied to managed dictionaries then disposed
(`Data/IEegDataContainer.cs`). Workspaces/preferences = JSON with custom converters.

Key issues (beyond §1.2/1.3):

- **Silent save/load failures everywhere** — `DBFile3.Save` (`DBFile3.cs:60-64`), `DBFile2.cs:59-63`,
  `WorkspaceFile.cs:77-81`, `UserPreferencesFile.cs:62-66` catch-all and log;
  `SubjectRepository.Save:128` then updates `FilePath` as if it succeeded. Loaders
  (`DBFile.cs:51`, `DBFile2.cs:39`, `DBFile3.cs:40`, `BtvFile.cs:65`, `PosFile.cs:58`,
  `WorkspaceFile.cs:50`, `UserPreferencesFile.cs:42`) catch `Exception`, write to
  `Console.WriteLine` (invisible in Unity), substitute an **empty list**, return `-1` that no
  caller checks. Corrupt file → empty DB → next save wipes it.
- **`TypeNameHandling.Auto`** (`DBFile2.cs:34`, `DBFile3.cs:35`, `WorkspaceFile.cs:40`,
  `UserPreferencesFile.cs:37`, `MatchFile.cs:40`) — Json.NET RCE gadget vector + couples saved
  DBs to `Assembly-CSharp` type names (rename a class → every `.dbtv2` fails to load → see above).
- **Culture bugs** — `CsvFile.cs:16,38` writes/parses floats with CurrentCulture (`1,5` on
  fr-CH); `Tools/Extensions/NumberExtensions.cs:9-16` tries CurrentCulture→fr-FR→en-GB→en-US in
  order, so `"1.000"` parses as 1000 on French locales. Everything should be InvariantCulture,
  multi-culture only as legacy-read fallback. Legacy `.btv`/`.txt` from old BrainTV are likely
  Latin-1; `new StreamReader(path)` assumes UTF-8 → mojibake on French comments.
- **JSON Vector converters destroy data** — `Tools/Converters/Vector3Converter.cs:37-43` and
  `Vector2Converter.cs:37-42`: split on `(`,`)`,`,` yields 5 elements so the `== 3` check never
  passes → **ReadJson always returns zero**; indices also off by one. Every workspace round-trip
  zeroes stored vectors. (`ColorConverter.cs` does it right — but indexes `splitedColors[3]`
  *before* its length check at lines 40-45.)
- **Off-by-one** — `PtsFile.cs:56-65`, `PtsFile2.cs:53-62`: guard `>= 3` but reads token index 3.
- **Lost validation** — `ElanFileInfo.cs:53`: `Errors.Concat(notesError).ToList();` result
  discarded (missing `Errors =`). Interface typo `ChecKForErrors` (`IEegFileInfo.cs:12`).
- **Native interop hazards** — two duplicate `DLLCppImportBase` classes (`Tools/` and
  `Data/Files/EEG/`); finalizer + Dispose call `delete_DLL_class()` with no zero-handle/double-free
  guard (should be `SafeHandle`); `File.cs:166-189` never checks native `Create*` for null;
  `Electrode` wraps pointers owned by the `File` — usable after dispose.
- **Heavy I/O in property getters** — `File.cs:36-53` re-marshals all samples on every access;
  `BrainDataContainer.HasAnat` (`BrainDataContainer.cs:9-56`) does `FileInfo.Exists` on UNC
  paths per evaluation and is **serialized into every .dbtv2** (needs `[JsonIgnore]`).
- **Broken contracts** — value-wise `Equals` + `base.GetHashCode()` in `Subject`, `OldSubject`,
  `Experiment`, `BrainDataContainer`, `BtvEvent`; `BtvEvent.cs:43-44` copy ctor NREs when
  Correlation set; `BtvFile.Save` (`BtvFile.cs:111-120`) mutates in-memory events while saving
  and writes hardcoded `"00000"` sample column — data loss on round-trip.
- **Absolute paths only** — DBs store machine-absolute paths (`Subject.cs:59-61`), so no base is
  portable. Store relative to the `.dbtv2` or a `${DATAROOT}` token.

---

## 3. Core systems

**Architecture:** static services holding global mutable state (`EegFileService` — 6 EEG slots +
montage expression evaluation, `EventsService`, `TracesService`, `AnatomicalDataService`,
`CalculationManager` for STFT/FFT/z-score/correlation, `SubjectLoaderService` orchestrating loads).
Typed `Messenger` singleton — `ConcurrentDictionary<(recipient, context), Action<T>>`, dispatch by
LINQ scan. Brain = runtime meshes from `.tri`, electrodes as sphere GameObjects parked at
x=-10000 rendered by a dedicated camera into a RenderTexture. Traces = `LineRenderer`s, full
redraw per tick. **Video clock is the master**: `CustomVideoPlayer.Update` broadcasts the time
every frame; everything redraws from it. Threading via ThreadNinja (2014 asset).

Key issues:

- **Messenger by-design flaws** — key is (recipient, context) with `TryAdd`
  (`Messenger.cs:68-72`): second handler for same context **silently dropped**. `Send<T>`
  LINQ-scans all subscriptions, allocates per call, called every frame (`Messenger.cs:115-135`);
  one throwing handler aborts delivery to the rest.
- **Messenger leaks** — `Trace.cs:76-98`: registers 7 contexts, unregisters 6
  (`MontageMessage` never), and only if `m_initDone`; `EventWithDuration.cs:45` NRE path skips
  unregisters; `MontageManager.cs:17/26` registers `MontageMessage` but unregisters
  `LoaderMessage` (copy-paste); `Site.cs:163-169` unregisters in OnDisable with no OnEnable
  re-register — re-enabled electrodes stop animating.
- **Per-frame allocation/leaks** —
  - `EventWithDuration.cs:292,382-420`: **new `Texture2D` per video tick** per visible TF event,
    per-texel `SetPixel`, old texture never destroyed → unbounded native leak. Bonus:
    `DefineColorMap` (326-380) fills `Color` with 0–255 values where Unity expects 0–1.
  - `BrainWarden.cs:49-67`: new RenderTexture whenever `rectTransform.hasChanged` (every frame
    while dragging), old one only `Release()`d, never destroyed.
  - `BrainWarden.cs:182-251`: per tick, LINQ `Union().ToList()` then `GameObject.Find("Electrodes")`
    + `GetComponentsInChildren<Site>()` + `name.ToUpper()` inside nested electrode loops;
    `Site.cs:107` goes through `.materials[0]` (allocates array copy + instantiates leaked
    materials). Should be cached `Dictionary<string, Site>` + `MaterialPropertyBlock`.
  - `EventsService.cs:132-197` + `GraphEvents.cs:73-168`: four allocating LINQ window queries per
    trace per tick. `TracesDisplayer.cs:130-147`: `Events.FindAll` with closure per IMGUI pass.
- **Threading races** — `EventsManager.cs:380-424`: fire-and-forget ThreadPool writes
  `EventsService.Events[i].Correlation` while the main thread reads each tick (its
  `Ninja.JumpBack` markers are no-ops — started with plain `StartCoroutine`, line 177);
  `EegFileService.cs:52-66` mutates static `Montages[0]` from a worker thread;
  `CalculationManager.cs:15` funnels concurrent TF jobs through one shared field.
- **Invisible failures** — `CalculationManager.cs:71-74,138-141`: user-facing error dialog
  commented out; `LoadingManager.cs:51-53`: `task.Exception` assigned then discarded — a failed
  subject load shows nothing.
- **Sync correctness** — `SetVideoOffset` is an empty stub in both players
  (`UnityVideoPlayer.cs:156`, `GhostVideoPlayer.cs:156`); sync relies on files pre-aligned.
  `EventsService.GetEventId` (`EventsService.cs:106-112`) matches on timestamp only with
  `.First()` — two events at same ms → wrong event deleted.
- **Fragile scene coupling** — `ApplicationState.cs:28-36`: `GameObject.Find("ringSelect")`,
  `GetChild(3).GetChild(0)`; reload via spawning a "ReloadMedia" carrier + scene reload.
- **Dead code** — `EegSignal2.cs`, `AudioSignal2.cs`, `SignalDisp.cs`, `DebugFlorian.cs`,
  `c_LoadBrainAnatomy2`; ~250 unconditional `Debug.Log` (many in hot paths).

---

## 4. UI layer

**Architecture:** pure uGUI, Screen Space–Camera canvas, no TMP/asmdefs. Hand-rolled
window/dock system, virtualized list (vendored from HiBoP), tooltips, menus. Toolbar system
(`ToolbarSelector` → `ToolbarMenu` → 9 `Toolbar` subclasses → `Tool` components) is the clean
part. Two unrelated window systems; most "windows" are bare MonoBehaviours spawned by name
string through `WindowsManager`.

Key issues (beyond §1.1, §1.5):

- **Magic int protocol** — messages carry `TaskToExecute` ints 0–7 interpreted by switches in
  18 files; string routing (`"EEG1"`, window names). `MontageManager.cs:75` does
  `GameObject.Find(message.WindowName)` right after sending the spawn message.
- **Update() polling** — `WorkspaceManager.cs:26-36`, `SubjectDatabaseWindow.cs:59-63` poll
  `InitDone` forever; `ReloadMedia.cs:16-29` polls a bool; `Scene3DUI.cs:36-43`,
  `ColumnGUIManager.cs:48-56` poll `hasChanged` (use `OnRectTransformDimensionsChange`);
  `MainMenu.cs:55-72` raycasts all on every mouse-up (hardcoded layer 23); `Tooltip.cs:52-66`
  runs Update on every widget always.
- **Unguarded delegates** — ~30 Tool classes invoke without null check
  (`VideoOffset.cs:58`, `EegSignalGain.cs`, `ColorPicker.cs:97,107`).
- **Logic bugs** — `VideoOffset.cs:51-58`: parse failure resets input but missing `return` —
  stale value still fired. `WorkspaceManager.cs:229-231`: layout restore NREs on missing parent
  and ignores which layout matched. `ExperimentDataWidget.cs:167-176`: removes from list while
  forward-iterating, selection determined by **comparing button colors**;
  `:255-304`: extension→file-info factory pasted twice.
- **UI freezes** — `VersionWindow.cs:41`: synchronous `WebClient.DownloadString` of GitHub API in
  `Start()`; BugReporter sync SMTP; `TimeUI.cs:29-44` throws FormatException on empty input.
- **Duplication** — two byte-identical `Menu.cs`, ~95% identical `MainMenu.cs` pair,
  `EegSignalGain`/`VideoGain` + `EegSignalOffset`/`VideoOffset` differ only by delegate name.
- **Misc** — `Camera.main` UI math in 5 places (couples to Screen Space–Camera mode);
  `DropWindow.cs` squats `namespace UnityEngine.UI`; magic parent names `"Pannel"`/`"RightPannel"`;
  `SpawFrequencyChoiceWindow` misnomer used for all input dialogs; French/English mix throughout.

**UI Toolkit migration: not recommended.** The app leans on what uGUI does well (RawImage video/
trace compositing, custom drag/dock grid, pointer-heavy widgets). Stay on uGUI, decouple
logic from views instead.

---

## 5. Video, plugins, tools

**Architecture:** `IVideoPlayer` with `UnityVideoPlayer` (default — the
`Replace_Default_VLC_By_UnityPlayer` branch is merged), `GhostVideoPlayer` (Stopwatch fake clock
when no video, clean), and `VLCSharp` (hand-rolled libVLC P/Invoke, **dead code on master** —
nothing instantiates it, yet ~200 MB of VLC binaries ship: Windows VLC 3.0.0 from 2018 with
known CVEs, macOS VLC 3.0.8 x86_64-only — can't even load in the arm64 builds the builder now
produces; Linux never had libVLC). The dead VLC code is also the only user of `-unsafe` in
`csc.rsp` and the only thing needing `System.Drawing.dll`.

Key issues:

- **Re-init stacks components** — `CustomVideoPlayer.cs:134-145`: every `LoadVideo` adds a new
  player component + listeners with no teardown → second load = double-firing buttons +
  orphaned VideoPlayers.
- **Looping contradicts stop logic** — `UnityVideoPlayer.cs:145` sets `isLooping = true` while
  `CustomVideoPlayer.cs:110` stops past the end; silent wrap to t=0 is the worst failure mode
  for EEG/video sync. `IsStopped => !isPrepared` (`UnityVideoPlayer.cs:27`) conflates states.
- **Heuristic seek sync** — `CustomVideoPlayer.cs:87-104` busy-polls clock movement instead of
  `seekCompleted`; `TotalVideoTime` has a self-described "Ugly ass patch"
  (`UnityVideoPlayer.cs:18-21,75-77`). Time-based (ms) not frame-based seeking → scrubbing not
  frame-accurate.
- **If VLC is ever resurrected, don't** — use-after-free in `VLCMemory.cs:408-424` (callback
  unregistration commented out), instance leak in `VLCSharp.Cleanup():506-524`, ANSI-vs-UTF8 MRL
  marshalling + unencoded `file:///` concat (`VLCSharp.cs:34-35,471`), per-frame
  Bitmap→PNG-encode→PNG-decode pipeline with unsynchronized cross-thread fields
  (`VLCSharp.cs:591-606`), output forced 512×512 (`:484`), inverted enum check
  (`VLCMemory.cs:497`), `Height = i_width` copy-paste (`VLCMemory.cs:200`). If exotic codecs
  ever matter, use LibVLCSharp + official VLC Unity plugin, or transcode on import.
- **Platform inconsistencies** — `Plugins/x86_64/MacOS` contains **arm64-only** in-house libs
  (folder name lies; Intel Macs broken); sloppy .meta platform flags; FFTW3 + MSVC140 runtime +
  loose `Newtonsoft.Json.dll`/`System.Drawing.dll` at plugin root.
- **Tools** — `NumberExtensions.cs:9-16` culture cascade (see §2);
  `ChannelContext.cs:26-35` bounds-checks only on cache miss;
  `Tools/DLLCppImportBase.cs:49-80` finalizer calls virtual method, no SafeHandle.
- **Builder** — hardcoded `/Users/florian/Desktop/builds/` (`BTVReplayBuilder.cs:15-17`); dead
  `Builder.cs` with `D:/Users/Florian/...`; `BuildProjectAndZipIt` doesn't zip; build report not
  checked before copying Config.

---

## 6. Project health

- Unity **2021.3.16f1** (EOL since 2024); project originated in Unity 5.5 era.
- **Binary asset serialization** — `_main.unity` (700 KB) and ProjectSettings are binary:
  undiffable, unmergeable. Switch Editor Settings to Force Text.
- Git: `BTVReplay.VC.db` (10 MB VS cache), `UserSettings/`, `Logs/Packages-Update.log` tracked;
  **122 MB `Assets/Config/Sounds/Drum2.wav`**; 221 MB of native plugins in plain git
  (`.git` = 305 MB). No LFS, no asmdefs, no tests, no README.
- `manifest.json` still includes `com.unity.ads` (leftover default) and no TextMeshPro usage
  despite the package.
- All remote branches fully merged into master except `switch_camera_overlay` (1 orphan commit,
  2020). Last activity Nov 2023.
