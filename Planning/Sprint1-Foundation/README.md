# Sprint 1 foundation results

26 October 2026. Target: Android and iOS. Reference: [Bulldozer Master: Crush & Mine](https://play.google.com/store/apps/details?id=io.supercent.bulldozermasters&hl=en). This sprint delivers an audit, licensed asset selection, two installed tools, and an isolated character animation test. Environment creation remains Sprint 2.

| Deliverable | Result |
|---|---|
| [Project audit](PROJECT_AUDIT.md) | Current files, actual Editor version, reusable content, integration gaps and risks |
| [Asset register](ASSET_REGISTER.md) | Sources, formats, licenses, acquisition status and intended uses |
| [Tooling and validation](TOOLS_AND_VALIDATION.md) | Exact package additions, playback evidence, known errors and device-test limits |
| [Improvement and Sprint 2 plan](SPRINT2_PLAN.md) | Original mechanics, controls, presentation and environment acceptance gates |
| Character test | `Assets/Sprint1Foundation/CharacterAnimationTest.unity`; four automatic animation loops |

Installed Cinemachine 3.1.7 and Android Logcat 1.4.7 through Unity Package Manager. Imported one CC0 Quaternius worker FBX. Authored four simple Blender test motions after the pack's animation download returned a quota error. Created four Animator Controllers, URP materials, a worker prefab and a test scene. No gameplay environment, production controller, shipping UI or progression system was built.

Open the test scene, enter Play Mode and focus Unity's Game view. The hierarchy names identify Idle, Walk, Run and Interact. From the front-facing camera, screen order is reversed: Interact, Run, Walk, Idle. These are in-place diagnostic loops, not production locomotion. The original MainMenu scene is restored after validation; the build scene list is unchanged.

The saved screenshots are camera captures, not phone screenshots. See `Assets/Sprint1Foundation/AnimationTestPreview.png`. Reproducible Blender source and the Unity scene-generation script are in `Source/`; downloaded-file hashes and actual validation responses are in `Evidence/`.

Remaining gates: choose baseline Android and iPhone devices; install platform build modules; resolve the existing hierarchy-icons exception and legacy-input integration; optimize worker material slots; test production foot contact and mobile controls. No Android/iOS build or performance certification is claimed.
