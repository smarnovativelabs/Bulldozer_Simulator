# Sprint 1 — prove the playable loop

## Outcome

A local Unity build in which a new player can drive an original rover, break rocks, collect fragments, sell them, buy a drill improvement, complete one contract, and resume from a saved state. A 5–8 minute first completion is a **playtest target**, not a measured prediction or reference-game value.

The sprint is ordered by dependency, not a promised calendar duration. A small team can use it as a one-week timebox, but quality gates determine completion.

## Included scope

- One 3D scene, one rover, one camera, one fixed handcrafted site.
- Thirty breakable deposits of one type, three fragments per deposit.
- Keyboard and touch movement; automatic short-range drilling.
- Capacity of eight units with visible cargo and retained overflow pickups.
- One depot that immediately converts fragments to credits.
- One workshop offering one permanent drill upgrade for this save.
- One contract: 60 delivered fragments plus the upgrade. Beacon completion feedback.
- A minimal HUD, step-by-step hints, pause, mute, and explicit new-run/reset control.
- Local save/load and validation in a standalone build.

## Deferred scope

Refining, workers, hiring, stamina, vehicle switching, fuel, multiple materials, procedural worlds, permanent multi-stage progression, advertisements, purchases, currencies beyond credits, secretary/character collection, daily rewards, events, analytics services, accounts, cloud saves, leaderboards, multiplayer, and store submission. Do not create empty systems for all these features now.

## Ordered backlog

| ID | Task and deliverable | Depends on | Completion check |
|---|---|---|---|
| S1-01 | Inspect the target project; record Editor, packages, pipeline, platform assumptions; create a clean scene if needed | — | Project opens and compiles; scene has correct materials and lighting |
| S1-02 | Rover movement, boundaries, camera, desktop/touch input | 01 | Release stops movement; diagonal speed is bounded; no wall penetration |
| S1-03 | Deposit HP, target selection, drill cadence, break feedback | 02 | Exactly three baseline hits break a fresh deposit; only one destruction event |
| S1-04 | Fragment spawning, collection, capacity, cargo visualization | 03 | Eight-unit cap holds; overflow remains collectible after unloading |
| S1-05 | Depot transfer, wallet, delivery count, HUD | 04 | A full load gives 40 credits once; remaining inside cannot pay repeatedly |
| S1-06 | Workshop UI and one drill upgrade | 05 | Costs 60; fresh deposits then need two hits; purchase cannot repeat |
| S1-07 | Contract, hints, beacon, pause/mute/reset | 06 | 60 delivered + upgrade completes once; player can continue or start a new run |
| S1-08 | Snapshot save/load and recovery | 03–07 | Relaunch preserves wallet, cargo, deposits, loose pickups, upgrade, and objective |
| S1-09 | Original visual pass and performance pass | 02–08 | Consistent readable art; no reference assets; no obvious stutter in stress case |
| S1-10 | Acceptance run, build, evidence, handoff | 01–09 | All required checks pass or blockers are precisely recorded |

Define stable IDs and runtime data during S1-03, even though the complete persistence pass is S1-08.

## Practical checkpoints

**Checkpoint A — control:** Move and collide in the empty site. Do not add the economy until this feels responsive.

**Checkpoint B — physical loop:** Break, collect, fill cargo, return, sell. Keep the workshop closed until transfer accounting works.

**Checkpoint C — motivation:** Upgrade changes the hit count. Objective and beacon provide a clear end to the slice.

**Checkpoint D — reliability:** Save/relaunch and edge cases pass; visuals are coherent; produce a real build when tooling permits.

## Proposed tuning

| Setting | Initial value | Reason |
|---|---:|---|
| Rover speed | 4 world units/second | Comfortable traversal; tune after camera scale is set |
| Deposit health | 60 | Clear three-hit baseline |
| Drill damage | 20 → 30 | Upgrade visibly changes three hits to two |
| Strike interval | 0.45 seconds | Short repeat cadence; first valid hit is immediate |
| Strike range / forward cone | 1.6 units / 100° total | Intentional proximity, not omnidirectional remote damage |
| Fragment yield | 3 per deposit | Readable reward with a small object count |
| Cargo | 8 units | Forces several trips without very long returns |
| Pickup radius | 1.5 units | Reduces precision burden |
| Fragment price | 5 credits | One full trip yields 40 credits |
| Upgrade price | 60 credits | Affordable after two full loads |
| Contract | 60 delivered + upgrade | Requires repeated use of the full loop |
| Deposits | 30 × 3 = 90 units total | Enough supply to complete with reserve |

At 60 delivered, total gross earnings are 300 credits. After the single upgrade, net credits are 240 if there is no other spending. The contract counter tracks lifetime deliveries in the run; it must not use the current wallet value. A final full load may take deliveries over 60; do not discard overflow.

The first hit occurs immediately, so baseline break time after acquiring a target is about 0.90 seconds; upgraded is about 0.45 seconds, excluding travel and presentation. Tune actual feel through playtesting. All these numbers are original prototype defaults in `prototype_config.json`.

## Definition of done

The laptop agent supplies the Unity project changes, exact open/run instructions, a build or explicit build-tooling blocker, test results, two or more screenshots, a short gameplay recording when possible, and a list of remaining issues. A code-only response, a non-playable mockup, or an Editor scene with missing references does not complete the sprint.

## APK-informed refinements within the same scope

Apply [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md) during the existing tasks. Use stable material and world IDs, authoritative transfers independent of visual effects, complete collider cleanup, reset reusable views, and advance hints only on successful actions. Add the matching A19–A21 checks from [05_ACCEPTANCE_AND_PLAYTEST.md](05_ACCEPTANCE_AND_PLAYTEST.md). The 30-deposit layout, 90-fragment supply, 8-unit capacity, 60-credit upgrade, 60-delivery goal and direct-sale loop are unchanged. APK table sizes are research evidence, not production content targets.
