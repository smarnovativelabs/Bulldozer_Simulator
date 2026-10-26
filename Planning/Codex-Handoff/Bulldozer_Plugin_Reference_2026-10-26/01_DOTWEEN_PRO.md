# DOTween Pro — local implementation guide

## Baseline and evidence

Local root: `Assets/Plugins/Demigiant/`. Core DLL contains product string **1.2.420**; Pro DLL contains **1.0.244**. Assembly/file-version strings also include `1.0.0.0`, which must not be mistaken for the product version. These were inspected as binary data, not executed. Individual loose scripts may differ from the release indicated by a DLL.

Source entry points:

- `DOTween/DOTween.XML`: installed core signatures and lifecycle documentation.
- `DOTween/Modules/DOTweenModuleUI.cs`: CanvasGroup, Image and RectTransform extensions.
- `DOTween/Modules/DOTweenModulePhysics.cs`: Rigidbody extensions.
- `DOTweenPro/DOTweenAnimation.cs`: visual component behavior.
- `DOTweenPro/DOTweenTextMeshPro.cs`: TMP shortcuts and per-character animator.
- `DOTweenPro/DOTweenPro.XML`: path component API.
- `DOTweenPro/Editor/DOTweenAnimationInspector.cs`: actual installed Inspector features.

Paths above are relative to the Demigiant root. The machine-readable API index supplies full project-relative paths and line numbers.

## What to use it for

Use DOTween for explicitly controlled value changes: opening the upgrade panel, fading a tooltip, pulsing a cargo icon, sliding a tutorial arrow, cosmetic resource movement, or counting a displayed value toward an already-committed balance.

Core already provides code tweens, easing and sequences. Pro adds visual Animation/Path components and extra scripting support such as TMP per-character effects. Pro is not required merely to call `DOMove` or build a sequence. The publisher describes DOTween Path as a simple path editor with restrictions on path rotation and UI RectTransforms. Use RectTransform code tweens for screen-space UI routes. [Publisher overview](https://dotween.demigiant.com/pro.php)

For this project, use code for dynamic targets and pooled resource visuals; use `DOTweenAnimation` for stable, designer-tuned UI/prefab motion. Leave DOTween Path for authored decorative routes or later delivery presentation. Collision-aware vehicle navigation needs its own motor/pathfinding logic.

## Setup when the project is opened later

1. First let the existing project compile and record the first actual error. Do not replace DLLs simply because they are old.
2. Open **Tools > Demigiant > DOTween Utility Panel**, inspect setup/modules, and run Setup if required by the import state.
3. Local Audio, Physics, Physics2D, Sprite, UI and TMP modules have enabled source guards. Keep only supported modules enabled through the utility. TMP belongs to the Unity 6 uGUI package in this project; importing a second TMP package is not the first fix for missing types.
4. Use `using DG.Tweening;` in consumer scripts. Inspect compiler output if a shortcut is absent.
5. If game code uses a custom asmdef, loose module/Pro scripts cannot simply be referenced as Assembly-CSharp from that asmdef. Generate DOTween assembly definitions with its utility, then reference the resulting assemblies. Inspect the actual generated names instead of inventing assembly references. [Publisher support](https://dotween.demigiant.com/support.php)
6. Confirm one copy of DOTween core, Pro and DemiLib. Pro already includes core; do not import a second free copy alongside it.

No DOTween settings asset was found in the inspected Assets tree. The source modules show what is enabled, but the Utility Panel defaults/Safe Mode state still need confirmation inside Unity.

## Installed APIs to reach for

| Operation | API family | Project use |
|---|---|---|
| Move world/local visual | `DOMove`, `DOLocalMove` | Decorative debris or a cosmetic pickup |
| Move UI | `DOAnchorPos` | Panels and reward icons in Canvas coordinates |
| Scale or pulse | `DOScale`, `DOPunchScale` | Cargo/upgrade confirmation |
| Fade | `CanvasGroup.DOFade`, `Image.DOFade`, TMP `DOFade` | Menus and labels |
| Animate a filled Image | `Image.DOFillAmount` | Display-only cargo/progress fill |
| Numeric interpolation | `DOTween.To` | A displayed count, separate from the saved count |
| Order effects | `DOTween.Sequence`, `Append`, `Join`, `Insert`, `AppendInterval` | A controlled panel/reward presentation |
| Control lifetime | `Kill`, `Pause`, `Restart`, `SetAutoKill`, `OnKill` | Pool return, screen close and replay |
| Time mode | `SetUpdate(true)` | A menu that remains animated while gameplay is paused |
| TMP characters | `DOTweenTMPAnimator` | Optional title/reward flourish, after basic UI works |

This table comes from local module source/XML. The [online API](https://dotween.demigiant.com/documentation.php) is a searchable companion, but its newest features may not exist in these DLLs.

## Sequence and ownership rules

Use `Append` for one action after another and `Join` for actions starting at the preceding append position. Use `Insert` for an explicit timeline position. Assemble the sequence before it starts; hold one reference to the outer sequence so cancellation affects the whole presentation.

Assign a single writer to each property. A Feel scale feedback and a DOTween scale tween on the same transform will compete. A LayoutGroup, Animator or movement controller can also overwrite a tween. Animate a dedicated visual child when the parent owns layout, physics or navigation.

`SetAutoKill(false)` retains a completed tween for replay. It is different from `SetRecyclable`, which concerns reusing tween storage after killing. If recycling is used, clear references on kill. Default auto-kill is appropriate for one-shot sequences that are recreated on demand.

Local XML says `SetLink(gameObject)` kills on destruction and has no effect when applied to a tween nested inside a sequence. Link/control the outer sequence. Disabling a pooled object is not destruction: handle pool return explicitly, or use a verified lifecycle link policy. Do not call global `DOTween.KillAll()` when closing one panel.

## Original illustrative pattern: repeatable cosmetic pulse

This sample is reference code, not installed or compiled. Put it in your own game namespace and asmdef when implementing it. Give it exclusive ownership of the selected visual child's scale; do not attach another scale animator to that child.

```csharp
using DG.Tweening;
using UnityEngine;

public sealed class CargoIconPulse : MonoBehaviour
{
    [SerializeField] private Transform visual;
    private Vector3 restScale;
    private Sequence pulse;

    private void Awake()
    {
        if (visual != null) restScale = visual.localScale;
    }

    public void PlayPulse()
    {
        if (!isActiveAndEnabled || visual == null) return;
        StopPulse();
        pulse = DOTween.Sequence();
        pulse.Append(visual.DOScale(restScale * 1.10f, 0.08f));
        pulse.Append(visual.DOScale(restScale, 0.14f));
        pulse.SetUpdate(true);
        pulse.SetLink(gameObject);
        pulse.OnKill(() => pulse = null);
    }

    private void StopPulse()
    {
        pulse?.Kill();
        pulse = null;
        if (visual != null) visual.localScale = restScale;
    }

    private void OnDisable() => StopPulse();
}
```

The timings are proposed tuning values. They are not reference-game measurements. For a prefab whose scale changes after Awake, explicitly recapture the new resting scale after binding it; do not accumulate pulses from an already stretched scale.

## DOTweenAnimation details that matter in this installed copy

- `autoPlay` and `autoKill` default to true in source. Disable autoplay for event-triggered UI. Set auto-kill false for a retained replayable animation, or recreate it deliberately.
- Creation happens in `Awake`, except some target-based UI moves deferred to `Start`. Calling replay before creation can be a no-op.
- `DORestart(bool fromHere)` returns if the tween reference is null. It does not automatically recreate an auto-killed tween. Its `fromHere` reevaluation is conditional on a relative tween.
- `OnDestroy` kills the tween; there is no `OnDisable` handler in this loose component. Add pool/screen lifecycle handling or inspect the Pro Visual Manager when adopting it.
- Component control methods use the GameObject target and can affect multiple animations on that object. Keep unrelated behaviors on separate visual children or use explicit IDs/references.
- The installed AnimationType enum has no `FillAmount` entry. `Image.DOFillAmount` exists in the UI code module; use that for a fill bar. The current web Inspector may show features absent locally.
- `RecreateTween`/`RewindThenRecreate` are not methods in this installed `DOTweenAnimation.cs`. Avoid pasting examples that assume them.
- Setting a transform as a move endpoint is not continuous following. Recalculate an endpoint deliberately if the destination moves; for world-to-HUD effects, convert coordinates with the correct Canvas camera and coordinate space.

## Common integration problems

| Symptom | Check first |
|---|---|
| Missing `DOFade` or `DOAnchorPos` | Module enabled, correct target type, `DG.Tweening` import, asmdef visibility |
| Replay works once | Auto-kill removed the tween; restart alone cannot recover it |
| Pool item jumps to old location | Kill old presentation, reset local state, bind new endpoints before creating a new tween |
| UI continues to intercept taps after fading | CanvasGroup alpha is only visual; set interactable/blocksRaycasts as appropriate |
| Scale/position jitters | Another writer: Feel, Animator, LayoutGroup, motor or follow camera |
| Menu freezes during pause | Set independent update on the owning tween/sequence; verify input and coroutine time modes too |
| Material fade does nothing | The chosen URP shader/material must actually support transparency |
| Delivery gives extra money | Economy was tied to tween callbacks or duplicate event subscribers; make transaction IDs/idempotency part of game logic |

Tween only display state. A pickup must be accepted by inventory exactly once before the cosmetic flight begins. Interrupting or disabling the visual must not undo or repeat the resource grant.

## Update policy

Current store/publisher release is **Pro 1.0.435 (12 October 2026)**. That is newer than the local DLL. The publisher's upgrade instructions specifically cover pre-1.0.405 Pro/pre-1.2.815 core packages: upgrade in place, restart Unity and rerun setup. Schedule that as a separate tested change, keep the core/Pro pair aligned, and retain a rollback point. Do not mix current loose scripts with old DLLs. [Store](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) · [Upgrade instructions](https://dotween.demigiant.com/support.php)

Verify a panel tween, sequence cancellation, pooled reuse, TMP animation, repeated Play Mode and an Android build before accepting any upgrade. This audit did not prove either the installed or latest release compiles in this project.
