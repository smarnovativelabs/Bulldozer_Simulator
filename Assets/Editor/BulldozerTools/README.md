# Bulldozer editor tools

Open **Tools → Bulldozer**. All code is in an Editor-only assembly and contributes no player runtime callbacks or components.

## Tools

| Menu | What it does |
|---|---|
| Scene Switcher | Search the shared build scene list, all project scenes or favorites. Shows full paths, disabled entries and correct enabled-scene indices. Open normally or additively; ping an asset. Normal switches honor Unity's save/cancel prompt. |
| Game Data | Delete one exact PlayerPrefs key, or configure the actual save subfolder and archive/restore it. Reveals data and backup folders. |
| Reset PlayerPrefs... | Confirm before deleting all PlayerPrefs for the current Editor game identity; explicitly flushes the store. Never runs automatically. |
| Hierarchy → Component Icons | Persistent project-specific toggle. Shows up to four component icons with tooltips, dimmed disabled components and overflow menu. Click to inspect the component. Missing scripts show `!`. Narrow rows hide icons rather than overlap the object's label. |
| Mobile Audit | Scan open scenes or selected hierarchy subtrees, including inactive objects. Review missing scripts/materials, geometry, skinning, lights, cameras, input systems, physics and particles. Texture audit uses selected Project assets/folders and dependencies. Select findings or export JSON. |
| Selection → Copy Hierarchy Path | Copies the selected GameObject's scene/name path. |
| Validation → Run Editor Tool Checks | Runs isolated scene/file/texture fixtures; never resets actual preferences or saves. |

Legacy **Tools → Scene Switcher**, **Tools → Delete Prefs**, and **Tools → Clear Game Data** remain available. The last opens the new data window, not a destructive Janitor-specific operation.

## Data behavior

PlayerPrefs deletion affects the Editor's game identity, not an installed iPad/Android app and not EditorPrefs. Projects sharing company/product identity can share a store. There is no general public PlayerPrefs enumeration API; enter an exact key for selective deletion. All deletion buttons ask for confirmation and reject Play Mode/import/compilation.

Save files and PlayerPrefs are separate. No actual save folder has been assumed for this project. Configure a subfolder only after matching your save system. The tool refuses the persistent-data root, absolute paths, traversal, links/junctions and its own backup folder. It archives the chosen folder under `persistentDataPath/EditorToolBackups/` without overwriting. Restore refuses an existing destination. Keep/copy older archives manually; the one-click restore remembers the latest archive. Root-level save files are not reset by this folder tool. PlayerPrefs are not included in folder archives.

Scene favorites and tool settings use EditorPrefs keys scoped by project path, so a different checkout has separate preferences. Changing project paths therefore does not migrate favorites. Scene favorites themselves use GUIDs so asset renames/moves retain them within the same project.

## Audit limits

Audits are read-only snapshots, run only on demand. Nothing is auto-fixed, deleted or reimported. Results can become stale after edits. Scene counts include inactive objects, all LOD levels and repeated mesh instances; they are potential content costs, not visible-frame totals. Renderer count is not draw-call count. Transparent-material and particle-capacity findings need visual/device checks. Shader support is checked in the current Editor, not on every phone.

Texture findings account for source dimensions and Android/iPhone import caps. They do not claim exact VRAM savings or final atlas size, and do not force ASTC or alter alpha handling. A source smaller than 1024 is not flagged solely because its maximum import cap is larger. Existing explicit uncompressed platform overrides and sprite-atlas settings still need separate review. Large scans are cancellable; partial scans are labelled. Scene traversal caps at 50,000 objects and texture inspection at 5,000 importers; folder/dependency discovery happens before texture inspection.

Review thresholds (6k mesh triangles, 64 skin bones, 150 particle capacity per active emitter, 24 dynamic bodies, 150k potential triangles and 1024px textures) are initial authoring hints, not pass/fail mobile performance requirements. Some cameras/lights/materials legitimately exceed these counts. Test both walking and driving on the actual baseline phone.

## Source and changes

Adapted concepts/code from the owner's `I_Am_Janitor` project:

- `Assets/Editor/SceneViewWindow.cs`: removed per-row style allocation, added search/favorites/full-path disambiguation, correct enabled build indices, additive opening and editor-state guards; separated data actions.
- `Assets/Editor/Hierponent.cs` (Hierponent v1.0.2): replaced repeated callback registration, per-row component-list creation and ad-hoc offset pagination with one idempotent callback, bounded invalidated cache, null handling, overflow menu and component inspection.
- `Assets/Editor/ClearGameDataEditor.cs`: replaced hard-coded `JanitorSim` permanent folder deletion with explicit folder configuration, validated paths, archive and restore.
- `Assets/0_Project Data/Editor/MobileTextureAuditor.cs`: reused the audit-first concept, generalized selection scope, accounted for source dimensions, and removed forced imports/compression and misleading VRAM estimates.

The source project is unchanged. The Tenjin build processor and Janitor gameplay-specific authoring scripts were not copied. These tools have no dependency on Janitor runtime types, Tenjin, Feel, DOTween or uGUI assemblies. The existing IMGUI scene window stays IMGUI; new data/audit windows use UI Toolkit with a shared USS file.

Unity 6 Build Profiles may override the shared scene list. The scene switcher labels its shared-list indices accordingly and does not modify build configuration. Use Build Profiles for the final shipping scene order.

## Validation

See the project's `Planning/EditorTools/VALIDATION.md` for the actual compile/check results and UI verification limits. The included validation checks exercise unsafe path rejection, archive/restore byte preservation, overwrite refusal, scene indexing, inactive/deduplicated scene audit, missing materials, line topology, cancellation, texture source dimensions, Read/Write detection and unchanged import settings.

Reference APIs: [scene save prompt](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo.html), [component property editor](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/EditorUtility.OpenPropertyEditor.html), [PlayerPrefs deletion](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerPrefs.DeleteAll.html).
