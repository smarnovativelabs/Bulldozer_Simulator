# Bulldozer project exploration

Research date: 23 October 2026. This is a design proposal, not an approved specification or an implemented game.

## Current state

- The workspace contains AssetInbox; no Unity Assets, Packages, or ProjectSettings directories were present during this inspection.
- AssetInbox was empty during this inspection. No downloaded model, rig, licence file, or render-pipeline compatibility has been tested.
- Confirmed user intent: a Unity game inspired by the supplied mining game, minimal custom art, assets from legitimate libraries, distinctive presentation, and fun mechanics.
- Working assumptions only: Android first, single player, stylized 3D, touch steering plus keyboard for development. Platform, orientation, device target, final theme, and monetization remain undecided.
- The proposed clear-and-rebuild theme is not yet a user-approved direction. No editor installation, package installation, or game scaffolding has been performed.

## Reference evidence and limits

Source video: https://www.youtube.com/watch?v=qKD38PeG2q4

The video was sampled at multiple timestamps, not watched continuously or played interactively.

| Timestamp | Visible evidence | Design interpretation |
| --- | --- | --- |
| 00:28 / 01:54 | Worker, small cart/tool setup, block grid, crack marks, cargo indicator | Early progression includes hand-tool work; the previous general description of a small vehicle should not be read as proof of a starter bulldozer. |
| 04:16 | Large chunks erupt from a broken block while cargo reads MAX | Break feedback is visually strong. A still frame does not establish resource loss rules at full capacity. |
| 05:42 | Shop, upgrade station, furnace area, sell pads and trucks | Resources travel through several stations. |
| 07:36 | Upgrade menu lists Furnace / Processing Speed, Sales Trucks / Sales Capacity, Wheelbarrow / Cargo Capacity, Sledgehammer / Mining Power | Several independent throughput upgrades create potential bottlenecks; exact balancing is unverified. |
| 11:25 | Large tracked excavator working at the block-field edge | Equipment progression changes visual scale and impact. |
| 13:19 | Tall carried resource stack and MAX indicator | Cargo is communicated in the world as well as through UI. |
| 15:13 | Furnace load pad, worker building, selling area and additional base facilities | Base management expands beyond mining alone. |
| 17:07 | Loader-style vehicle moving loose material toward a large container beside rails | Breaking blocks and handling the resulting rubble are visibly distinct activities. Exact pickup and processing rules remain unverified. |

Store listing: https://play.google.com/store/apps/details?id=io.supercent.bulldozermasters&hl=en

The listing advertises offline income, workers, equipment upgrades and new zones. These are publisher claims, not independently tested systems. A small visible review sample complains about interruptions, grind, and a mismatch between machinery expectations and early gameplay. These reviews are qualitative signals, not representative market research or proof of retention problems.

Related reference: RoadCraft's publisher describes restoring infrastructure and choosing routes around obstacles: https://community.focus-entmt.com/focus-entertainment/roadcraft/blogs/140-roadcraft-world-premiere-reveal

Consequently, rebuilding infrastructure is not a claim of an unprecedented mechanic. Our opportunity is a compact mobile interpretation with our own levels, controls, art treatment and progression.

## Candidate product promise

Operate a chunky bulldozer, choose a route through satisfying rubble, and use recovered material to reconnect a small worksite.

Three pillars:
1. Responsive machinery from the first interaction.
2. Clearing decisions that change travel routes.
3. A visible before-and-after payoff at the end of each job.

## First job: reconnect the ridge

- One compact site with the broken bridge visible near the start.
- Depot and upgrade pad occupy the same hub, reducing navigation burden.
- A long open route is always available between the hub and the main clearing.
- A short blocked route offers a useful choice: clear it early to shorten later deliveries, or stay on the main objective.
- Stone contributes to bridge reconstruction. Scrap converts to upgrade credits at the same depot.
- The completed bridge opens a small playable overlook with the next job sign; the next complete level is outside sprint 1.
- Start with authored placement for predictable pacing. Later jobs can use seeded variants after the introductory job works.

## Concrete interaction rules to test

- Joystick direction is screen-relative; vehicle turns smoothly toward it. Release stops movement. No gears in the first prototype.
- The front blade affects a limited wedge; side and rear contact do not mine. A visible contact marker explains valid targets.
- Forward pressure breaks eligible rubble after a short readable impact sequence.
- Nearby pickups move into a visible collection rack. Since a bulldozer is not a haul truck, use a simple stylized salvage rack attached to the chassis; do not imply this is realistic vehicle simulation.
- Cargo is logical inventory. Display meshes are a representation, not physical inventory objects.
- At full cargo, stop resource-producing damage and point toward the depot. Do not silently discard valuable drops or let Power Push bypass capacity checks.
- Power Push becomes available after a small amount of productive clearing, then performs one brief forward burst. Do not consume it when there is no valid target. It cannot cross cliffs, locked gates or the unfinished bridge.
- If no room remains for the next rubble unit, leave the unit intact. A clearly signalled return trip is preferable to an ambiguous loss.
- Deposit and conversion happen at one generous pad without a processing timer. Upgrades are optional; the starter vehicle must be able to finish the job.

## Economy seed, for testing only

These values are illustrative starting points, not measurements from the reference and not a promise of pacing.

| Quantity | Initial value |
| --- | --- |
| Stone per rubble unit | 1 |
| Bridge requirement | 60 stone |
| Stone available | At least 90 reachable units |
| Starting mixed cargo | 15 units |
| Cargo upgrade | 24 units |
| Scrap exchange | 5 credits each |
| Early accessible scrap | At least 8 units, worth 40 credits |
| First blade or cargo upgrade | 20 credits each |
| Bridge visual milestones | 20 / 40 / 60 stone delivered |
| Standard rubble break time | Start testing around 0.8 seconds |
| First blade upgrade | Start testing around 0.6 seconds on the same rubble |

At 15 capacity, 60 stone needs at least four stone-only loads. At 24 capacity, it needs at least three. Mixed scrap, partial loads and buying the upgrade partway through change the actual number. Do not present these arithmetic bounds as a simulated session duration.

Require enough reachable stone and scrap on both route choices. Test completing the whole job with no purchases, either first upgrade, and no Power Push use. No required upgrade may consume resources needed to finish the only available objective.

## Fun experiments in priority order

1. Blade contact and steering: can the player line up rubble easily, and does impact feel heavy without delaying control?
2. Power Push: test a clear aim preview and short burst; compare with the ordinary blade using the same scene.
3. Shortcut: does clearing it create a noticeable travel benefit and an understandable decision?
4. Reconstruction payoff: pieces appear at three milestones, then the player drives over the finished bridge.

Use layered impacts, slight pitch variation, restrained camera kick and quick pickup motion. Show the objective beyond the rubble. A hidden cache is a later optional experiment; avoid stacking unrelated mechanics before this loop works.

## Sprint 1 work order and exit criteria

### A. Asset and driving trial
- Check source/license, geometry, scale, facing, materials, pivots and collider suitability for the chosen vehicle.
- Keep vendor files separate from our vehicle prefab and gameplay code.
- Build a small movement test area; validate touch direction, stopping, narrow turns and wall sliding.
- First decision gate: if driving is awkward, fix it before adding economy or more content.

### B. Destruction and collection
- Author a small rubble cluster with a few reusable meshes and logical health states.
- Front-only contact, feedback, cargo capacity, collection rack and immediate depot delivery.
- Pool transient effects and cap cosmetic fragments. Keep valuable resources in persistent logical state.
- Verify no duplicated rewards when several hits arrive in the same frame.

### C. Complete job
- Main route, optional shortcut, two materials, two upgrades, Power Push and bridge milestones.
- Save stable rubble IDs, remaining pickups if any, inventory, credits, upgrades and build progress.
- Save an internally consistent state after deposits and purchases; do not rely only on app quit.
- Verify reload at full cargo, after a deposit, after an upgrade, and after completion.

### D. Playtest and device check
- Build for the agreed device target and profile a dense break burst plus maximum visible cargo.
- Establish a minimum supported-device target before claiming frame-rate success. A provisional 30 fps floor is a design target, not a verified result.
- Ask 3-5 new testers to try without live coaching. Record confusion and route decisions; this sample is exploratory, not statistically conclusive.
- Initial pacing targets: first break within 15 seconds; first useful delivery around one minute; an upgrade available around two minutes; complete job roughly 5-8 minutes. Tune after observation.
- Record first-break time, first-delivery time, first-upgrade time, repeated full-cargo collisions, Power Push attempts/hits, shortcut use and job completion.

Success means the player understands the goal, enjoys operating the vehicle, notices an upgrade, and can finish without resource loss or progress corruption. The sprint does not succeed merely because every menu exists.

## Technical direction, pending Unity setup

- Use ordinary Unity GameObjects and small components for this scale; do not begin with terrain deformation or a full voxel engine.
- Separate vehicle movement, blade interaction, rubble state, cargo, depot conversion, upgrade configuration, job progression, feedback and save data.
- Keep currency changes and resource transfers in one authoritative transaction path to prevent duplicated or lost rewards.
- Use simple collision shapes for navigation and a separate blade detection volume for mining.
- Shared materials and repeated meshes are preferred. Measure on a device before deciding whether more rendering infrastructure is necessary.
- Unity's object pooling guidance supports reusing short-lived objects to reduce allocation and repeated creation/destruction: https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity

## Minimal art bill

Purchased/downloaded candidates: one bulldozer, 3-5 decorative rock shapes, a few trees, particle textures and impact sounds.

Generated/assembled originals: salvage rack, rubble variants, two pickup shapes/material families, a three-stage bridge, depot marker, upgrade marker and objective sign. The interactive diagram in the chat is a level-flow study, not a final art style or a Unity build.

Free shortlist:
- Bulldozer: https://poly.pizza/m/eY1N7Rz9Drr (attribution; archive and rig untested).
- Nature: https://kenney.nl/assets/nature-kit (CC0).
- Effects textures: https://kenney.nl/assets/particle-pack (CC0).
- Impact audio: https://kenney.nl/assets/impact-sounds (CC0).
- Later processing props: https://kenney.nl/assets/factory-kit (CC0).

No additional purchase is needed to test the proposed loop.

## Deferred work

Workers, on-foot gameplay, multiple vehicles, excavator arms, processing chains, fuel, daily rewards, offline earnings, advertising, purchases, cloud accounts, large worlds and procedural campaign generation.

Possible later jobs reuse the kit: clear a blocked road, reclaim a work yard, reopen a quarry entrance. Add a new interaction only when it supports the core vehicle experience.

## Identity and open decisions

Own the layout, UI, text, objectives, audio treatment, visual composition and branding. Retain third-party licences and attribution. Do not copy assets from the original game's build. A distinct concept reduces copying but is not legal clearance, and no proposed title has been trademark-checked.

Before implementation: confirm whether the user wants this proposed rebuilding direction or a closer mining-business loop; agree target platform/orientation and test device; select an installed/supported Unity version; inspect downloaded assets. These do not block research or a layout study.
