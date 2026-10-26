# Later roadmap — add one system at a time

All entries are proposals. This is a sequence of experiments, not a commitment to reproduce the reference's entire feature list. Each sprint starts only after the previous playable loop has been reviewed.

| Sprint | Addition | Why next | Gate before moving on |
|---|---|---|---|
| 1 | Rover, drilling, pickup, direct sale, one upgrade, save, contract | Proves control and reward loop | Full acceptance checklist |
| 2 | One processing station | Introduces the reference genre's logistics depth | Raw and refined inventories stay distinct; queues save; no deadlock |
| 3 | One automated carrier | Relieves a measured hauling bottleneck | Carrier never duplicates/loses material; avoids blocked routes; task ownership is clear |
| 4 | Two deposit types and a second small site | Adds meaningful progression and contrast | Upgrade choices affect throughput; stage changes preserve intended permanent state |
| 5 | A second rover tool / chassis | Adds a new way to work | Different handling or target behavior, not just a cosmetic stat increase |
| 6 | Deeper outpost restoration and worker roles | Builds the project's own identity | Visible restoration changes and a clear reason to assign roles |
| 7 | Save resilience, balance, onboarding, accessibility, device optimization | Makes the game usable beyond the prototype | Repeatable device tests and successful new-player sessions |
| 8+ | Optional offline simulation, events, monetization, cloud features | Only if product goals justify them | Separate design, technical, and user-experience decisions |

## Sprint 2 outline: processing

Add one input buffer, one timed processor, and one output area. The player deposits raw fragments, waits or continues mining, collects refined material, then delivers it. Make each count visible. Save input, work in progress, and output together. Avoid adding multiple recipes, conveyor networks, and worker automation at the same time.

Expose processing time, input/output capacity, batch size, and sale value as tuning. Compare mining rate, hauling rate, and processing rate to identify the slowest link. Allow the player to see the bottleneck before offering an upgrade for it.

## Sprint 3 outline: one carrier

Start with a single carrier and fixed station roles. Suggested state machine: Idle → ReservePickup → MoveToPickup → Collect → MoveToDestination → Deposit → Idle. Add a recoverable blocked/no-work state. Reservations must release when a target disappears or the carrier is disabled. Save job ownership or reconstruct it safely on load. Do not add recruitment rarity or stamina until basic transport is dependable.

## Reference ideas to revisit cautiously

- Worker roles and stats: useful only when tasks differ enough to make assignment meaningful.
- Fuel and stamina: add planning but can interrupt a satisfying loop; test before committing.
- Truck capacity and dispatch: interesting once the processing output needs another constraint.
- Equipment trees: start with two distinct choices rather than a large locked menu.
- Limited events and random modifiers: require a stable core, reliable persistence, and enough base content.
- Offline production: settle exactly what advances, capacity limits, elapsed-time caps, and clock manipulation behavior. Do not advertise it before testing real close/reopen behavior.
- Monetization: outside this prototype. Validate the game first, then design optional value that does not conceal basic controls or repeatedly interrupt movement.

## Backlog discipline

For every addition, write one player problem, one proposed behavior, one measurable success condition, and the new save data it creates. If a feature cannot state its purpose beyond “the reference has it,” keep it out of the next sprint.

## APK evidence that can inform later design

The [static APK inspection](08_APK_FINDINGS.md) found input/output recipe fields, separate worker profile/stat/recruitment tables, named worker recovery/waiting states, combo reset fields, heat/cooling settings and card-event data. Use these as questions for future prototypes, not a checklist to reproduce. Sprint 2 should prove one conversion and saved buffers; Sprint 3 should prove one carrier with ownership and recovery. Fuel, weight-based cargo, hiring, stamina, heat, combos and events still require separate design decisions. Presence in packaged data does not confirm that a feature is active for every player.
