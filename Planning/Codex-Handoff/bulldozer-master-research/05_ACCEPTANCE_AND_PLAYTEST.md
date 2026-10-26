# Sprint 1 acceptance and playtest

These are instructions for the future implementation agent. They have **not** been executed against a Unity game in this research task.

## Required gameplay checks

| ID | Action | Expected result |
|---|---|---|
| A01 | Start a fresh run | 0 credits, 0/8 cargo, 0 deliveries, upgrade unowned, 30 intact deposits |
| A02 | Move diagonally; release keyboard or touch; cancel touch | Speed stays bounded; input returns to zero; no stuck movement |
| A03 | Steer into a wall and intact deposit | Rover stays on valid ground without passing through blocking geometry |
| A04 | Enter drill range of a fresh deposit | Three baseline hits break it; leave range and strikes stop |
| A05 | Put a deposit behind a wall/outside the forward cone | It receives no remote or obstructed damage |
| A06 | Break one deposit | Exactly three gameplay fragments appear; break event cannot repeat |
| A07 | Collect with cargo at 7/8 and three fragments nearby | One enters cargo; two remain in the world |
| A08 | Keep mining at full cargo | Cargo remains 8; loose fragments remain; full-cargo hint is clear |
| A09 | Sell a full load and remain in depot | Inventory becomes zero, credits increase by 40, deliveries by 8, exactly once |
| A10 | Try workshop at 40 credits | Upgrade is unavailable; nothing is deducted |
| A11 | Sell a second full load and purchase at 80 credits | Balance becomes 20; upgrade level is 1 |
| A12 | Double-click upgrade or reopen workshop | No second deduction; upgrade stays capped at level 1 |
| A13 | Break a fresh deposit after upgrade | Two hits break it; damage value is visibly different in workshop UI |
| A14 | Deliver 60+ fragments without the upgrade; then buy it | Contract completes only when both conditions are met |
| A15 | Complete contract and reload | Completion persists; beacon is active; completion reward/transition cannot repeat |
| A16 | Press a UI button while a movement touch is active | UI receives its own touch; no phantom steering or accidental spending |
| A17 | Open pause/workshop modal | Movement and drilling stop; release/cancel input is handled when resuming |
| A18 | Use Reset/New Run | Clear only this run after deliberate in-game confirmation; return to A01 state |
| A19 | Break a deposit, cross its footprint, save and reload | Path remains open; no ghost collider, revived deposit or duplicate drops |
| A20 | Reuse a pooled view/effect for another object, if pooling is used | New ID binding, collider, flags and timers are clean; no stale ownership or reward. If pooling is absent, record N/A for reuse and verify spawn/despawn cleanup |
| A21 | Follow hints, attempt an unaffordable upgrade, then buy successfully and reload | Failed actions do not advance hints; successful milestones advance once; completed guidance stays complete |

## Save/reload cases

Test these in a standalone build as well as the Editor when build tooling is available:

1. Partially damage a deposit, save, restart: the remaining HP is preserved.
2. Break a deposit, collect one of its three fragments, restart: the deposit stays broken; exactly two fragments remain.
3. Save with seven carried units, some loose fragments, and a purchased upgrade: all are restored without duplication.
4. Sell, then immediately pause/background and relaunch: credits, cargo, and delivery counter agree with the saved transaction.
5. Resume at the depot: an empty inventory cannot generate another payment.
6. Corrupt a test copy of the primary save while retaining a valid backup: recover the backup and report recovery.
7. Test no save, incompatible schema/layout, invalid position, and negative values: fail safely and preserve diagnostic evidence.
8. Confirm pooled pickups do not keep a previous drop's ID or interaction state.

## Small, meaningful automated tests

Prioritize pure rules rather than UI snapshots or implementation-mirroring assertions:

- Capacity accepts only available space and never produces negative quantities.
- Sale conserves material: transferred quantity leaves inventory and increments both wallet and deliveries once.
- Upgrade purchase is affordable, atomic, capped, and repeat-safe.
- Destruction and pickup IDs prevent duplicate rewards.
- Save round-trip preserves partially mined and partially collected state.
- Completion requires cumulative deliveries and the upgrade, irrespective of current cash.

A few EditMode tests can cover these rules; use PlayMode integration checks for colliders, input, and presentation. Run relevant tests again after fixes; a long list of tests that merely restates the code is unnecessary.

## Visual and performance review

- Check at portrait 9:16 and a tall-phone aspect ratio, plus a desktop Game view.
- Keep HUD and touch controls inside the safe area. Text, meters, and button hit areas remain legible.
- Confirm camera/rover/rock scale lets the player anticipate collisions.
- Full cargo, unavailable upgrade, and successful sale are understandable without guessing.
- Inspect maximum loose pickups and repeated effects. No growing object count after effects expire.
- Target 60 fps when practical, with at least a stable 30 fps on the chosen mobile test device. Record device and build settings; these are targets, not guarantees.
- No pink materials, missing references, runaway Console messages, or exceptions during the full loop.

## Five-minute observation sheet

Ask a first-time tester to play without coaching. Record time to first movement, first break, first delivery, first purchase, and completion. Note wrong turns, missed UI, repeated attempts to collect at capacity, or uncertainty about the depot. Afterward ask what the upgrade changed and what they wanted to do next.

If the first delivery is unclear, adjust guidance and layout before adding content. If the upgrade is not noticeable, tune break cadence or presentation. If travel dominates, shorten the route or adjust speed. If the loop is already satisfying, add refining in the next sprint.

## Completion report template

```text
Editor / pipeline / packages:
Scene and run steps:
Build and platform:
Controls:
Acceptance checks passed / failed / not run:
Automated test results:
Save/relaunch evidence:
Device and performance evidence:
Screenshots / gameplay recording:
Original or licensed asset provenance:
Known issues:
Changes from proposed tuning:
Next recommended sprint:
```

## Conservation check for this original layout

At any stable checkpoint, **delivered + cargo + loose fragments + potential drops from unbroken deposits = 90**. Each unbroken deposit contributes three potential drops even if partially damaged. Run this check after partial collection, sale and reload. The invariant assumes our fixed 30-deposit, break-only-drop design; it is not a claim about the reference. APK-derived recommendations and limits are recorded in [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md).
