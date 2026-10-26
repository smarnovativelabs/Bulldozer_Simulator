# Common UI and automatic scene loading

Implemented and verified in Unity 6000.3.12f1 on 26 October 2026.

**Confirmed platform direction: portrait only on Android and iOS.** Player Settings use Portrait with landscape and upside-down rotation disabled. The Editor Game View is set to 1080 × 1920 portrait. Design future gameplay, camera framing, and HUDs for portrait safe areas.

## UI artwork rule

Use dummy images and placeholder UI assets for now. The user will later supply a UI reference image, a sprite sheet, or both. From a UI reference, create the needed sprites and assemble them into a sprite sheet. If the user supplies a sprite sheet, use its sprites and follow the supplied UI reference for the layout. Keep placeholders easy to replace with these final assets. This rule also lives in the project-root `AGENTS.md` for future agent work.

## Try it

Open `Assets/0_ProjectData/0_Scenes/0_MainMenu.unity` and press Play. MainMenu is an entry scene with no menu controls. It shows the shared loading panel and asynchronously opens `Assets/0_ProjectData/0_Scenes/1_Gameplay.unity`.

Gameplay still contains its existing camera and lighting. This change adds the scene transition and shared UI; it does not create the worksite environment or player gameplay.

## What was added

- Editable prefab: `Assets/0_ProjectData/UI/Common/CommonUI.prefab`.
- Persistent `CommonUI` root, one shared Canvas and EventSystem, and a reusable `SceneLoadingService`.
- Loading artwork with an animated pickaxe, stone impact, spark flashes, a content entrance, smooth progress, and a fade into the destination.
- Warm cream, dark teal, and gold styling with original geometric artwork. Portrait uses a vertical composition. An existing wide-layout fallback remains in the component, but landscape is not a supported shipping orientation.
- Existing `Crystal.SafeArea` on the content container. The background fills the entire screen, while content respects the safe area.
- Existing DOTween, TextMeshPro, and Input System reused. The prefab references the existing `Assets/InputSystem_Actions.inputactions`, including touch bindings. No packages were installed for this change.
- A local copy of the project's existing Roboto bold TMP font, `LoadingHeading SDF.asset`, keeps TMP serialization changes out of the vendor asset. Other text uses existing Liberation Sans. No external artwork or font downloads.
- MainMenu first and Gameplay second in the build scene list; the previous SampleScene entry remains available.

The Janitor project was inspected read-only for its persistent canvas, safe-area hierarchy, async loading, and fade pattern. This implementation has its own artwork and components and no dependency on that project.

## Runtime responsibilities

| File | Responsibility |
| --- | --- |
| `CommonUI.cs` | Persistent singleton, duplicate prevention, shared EventSystem coordination, gameplay input blocking flag. |
| `MainMenuBootstrap.cs` | Resolves or creates CommonUI and requests Gameplay. Also handles returning through MainMenu during an existing transition. |
| `SceneLoadingService.cs` | Validates scene availability, rejects overlapping requests, loads asynchronously, controls activation and reveal, and exposes errors/retry. |
| `LoadingPanel.cs` | Progress, status, responsive layout, animations, visibility, raycast blocking, and retry event. |
| `LoadingDiscGraphic.cs` | Flat feathered UI circles without external image textures. |

All scripts are under `Assets/0_ProjectData/UI/Common/`. Edit the prefab to change styling, timings, and references. `minimumDisplaySeconds` defaults to 0.8 seconds, reveal to 0.35 seconds. The `reducedMotion` Inspector setting disables the looping artwork and animated reveal; a player-facing accessibility setting is a future task.

### Calling another transition

```csharp
using Bulldozer.UI;

bool accepted = CommonUI.Instance.SceneLoading.TryLoadScene(
    "Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");
```

Call through the persistent service after entering through MainMenu. A `false` result means the request was rejected because another operation is active, the service is disabled, or the destination is missing. The loader displays an error for a missing destination. Retry repeats that destination; an invalid build configuration must be corrected before it can succeed.

Future player controls should return early while `CommonUI.Instance != null && CommonUI.Instance.BlocksGameplayInput`. Canvas raycast blocking alone does not stop raw gameplay input. The current destination has no player controller to integrate yet.

The loader reserves the final progress stage for scene activation, waits two destination frames, then shows 100% and reveals the scene. Progress reflects Unity's scene operation, not elapsed-time prediction. When future save loading or other asynchronous gameplay initialization is added, extend the readiness stage before revealing the scene.

Unity scene operations cannot be cancelled. Disabling the service releases the activation gate; it does not pretend to cancel the operation. Slow loads receive a status message after 30 seconds.

## Verification completed

- C# compilation: zero errors and zero warnings from the final compile.
- Play Mode: MainMenu automatically reaches Gameplay, panel hides, and input is released.
- Repeated transitions: one persistent CommonUI and one active EventSystem.
- Returning to MainMenu automatically opens Gameplay again.
- Loading and reveal complete with `Time.timeScale = 0`; simultaneous requests are rejected.
- Invalid scene is rejected before starting a load and displays the retry panel. The retry button callback was verified after correcting the target in the test harness.
- Fixed Game View resolutions: 1080 × 1920 portrait and 1920 × 1080 landscape with simulated iPhone X safe areas; 720 × 1280 retry layout with the existing helper's simulated Pixel 3 XL safe area. All active text rectangles stayed inside the safe area and reported no TMP overflow. These are Editor simulations, not physical-device tests. The landscape check predates the confirmed portrait-only direction and is historical fallback evidence only; use `Previews/LoadingPortrait.png` as the intended presentation.
- The prefab now has persistent Point/Click references to the existing input asset and its touchscreen bindings.

Evidence: `runtime-validation.txt`, `layout-validation.jsonl`, and the images in `Previews/`. The first two layout records were sampled from the Editor command context and report the Editor panel size; use the subsequent player-frame records for the fixed-resolution results. The 62% shown in previews is a deliberate visual inspection state, not a measured scene-load duration.

`ValidateRuntime.cs` and `InspectLayout.cs` are one-off verification scripts outside Assets, evaluated through the Unity CLI. They are not shipped or automatically run. `BuildCommonUI.cs`, `RefineCommonUI.cs`, and `BindExistingInput.cs` record the original authoring steps; the saved prefab is the maintained source. The initial builder refuses to overwrite an existing prefab.

Console evidence includes the deliberately induced missing-scene error. It also exposes a separate existing Editor issue: `HierarchyComponentIcons.cs:110` accesses destroyed cached components during scene changes. No CommonUI runtime exception occurred in the passing test. That Editor tool was left unchanged.

No Android/iOS player build or hardware performance test was run. Device touch, cutouts, application suspend/resume, and scene-load timing remain device QA tasks. Temporary test resolutions, simulated safe areas, and background-running changes were restored after verification.

## Next integration

Build the player movement and pickaxe interaction in Gameplay, connect its input guard to CommonUI, then add explicit readiness if gameplay initialization becomes asynchronous. Keep shared overlays under CommonUICanvas and place future gameplay-only HUD elements in Gameplay.

The original MainMenu scene and build settings were backed up in `Before/`. Existing project changes from earlier work were preserved.
