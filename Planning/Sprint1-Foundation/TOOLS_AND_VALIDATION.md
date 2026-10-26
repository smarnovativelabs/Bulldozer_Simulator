# Tools and validation

## Installed in this sprint

| Tool | Version and license | Compatibility and platform role | Result |
|---|---|---|---|
| [Cinemachine](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.cinemachine.html) | 3.1.7; no separate package charge; Unity Companion License | Registry metadata minimum Unity 2022.3; registry query succeeded in 6000.3.12f1. Runtime cameras suitable for Unity mobile projects. | Installed and compiled. Production follow/orbit/collision camera still needs configuration and Android/iOS validation. |
| [Android Logcat](https://docs.unity3d.com/Packages/com.unity.mobile.android-logcat@1.4/manual/index.html) | 1.4.7; no separate package charge; Unity Companion License | Metadata minimum 2021.3; Editor diagnostic tool for Android, not iOS. macOS menu: Window → Analysis → Android Logcat. | Installed and compiled; SDK/ADB/device capture not tested because Android Build Support is absent. |

The official registry searches, installation completion, package licenses and before-state manifests are saved under `Evidence/`. [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license) applies to Unity-dependent projects; this is not a general-purpose CC0 license. No purchases occurred.

Only these two direct dependencies were added through the asynchronous Package Manager Client API. UPM also added Splines 2.8.4 and Settings Manager 2.1.1 as dependencies. No pre-existing direct version was changed, and the manifest was not hand-edited. The previous five-package installer in `Planning/Tools` was not run: Animation Rigging, FBX Exporter and Memory Profiler are unnecessary for this small test.

## Existing tools and deferred integration

| Tool | Price/license evidence | Practical use and status |
|---|---|---|
| [Safe Area Helper](https://assetstore.unity.com/packages/tools/gui/safe-area-helper-130488) | Current listing FREE, Standard Asset Store EULA, version 1.0.8; compatibility table includes 6000.3.13f1. Installed source version not conclusively identified. | Reuse existing `Crystal.SafeArea` on a HUD content panel for Android/iOS. No duplicate imported. Test notches, home indicator, orientation and aspect ratios once HUD exists. |
| [Unity Logs Viewer](https://github.com/aliessmael/Unity-Logs-Viewer) | Existing local copy. Free public distribution found; this review did not establish a separate explicit repository license grant for the installed copy. | Keep existing files; do not redownload or assume unrestricted redistribution. Legacy Input calls conflict with current new-input-only setting. In-game gesture/touch operation remains unverified; resolve provenance and input adaptation before enabling in a shipping scene. |
| Unity Profiler, Frame Debugger and Device Simulator | Included Unity Editor facilities, governed by Unity terms; no new paid package | Use Profiler on actual devices and Simulator for screen layouts. Simulator is not a device performance benchmark. None of these prove thermal performance in the lab. |
| Mobile Audit and scene tools | Existing project tools | Reuse on demand. Hierarchy icon cache needs repair as described in the audit. |
| Input System and KCC | Already installed; do not duplicate | Wire a single common movement source to foot/vehicle control in a later implementation step. KCC example scripts require adaptation. |

Memory Profiler can be added when a real device memory investigation is needed. Animation Rigging can wait for a specific IK constraint; Blender already exported FBX without adding Unity's FBX Exporter. Avoid adding monetization, multiplayer or additional VFX/input frameworks during foundation work.

## Character test evidence

The official Quaternius Worker FBX imported successfully; its Avatar is valid and Human. A separate background Blender 5.2.1 process authored four original skeletal test clips and exported `WorkerTestMotions.fbx`. The user's live Blender scene was not edited. Source `.blend` and generation script are retained outside Assets. Mixamo was researched but not used.

| Check | Observed result |
|---|---|
| Source identity | Downloaded FBX header verified; SHA-256 in `asset-hashes.json`; quota-error HTML was not imported |
| Humanoid clips | Idle 2 s, Walk 1 s, Run 0.667 s, Interact 1.5 s; all report human motion |
| Retargeting | Four controllers apply the motion-source clips to the separate Worker Avatar |
| Actual playback | Initially frozen at frame 2 while unfocused; temporary runInBackground fixed it. Later samples show frame 163 and advancing normalized animation times; subsequent sample advances again. Background setting restored to false. |
| Bone motion | Preview sampling at normalized 0 and 0.25: Idle changes 1 bone, Walk 8, Run 8, Interact 4; see `pose-sampling.json` |
| Rendering | Camera screenshot inspected: worker geometry and flat-color URP materials visible, no pink shaders. No floor/environment created. |
| Runtime console | No new errors after cursor 310 during the captured playback interval; `runtime-errors.json` |
| Editor compile | `compilationFailed: false`; post-exit console contains the existing hierarchy-icons exception, so the whole Editor is not described as error-free |
| Restoration | MainMenu reopened; Play Mode stopped; prior background setting restored; original scenes and build list unchanged |

The motion is a technical smoke test. It has no locomotion motor, IK, ground contact, tool alignment, root-motion movement or mobile input. Do not use its walk/run quality as an animation acceptance result. The test always animates all four workers to expose retargeting; production should cull inactive actors and reduce material slots.

No mobile player was built, no connected-device Logcat session ran, and no FPS, memory or battery claims are made. Final device acceptance requires matching Editor modules, target phones and iOS signing/Xcode configuration.
