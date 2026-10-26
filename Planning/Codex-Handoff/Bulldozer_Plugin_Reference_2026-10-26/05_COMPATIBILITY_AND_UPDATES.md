# Compatibility, known issues, storage and updates

This is a static audit of the local checkout on 26 October 2026. A known source defect, a conditional integration risk and an unverified runtime behavior are different things. None of the entries below is a claim that Unity compilation was run.

## High-value findings

| ID | Evidence/status | Impact and next action |
|---|---|---|
| P01 | **Confirmed source defect:** Joystick.cs line 24 getter returns its own `AxisOptions` property | Reading that getter recurses until stack overflow. In a separate implementation change, return the `axisOptions` backing field and verify get/set plus drag behavior. Reading `Direction` alone does not invoke this getter in the same way. |
| P02 | **Confirmed configuration:** Active Input Handling = 1; legacy Input calls remain in examples, Reporter and TouchControlsKit | Enabling affected scripts can produce runtime input exceptions. Prefer Input System UI/Actions for new work; adapt selected tools or deliberately evaluate Both mode. Do not change project input handling just to silence all vendor demos. |
| P03 | **Confirmed version boundary:** local Feel 5.6.1; online Feel 6.3 requires Unity 6000.5+ | Project is 6000.3.13f1. Keep baseline until a separate Editor/package migration is planned and tested. |
| P04 | **Confirmed local API differences:** old DOTween DLLs and restricted AnimationType/API | New Inspector FillAmount and RecreateTween examples do not match local source. Use module `DOFillAmount` or the verified installed APIs. |
| P05 | **Confirmed lifecycle defaults:** MMF_Player disable flags false; DOTweenAnimation only kills on destroy | Pooled/disabled visuals need explicit stop/reset ownership. Do not assume disabling equals disposal. |
| P06 | **Confirmed overlap:** two tween libraries, two mobile haptic routes, multiple input/VFX libraries | Select one writer per property and one route per gameplay signal. Keep unselected libraries until references are assessed. |
| P07 | **Confirmed conditional haptics:** Nice Vibrations symbol is serialized for Android | Switching targets may hide/no-op feedbacks. Verify per-target symbols and native plugin import/build on the actual target. |
| P08 | **Needs Unity verification:** old shader/demo assets, custom assembly layout and GUID package references | Resolve actual compiler/material errors before migration decisions. File presence does not prove a compatible compiled feature. |
| P09 | **Confirmed partial imports:** JMO Assets, Particlecollection Free samples and Sparkles contain scripts/metadata only | They do not currently supply usable effect prefabs/materials/textures. Choose from the actual Epic Toon FX or Particle Image content instead of referencing missing assets. |

Exact paths and code anchors are in `plugin_inventory.json`, `installed_api_index.json` and `source_fingerprints.json`.

## Input route

The installed Joystick Pack base implements uGUI pointer handlers and exposes `Direction`, `Horizontal`, `Vertical`, `DeadZone` and `HandleRange`. Pair one chosen joystick with an EventSystem using the new Input System UI module and a GraphicRaycaster. Read its direction in the game input adapter; combine it with keyboard/gamepad actions using a clear priority rule.

TouchControlsKit's `TCKInput.CheckUIEventSystem` can create a `StandaloneInputModule`, and its joystick/button sources call `Input.touchCount`. Reporter reads old touch/mouse APIs. KCC example controllers and Standard Assets also include legacy calls. These are specific risks for instantiated components, not evidence that all installed scripts cause errors at startup.

The top-level `Assets/Joystick Pack` contains prefab/art content while runtime classes live under `Assets/Unity_Packages/Joystick Pack`. The earlier GUID check found FixedJoystick prefabs pointing at the surviving script. Do not report duplicate joystick C# classes or delete one folder solely because the names are similar.

The new Input System's installation guide explains the old/new backend choice. [Unity Input System 1.19 installation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Installation.html)

## Assembly definitions and hidden dependencies

- **Feel:** Runtime and editor asmrefs are consolidated onto the Tools assemblies. Verify every GUID using the imported package metadata if compilation reports an unresolved reference. A namespace such as `MoreMountains.Feedbacks` does not imply a same-named assembly exists here.
- **DOTween:** Core/Pro DLLs coexist with loose modules and scripts. A custom game asmdef needs a coherent assembly setup for extension methods, not just a DLL reference.
- **Particle Image:** The runtime asmdef references packages associated with Burst, Collections and Mathematics. Those package IDs are in the lockfile transitively; their absence from direct manifest entries is not proof they are missing. Verify resolution after import.
- **Asset Cleaner:** Its asmdef explicitly includes only Editor and is not auto-referenced. `using UnityEditor` in its non-Editor subfolders is therefore not, by itself, an Android compile blocker.
- **NavMesh:** Navmesh Path Draw bundles legacy `UnityEngine.AI.NavMeshSurface`/related components, while modern AI Navigation is also installed. Choose the intended implementation and namespace before wiring surfaces. Different namespaces do not automatically mean duplicate-type compile failure.
- **Nested package.json files:** UIEffect and CandyCoded are vendored inside Assets. A package.json there is useful version evidence but does not mean UPM is managing that folder. Adding the same package through UPM can create duplicate implementations.

## URP and materials

Epic Toon FX includes an old URP compatibility note; its named upgrade archive was not found as a `.unitypackage` in the inspected checkout. Treat this as a historical workflow, not an instruction to run a missing or automatically compatible converter on Unity 6.

Select only the desired effects and inspect their actual shader/material references. Verify them in the project's Android URP renderer and chosen camera. The bundled ETFX note points to depth/soft-particle settings for invisible particles; inspect those when relevant, but do not globally change every shader or enable expensive depth features without a concrete effect requirement.

The local Cartoon FX effect scripts include camera-shake behavior, but the JMO folder contains no effect prefabs/materials/textures. Particlecollection Free samples and Sparkles are also source-only remnants. Do not treat those folder names as complete VFX libraries. If complete assets are imported later, coordinate camera-shake ownership with Feel and verify their materials. Demo bloom scripts are not automatically the right post-processing setup for URP.

Feel's online 6.x demo pipeline instructions differ from the local 5.6.1 readme. A pink demo material and a broken runtime feedback are separate diagnoses.

## Storage priorities

This project's earlier tracked-tree review measured roughly **997 MiB** of file payload, dominated by Epic Toon FX (~543 MiB) and Feel (~365 MiB). Those are source checkout figures, not APK size, import cache size or memory usage. The new inventory includes current per-folder byte counts; nested rows overlap and must not be summed as independent totals.

To work within low storage:

1. Keep these reference files under Planning, outside Assets, so Unity does not import the notes.
2. Do not import additional demo archives or install optional Cinemachine/HDRP packages for features not in use.
3. Keep a recoverable full source copy. On a separate cleanup change, identify demo scenes/materials/prefabs that are truly unused and remove selected content with corresponding `.meta` files after dependency checks.
4. Keep runtime shader/material/audio/native libraries referenced by selected prefabs. Include indirect references from Resources, string-based loading, Addressables if added later, and build configuration in the assessment.
5. Use Asset Cleaner as evidence of serialized references, not proof that an asset is safe to delete. Its scan cannot replace an understanding of dynamic loading.
6. Measure Unity Library and the actual build separately. Deleting Library causes reimport and can temporarily worsen the storage/time problem.

No cleanup or deletion was performed by this research.

## Update procedure for a future authorized implementation

First capture a rollback point and the actual Console/build baseline. Upgrade one dependency family at a time. Pin a chosen version; read that version's minimum Editor and migration notes. Keep vendor files together and preserve game-owned adapters/variants outside vendor folders.

For DOTween, the [publisher's upgrade guide](https://dotween.demigiant.com/support.php) covers the restart/setup sequence for older core/Pro versions. Avoid mismatched DLLs or a second core copy. Verify new assembly definitions/modules after migration.

For Feel, the [current installation guide](https://feel-docs.moremountains.com/how-to-install.html) recommends a backup and clean replacement to avoid files left by an overlay import. Apply that guidance only as part of a deliberate, recoverable upgrade; inspect this checkout's customized asmrefs and game references first. Do not delete the installed Feel folder during routine documentation work.

After any update, run the integration checklist, refresh the changed plugin's version/evidence entries and regenerate fingerprints. Keep the stable recipe documents if behavior has not changed. Never claim compatibility solely from Asset Store's original Unity version field.

## Asset provenance

Keep plugin licenses, purchase records and original `.meta` relationships with the project. The reference ZIP deliberately excludes vendor binaries and assets. Kenney's bundled license files identify their packs as CC0; the catalog records this as local provenance evidence. Do not treat generic game mechanics as permission to copy another game's branded art, UI, levels, APK assets or marketing. This pack does not guarantee freedom from third-party claims.
