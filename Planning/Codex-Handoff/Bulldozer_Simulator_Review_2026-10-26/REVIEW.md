# Bulldozer Simulator repository review

Reviewed **26 October 2026** at commit [`b57be43`](https://github.com/smarnovativelabs/Bulldozer_Simulator/commit/b57be43724b8a2988bb7fdd024a4dfdb907d678e) on `main`.

**Verdict: the repository is a Unity setup and asset collection; it is not yet a playable first sprint.** Its project scenes are empty of gameplay. The most useful next step is a small playable slice with a deliberately selected asset set.

This was a read-only review. I inspected the full GitHub path/size inventory and 117 selected text files (369,448 bytes), verified their Git blob hashes, and checked scene YAML, package configuration, assembly routing, input examples and planning. I did not clone the full project, import it into Unity, install packages, change the repository, or run a build. No compile-success or gameplay-performance claim is made.

## Findings, in priority order

### R1 — High: gameplay has not been implemented in the intended scenes

`0_MainMenu.unity` and `1_Gameplay.unity` each contain only **Main Camera** and **Directional Light**. Neither contains a MonoBehaviour block or prefab instance. The complete tree has no C# scripts beneath `Assets/0_ProjectData`.

That means there is no integrated vehicle, mineable deposit, inventory, depot, upgrade, HUD, completion objective or save/load system in those scenes. The many imported scripts are not evidence of an implemented mining game. This is an implementation gap, not a claim that a completed feature is malfunctioning.

Evidence: [menu scene](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/0_ProjectData/0_Scenes/0_MainMenu.unity#L133), [gameplay scene](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/0_ProjectData/0_Scenes/1_Gameplay.unity#L134), [project folder](https://github.com/smarnovativelabs/Bulldozer_Simulator/tree/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/0_ProjectData).

**Next action:** create one playable scene and prove movement → break → collect → deliver → upgrade. Use primitives if the vehicle model is not ready. Add saving and the selected completion objective before more content.

### R2 — High: verify rights to publish the bundled vendor source

The repository is **public**, and raw packages include Feel, Epic Toon FX and DOTween Pro. I have not verified your licenses or any separate redistribution agreement. Unity's standard EULA distinguishes embedding assets in a product from otherwise distributing the assets. A purchase alone should not be treated as permission to publish their source. Check applicable terms before keeping these files public; consider private collaboration or a public repository containing only redistributable content. This is a rights-review item, not a determination that you lack permission. [Unity Asset Store EULA, section 2.2](https://unity.com/legal/as-terms)

Repository evidence: [Feel](https://github.com/smarnovativelabs/Bulldozer_Simulator/tree/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/Feel), [Epic Toon FX](https://github.com/smarnovativelabs/Bulldozer_Simulator/tree/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/Epic%20Toon%20FX), [DOTween Pro](https://github.com/smarnovativelabs/Bulldozer_Simulator/tree/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/Plugins/Demigiant/DOTweenPro). No visibility or history changes were made.

### R3 — Medium: the committed build scene is still the template scene

The only enabled entry in `EditorBuildSettings.asset` is `Assets/Scenes/SampleScene.unity`. A build using that list will launch the template scene, even after someone starts implementing the separately named gameplay scene. No committed BuildProfile asset was found in the complete path inventory. An uncommitted local profile could differ; it was outside this review.

Evidence: [enabled scene list](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/EditorBuildSettings.asset#L7).

**Next action:** once the prototype scene exists, set the intended entry scene in the build configuration and test a standalone launch. A menu scene is optional for the first playable slice.

### R4 — Medium, conditional: legacy example input does not match the project backend

The project records `activeInputHandler: 1`, selecting the new Input System. The bundled KinematicCharacterController `ExamplePlayer` still reads `UnityEngine.Input.GetMouseButtonDown`, `GetKeyDown` and `GetKeyUp` without a legacy-backend guard. Enabling that example under the current settings can raise input-operation errors. It is not attached in the empty project scenes, so this is an integration issue to address before reuse, not an observed failure of the current game.

Evidence: [input setting](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/ProjectSettings.asset#L930), [mouse read](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/KinematicCharacterController/ExampleCharacter/Scripts/ExamplePlayer.cs#L33), [keyboard reads](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/KinematicCharacterController/ExampleCharacter/Scripts/ExamplePlayer.cs#L88).

**Next action:** use Input System actions for the original vehicle controller and UI. If a legacy demo is necessary, make and test a deliberate compatibility choice. Unity documents that new-only operation disables the old backend and that Both is a separate option. [Input System 1.19 installation guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Installation.html)

### R5 — Medium: two first-sprint proposals disagree

The repository's planning file labels itself an unapproved proposal and still says no Unity project exists. That historical statement no longer describes this commit. Its gameplay rules also differ from the earlier research pack supplied in this chat:

| Decision | Earlier handoff proposal | Repository proposal |
|---|---|---|
| Identity / objective | Strata Salvage; delivery beacon | Rebuild a bridge and reconnect a ridge |
| Materials | One | Stone plus scrap |
| Cargo | 8 units | 15 initially, upgrade to 24 |
| Upgrade | One drill upgrade, 60 credits | Blade or cargo upgrade, 20 credits |
| Completion | 60 deliveries plus purchased upgrade | 60 stone; upgrades optional |
| Cargo full | Mining continues; loose drops remain | Stop resource-producing damage |
| Extra mechanic | None in Sprint 1 | Power Push and an optional shortcut |

These are valid alternative concepts, but combining them would create conflicting economy and acceptance rules. Nothing in this review establishes that the newer proposal has been approved elsewhere.

Evidence: [proposal status and stale workspace description](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Planning/Project-Exploration.md#L3), [bridge objective](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Planning/Project-Exploration.md#L47), [full-cargo and upgrade rules](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Planning/Project-Exploration.md#L64), [economy table](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Planning/Project-Exploration.md#L73).

**Next action:** write one authoritative current Sprint 1 specification using the latest human-approved direction, and mark the other as historical. Update the workspace description. If approval is still unclear when implementation begins, resolve the objective and full-cargo behavior before coding dependent rules.

### R6 — Medium: asset volume dominates the project before gameplay exists

The current tree contains **22,083 files totaling 1,045,523,843 bytes**: about **997.09 MiB** or **1.046 decimal GB**, before Unity's generated Library and package caches. Epic Toon FX and Feel together account for **907.05 MiB (90.97%)**.

| Tracked folder | Files | Payload MiB |
|---|---:|---:|
| `Assets/Epic Toon FX` | 5,092 | 542.54 |
| `Assets/Feel` | 4,901 | 364.51 |
| `Assets/1_Plugins` | 9,371 | 59.67 |
| `Assets/TextMesh Pro` | 366 | 9.45 |
| `Assets/LeanTween` | 434 | 7.15 |
| `Assets/AssetKits` | 196 | 4.16 |

These are uncompressed Git blob sizes summed across current paths, not disk allocation, runtime memory or final APK size. GitHub separately reports approximately **325.24 MiB** for repository size; that is not the complete working checkout or Unity import footprint. A shallow clone reduces history, but does not remove assets in the current checkout.

The following non-overlapping demo folders total **289.29 MiB** and are sensible dependency-review candidates:

| Candidate folder | Tracked MiB |
|---|---:|
| `Assets/Feel/FeelDemos` | 161.27 |
| `Assets/Feel/FeelDemosHDRP` | 27.36 |
| `Assets/Feel/MMFeedbacks/Demos` | 57.09 |
| `Assets/Feel/MMTools/Demos` | 33.02 |
| `Assets/Epic Toon FX/Demo` | 10.55 |

This is not a safe-delete list. Shared materials, scripts, shaders, runtime loading and vendor assembly references must be checked first. Epic Toon FX's prefab folder alone is about **477.13 MiB**, so simply removing demo scenes will not remove most of its footprint. Keep only a small dependency-complete set of effects for the first sprint, if they are needed at all.

**Next action:** prepare a lean prototype branch or checkout on the machine with space, preserve every retained asset's `.meta` and license, and keep the full vendor libraries recoverable outside the active prototype. Measure the result before importing. Do not delete arbitrary textures or rewrite history to meet a guessed target.

## What is already useful

- **Unity version is pinned:** `6000.3.13f1`; the manifest declares URP `17.3.0`, Input System `1.19.0`, AI Navigation `2.0.16` and Test Framework `1.6.0`. Direct manifest dependencies match the lock file. Reuse this project version unless a tested compatibility reason requires a change. [Editor version](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/ProjectVersion.txt#L1); [manifest](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Packages/manifest.json#L2).
- **URP has mobile and PC configurations.** Android selects the Mobile quality level; its render-pipeline GUID resolves to `Mobile_RPAsset`. This is a useful foundation, not proof of device performance. [platform quality mapping](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/QualitySettings.asset#L119).
- **Generated Unity directories are excluded.** `.gitignore` covers Library, Temp, builds and other generated files; those root directories are absent from the remote tree. The current size is mainly tracked content, not an accidentally committed Library folder. [ignore rules](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/.gitignore#L8).
- **Asset Cleaner is Editor-only.** Its assembly definition correctly restricts it to Editor, so its `using UnityEditor` statements outside an `Editor` folder are not by themselves a player-build defect. [assembly platform restriction](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/Assets/Unity_Packages/Asset%20Cleaner/AssetCleaner.asmdef#L4).
- **Joystick cleanup has some valid structure.** The top-level Joystick Pack has no C# scripts; the other copy contains the script implementation. Both inspected Fixed Joystick prefabs reference the surviving FixedJoystick script GUID. I did not find evidence for a duplicate-class error from those two folders.
- **Local assembly-reference routes resolve.** The 21 inspected Feel `.asmref` files point to the existing Tools or Tools.Editor definitions. This limited check does not verify all UPM/precompiled assembly references or compilation of every vendor source file.
- **Four Kenney packs preserve license files.** Keep these with selected assets. Do not assume every other bundled package has the same permissions.

## Readiness against the first playable loop

| Area | Status at reviewed commit |
|---|---|
| Editor/pipeline/package baseline | Present; actual import/compile untested |
| Vehicle movement and follow camera | No implementation in intended scenes |
| Mining and destructible deposits | No implementation in intended scenes |
| Cargo, pickups and direct sale | No implementation in intended scenes |
| Upgrades and completion objective | Planning only |
| Save/load and conservation rules | Planning only |
| HUD and touch controls | Libraries present; not integrated |
| Tests / CI / reproducible build instructions | No project tests or GitHub workflow found; no root README |
| Art readiness | Broad library available; chosen vehicle and gameplay assets not validated |

The repository's **1,351 C# file paths** are largely vendor packages and examples. That count is not progress toward 1,351 gameplay features. I inspected a targeted subset, not every implementation. Meshes, textures and audio were not downloaded or visually auditioned. No clearly named bulldozer model appeared in the path search, but differently named models may exist; this is not proof that a suitable mesh is absent.

## Recommended order for the laptop agent

1. Reconcile the current spec and record exact Editor/package versions, scene entry point and run instructions.
2. Resolve distribution rights for the public source; make any visibility/history change only under explicit user authorization.
3. Prepare the smallest required asset set on a branch, with dependency checks and an asset provenance record. Keep licensed vendor source out of any public distribution that does not permit it.
4. On a machine with sufficient space, import once and obtain a clean Console plus a minimal standalone build. Check assembly references, shaders and the input backend. Do not install more packages merely because a planning helper lists them.
5. Implement one complete loop in one scene with placeholder art as needed. Commit after each working checkpoint.
6. Add the chosen completion objective and coherent persistence; run the agreed acceptance checks. Record what was actually tested, then review visual polish.

Before an Android build, replace the template application identifier and agree orientation. The committed identifier is still `com.UnityTechnologies.com.unity.template.urpblank`, and autorotation allows all four orientations. These are normal template defaults to settle, not evidence of a working portrait-mobile product. [identifier](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/ProjectSettings.asset#L169); [rotation options](https://github.com/smarnovativelabs/Bulldozer_Simulator/blob/b57be43724b8a2988bb7fdd024a4dfdb907d678e/ProjectSettings/ProjectSettings.asset#L62).

There is no need to match the reference APK's Unity 2022 version. This project already records Unity 6.3. The task now is to build the original loop and verify it in this project.

## Review limits and portable files

This review applies only to `b57be43724b8a2988bb7fdd024a4dfdb907d678e`. Later commits and uncommitted work on the laptop can change the conclusion. No files were changed in GitHub and no PR or issue was posted. The review is not a comprehensive security audit or license clearance.

- [REVIEW_FINDINGS.json](REVIEW_FINDINGS.json): exact metrics, source links, findings and inspection limits.
- [LAPTOP_AGENT_CHECKLIST.txt](LAPTOP_AGENT_CHECKLIST.txt): concise ordered follow-up for the implementation agent.
- [VALIDATION.txt](VALIDATION.txt): checks completed during this remote review.

The previous mining-game research ZIP remains separate and unchanged. Resolve the specification differences before treating either proposal as the implementation contract.
