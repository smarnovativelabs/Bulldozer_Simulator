# Applying the video lessons to Sprint 1

Keep one playable scene and complete the chosen core loop. The new observations mainly improve clarity, feedback and edge cases; they do not justify adding a refinery, workers, fuel or multiple vehicles now.

## Resolve the existing design difference

Two earlier documents are proposals. Neither becomes the approved specification merely because this addendum links to it.

| Decision | Original research pack | Repository Project Exploration proposal |
|---|---|---|
| Identity and end goal | Salvage rover, 60 delivered units plus one required upgrade | Bulldozer, restore a bridge with 60 stone |
| Materials | One | Stone and scrap |
| Initial cargo | 8 | 15, upgrade to 24 |
| Full cargo | Mining may continue; valuable overflow remains as pickups | Stop resource-producing damage when the next yield will not fit |
| Upgrades | One required drill upgrade, cost 60 | Optional blade or cargo improvement, cost 20 |
| Additional interaction | Basic drilling and delivery | Power Push and optional shortcut |

Use the latest explicit human selection when one exists. Until then, common work such as steering, camera, target identification, inventory correctness and depot feedback can proceed without combining the conflicting rules. Record the chosen rules in one active implementation specification before tuning the economy or finalizing acceptance tests.

The repository `Planning/Project-Exploration.md` also describes an earlier AssetInbox-only state. That historical inventory is superseded by the newer project review and plugin inventory; do not infer that today's project lacks Assets, Packages or ProjectSettings.

## Small implementation candidates

These are proposals with no copied reference tuning. Integrate them into the existing backlog rather than starting a parallel framework.

| Candidate | Concrete behavior | Completion check |
|---|---|---|
| Cargo clarity | Numeric amount/capacity, compact rack and explicit full state | Fill, unload and reload a save; all views agree with inventory |
| Return cue | Activate a depot arrow or marker when full or prompted to deliver | Cue is readable at each mine edge, works after camera rotation, and clears after the delivery state changes |
| Valid blade target | Highlight/contact cue only for an eligible target in the selected front area | Side/rear/ineligible contacts do not imply a productive hit |
| Deposit lifecycle | Readable damage/break stages; one logical destruction result | Multiple contacts in one update cannot duplicate yield; broken deposit stops obstructing its cleared space |
| Delivery transaction | Clear pad entry/active/empty feedback; award once per actual transfer | Standing in the pad, re-entering empty and interrupting effects cannot pay twice |
| Guided objective | One action prompt at a time | Advance after successful break, pickup, delivery and upgrade events, not merely touching a UI element |
| Readable completion | One objective counter and one completion state | Reload after completion cannot award twice; objective matches the selected spec |
| Bounded effects | Reusable cosmetic chunks and restrained feedback | Long clearing session stabilizes active effects; collected resources remain correct with all feedback disabled |

## Full cargo acceptance depends on the selected rule

If retaining loose pickups, a full vehicle may break another deposit, but its valuable output must remain available, survive saving and load correctly after an unload. Cosmetic chunks may expire; valuable quantities cannot expire by accident.

If stopping damage at full capacity, check the entire pending yield before the hit commits. The deposit remains intact when its yield cannot fit. Any burst ability follows the same rule, and the HUD explains the return trip. Do not leave an almost-broken deposit with no way to recover its resource.

In both cases, gameplay data is authoritative. Disabling a pickup tween, interrupting a camera effect or returning an effect to a pool must not create, erase or duplicate resources.

## Recommended implementation order

1. Confirm the current project settings and the active scope document; use the existing plugin reference.
2. Validate steering, stopping, blade-facing and camera on a tiny test area.
3. Build one complete deposit-to-inventory lifecycle with the selected full-capacity rule.
4. Add the depot transaction, cargo display and return cue.
5. Add the chosen upgrade and completion objective.
6. Persist deposits, loose resources if applicable, inventory, currency, upgrades and objective state consistently.
7. Apply restrained effects and test touch input, rapid repeated actions, pause/restart and device performance.

The existing first-sprint acceptance list remains the base. Add the relevant checks above without importing the reference's full progression. Do not use sampled video counts, prices, level percentages or travel distances as our balance values.
