# Editor tools — implementation and validation

Completed 26 October 2026 in the running **Bulldozer_Simulator / Unity 6000.3.12f1** Editor.

Open **Tools → Bulldozer → Scene Switcher / Game Data / Mobile Audit**. Component icons are enabled by default; toggle under **Hierarchy → Component Icons**. See `Assets/Editor/BulldozerTools/README.md` for behavior and limitations.

## Implemented

- Ported and improved the Janitor scene switcher, preference reset, save-data management and hierarchy component display.
- Added search, GUID favorites, full scene paths, correct enabled build indices, additive opening, and Play Mode/import guards.
- Replaced permanent hard-coded save-folder deletion with configured subfolder archive/restore. PlayerPrefs reset is separate and confirmed; selective key deletion is available.
- Made hierarchy registration idempotent, bounded/invalidation-based, null-safe and persistent per project. Component icons support inspection, disabled tint, missing-script markers and overflow.
- Added read-only scene/texture mobile audits, JSON export, missing-script selection and hierarchy-path copying.
- Isolated all code in an Editor-only assembly, with shared UI Toolkit styling for new windows.

## Verified

- Initial native Unity compilation: **0 errors, 0 warnings**. Final recompile reports `up_to_date`, no compiler errors/warnings; Editor state confirms `scriptCompilationFailed = false`.
- **23 native isolated checks passed**. Tests covered unsafe paths, archive/restore bytes, overwrite refusal, scene indexing, overlapping/inactive audit roots, missing materials, cancellation, non-triangle topology, source texture size, Read/Write detection and read-only import inspection.
- Repeatedly registering hierarchy icons leaves **exactly one callback**.
- CompilationPipeline's player-assembly list excludes `Bulldozer.EditorTools`.
- Visual inspection: all three windows render correctly; scene filtering finds the real `0_MainMenu` scene; search-field sizing was corrected; component icons appear in the live Hierarchy, including prefab-stage objects.
- Clicked the real **Scan open scenes** control: current scene reported 2 objects, 0 renderers, no missing scripts or findings. This is only the current sparse menu scene, not a finished gameplay audit.
- Opened the real PlayerPrefs confirmation and clicked **Cancel**. No real preference key, save folder or installed device app data was reset.
- Active scene remained `Assets/0_ProjectData/0_Scenes/0_MainMenu.unity`, clean and outside Play Mode. All temporary validation assets/scenes were cleaned up.
- The four Janitor source files still match their recorded pre-port SHA-256 hashes.

## Console and environment observations

The Console retains earlier Unity Build Profile errors and ParticleImage/uGUI/GameObject Inspector stale-target errors observed around script reload. Their stacks do not name the new tools. Refreshing the Inspector was followed by a cursor-based check reporting **no new errors**. Historical errors were not cleared, and these unrelated vendor/Editor issues are not represented as fixed.

One CLI recompile connection was interrupted by domain reload; retry returned `up_to_date`. One quoted multiword console command was rejected; the corrected command succeeded. The installed Unity CLI was updated from 1.0.0-beta.13 to beta.14 for the workflow; Unity Editor and project packages were not upgraded by this task.

Compared with the pre-install tracked-file hashes, `Assets/StreamingAssets/build_info` changed during the live Editor session. It was already modified before this task, so it was preserved rather than reverted. No other pre-existing tracked file changed in that comparison. Existing project modifications and untracked files were retained.

## Remaining manual coverage

Actual destructive PlayerPrefs confirmation was deliberately not accepted. The save-dialog Cancel path on a user-modified scene, extended large-hierarchy performance, full device builds, mobile runtime performance and every project's save-layout convention were not tested. Archive/restore was tested only with isolated dummy files. Build Profile scene-list overrides remain outside this tool's shared-list view. Texture audits flag review candidates rather than changing formats or estimating exact VRAM savings.

This is an editor-tool implementation and functional verification, not a certification of game runtime performance.

## Evidence

Adjacent JSON files contain native validation results, final compile status, Editor/assembly checks, the post-UI console cursor check and source provenance. File hashes in `SHA256SUMS.json` cover the installed tool payload and copied evidence files, excluding the checksum file itself.
