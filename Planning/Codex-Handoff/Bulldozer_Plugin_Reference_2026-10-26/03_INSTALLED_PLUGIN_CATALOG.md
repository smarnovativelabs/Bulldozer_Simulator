# Installed plugin catalog

Local snapshot: 26 October 2026. Each entry describes the installed files; unknown versions are intentionally not guessed. Roles and Sprint recommendations are proposals. All runtime, render and device behavior still needs Unity validation.

## At a glance

| Tool/library | Version evidence | Sprint recommendation | Source payload |
|---|---|---|---|
| DOTween core + Pro + DemiLib | core DLL 1.2.420; Pro DLL 1.0.244; DemiEditor DLL string 1.0.060 | Use for UI and cosmetic tweens | 1.47 MiB |
| Feel / MMFeedbacks | 5.6.1 | Use a small event-driven subset | 364.51 MiB |
| MMTools (bundled with Feel) | bundled in Feel 5.6.1 | Keep required runtime; adopt utilities only when needed | 72.14 MiB |
| Nice Vibrations / Lofelt | 4.1.2; Lofelt SDK 1.3.4 | Optional after the loop works | 43.24 MiB |
| LeanTween | 2.50 | Keep for existing dependencies; prefer DOTween for new work | 7.15 MiB |
| Particle Image | 1.2.0 (editor source header) | Optional UI polish | 4.16 MiB |
| Joystick Pack | Not established | Use one chosen joystick with the Input System UI route | 1.25 MiB |
| TouchControlsKit Lite | Not established | Defer unless retaining a specific existing consumer | 0.15 MiB |
| Crystal Safe Area | Not established | Useful in Sprint 1 | 0.27 MiB |
| Coffee UIEffect / UI Shiny | 4.0.0-preview.9 | Optional upgrade-button polish | 0.22 MiB |
| AwesomeMask | Not established | Later tutorial onboarding | 2.14 MiB |
| Kinematic Character Controller | Not established | Evaluate only if choosing character-style movement | 0.58 MiB |
| Navmesh Path Draw + legacy NavMeshComponents | Not established | Later objective/depot route hint | 0.93 MiB |
| Pixel Play Off-Screen Indicator | not established; bundled guide filename says 3.0 | Later, if a depot can leave the camera view | 0.15 MiB |
| LitJson | Not established | Use only if the chosen data format needs it | 0.12 MiB |
| CandyCoded Haptic Feedback | 1.0.3 | Alternative to Nice Vibrations, not an additional simultaneous route | 0.01 MiB |
| Unity Logs Viewer / Reporter | readme changelog latest entry 1.8 | Development-only option after input adaptation | 0.93 MiB |
| Asset Cleaner (with internal LeoECS helper) | Not established | Use later for a reviewed cleanup | 0.28 MiB |
| MiniGames / Tiny Trail | Not established | Later vehicle/tool visual trail | 0.02 MiB |
| Epic Toon FX | changelog latest entry v1.81 (2023-03-01) | Use a small curated set | 542.53 MiB |
| JMO Cartoon FX Remaster + Easy Editor | Not established | Reference helpers; no ready VFX library here | 0.19 MiB |
| Particlecollection Free samples | Not established | Defer | 0.05 MiB |
| Sparkles | Not established | Do not depend on this as a ready effect pack | 0.01 MiB |
| Legacy Unity Standard Assets | Not established | Reference only unless deliberately adapted | 0.30 MiB |
| TextMesh Pro project resources and examples | runtime supplied by uGUI 2.0.0; resource asset version not established | Use TMP for HUD text; keep examples out of gameplay | 9.45 MiB |
| Kenney Factory Kit | Factory Kit (3.0) | Select original-game prototype content as needed | 12.83 MiB |
| Kenney Nature Kit | Nature Kit (2.1) | Select original-game prototype content as needed | 35.91 MiB |
| Kenney Impact Sounds | Impact Sounds (1.0) | Select original-game prototype content as needed | 1.05 MiB |
| Kenney Particle Pack | Particle Pack (1.1) | Select original-game prototype content as needed | 9.87 MiB |

Sizes include metadata and overlap for Feel/MMTools/Nice Vibrations. They are neither APK size nor runtime memory. No assets were removed.

## DOTween core + Pro + DemiLib

**Category:** runtime tween engine + editor tooling. **Decision:** Use for UI and cosmetic tweens.

Location: `Assets/Plugins/Demigiant`.

**Version:** core DLL 1.2.420; Pro DLL 1.0.244; DemiEditor DLL string 1.0.060.

Local file types (excluding .meta): .cs: 15, .dll: 6, .jpg: 1, .lighting: 3, .png: 121, .txt: 3, .unity: 3, .xml: 6, .zip: 1.

Code tweens, sequences, visual Animation/Path components, TMP animation; DemiLib supports vendor tooling.

**Setup/API:** See 01_DOTWEEN_PRO.md. Use DG.Tweening. Configure modules through the Utility Panel. Add coherent assembly definitions if custom game assemblies need loose modules.

**Watch for:** DLL product strings do not certify every loose script version. Handle pool return explicitly. Avoid a second core copy and competing Feel/Animator property writers.

Entry points:

- `Assets/Plugins/Demigiant/DOTweenPro/DOTweenAnimation.cs`
- `Assets/Plugins/Demigiant/DOTweenPro/DOTweenTextMeshPro.cs`
- `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUI.cs`
- `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModulePhysics.cs`
- `Assets/Plugins/Demigiant/DOTween/DOTween.XML`
- `Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.XML`

Version evidence: `Assets/Plugins/Demigiant/DOTween/DOTween.dll`, `Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.dll`, `Assets/Plugins/Demigiant/DemiLib/Core/Editor/DemiEditor.dll`.

## Feel / MMFeedbacks

**Category:** runtime feedback orchestration + demos. **Decision:** Use a small event-driven subset.

Location: `Assets/Feel`.

**Version:** 5.6.1.

Local file types (excluding .meta): [no extension]: 1, .aar: 1, .anim: 19, .asmdef: 5, .asmref: 21, .asset: 76, .controller: 16, .cs: 872, .curves: 4, .dll: 1, .dylib: 1, .exr: 12, .fbx: 35, .h: 1, .haptic: 141, .hdr: 3, .lighting: 11, .mask: 1, .mat: 258, .md: 1, .mixer: 1, .modulemap: 1, .obj: 1, .otf: 2, .physicmaterial: 2, .physicsmaterial2d: 1, .plist: 1, .png: 300, .prefab: 73, .rendertexture: 5, .shader: 14, .tga: 4, .tss: 1, .ttf: 24, .txt: 12, .unity: 43, .uss: 9, .uxml: 1, .wav: 233, .webm: 3.

Inspector-authored coordinated visual, audio, UI, camera and haptic responses.

**Setup/API:** See 02_FEEL.md. Bind MMF_Player and explicit targets; play after an accepted gameplay event. Code namespace MoreMountains.Feedbacks.

**Watch for:** Online docs are 6.3, with APIs absent locally and a higher minimum Editor. The root includes MMTools, Nice Vibrations and demos; nested inventory sizes overlap.

Entry points:

- `Assets/Feel/MMFeedbacks/MMFeedbacks/Core/MMF_Player/MMF_Player.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Core/MMF_Player/MMF_Feedback.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Core/Legacy/MMFeedbacks.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Feedbacks/MMF_ParticlesInstantiation.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Feedbacks/MMF_CameraShake.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Feedbacks/MMF_Scale.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacks/Feedbacks/MMF_Pause.cs`

Version evidence: `Assets/Feel/readme.txt`.

## MMTools (bundled with Feel)

**Category:** runtime utility library + editor helpers. **Decision:** Keep required runtime; adopt utilities only when needed.

Location: `Assets/Feel/MMTools`.

**Version:** bundled in Feel 5.6.1.

Local file types (excluding .meta): .anim: 2, .asmdef: 2, .asmref: 5, .asset: 27, .controller: 2, .cs: 380, .curves: 2, .exr: 4, .fbx: 1, .lighting: 2, .mat: 62, .mixer: 1, .png: 103, .prefab: 28, .shader: 13, .ttf: 19, .txt: 2, .unity: 18, .uss: 1.

Audio management, pools, progress bars, time helpers and numerous supporting systems used by feedbacks.

**Setup/API:** MoreMountains.Tools namespace. MMSoundManager can own sound tracks/pooling; MMProgressBar displays game values. Inspect the local API index and demo before adopting an optional helper.

**Watch for:** The runtime assembly is also the target of Feel asmrefs in this checkout. Do not delete it as an optional demo library. Avoid adding another save/time/pool system without a clear owner.

Entry points:

- `Assets/Feel/MMTools/Core/MoreMountains.Tools.asmdef`
- `Assets/Feel/MMTools/Core/MMAudio/MMSoundManager/MMSoundManager.cs`
- `Assets/Feel/MMTools/Core/MMUI/MMProgressBar.cs`
- `Assets/Feel/MMTools/Core/MMTimeManager/MMTimeManager.cs`

Version evidence: `Assets/Feel/readme.txt`.

## Nice Vibrations / Lofelt

**Category:** native mobile/gamepad haptics. **Decision:** Optional after the loop works.

Location: `Assets/Feel/NiceVibrations`.

**Version:** 4.1.2; Lofelt SDK 1.3.4.

Local file types (excluding .meta): [no extension]: 1, .aar: 1, .anim: 7, .asmdef: 3, .controller: 3, .cs: 43, .dll: 1, .dylib: 1, .h: 1, .haptic: 141, .mat: 3, .md: 1, .modulemap: 1, .otf: 1, .physicsmaterial2d: 1, .plist: 1, .png: 24, .prefab: 5, .shader: 1, .ttf: 4, .txt: 8, .unity: 1, .wav: 145.

Preset, emphasis, continuous and authored-clip haptics, exposed through direct APIs and Feel feedbacks.

**Setup/API:** Prefer MMF_NVPreset/MMF_NVEmphasis through a game haptic adapter. Check MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED per target; Android has the define in this checkout.

**Watch for:** Device verification required. MMF_Haptics is deprecated. Choose this or CandyCoded per event; do not double-fire. OlderVersions content is not another package that needs importing.

Entry points:

- `Assets/Feel/NiceVibrations/Scripts/Components/HapticPatterns.cs`
- `Assets/Feel/NiceVibrations/Scripts/Components/HapticController.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacksForThirdParty/NiceVibrations/Feedbacks/MMF_NVPreset.cs`
- `Assets/Feel/MMFeedbacks/MMFeedbacksForThirdParty/NiceVibrations/Feedbacks/MMF_NVEmphasis.cs`

Version evidence: `Assets/Feel/NiceVibrations/readme.txt`.

## LeanTween

**Category:** runtime tween engine + demos. **Decision:** Keep for existing dependencies; prefer DOTween for new work.

Location: `Assets/LeanTween`.

**Version:** 2.50.

Local file types (excluding .meta): .cs: 47, .css: 1, .fbx: 14, .html: 15, .ico: 1, .lighting: 33, .mat: 32, .physicmaterial: 1, .png: 6, .prefab: 14, .psd: 6, .txt: 2, .unity: 33, .zip: 3.

Alternative tween implementation using LeanTween static calls and LTDescr handles.

**Setup/API:** Typical routes are LeanTween.move/scale/value and cancel by the specific tween ID. Framework and examples are separate folders; the readme points to bundled/offline documentation.

**Watch for:** Do not run LeanTween and DOTween on the same property. Inventory its callers before a later removal. Tween cancellation and pool baseline reset remain necessary.

Entry points:

- `Assets/LeanTween/Framework/LeanTween.cs`
- `Assets/LeanTween/Framework/LTDescr.cs`
- `Assets/LeanTween/ReadMe.txt`
- `Assets/LeanTween/License.txt`

Version evidence: `Assets/LeanTween/Framework/LeanTween.cs`.

## Particle Image

**Category:** uGUI particle renderer. **Decision:** Optional UI polish.

Location: `Assets/AssetKits/ParticleImage`.

**Version:** 1.2.0 (editor source header).

Local file types (excluding .meta): .asmdef: 2, .cs: 16, .mat: 1, .md: 1, .png: 38, .prefab: 19, .shader: 1, .unity: 13.

Canvas-based particles, attractors, trails, texture-sheet animation, sub-particles and masking.

**Setup/API:** Create GameObject > UI > Particle Image. Runtime type AssetKits.ParticleImage.ParticleImage exposes Play, Pause, Stop, Stop(bool), Clear and AddBurst. Use for reward sparkles in Overlay/Camera Canvas modes.

**Watch for:** This is not a world ParticleSystem. Its asmdef has Burst/Collections/Mathematics dependencies. Check UI material/masking and particle counts; particle-completion events must not grant currency.

Entry points:

- `Assets/AssetKits/ParticleImage/README.md`
- `Assets/AssetKits/ParticleImage/Runtime/ParticleImage.cs`
- `Assets/AssetKits/ParticleImage/Runtime/ParticleImage.asmdef`

Version evidence: `Assets/AssetKits/ParticleImage/Editor/ParticleImageEditor.cs`.

## Joystick Pack

**Category:** uGUI mobile input controls. **Decision:** Use one chosen joystick with the Input System UI route.

Location: `Assets/Unity_Packages/Joystick Pack`, `Assets/Joystick Pack`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 11, .mat: 4, .pdf: 2, .png: 48, .prefab: 8, .unity: 2.

Fixed, floating, dynamic and variable touch joystick variants; analog Direction/Horizontal/Vertical values.

**Setup/API:** Use a Canvas/GraphicRaycaster and one EventSystem with InputSystemUIInputModule. Assign background/handle and read Direction from a game input adapter. The bundled PDF is Documentaion.pdf (original spelling).

**Watch for:** Confirmed AxisOptions getter recursion at base Joystick.cs:24; fix before calling it. Top-level folder is prefab/art content, not a proven second script implementation. Prefabs still need input-module verification.

Entry points:

- `Assets/Unity_Packages/Joystick Pack/Scripts/Base/Joystick.cs`
- `Assets/Unity_Packages/Joystick Pack/Scripts/Joysticks/FloatingJoystick.cs`
- `Assets/Unity_Packages/Joystick Pack/Scripts/Joysticks/FixedJoystick.cs`
- `Assets/Unity_Packages/Joystick Pack/Documentaion.pdf`

## TouchControlsKit Lite

**Category:** touch joystick, button and touchpad framework. **Decision:** Defer unless retaining a specific existing consumer.

Location: `Assets/Unity_Packages/VictorsAssets/TouchControlsKit-Lite`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 27, .png: 1.

Named controllers queried through TouchControlsKit.TCKInput; contains first-person examples.

**Setup/API:** GetAxis(controllerName), GetButtonDown(name), GetButton(name), GetButtonUp(name); controllers and names must exist. Review TCKInput initialization before bringing a prefab into the scene.

**Watch for:** CheckUIEventSystem creates legacy StandaloneInputModule and controls read Input.touchCount. This conflicts with assuming a new-only Input System setup. Do not run a second movement-input route.

Entry points:

- `Assets/Unity_Packages/VictorsAssets/TouchControlsKit-Lite/Scripts/Runtime/TCKInput.cs`
- `Assets/Unity_Packages/VictorsAssets/TouchControlsKit-Lite/Scripts/Runtime/Controllers/TCKJoystick.cs`
- `Assets/Unity_Packages/VictorsAssets/TouchControlsKit-Lite/Scripts/Runtime/Controllers/TCKButton.cs`

## Crystal Safe Area

**Category:** uGUI layout utility. **Decision:** Useful in Sprint 1.

Location: `Assets/CrystalFramework`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .asset: 1, .cs: 2, .exr: 1, .lighting: 1, .prefab: 1, .rtf: 1, .unity: 1.

Fits a RectTransform to Screen.safeArea and responds to screen/orientation changes; includes device simulation options.

**Setup/API:** Add Crystal.SafeArea to the intended HUD safe-area container under the Canvas. Keep full-screen backgrounds outside that constrained container. Validate layout anchors on the chosen aspect ratios.

**Watch for:** Do not apply it to every nested element or double-apply safe-area compensation. Check scaled Canvas behavior and orientation in the actual game. Simulation is not a replacement for a phone test.

Entry points:

- `Assets/CrystalFramework/Utility/SafeArea.cs`
- `Assets/CrystalFramework/Utility/Demos/SafeAreaDemo.cs`

## Coffee UIEffect / UI Shiny

**Category:** uGUI material/mesh effects. **Decision:** Optional upgrade-button polish.

Location: `Assets/Unity_Packages/UI Shiny/UIEffect-upm`.

**Version:** 4.0.0-preview.9.

Local file types (excluding .meta): .cginc: 2, .cs: 31, .json: 1, .md: 3, .png: 1, .shader: 5.

UIShiny, dissolve, transition, HSV, gradients, shadows and related graphic effects.

**Setup/API:** Add the appropriate Coffee.UIEffects component to a supported Graphic. UIShiny.Play(bool reset=true) and Stop(bool reset=true) exist locally. The bundled README describes components and effect parameters.

**Watch for:** Old preview version vendored under Assets; current upstream APIs may differ. Do not install a second UPM copy. Test masking, material compatibility and modified mesh/material interactions. Local LICENSE.md is MIT.

Entry points:

- `Assets/Unity_Packages/UI Shiny/UIEffect-upm/Scripts/UIShiny.cs`
- `Assets/Unity_Packages/UI Shiny/UIEffect-upm/Scripts/UIEffect.cs`
- `Assets/Unity_Packages/UI Shiny/UIEffect-upm/Scripts/UITransitionEffect.cs`
- `Assets/Unity_Packages/UI Shiny/UIEffect-upm/README.md`
- `Assets/Unity_Packages/UI Shiny/UIEffect-upm/package.json`

Version evidence: `Assets/Unity_Packages/UI Shiny/UIEffect-upm/package.json`.

## AwesomeMask

**Category:** uGUI spotlight/masking utility. **Decision:** Later tutorial onboarding.

Location: `Assets/_CrazyMinds/AwesomeMask`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 3, .mat: 4, .pdf: 1, .png: 5, .prefab: 3.

Screen overlay with hollow areas to highlight UI/objects and control pointer passthrough.

**Setup/API:** Crazyminds.AwesomeMask.AwesomeMask with AwesomeMaskHollow. Available methods include Add, Remove, SetElementPosition and CanRayCast. Use supplied prefabs/manual when binding highlight shapes.

**Watch for:** Verify actual raycast behavior before a tutorial can block the screen. Check Canvas coordinate conversion, safe area and aspect ratio. It is not a gameplay unlock/step system.

Entry points:

- `Assets/_CrazyMinds/AwesomeMask/Scripts/Core/AwesomeMask.cs`
- `Assets/_CrazyMinds/AwesomeMask/Scripts/Core/AwesomeMaskHollow.cs`

## Kinematic Character Controller

**Category:** kinematic character locomotion framework + examples. **Decision:** Evaluate only if choosing character-style movement.

Location: `Assets/KinematicCharacterController`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 60, .lighting: 1, .mat: 1, .unity: 1.

Capsule-based character motor, ground/collision handling and physics movers. This is not a bulldozer driving simulator.

**Setup/API:** Implement ICharacterController callbacks such as UpdateRotation and UpdateVelocity and assign it to KinematicCharacterMotor. Review core separately from walkthrough/example input and camera code.

**Watch for:** Examples include legacy Input and locally present mobile joystick/TCK dependencies. Do not use a demo as the production input contract. Vehicle collision geometry and pushing need an explicit design.

Entry points:

- `Assets/KinematicCharacterController/Core/ICharacterController.cs`
- `Assets/KinematicCharacterController/Core/KinematicCharacterMotor.cs`
- `Assets/KinematicCharacterController/ExampleCharacter/Scripts/ExamplePlayer.cs`
- `Assets/KinematicCharacterController/ExampleCharacter/Scripts/ExampleCharacterController.cs`

## Navmesh Path Draw + legacy NavMeshComponents

**Category:** navigation route visualization + bundled legacy components. **Decision:** Later objective/depot route hint.

Location: `Assets/Navmesh Path Draw`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .asset: 1, .cs: 14, .mat: 5, .md: 1, .pdf: 1, .png: 4, .prefab: 1, .unity: 1.

Draws a path with LineRenderer; local code has movement/recalculation thresholds, ground projection and direct fallback settings.

**Setup/API:** Assign destination and LineRenderer/material, ground layers and agent settings. Local methods setDestinationAndDrawPath(Transform), Draw() and Stop() are case-sensitive.

**Watch for:** A line is not vehicle control. Inspect fallback paths for unreachable destinations. Bundled UnityEngine.AI.NavMeshSurface coexists with modern AI Navigation; choose the intended namespace rather than assuming identical components.

Entry points:

- `Assets/Navmesh Path Draw/Scripts/NavmeshPathDraw.cs`
- `Assets/Navmesh Path Draw/Scripts/NavMeshComponents/Assets/NavMeshComponents/Scripts/NavMeshSurface.cs`
- `Assets/Navmesh Path Draw/Readme.md`

## Pixel Play Off-Screen Indicator

**Category:** UI navigation/target indicators. **Decision:** Later, if a depot can leave the camera view.

Location: `Assets/Pixel Play`.

**Version:** not established; bundled guide filename says 3.0.

Local file types (excluding .meta): .cs: 7, .pdf: 1, .png: 2, .prefab: 2, .unity: 1.

Targets with off-screen arrows, boxes and distance labels, with associated indicator pools.

**Setup/API:** Bind the camera and Canvas/indicator prefabs in OffScreenIndicator. Add Target to desired objects. Target enable/disable calls SetTargetState; configure NeedBoxIndicator/NeedArrowIndicator/NeedDistanceText in its Inspector.

**Watch for:** Target and indicator classes have broad/global names; use a game namespace for your code. Validate camera behind/front tests, safe-area boundaries and despawn cleanup. ExtendedFlycam is a demo camera, not production vehicle input.

Entry points:

- `Assets/Pixel Play/Scripts/OffScreenIndicator/OffScreenIndicator.cs`
- `Assets/Pixel Play/Scripts/OffScreenIndicator/Target.cs`
- `Assets/Pixel Play/Scripts/OffScreenIndicator/OffScreenIndicatorCore.cs`

## LitJson

**Category:** JSON serialization library. **Decision:** Use only if the chosen data format needs it.

Location: `Assets/LitJson`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 10.

JsonData/JsonReader/JsonWriter and JsonMapper conversions between objects and JSON.

**Setup/API:** Use LitJson.JsonMapper.ToJson and ToObject<T> with deliberate data transfer types. Add schema versioning, validation, defaults and migration rules at the game layer.

**Watch for:** A serializer is not a complete save system. Avoid multiple serializers for one save schema; verify IL2CPP/reflection/stripping behavior for chosen types and corrupted-file recovery on Android.

Entry points:

- `Assets/LitJson/JsonMapper.cs`
- `Assets/LitJson/JsonData.cs`
- `Assets/LitJson/JsonReader.cs`

## CandyCoded Haptic Feedback

**Category:** native mobile haptic wrapper. **Decision:** Alternative to Nice Vibrations, not an additional simultaneous route.

Location: `Assets/Haptic/Plugins/CandyCoded.HapticFeedback`.

**Version:** 1.0.3.

Local file types (excluding .meta): .aar: 1, .asmdef: 3, .cs: 4, .java: 1, .json: 1, .mm: 1.

Simple LightFeedback, MediumFeedback and HeavyFeedback calls with Android/iOS bridges.

**Setup/API:** API namespace/type CandyCoded.HapticFeedback.HapticFeedback. Keep a game-level preference/throttle and call one selected provider only. Inspect native target import and AndroidJNI module dependency.

**Watch for:** Package JSON is vendored in Assets, not installed as a manifest dependency. No device test was run. The local package metadata declares MIT; retain original notices.

Entry points:

- `Assets/Haptic/Plugins/CandyCoded.HapticFeedback/Scripts/HapticFeedback.cs`
- `Assets/Haptic/Plugins/CandyCoded.HapticFeedback/Plugins/Android/HapticFeedback.cs`
- `Assets/Haptic/Plugins/CandyCoded.HapticFeedback/Plugins/iOS/HapticFeedback.cs`
- `Assets/Haptic/Plugins/CandyCoded.HapticFeedback/package.json`

Version evidence: `Assets/Haptic/Plugins/CandyCoded.HapticFeedback/package.json`.

## Unity Logs Viewer / Reporter

**Category:** in-game diagnostics overlay. **Decision:** Development-only option after input adaptation.

Location: `Assets/Unity-Logs-Viewer`.

**Version:** readme changelog latest entry 1.8.

Local file types (excluding .meta): .cs: 7, .css: 1, .guiskin: 1, .htm: 1, .lighting: 3, .md: 1, .png: 50, .unity: 3.

Shows console logs in a running game, traditionally opened with a circle gesture.

**Setup/API:** Bundled README describes Reporter > Create and script execution order. Inspect Reporter/Reporter.cs and ReporterGUI.cs. Verify opening/closing without intercepting normal gameplay.

**Watch for:** Reporter uses UnityEngine.Input touch/mouse calls and needs attention under new-only input. Keep logs bounded and avoid shipping noisy debug UI or sensitive game/session data in release builds.

Entry points:

- `Assets/Unity-Logs-Viewer/Reporter/Reporter.cs`
- `Assets/Unity-Logs-Viewer/Reporter/ReporterGUI.cs`
- `Assets/Unity-Logs-Viewer/README.md`

Version evidence: `Assets/Unity-Logs-Viewer/README.md`.

## Asset Cleaner (with internal LeoECS helper)

**Category:** Editor-only dependency/reference tool. **Decision:** Use later for a reviewed cleanup.

Location: `Assets/Unity_Packages/Asset Cleaner`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .asmdef: 1, .asset: 1, .cs: 43, .png: 7.

Reference/search and cleanup tooling. The vendored LeoECS sources support the tool, not an established gameplay ECS architecture.

**Setup/API:** Inspect the editor window/menu entry and search results. Start with reference inspection and a reversible cleanup plan.

**Watch for:** AssetCleaner.asmdef explicitly targets Editor. Do not flag its UnityEditor usages as player failures based only on folder names. Dynamic/string loads and Resources require manual review before deleting assets.

Entry points:

- `Assets/Unity_Packages/Asset Cleaner/AssetCleaner.asmdef`
- `Assets/Unity_Packages/Asset Cleaner/Systems/External/AufWindow.cs`
- `Assets/Unity_Packages/Asset Cleaner/Systems/SysProcessSearch.cs`

## MiniGames / Tiny Trail

**Category:** runtime procedural trail + editor. **Decision:** Later vehicle/tool visual trail.

Location: `Assets/Unity_Packages/MiniGames/Trail`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 2.

Builds a mesh trail along an array of points, with duration, material and corner settings.

**Setup/API:** Use Tiny.Trail; configure Points, material and duration. Clear() exists for restarting from a new position. The source header contains an MIT-style permission notice.

**Watch for:** A generated visual trail is not persistent tire tracks or terrain deformation. Check Clear and enable/pool behavior, URP material and mesh allocations before frequent reuse.

Entry points:

- `Assets/Unity_Packages/MiniGames/Trail/Trail.cs`
- `Assets/Unity_Packages/MiniGames/Trail/Editor/TrailEditor.cs`

## Epic Toon FX

**Category:** world particle/audio library + demo controllers. **Decision:** Use a small curated set.

Location: `Assets/Epic Toon FX`.

**Version:** changelog latest entry v1.81 (2023-03-01).

Local file types (excluding .meta): .anim: 2, .asset: 2, .controller: 2, .cs: 17, .exr: 1, .fbx: 21, .mat: 358, .pdf: 1, .png: 369, .prefab: 1449, .shader: 2, .txt: 5, .unity: 39, .wav: 79.

Large selection of impact, dust, explosion, environment, pickup and reward prefabs.

**Setup/API:** Choose a few prefabs, inspect their components, then create game-owned variants. Drive playback from Feel or one VFX service. ETFXLightFade, ETFXPitchRandomizer and ETFXRotation are small supporting scripts.

**Watch for:** Largest source folder. Old URP notes do not prove Unity 6 renderer compatibility. Check lights, soft particles, sound, stop action and demo scripts before pooling. No visual preview was run in this project.

Entry points:

- `Assets/Epic Toon FX/Changelog.txt`
- `Assets/Epic Toon FX/Upgrade/URP Compatibility.txt`
- `Assets/Epic Toon FX/Scripts/ETFXLightFade.cs`
- `Assets/Epic Toon FX/Scripts/ETFXPitchRandomizer.cs`
- `Assets/Epic Toon FX/Prefabs/Combat/Explosions (Misc)/HitDustExplosion.prefab`

Version evidence: `Assets/Epic Toon FX/Changelog.txt`.

## JMO Cartoon FX Remaster + Easy Editor

**Category:** partial import: effect/runtime/editor scripts only. **Decision:** Reference helpers; no ready VFX library here.

Location: `Assets/JMO Assets`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 16.

The local folder contains 16 C# scripts and metadata, including effect behavior, particle text and material/shader editor helpers. No prefabs, textures, materials or shaders were found in this root.

**Setup/API:** Inspect CFXR_Effect before integrating a separately supplied, licensed complete effect. Its camera-shake and auto-disable/destroy behavior need one clear owner. Particle text needs its supporting assets.

**Watch for:** Do not ask the agent to select a Cartoon FX prefab from this checkout: none was found. Camera shake can duplicate Feel. Demo Kino Bloom is not the URP bloom setup. Source presence does not imply a complete installed asset pack.

Entry points:

- `Assets/JMO Assets/Cartoon FX Remaster/CFXR Assets/Scripts/CFXR_Effect.cs`
- `Assets/JMO Assets/Cartoon FX Remaster/CFXR Assets/Scripts/CFXR_Effect.CameraShake.cs`
- `Assets/JMO Assets/Cartoon FX Remaster/CFXR Assets/Scripts/CFXR_ParticleText.cs`

## Particlecollection Free samples

**Category:** partial import: effect helper scripts only. **Decision:** Defer.

Location: `Assets/Particlecollection_Free samples`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 11.

The local folder contains 11 C# scripts and metadata, including custom effect and rendering/editor helpers. No actual particle prefabs, materials, textures or shaders were found here.

**Setup/API:** Inspect EffectController and RenderEffect only if a future selected effect needs them. Do not assume the sample effect content is installed.

**Watch for:** Not a mandatory dependency for Feel or DOTween. A complete licensed effect and compatible shader would need to be supplied separately before visual use. Avoid bringing example camera/input controllers into gameplay.

Entry points:

- `Assets/Particlecollection_Free samples/Plugins/Effect/Script/EffectController.cs`
- `Assets/Particlecollection_Free samples/Plugins/Effect/Script/RenderEffect.cs`

## Sparkles

**Category:** partial import: demo scripts only. **Decision:** Do not depend on this as a ready effect pack.

Location: `Assets/Sparkles`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 4.

Only four C# demo scripts and metadata were found: looping, mouse orbit, scene button and gem rotation. No sparkle effect prefab/material/texture/shader is present in this root.

**Setup/API:** Treat SparkleLoopEffect and other controllers as sample code. Choose an actual existing Epic Toon FX or Particle Image effect for current implementation instead.

**Watch for:** Folder name is not proof of usable VFX content. A future complete pack would require a separate license/import/render check; do not create references to nonexistent Sparkles assets.

Entry points:

- `Assets/Sparkles/Demo/Scene Setup/Scripts/SparkleLoopEffect.cs`
- `Assets/Sparkles/Demo/Scene Setup/Scripts/GemSimpleRotate.cs`

## Legacy Unity Standard Assets

**Category:** partial legacy sample source library. **Decision:** Reference only unless deliberately adapted.

Location: `Assets/Standard Assets`.

**Version:** Not established from inspected metadata; keep the local source fingerprint..

Local file types (excluding .meta): .cs: 88.

88 C# scripts and metadata for older cross-platform input, first/third-person controllers, cars/aircraft, follow cameras and particle utilities. No ready prefabs/materials/scenes were found in this root.

**Setup/API:** Inspect the specific subsystem if reusing it. Existing CrossPlatformInput and vehicle examples are separate architectures, not the current Input System setup.

**Watch for:** Contains legacy Input calls. CarController is not an automatically suitable tracked-vehicle motor. A future complete example scene could add conflicting cameras/input/effects; none is supplied in this folder.

Entry points:

- `Assets/Standard Assets/CrossPlatformInput/Scripts/CrossPlatformInputManager.cs`
- `Assets/Standard Assets/Vehicles/Car/Scripts/CarController.cs`
- `Assets/Standard Assets/Vehicles/Car/Scripts/CarUserControl.cs`
- `Assets/Standard Assets/Cameras/Scripts/AutoCam.cs`

## TextMesh Pro project resources and examples

**Category:** fonts/resources/examples. **Decision:** Use TMP for HUD text; keep examples out of gameplay.

Location: `Assets/TextMesh Pro`.

**Version:** runtime supplied by uGUI 2.0.0; resource asset version not established.

Local file types (excluding .meta): .asset: 18, .cginc: 4, .cs: 34, .hlsl: 1, .jpg: 10, .json: 1, .mat: 26, .png: 4, .prefab: 3, .psd: 6, .shader: 14, .shadergraph: 4, .ttf: 7, .txt: 10, .unity: 31.

Project font/material resources and sample content used with Unity TMP components.

**Setup/API:** Use TMP_Text/TextMeshProUGUI with chosen font assets and required glyphs. DOTween has TMP shortcuts and Feel has conditional TMP feedbacks.

**Watch for:** Do not install duplicate standalone TMP runtime based on an old tutorial. Keep the font/material/glyph resources actually referenced by game UI. Example scripts are not the HUD implementation.

Entry points:

- `Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshProFloatingText.cs`

## Kenney Factory Kit

**Category:** art/audio source library. **Decision:** Select original-game prototype content as needed.

Location: `Assets/1_Plugins/kenney_factory-kit_3.0`.

**Version:** Factory Kit (3.0).

Local file types (excluding .meta): .fbx: 143, .glb: 143, .html: 1, .mtl: 143, .obj: 143, .png: 150, .txt: 1, .url: 3.

Reusable source content rather than a runtime plugin. Use selectively for prototype scenery, structures or effect textures.

**Setup/API:** Keep original license files; choose assets matching the new game visual direction and create game-owned variants/materials. Names and versions above come from the bundled license heading.

**Watch for:** Bundled notices identify CC0. Actual models/materials/audio were not previewed or auditioned here. Check scale, colliders, texture import and renderer settings; folder presence does not make a prefab gameplay-ready.

Entry points:

- `Assets/1_Plugins/kenney_factory-kit_3.0/License.txt`

Version evidence: `Assets/1_Plugins/kenney_factory-kit_3.0/License.txt`.

## Kenney Nature Kit

**Category:** art/audio source library. **Decision:** Select original-game prototype content as needed.

Location: `Assets/1_Plugins/kenney_nature-kit`.

**Version:** Nature Kit (2.1).

Local file types (excluding .meta): .dae: 329, .fbx: 329, .glb: 329, .mtl: 329, .obj: 329, .png: 1640, .stl: 329, .txt: 1, .url: 3.

Reusable source content rather than a runtime plugin. Use selectively for prototype scenery, structures or effect textures.

**Setup/API:** Keep original license files; choose assets matching the new game visual direction and create game-owned variants/materials. Names and versions above come from the bundled license heading.

**Watch for:** Bundled notices identify CC0. Actual models/materials/audio were not previewed or auditioned here. Check scale, colliders, texture import and renderer settings; folder presence does not make a prefab gameplay-ready.

Entry points:

- `Assets/1_Plugins/kenney_nature-kit/License.txt`

Version evidence: `Assets/1_Plugins/kenney_nature-kit/License.txt`.

## Kenney Impact Sounds

**Category:** art/audio source library. **Decision:** Select original-game prototype content as needed.

Location: `Assets/1_Plugins/kenney_impact-sounds`.

**Version:** Impact Sounds (1.0).

Local file types (excluding .meta): .ogg: 130, .txt: 1, .url: 2.

Reusable source content rather than a runtime plugin. Impact clips are candidates for drill/deposit sound variation.

**Setup/API:** Keep original license files; choose assets matching the new game visual direction and create game-owned variants/materials. Names and versions above come from the bundled license heading.

**Watch for:** Bundled notices identify CC0. Actual models/materials/audio were not previewed or auditioned here. Check scale, colliders, texture import and renderer settings; folder presence does not make a prefab gameplay-ready.

Entry points:

- `Assets/1_Plugins/kenney_impact-sounds/License.txt`

Version evidence: `Assets/1_Plugins/kenney_impact-sounds/License.txt`.

## Kenney Particle Pack

**Category:** art/audio source library. **Decision:** Select original-game prototype content as needed.

Location: `Assets/1_Plugins/kenney_particle-pack`.

**Version:** Particle Pack (1.1).

Local file types (excluding .meta): .png: 193, .txt: 1.

Reusable source content rather than a runtime plugin. Use selectively for prototype scenery, structures or effect textures.

**Setup/API:** Keep original license files; choose assets matching the new game visual direction and create game-owned variants/materials. Names and versions above come from the bundled license heading.

**Watch for:** Bundled notices identify CC0. Actual models/materials/audio were not previewed or auditioned here. Check scale, colliders, texture import and renderer settings; folder presence does not make a prefab gameplay-ready.

Entry points:

- `Assets/1_Plugins/kenney_particle-pack/License.txt`

Version evidence: `Assets/1_Plugins/kenney_particle-pack/License.txt`.

## Direct Unity packages

These values are read from Packages/manifest.json and compared with packages-lock.json. A package being installed does not mean its feature is used by a gameplay scene. Unity built-in modules are retained in the JSON inventory.

| Package | Manifest / lock version | Purpose here |
|---|---|---|
| `com.unity.ai.navigation` | 2.0.16 / 2.0.16 | NavMesh authoring/runtime support; optional for later NPC/helper or route visualization, not required for direct player steering. |
| `com.unity.collab-proxy` | 2.12.4 / 2.12.4 | Unity Version Control integration; not a gameplay dependency or a reason to change the existing Git workflow. |
| `com.unity.ide.rider` | 3.0.39 / 3.0.39 | Rider IDE integration; editor convenience. |
| `com.unity.ide.visualstudio` | 2.0.26 / 2.0.26 | Visual Studio IDE integration; editor convenience. |
| `com.unity.inputsystem` | 1.19.0 / 1.19.0 | Primary input package; use actions and the matching UI module for new input. |
| `com.unity.multiplayer.center` | 1.0.1 / 1.0.1 | Editor guidance for multiplayer tooling; not evidence that networking is implemented. |
| `com.unity.render-pipelines.universal` | 17.3.0 / 17.3.0 | Active graphics framework; check materials/effects against Android renderer settings. |
| `com.unity.test-framework` | 1.6.0 / 1.6.0 | Edit/Play Mode tests when implementing behavior; this research did not run them. |
| `com.unity.timeline` | 1.8.12 / 1.8.12 | Authored temporal/cinematic tracks; optional and unnecessary for a few UI pulses. |
| `com.unity.ugui` | 2.0.0 / 2.0.0 | Canvas/uGUI and Unity 6 TMP runtime used by the installed UI tools. |
| `com.unity.visualscripting` | 1.9.11 / 1.9.11 | Unity graph-based scripting; distinct from DOTween Pro visual components. No graph architecture chosen. |
| `com.unity.pipeline` | 0.9.1-exp.1 / 0.9.1-exp.1 | Experimental package entry. Its actual role/API was not established by manifest inspection; do not assume it is URP or needed for gameplay. |

Transitive package dependencies are described by the lockfile, not duplicated as independent purchases. Burst, Collections and Mathematics are present in that lockfile and relevant to Particle Image.

## Things this catalog does not claim

- No new assets need purchasing for the proposed first feedback pass.
- No claim is made that every plugin is necessary or compatible simply because it exists on disk.
- Missing explicit versions are not assigned a current Asset Store version.
- No ad mediation, IAP or analytics SDK was established from the inspected named plugin roots/direct manifest. Dependencies observed in the reference APK belong to that other game, not automatically to this project.
- Demo scripts, template folders and internal helper libraries are not separate gameplay architectures to adopt wholesale.
