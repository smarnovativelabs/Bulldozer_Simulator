# Project audit

## Current baseline

The live Editor and `ProjectVersion.txt` agree on **6000.3.12f1**. Some previous handoff notes say 6000.3.13f1; they do not describe this checkout's current Editor. URP is 17.3.0, Input System 1.19.0, uGUI 2.0.0, AI Navigation 2.0.16, Timeline 1.8.12 and Test Framework 1.6.0. Android is the active build target; both Android and iOS are required by the user.

The project was already dirty. `Evidence/git-status-before.txt`, `manifest-before.json` and `packages-lock-before.json` preserve the starting state. Existing DOTween changes, editor tools, handoff edits and asset imports were left intact. Only the two requested package additions changed existing dependency files. An incidental Unity migration of `Mobile_RPAsset.asset` was reversed to its previously clean contents; its patch is retained as evidence.

## Reusable content

| Content | Current state and reuse decision |
|---|---|
| MainMenu and Gameplay | Both project scenes contain only a camera and directional light. They are foundations, not a working gameplay loop. |
| Shared build list | Still enables `Assets/Scenes/SampleScene.unity`. Review Build Profiles before a future build; this sprint did not replace the list. |
| Kenney Nature Kit 2.1 | Actual location is `Assets/1_Plugins/Kenney/kenney_nature-kit/`, with CC0 license and FBX models. Earlier notes point to a now effectively empty sibling folder. Reuse rocks/cliffs and sparse vegetation. |
| Kenney Factory Kit 3.0 | Already installed in `Assets/1_Plugins/Kenney/kenney_factory-kit_3.0/`, with CC0 license. Inspect selected factory/conveyor meshes for depot use in Sprint 2 before downloading another building pack. |
| Kenney Impact Sounds | Already installed, with CC0 license. Audition a small impact and reward set. Engine-loop coverage still needs selection. |
| Kinematic Character Controller | Existing motor and examples; candidate for on-foot gameplay. ExamplePlayer mixes legacy `Input` and TouchControlsKit. It is not wired into the empty gameplay scene. |
| Input and UI | Input System, Joystick Pack, TouchControlsKit Lite, uGUI/TMP, SafeArea, offscreen indicators and mask utility exist. Select one input adapter and one gameplay owner; avoid adding another joystick/motor framework. |
| Feedback | DOTween/core and Pro, Feel/Nice Vibrations, CandyCoded Haptic, LeanTween, Epic Toon FX and small particle packs exist. Reuse a limited selection; overlapping tween/haptic frameworks should not all drive one event. Paid vendor content is existing content, not newly verified free content. Ownership/seat records remain the owner's responsibility. |
| Diagnostics | Unity Logs Viewer exists. Custom scene switcher, game-data tools, hierarchy icons and Mobile Audit exist under `Assets/Editor/BulldozerTools`. Use the audit without automatic fixes. |

The pre-import inventory found 1,350 C# files, 542 FBX files and 175 scenes, largely vendor content and examples. Source-folder sizes include roughly 556 MB Epic Toon FX and 378 MB Feel; these are disk sizes, not measured player size or runtime memory. Full extension counts are in `Evidence/inventory.json`.

## Gaps and priorities

1. **No core game yet.** Dedicated player/vehicle input ownership, driving, deposits, authoritative inventory, depot transactions, save/load, onboarding and progression still need implementation. Folder presence is not integration.
2. **Legacy-input mismatch.** Player Settings uses new Input System only (`activeInputHandler: 1`). Reporter gesture handling calls `Input.touches` and mouse APIs; KCC's example also uses legacy input. Neither is certified for this configuration. Adapt input in a reviewed change rather than switching the whole project to Both silently.
3. **Existing editor exception.** Exiting the lab triggered `MissingReferenceException` in `HierarchyComponentIcons.Draw`, line 110, on a destroyed URP light-data component. Add a Unity-object null check and cache invalidation in a separate approved edit. The current sprint did not overwrite that existing file. See `Evidence/editor-exit-error.json`.
4. **Platform toolchain missing.** The installed Editor's PlaybackEngines directory contains MacStandaloneSupport only. Android Logcat can compile, but an Android SDK/ADB/device session is not validated. Install matching Android Build Support including SDK/NDK/OpenJDK and iOS Build Support; verify Xcode separately before mobile builds.
5. **Rendering needs measurement.** Mobile URP currently has 0.8 render scale, HDR enabled, depth/opaque textures off, MSAA value 1 and 50-unit shadow distance. No settings were tuned. Verify the assigned quality tier and effects before disabling features.
6. **New worker is a test asset.** 5,240 triangles, 9,324 imported vertices, four skinned renderers and 13 material slots (11 unique materials). Geometry fits the earlier rough 3–6k triangle target; material submissions need reduction. Merge/atlas only the chosen appearance in Blender in Sprint 2, retaining the original FBX.

Android settings presently include minimum SDK 25 and ARM64 target flags. These are observed project settings, not a current store-submission recommendation. Default company/product identity and auto-orientation also need a deliberate release configuration later.

## Reference coverage

Read the existing master reference research, video gameplay lessons, Sprint 1 scope comparison, mobile asset guide and installed-tool notes. The supplied local-recording analysis distinguishes character/equipment cargo, refinery input/output and variable truck capacities. These are useful behavioral observations; extracted reference content must not become shipping assets.

The current store listing confirms the mining/machinery/upgrade theme. Existing footage notes support a break → carry → process → sell loop with visible cargo and expanding equipment. Strengths are clear physical feedback, visible progress and meaningful throughput bottlenecks. Design risks include repetitive return journeys, obscured machinery under large effects/stacks, unclear equipment interactions and competing onboarding recommendations. Those risks are design judgments from the recorded observations, not measured reference performance defects.

The current user scope takes precedence over older handoffs that proposed a complete playable loop in Sprint 1. Environment and gameplay construction remain future work.
