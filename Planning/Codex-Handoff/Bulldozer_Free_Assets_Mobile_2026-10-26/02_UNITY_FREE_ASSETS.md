# Separate guide: free Unity Asset Store options

All seven listings below showed **FREE on 26 October 2026**. Sizes are store package sizes, not build size, texture memory or a performance rating. None was downloaded or imported in this review.

Local project baseline: Unity **6000.3.13f1**, URP **17.3.0**, Input System **1.19.0**, uGUI **2.0.0**. Compatibility labels are publisher statements; actual project validation is still required.

| Asset | Listed version / size | What we can use | Decision |
|---|---|---|---|
| [First Person + Third Person — Character Controllers, Unity](https://marketplace.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526) | 2.0.1 / 76.7 MB | On-foot movement/camera starting point; description includes mobile UI Toolkit overlays, Input System and Cinemachine | **Alternative to installed KCC**, not an additional motor. Current listing includes URP/6000.3 compatibility. It does not control a bulldozer. |
| [Human Basic Motions FREE, Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271) | 2.4.2 / 15.3 MB | Humanoid locomotion clips for the worker | Optional backup if Quaternius clips do not meet our needs. Test retargeting; enter/exit animation coverage is unverified. |
| [Low-Poly Simple Nature Pack, JustCreate](https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-simple-nature-pack-162153) | 1.24 / 1.9 MB | 24 prefabs / shared texture in indexed description; pipeline conversion packages described | Good alternative style, but **not needed while using installed Kenney Nature**. Verify URP 17.3 conversion. |
| [Low Poly Road Pack, Broken Vector](https://assetstore.unity.com/packages/3d/environments/roadways/low-poly-road-pack-67288) | 1.3 / 1.2 MB | Roads; author describes 61 tiles and two cars | Choose this OR Kenney Roads. Original Unity 5.6 package: budget for material setup. |
| [Free Low Poly Pack, Broken Vector](https://assetstore.unity.com/packages/3d/free-low-poly-pack-65375) | 1.2 / 91.9 KB | Two trees, one rock, one fence | Small fallback; redundant with existing nature assets. Old shaders need inspection. |
| [FREE Casual Game SFX Pack, Dustyroom](https://assetstore.unity.com/packages/audio/sound-fx/free-casual-game-sfx-pack-54116) | 1.1 / 8.6 MB | Audition UI/reward cues | Useful optional small addition. Select a few clips; no engine-loop claim. |
| [POLYGON Starter Pack, Synty](https://assetstore.unity.com/packages/3d/environments/polygon-starter-pack-art-by-synty-156819) | 1.2.1 / 109.4 MB | Alternative prototype art | Defer given storage constraints. This is **not POLYGON Construction**; vehicle contents are unverified. |

All except the Unity controller list the Standard Unity Asset Store EULA. The controller lists a non-standard EULA and its description identifies the Unity Companion License. See [license handling](05_LICENSES_AND_ATTRIBUTION.md). Broken Vector's [author road-pack page](https://brokenvector.itch.io/low-poly-road-pack) corroborates contents, but no CC0 assumption is made for that distribution.

## What to use first

Start the worker with the selected Quaternius model/animations and assess the **existing Kinematic Character Controller** integration. Keep one motor. If it takes more effort than the official controller for this simple walking loop, evaluate the Unity controller in an isolated test scene, then choose one. Do not put both on the worker.

The official controller brings Input System/Cinemachine dependencies. Input System is already installed; **Cinemachine is not in the current manifest**. Review any dependency changes before accepting an import. Mobile UI Toolkit overlays are available in its description, but our existing uGUI joystick/HUD can remain the common input presentation if deliberately wired. Avoid duplicate EventSystems or two touch sticks driving the same actor.

Use Human Basic Motions only for missing locomotion needs, rather than importing another complete animation library preemptively. Art licensing and character-to-clip retargeting are separate checks.

## Existing tools already cover these needs

| Need | Existing choice to reuse |
|---|---|
| UI movement and pickup presentation | DOTween Pro/core |
| Contact/break/delivery feedback | Feel; keep gameplay state independent of effects |
| Touch movement | Input System + one Joystick Pack control |
| Notches and screen edges | Existing SafeArea utility |
| World impact particles | A few Epic Toon FX effects with reduced particles/materials |
| Third-person worker motor | Existing KCC is a candidate; verify implementation, not just folder presence |

The earlier `Bulldozer_Plugin_Reference_2026-10-26` pack documents their actual local versions/APIs. No new tween, haptic, joystick or large VFX framework is required for this asset plan. `com.unity.modules.adaptiveperformance` in the manifest is an engine module, not evidence that a provider-backed thermal/performance system is configured.

## Two misleading links to avoid

- [Low-Poly Dump Truck, Premisul](https://assetstore.unity.com/packages/3d/vehicles/land/low-poly-dump-truck-244775) currently shows **$4.99 USD**, despite older free-asset references. It is outside this free shortlist.
- [Older Starter Assets URP listing, ID 267961](https://assetstore.unity.com/packages/essentials/starter-assets-character-controllers-urp-267961) is deprecated for new acquisition. Use current ID **196526** if choosing the official controller.

Import only chosen models, materials, textures, animations and required scripts. Keep demos out of build scenes. Inspect dependencies before excluding files; deleting random vendor files can break references. Larger package size alone does not make an asset slow, and tiny packages can still produce expensive shaders or excessive draw calls.
