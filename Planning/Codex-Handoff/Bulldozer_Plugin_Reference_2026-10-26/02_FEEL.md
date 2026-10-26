# Feel 5.6.1 — local implementation guide

## What is installed

`Assets/Feel/readme.txt` identifies **Feel 5.6.1**. The package includes MMFeedbacks, MMTools, Nice Vibrations and several demo families. Nice Vibrations is **4.1.2**, using Lofelt SDK **1.3.4**, according to its local readme. This project uses Unity 6000.3.13f1 and URP 17.3.0.

Use `MoreMountains.Feedbacks.MMF_Player` as the main component for new effects. `MMF_Feedback` is one item in its serialized feedback list. The remaining `MMFeedbacks` class provides legacy/base behavior; old videos using that name do not mean new work should instantiate the old workflow.

Online docs currently describe Feel 6.3. Useful concepts carry across, but the current docs are not a version-matched API contract for this project. [MMF_Player documentation](https://feel-docs.moremountains.com/mmf-player.html)

## Recommended role in this game

Make small named players such as `FX_DrillHit`, `FX_DepositBreak`, `FX_PickupAccepted`, `FX_DeliveryCommitted` and `FX_UpgradePurchased`. Gameplay code emits an event only after its state change succeeds; a presentation adapter then plays the appropriate player.

Feel can coordinate particles, audio, transforms, UI, camera effects, time effects and haptics. It should not own resource counts, prices, damage validation, unlock rules or save transactions. A cosmetic player can be missing, disabled or rejected by its cooldown without breaking game progression.

Use one feedback owner per event/property. If DOTween owns a cargo icon's scale pulse, the Feel player should handle its sound/particles instead of also scaling that icon.

## Minimal scene setup for the installed version

1. Create an empty GameObject with `MMF_Player` in your own gameplay hierarchy.
2. Leave Auto Play on Start/Enable off for events. Assign explicit targets and keep missing-target diagnostics visible during setup.
3. Add only the feedbacks needed for one event; start with scale of a visual child plus a sound or existing ParticleSystem.
4. Bind the player to a presentation script or a UnityEvent and call `PlayFeedbacks()` after the gameplay action succeeds.
5. Test its Inspector Initialize/Play/Stop/Reset controls in Play Mode. Verify the target returns to its baseline when interrupted.
6. Add cooldown/repeat/range behavior deliberately. Most feedbacks begin together unless you configure delays/pauses; moving an item lower in the list does not automatically mean “wait until the previous one finishes.”

This installed player has no Feel 6 sequential PlayMode selector. For sequencing in 5.6.1, inspect `MMF_Pause`, initial delays and holding pauses, or let one external presentation owner schedule the events. Do not paste `PlayMode = Sequential` from current examples.

## Local API and lifecycle

Primary file: `Assets/Feel/MMFeedbacks/MMFeedbacks/Core/MMF_Player/MMF_Player.cs`.

| Method/setting | Local behavior and use |
|---|---|
| `Initialization(bool forceInitIfPlaying = false)` | Initializes each listed feedback; call after runtime binding where needed |
| `Initialization(GameObject owner)` | This override forwards to parameterless initialization; do not assume it binds an external owner the way old examples imply |
| `PlayFeedbacks()` | Plays using the player transform position and configured intensity |
| `PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1f, bool forceChangeDirection = false)` | Supplies a contextual position/intensity; each feedback decides how to use it |
| `StopFeedbacks()` | Stops the player and asks individual feedbacks to stop |
| `ResetFeedbacks()` | Calls each active feedback's reset behavior; it is not a universal transform rollback |
| `RestoreInitialValues()` | Asks active feedbacks to restore captured values; only implemented behaviors can restore themselves |
| `ResetAllCooldowns()` | Available for controlled reuse; do not clear it every frame to bypass throttling |
| `StopFeedbacksOnDisable`, `RestoreInitialValuesOnDisable` | Both default false in this installed source; configure for the intended lifecycle |
| `CanPlayWhileAlreadyPlaying` | Defaults true in the base; choose whether repeated hits should overlap |
| `CooldownDuration`, `ChanceToPlay`, `OnlyPlayIfWithinRange` | Cosmetic gating; never use them to decide whether damage/collection happened |

Original, illustrative call site; not installed or compiled:

```csharp
using MoreMountains.Feedbacks;
using UnityEngine;

public sealed class DepositFeedbackView : MonoBehaviour
{
    [SerializeField] private MMF_Player hit;

    // Call after the deposit's game logic accepts damage.
    public void OnDamageApplied(Vector3 worldPoint)
    {
        if (hit != null && hit.isActiveAndEnabled)
            hit.PlayFeedbacks(worldPoint, 1f);
    }

    public void BeforeReturningToPool()
    {
        if (hit == null) return;
        hit.StopFeedbacks();
        hit.RestoreInitialValues();
        hit.ResetAllCooldowns();
    }
}
```

Pool spawn should bind targets and capture/reset baselines before playing. Pool return should stop then restore while targets still exist. Destroy/break effects that must outlive a deposit should belong to a separate effect object/service, not a GameObject immediately disabled with the deposit.

## Feedbacks already available and useful here

| Feedback/system | Appropriate use | Required setup |
|---|---|---|
| `MMF_Scale` | A short hit pulse on a deposit's visual child | Assign transform, keep collider/motor root stable |
| `MMF_Particles` | Play an already-placed particle system | Assign the system; decide whether replay clears current particles |
| `MMF_ParticlesInstantiation` | Spawn impact/debris at each hit location | Assign prefab, delivery mode and position mode |
| `MMF_Sound` | Small isolated sound feedback | Configure clip/audio behavior in the installed inspector |
| `MMF_MMSoundManagerSound` | Pooled/mixed UI and world sound | One configured MMSoundManager and a clip; select track/spatial blend |
| `MMF_CameraShake` | Subtle break/upgrade emphasis | Matching `MMCameraShaker` receiver/channel; avoid follow-camera ownership conflict |
| `MMF_FloatingText` | Display a delivery amount | Configured floating-text spawner/prefab and matching channel |
| `MMF_Pause` | Deliberate feedback sequence pauses | Inspect pause type and actual declared duration of prior effects |
| `MMF_NVPreset`, `MMF_NVEmphasis` | Brief mobile touch feedback | Nice Vibrations symbol/platform support and user preference |
| URP feedbacks and shakers | Optional volume changes | URP Volume, target overrides and corresponding shakers |
| `MMProgressBar` | Cargo/goal display | Bound foreground/background bars; feed actual game values |

The generated API index contains the full locally discovered feedback menu list, including less relevant categories, so an agent can search it offline. Availability in a compiled Inspector may still depend on package/define conditions.

## Particles: the most useful details

Local `MMF_ParticlesInstantiation.cs` offers **Cached, OnDemand and Pool** modes, with Pool the source default and initial pool size 5. Position modes are **FeedbackPosition, Transform, WorldPosition and Script**.

For a contextual hit call such as `PlayFeedbacks(hitPoint)`, select **Script** position mode; otherwise passing a position is not enough to make this particular feedback spawn there. Use a scene-level/shared effect owner where repeated hits would otherwise allocate many per-deposit pools. `MutualizePools` exists but defaults false; enable only after testing lifecycle and prefab ownership.

Set particle lifetimes and `DeclaredDuration` coherently: the latter communicates timing to the feedback player, rather than changing the particle system's lifetime. Pooled particles must return to a usable disabled state; a prefab that destroys itself on stop is unsuitable without adapting its lifecycle. Confirm that disabling the deposit does not disable its still-visible break effect.

Set budget limits for concurrent effects. Pooling avoids some instantiate churn; it does not eliminate GPU overdraw or make unlimited particles cheap. Keep spawn counts, particle sizes, lights and distortion modest on mobile.

## Camera, time and accessibility

The installed `MMF_CameraShake` broadcasts an event. A compatible shaker must receive it. Separate the follow rig from a shake child so both systems do not write the same transform. Match channels and stop looping shakes on state changes. The local automatic setup can restructure the camera hierarchy; review that change instead of running it on an established rig without inspection.

Use very small shake for breaking a deposit, and consider no shake for ordinary pickup. Provide a reduced-motion setting and a haptics toggle. Do not rely on motion, color or vibration as the only signal that cargo is full.

`MMTimeManager` and freeze/time feedbacks exist, but global time effects are a poor first dependency for a mobile collection loop. Defer hit-stop until pause, UI animation, input and pooled lifetimes work. If adopted, one system must own time-scale restoration, including interruption and scene changes.

## Sound ownership

MMSoundManager is optional and built on Unity audio. The installed implementation has Master/Music/Sfx/UI track concepts, audio source pooling and per-sound controls. Source default pool size is 10 and expansion is allowed; this is not a project performance recommendation. Use UI sounds in 2D and choose spatial blend for world impacts consciously.

`MMF_MMSoundManagerSound` exposes volume/pitch ranges, clip variation, loop/persistent settings, `DoNotPlayIfClipAlreadyPlaying`, and `StopSoundOnFeedbackStop`. Do not let each successful physics contact start a new long looping engine sound. Use one owned loop for the vehicle, with an explicit stop path.

The current online sound guide includes newer custom-track and track-pitch features. The local `MMSoundManager.cs` does not expose that newer track-pitch API. Use the installed inspector/signatures. [Current sound guide](https://feel-docs.moremountains.com/mmsoundmanager.html)

## Haptics

Use current installed `MMF_NV*` feedbacks, not `MMF_Haptics`, whose local menu explicitly says deprecated. `MMF_NVPreset` is under `MoreMountains.FeedbacksForThirdParty` and is gated by `MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED`. The project defines this symbol for **Android** in ProjectSettings; do not assume another target has the same symbol.

The local preset feedback also checks `HapticSettings` and device eligibility. An Editor click is not proof of vibration on a phone. Add a real Android device check, respect a player preference, and throttle frequent hits. Use either Nice Vibrations or the separate CandyCoded plugin behind one game adapter, not both for the same event.

## URP, assembly definitions and demos

Feel runtime feedback logic is broadly separate from rendering; materials, demos and post-processing targets are pipeline-specific. The **local 5.6.1 readme** describes a URP demo override package, while **current 6.x docs** describe URP demos as the default. Follow the local package layout; do not import all demos or HDRP just to get a scale/audio feedback. The referenced upgrade archive was not found as a `.unitypackage` in the inspected checkout. [Render pipeline guide](https://feel-docs.moremountains.com/render-pipelines.html)

This checkout consolidates Feel runtime asmrefs onto `MoreMountains.Tools`, and editor asmrefs onto `MoreMountains.Tools.Editor`. Their locations are under `Assets/Feel/MMTools/Core/`. Do not assume a stock multi-assembly layout or delete MMTools because the name sounds optional. Many feedbacks depend on it. Package GUID references still need to resolve during an actual Unity import.

Cinemachine, the separate Built-in Post Processing package, HDRP and VFX Graph are not direct manifest dependencies in this project. Add one only for an intentionally chosen feature. The base camera-shake route does not require Cinemachine. Some demos use old input; a demo input exception does not prove the whole feedback framework is unusable.

## Version boundary

Feel 6.3 is the online release on the research date and requires Unity 6000.5.0f1+. Newer sequential playback, MMF_Player event-channel control and certain audio features are not established APIs for this local 5.6.1 copy. Use direct serialized references and the local signatures for Sprint 1. A future upgrade must include a compatible Editor decision, prefab/serialization checks, compile, build and device verification. [Release history](https://feel.moremountains.com/feel-releases) · [Current installation guide](https://feel-docs.moremountains.com/how-to-install.html)
