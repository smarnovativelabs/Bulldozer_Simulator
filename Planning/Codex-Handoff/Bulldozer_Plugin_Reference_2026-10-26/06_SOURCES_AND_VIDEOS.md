# Sources, video coverage and evidence limits

Research date: 26 October 2026. Installed source is the primary technical reference for this pack. Current publisher documentation supplies context and update requirements. Recommendations and effect timings are original design proposals.

## Evidence hierarchy

1. **Local code/configuration:** methods, guards, defaults, package versions and source defects recorded with paths/line numbers. Exact source file hashes are preserved.
2. **Local version declarations:** Feel/Nice Vibrations readmes, package.json files, source headers, DLL product strings and ETFX changelog. These do not prove the whole folder is an untouched upstream release.
3. **Publisher pages:** current capabilities, release requirements and migration guidance. They may describe versions newer than this checkout.
4. **Supplied videos:** verified titles/creators and selected paused samples. They provide learning context, not a version-matched implementation contract.
5. **Engineering proposals:** plugin ownership, event names, suggested tuning, adoption order and future tests. They have not been validated in a running game.

No vendor source archive, APK assets, video files or full transcripts are included. Paths in the catalog are project-relative so the same notes work on the laptop.

## Supplied DOTween links

| Source | Access and useful result |
|---|---|
| [Asset Store: DOTween Pro](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) | Listing read. Current listing: 1.0.435, released 12 October 2026. This is not the installed version. |
| [Publisher: DOTween Pro](https://dotween.demigiant.com/pro.php) | Web reader timed out; the browser successfully loaded the overview/API/changelog. Used for current Pro capability distinctions and migration context. |
| [DOTween Pro — Animation Visual Editor](https://www.youtube.com/watch?v=gkI7iQ23pw0) | Daniele Giardini (Demigiant); displayed duration ~7:06. Page/description read and sample sought to 2:07; text/fade demonstration context visible. Useful for the visual Animation component. |
| [ANIMATE LIKE A PRO with DOTWEEN — Unity Plugin Review](https://www.youtube.com/watch?v=rwjhXCmXyO0) | TapHub Games; ~9:49. Title/page read and sample sought to 4:54 in the Animation component chapter. Use as an overview; technical API claims in this pack rely on local/publisher evidence instead. |
| [DOTween Pro — Path Visual Editor](https://www.youtube.com/watch?v=XkJoSNlRMWI) | Daniele Giardini (Demigiant); ~5:26. Description read and sample sought to 2:43; waypoint/live-preview context visible. Useful for authored paths, with publisher restrictions still applying. |

The older publisher videos show an older Editor UI. Follow the installed source for replay, auto-kill and supported animation types. Samples are observation locations, not a complete chapter map or proof every frame at that timestamp was reviewed.

## Supplied Feel links

| Source | Access and useful result |
|---|---|
| [Asset Store: Feel](https://assetstore.unity.com/packages/tools/particles-effects/feel-183370) | Listing read. Current listing: 6.3, released 20 October 2026; local readme says 5.6.1. |
| [Feel release history](https://feel.moremountains.com/feel-releases) | Read version/Editor requirements and selected release changes. Feel 6.x has a 6000.5.0f1+ floor, newer than this project's 6000.3 Editor. |
| [FEEL For Unity — Make Your Game Feel Good!](https://www.youtube.com/watch?v=uL92FChNHoE) | The RealTime Essentials; ~6:42. Page read and sample sought to 3:21 in the installation/enabling chapter. Useful introductory overview; technical details are verified against local/publisher sources. |
| [Feel Tutorial: Getting Started](https://www.youtube.com/watch?v=gK_7etknD4k) | More Mountains; ~35:23. Page/description read and sample sought to 17:41 in “Adding more feedbacks.” Description warns about the transition from MMFeedbacks to MMF_Player. Useful conceptual workflow, not current component/API naming. |
| [Feel Tutorial: Advanced Features](https://www.youtube.com/watch?v=RFmi4UcQxgg) | More Mountains; ~1:08:59. Page read and sample sought to 34:29 in “MMSequencer”; a progress-bar/demo context was visible. Advanced utilities are optional for Sprint 1. |

**Video coverage:** all six supplied video pages were accessed through the browser after the web reader failed. Titles, creators and durations were verified; selected frames/captions were sampled. The videos were not watched end to end, no full transcript was captured, and no hidden audio content is claimed as reviewed. This pack covers their relevant tools mainly through the much more precise installed source and official documentation.

## Additional official technical references

| Reference | How it was used |
|---|---|
| [DOTween API](https://dotween.demigiant.com/documentation.php) | Cross-check/search companion; local XML/modules determine the available APIs |
| [DOTween support and upgrades](https://dotween.demigiant.com/support.php) | Assembly-definition setup, older-version migration and lifecycle guidance |
| [Feel MMF_Player](https://feel-docs.moremountains.com/mmf-player.html) | Current concepts and legacy naming comparison; newer controls gated by local inspection |
| [Feel installation](https://feel-docs.moremountains.com/how-to-install.html) | Current minimum Editor and upgrade/dependency guidance |
| [Feel render pipelines](https://feel-docs.moremountains.com/render-pipelines.html) | Runtime/rendering distinction and URP/HDRP-specific effects |
| [Feel MMSoundManager](https://feel-docs.moremountains.com/mmsoundmanager.html) | Sound-system overview and identification of newer track features absent locally |
| [Feel screen shakes](https://feel-docs.moremountains.com/screen-shakes.html) | Companion reference for shaker setup; local CameraShake/shaker code was inspected |
| [Feel object pooling](https://feel-docs.moremountains.com/mmtools-object-pooling.html) | Companion reference; local particle pooling behavior takes precedence |
| [Input System 1.19 installation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Installation.html) | Backend handling context from the earlier repository review |

The initial guessed Feel paths `/urp.html` and `/mm-sound-manager.html` were inaccessible; the working links above were obtained from the publisher navigation. Do not reuse those guessed URLs.

## Local evidence that answers common future questions

| Question | File to inspect |
|---|---|
| What did this project actually install? | Packages/manifest.json, Packages/packages-lock.json, plugin_inventory.json |
| Which Unity version? | ProjectSettings/ProjectVersion.txt |
| Is old input enabled? | ProjectSettings/ProjectSettings.asset: activeInputHandler |
| What are the DOTween versions? | Product strings in DOTween.dll/DOTweenPro.dll; hashes in source_fingerprints.json |
| Which DOTween UI methods exist? | Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUI.cs |
| Why does a pooled DOTweenAnimation only play once? | DOTweenAnimation.cs: autoKill, DORestart, OnDestroy |
| What Feel methods exist here? | MMF_Player.cs and its base MMFeedbacks.cs |
| Why do particles ignore a passed hit point? | MMF_ParticlesInstantiation.cs: PositionModes.Script |
| Why are haptic feedbacks missing on another platform? | MMF_NVPreset.cs guards and per-target scriptingDefineSymbols |
| Which joystick source has the recursion? | Assets/Unity_Packages/Joystick Pack/Scripts/Base/Joystick.cs:24 |
| Which packages use which assembly? | The installed .asmdef/.asmref files, not inferred namespace names |
| Is an unknown plugin version current? | Unknown means unestablished. Do not substitute a current website version. |

## What still needs a targeted check later

Unity import/compilation, native Android plugin linking, actual device input/haptics, selected particle shaders, UI masking, runtime pool behavior, performance and package GUID resolution remain untested. Reopen a source only when its fingerprint changes, an error points to it, or a newly requested feature falls outside this reference. A new feature can require deeper investigation even when existing files are unchanged.
