# Gameplay lessons and original adaptations

## The loop has several distinct jobs

The footage repeatedly shows breaking deposits, gathering or carrying material, refinery handling and sale trucks. Refinery input and output have separate interaction areas in [Part 1 at 06:25](https://www.youtube.com/watch?v=ovlJLqLo9I8&t=385s). The hub and sale presentation remain recognizable as late as [episode 11 at 29:37](https://www.youtube.com/watch?v=jKxhm3XFG_0&t=1777s).

For our design, treat these as separate responsibilities even when Sprint 1 combines unloading and selling into one depot. The reference does not require us to start with every station. A compact direct-delivery loop is enough to test driving, breaking, carrying and improving the machine.

When a refinery is introduced later, distinguish raw inventory, processing input, processed output and sold quantity. Give each transfer one clear destination and an understandable result. Define the transaction in gameplay code before adding moving stacks or currency effects.

## Cargo should be readable without opening a menu

The early footage has a local cargo indicator and increasingly tall carried stacks; a red full-capacity cue appears in [qKD38PeG2q4 at 13:19](https://www.youtube.com/watch?v=qKD38PeG2q4&t=799s). Later vehicles preserve separate cargo and fuel displays, as in [episode 15 at 25:57](https://www.youtube.com/watch?v=7DGYwDg_-Hc&t=1557s).

Use a logical inventory count plus a compact visible rack on our vehicle. Cap or group the visible pieces so a large inventory cannot obscure steering or landmarks. The reference's towering stacks are an observation, not a requirement to create one render object for every resource. Show a capacity number and distinct full state; do not rely on red alone.

Full-capacity frames do not prove whether new damage stops, drops stay on the ground, pickups disappear or overflow goes elsewhere. Our full-cargo rule must be deliberate and tested; the existing proposals disagree on it.

## Navigation cues support the return trip

A home arrow and distance appear during early mining in [Part 1 at 00:35](https://www.youtube.com/watch?v=ovlJLqLo9I8&t=35s). A long line of yellow direction markers appears on a loaded return route in [episode 3 at 13:58](https://www.youtube.com/watch?v=x2Uq2HHjeD8&t=838s). Ground pads identify station interactions in [qKD38PeG2q4 at 09:31](https://www.youtube.com/watch?v=qKD38PeG2q4&t=571s).

Our first site should keep the depot recognizable and reachable. Add one destination cue when cargo becomes full or a delivery objective becomes active. A compass arrow is adequate for a small open site; do not imply a route through a wall. If later maps need breadcrumbs, generate them from a valid traversable path. Use our own pad shapes, labels, colors and spacing.

## Clearing changes the shape of the site

The mine is an orderly set of deposits with cracked faces, removed sections and loose fragments. Compare [qKD38PeG2q4 at 01:54](https://www.youtube.com/watch?v=qKD38PeG2q4&t=114s) with [Part 1 at 10:41](https://www.youtube.com/watch?v=ovlJLqLo9I8&t=641s), where corridors and curved rails constrain movement.

A practical original implementation can use authored deposit cells with stable IDs, health and yields. This is an engineering proposal; the videos do not reveal the original terrain engine. Start with generous lanes and a guaranteed return route. A shortcut can be added if the selected first-sprint design includes it. Avoid requiring a terrain voxel system or unrestricted rigidbody rubble simulation to reproduce the visual satisfaction.

## Machinery changes scale and activity

Early recordings show hand mining, cart carrying, a roller and an articulated excavator. Later footage includes drill-nose machinery, large haulers and a tanker-like vehicle: [episode 7](https://www.youtube.com/watch?v=LPnO57yl8lg&t=1219s), [episode 8](https://www.youtube.com/watch?v=hoZzYcfZ6-U&t=1201s), and [episode 10](https://www.youtube.com/watch?v=mka1GPEnH-w&t=1379s). These samples do not reveal handling curves, exact vehicle roles or unlock requirements.

For our game, give each later machine one understandable advantage, such as a wider clearing strip, better collection reach or a larger load. Introduce one at a time and validate its turning space around the hub. If our promise is driving a bulldozer immediately, use that as an original product choice; the reference's hand-tool opening need not be copied.

## Progression combines familiar jobs with new presentation

The sampled series moves from Levels 1–3 to 4–10, then Stage 13, while the hub loop remains recognizable. Icy terrain and cyan fragments appear in [episode 14 at 20:45](https://www.youtube.com/watch?v=lXuiGjgeTSM&t=1245s); the later site has green deposits with purple faces in [episode 17 at 22:07](https://www.youtube.com/watch?v=NGAMLxMZhbs&t=1327s).

Use original palettes, deposit silhouettes, landmark arrangements and contract goals. New colors alone are a weak reason to repeat a long job; a later site should add one tested route, material or machine decision. Video duration is not evidence of required stage length because recordings may be edited or span different activities.

Completion controls are visible alongside 100% in [episode 16 at 1:11:04](https://www.youtube.com/watch?v=FF1XXfoSh8A&t=4264s) and [episode 17 at 39:50](https://www.youtube.com/watch?v=NGAMLxMZhbs&t=2390s). For our prototype, calculate progress from the selected objective, make completion happen once, and let the player understand what comes next. The reference's exact percentage formula and stage-reset behavior remain unknown.

## Strong feedback needs visual limits

Large fragments and bright effects convey force in [episode 7 at 20:19](https://www.youtube.com/watch?v=LPnO57yl8lg&t=1219s). The cyan debris in [episode 14 at 20:45](https://www.youtube.com/watch?v=lXuiGjgeTSM&t=1245s) covers much of the machine in that frame. This suggests a useful readability test for our build, not a performance diagnosis of the reference.

Separate meaningful resource pickups from cosmetic chunks. Limit the duration and simultaneous count of cosmetic effects, keep the blade and destination readable, and test on the target device. Use a distinct small hit response, stronger break response and clear delivery reward. Sound and haptic recommendations in this handoff are our design choices; the sampled frames do not establish the original sound mix.

## Extra systems can wait

The [ocean-site display in episode 9](https://www.youtube.com/watch?v=Apa6GFfilGk&t=1363s) has a countdown and numbered site. The strategy guide shows a reward track and rental task at [03:47](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=227s), and separate mining/carrying progression at [08:32](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=512s). These are later-system references. Do not add event timers, rentals, premium currencies, worker management or multiple upgrade trees to prove the first playable loop.

## Additional lessons from the supplied 9:22 recording

See [the local sequence review](02_OBSERVATIONS.md#supplied-phone-recording--deeper-sequence-review-26-october-2026) for observations L01–L09 and saved evidence.

1. **Make mining and hauling distinct capabilities.** The observed wheelbarrow expands capacity from 6 to 25 while pickaxe work happens on foot. If our machine does both, make that deliberate. If it does only one, explain its role before purchase.
2. **Make the next interaction explicit.** Pair a rented vehicle's arrival with a visible destination and a clear “Use” control. When dismounting requires stopping, show the reason immediately. The reference recording spends substantial time around the wheelbarrow before first use.
3. **Keep recommendations consistent with the active lesson.** Here, the quest asks for a sledgehammer while the recommended wheelbarrow consumes most available cash. In our onboarding, either align the recommendation or acknowledge the alternate valid path and update guidance.
4. **Track inventories independently.** If processing and vehicles enter scope, model character cargo, vehicle cargo, refinery input, refinery output, and truck load as separate owners. Mount/dismount must not silently merge or discard them. The visual evidence supports this separation without revealing the original code.
5. **Choose a clear full-capacity rule.** The observed final hit can complete after MAX appears, followed by a pause at the next face. For our design, specify whether an already committed hit may finish and where any resulting resources go. This is a proposed rule to resolve, not a proven reference algorithm.
6. **Separate site clearing from earnings.** Progress increases at the mine before selling. A prototype may use cleared deposits or a delivery contract, but its displayed progress should explain the selected objective consistently.
7. **Do not treat a truck counter as an upgrade level.** The panel shows a capacity range and consecutive trucks vary within it. If used later, varied trucks could change collection cadence; it is not necessary for the first playable loop.

Suggested checks when these features are implemented: mount with nonempty personal cargo, dismount after vehicle unloading, collect while personal cargo is full, leave processing input with blocked output, and deliver across a departing truck. Verify conservation of resources at each transition. These are design acceptance cases, not tests run against the reference or Unity project.
