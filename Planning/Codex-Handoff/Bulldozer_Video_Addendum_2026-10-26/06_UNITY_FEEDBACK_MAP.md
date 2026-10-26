# Unity tools for the observed feedback needs

Use the [installed plugin reference](../Bulldozer_Plugin_Reference_2026-10-26/00_START_HERE.md) for verified local APIs, versions and known issues. The mapping below is a proposal for our game. The videos do not establish that the reference uses these plugins.

## Keep gameplay authoritative

The gameplay system commits an accepted action and its quantities, then emits a presentation event. A deposit can finish breaking, inventory can change and a delivery can complete even when all effects are disabled. Each event should provide the position, relevant amount and owning object or stable transaction identity needed by its view.

For pooled views, stop the view's active effects, remove event subscriptions and reset its position/scale/color on release. Do not wait for a tween completion callback to grant a resource. A callback can arrive late, be killed or refer to a reused object.

## One owner for each animated property

| Event | DOTween role | Feel and asset role | Guardrail |
|---|---|---|---|
| Valid hit | Optional short recoil on a cosmetic child | Small impact sound/particle response | Movement code keeps control of the vehicle root; cooldown prevents effect spam |
| Deposit break | Optional brief scale presentation before hiding a view | Stronger impact, dust and limited fragments | One break event and one yield transaction; no double reward from a visual callback |
| Pickup accepted | Cosmetic travel toward the rack; HUD count transition | Small optional pickup sound | Inventory already owns the quantity; interrupted travel has no economic effect |
| Cargo full | One brief pulse of the capacity display | Optional soft haptic cue through one adapter | Trigger on entering the full state, not every frame; also use text/shape |
| Depot delivery | Cosmetic resource or currency flight and counter movement | Short success sound/particles | Award once for the committed transfer, then present it; empty pad dwell gives no reward |
| Upgrade purchased | Panel motion and stat-change highlight | One confirmation effect | Insufficient funds and repeat purchase are handled by game code |
| Objective completed | Reveal an original marker or structure milestone | A restrained celebration | Completion and its save state commit once; canceling effects cannot reopen the reward |

The existing recipes cover code-driven DOTween lifetime handling and `MMF_Player` behavior. Prefer those installed-version examples over fresh generic snippets. Do not let DOTween and a Feel feedback simultaneously animate the same transform property. If Feel orchestrates a response, assign each constituent property to one component.

## Existing project choices

- Use the project's Input System route for driving. A tween should not determine physics movement or authoritative blade contacts.
- Use a small adapted subset of Epic Toon FX for world feedback. Particle Image is optional later UI polish.
- Use one haptic path if enabled, with a player setting. The installed Feel/Nice Vibrations path and CandyCoded should not fire the same event twice.
- Joystick Pack and SafeArea are useful to the mobile shell, subject to the documented integration checks. The existing joystick getter issue is recorded in the plugin pack and was not fixed in this research task.
- Do not plan around missing effect prefabs: the earlier local review found script-only partial imports in JMO Assets, Sparkles and Particlecollection.
- Keep the installed Unity and plugin versions for this task. The earlier compatibility guide records why a blind Feel update is unsuitable for the current Editor.

## Validation when implemented

Test one feedback chain at a time. Then break several deposits rapidly, fill cargo, sell, pause, disable/re-enable a pooled view and reload the save. Check that no duplicate feedback subscription remains, no tween moves a recycled object, and resource counts are unchanged when effects are muted or disabled.

Profile on the chosen device before increasing particles, fragment lifetimes or camera motion. The dense footage establishes a visual reference only; it provides no measured CPU/GPU budget. Document actual limits in the project after measurement.
