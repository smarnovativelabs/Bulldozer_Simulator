# Game Design Document

**Project:** Bulldozer Simulator, internal working name only  
**Version:** 1.1  
**Date:** 26 October 2026  
**Platforms:** Android and iOS  
**Orientation:** Portrait only (confirmed); landscape and upside-down rotation disabled.  
**Purpose:** Define the first complete release and the progression principles for future updates.

The player starts as a worker with a basic pickaxe and a small backpack. They break stones, collect material, process it, sell it, and gradually earn equipment that makes familiar work easier. The journey should feel like a story of becoming more capable: experience a manageable challenge, discover a useful improvement, earn it, enjoy its benefit, then take on a new challenge.

**Sprint 1 is the first complete release intended for the stores. It includes the environment and the full worker progression. It does not include a bulldozer.** The wheelbarrow is an optional hauling unlock within Sprint 1. Bigger equipment and heavy machinery arrive in later updates, one useful step at a time.

## 1. Design authority and scope

### Confirmed direction

- Begin on foot with a pickaxe, animated stone strikes, and a backpack carried by the worker.
- Design gameplay, camera framing, controls, and UI for portrait mobile screens and their safe areas.
- Improve and unlock tool levels gradually; the player must feel the benefit of an upgrade.
- Unlock a wheelbarrow for greater hauling capacity while keeping backpack carrying available and useful.
- Build a guided, story-like progression through objectives, equipment milestones, and visible changes.
- Introduce solutions after the player understands the need. Avoid unexplained unlocks and excessive early complexity.
- Exclude the bulldozer from Sprint 1. Its eventual release milestone is not assigned yet.
- Create original environments, models or licensed adaptations, UI, branding, objectives, and balance values.
- Deliver a complete, tested Android/iOS first-release experience rather than treating the existing technical test as a finished sprint.

The mechanics and content quantities below are the proposed implementation baseline. Names, numbers, timing, and economic values remain tunable through playtesting. Portrait orientation is confirmed. These proposals are not claims about how the reference is implemented.

This document supersedes conflicting scope proposals in `Sprint1-Foundation/README.md`, `Sprint1-Foundation/SPRINT2_PLAN.md`, and earlier handoff plans. In particular, environment creation belongs inside the first-release work, and the bulldozer does not. Those files remain historical research and technical evidence; they do not override this design.

Creating this document does not authorize a store submission or imply that the described game has already been built.

## 2. Player experience

### Core promise

“I started with simple equipment. I earned better tools, made my work easier, and grew into bigger jobs.”

The game should be approachable for a player who wants short mobile sessions and visible progress. A return session should immediately explain what the player was doing, where their equipment is, and what improvement comes next.

### Design principles

| Principle | How it affects the game |
|---|---|
| Learn through doing | Teach movement, one stone, one load, and one sale before introducing upgrade choices. |
| Earn meaningful ease | An improved pickaxe reduces effort; a larger bag reduces trips; a wheelbarrow changes hauling capacity. |
| One new concern at a time | Introduce tougher stone, larger orders, and new transport separately. |
| Preserve useful choices | Unlocking a wheelbarrow never removes the backpack or forces every job to use it. |
| Make work satisfying | Align strikes, stone damage, sound, chips, and haptics around the same impact. |
| Explain the next step | Keep one main objective visible and show relevant guidance at the point of need. |
| Protect progress | Cargo, purchases, processing, and rewards survive interruption without duplication or loss. |

Difficulty comes from a reasonable amount of work and a new decision, not punishment, deliberately bad controls, excessive walking, or long waits. After an upgrade, give the player time to enjoy easier work before introducing a tougher task. Do not immediately increase every stone's health to cancel the upgrade's benefit.

## 3. Core gameplay loop

**Approach a deposit → strike and break it → collect material → carry it to processing → collect processed output → deliver and sell → improve equipment → complete a chapter objective.**

Mining and hauling are different activities. The pickaxe breaks stones. The backpack and wheelbarrow carry material. The wheelbarrow does not mine.

The moment-to-moment loop should remain usable before any optional equipment purchase. Early deposits and sales provide an ongoing way to earn money, so spending on a different valid upgrade cannot permanently block progression.

## 4. Sprint 1 story and progression

### Story structure

Proposed setting: the worker takes responsibility for a small, neglected quarry worksite and completes increasingly useful supply jobs. A site contact provides brief instructions and recognizes progress. This needs a few portraits or short messages, not cutscenes or a fully simulated NPC workforce. Final names and dialogue are still to be written.

Sprint 1 contains one complete story chapter with several progression milestones. Each milestone has a practical objective, a visible result, and a clear equipment lesson. Completing deliveries can repair a work area, improve the depot, or open a route. The final milestone completes the first site's main job and recognizes the worker's growth.

| Chapter beat | Player activity and manageable difficulty | Guidance and reward | Result |
|---|---|---|---|
| First work | Walk to a nearby soft stone, break it, and collect a small load. | A short prompt teaches moving and stopping to work. | The player understands the basic action without an upgrade screen. |
| First delivery | Deposit raw material, collect processed output, and sell it nearby. | Distinct station signs and one destination cue. | The player sees the entire earning loop. |
| A better pickaxe | Complete a slightly larger mining job with the basic tool. | Explain that a stronger pickaxe can reduce the required hits; make the first upgrade attainable. | The player experiences an immediate improvement on familiar stone. |
| More useful trips | Improved mining fills the small backpack more quickly. | Introduce the next backpack capacity level. | The player carries more on foot and makes fewer trips. |
| A larger order | Present a delivery job where hauling capacity matters. | Introduce the wheelbarrow and explain its capacity and handling. | The wheelbarrow becomes available to earn through ordinary play. |
| Choose how to work | Try loading and unloading the wheelbarrow, then leave it and mine on foot again. | “Use wheelbarrow” and “Leave wheelbarrow” make the change explicit. | The player understands that backpack and wheelbarrow remain separate options. |
| A more capable worker | Add a tougher deposit type and further tool levels, one at a time. | Preview the benefit of the recommended improvement. | Bigger jobs become comfortable with earned equipment. |
| Finish the first site | Complete a final supply or site-improvement contract. | A visible completion change and a clear summary of the worker's achievements. | A satisfying end to the first chapter of the game, without requiring machinery. |

Unlocking means the equipment becomes available. Buying or claiming it gives ownership. Equipping or using it is a separate player action. The UI must distinguish these states.

A small tutorial can demonstrate the wheelbarrow, but ordinary contracts remain completable using backpack trips. Do not make its purchase an unexplained mandatory gate. A future-content message must not imply that an unavailable bulldozer is playable in the current build.

### Guidance examples

- After several strikes: “A stronger pickaxe will help with these stones.”
- After the player experiences full cargo: “A larger backpack means fewer trips.”
- Before a larger order: “A wheelbarrow can carry more. Your backpack is still useful for smaller loads.”
- On returning to the game: “Your order is partly complete. Your loaded wheelbarrow is near the processor.”

Guidance should appear once when useful, remain available through the objective card, and avoid interrupting every repeated action. Recommendations must support the active lesson and recognize an alternative valid upgrade.

## 5. Movement and mining

### Controls

Proposed default: one movement joystick, contextual equipment interaction, and an elevated follow camera. Avoid requiring a second thumb for every pickaxe strike.

When on foot, stopping within range of a valid stone selects it, turns the worker toward the impact point, and begins mining. Moving cancels further strikes immediately. Target selection favors a nearby reachable deposit in front of the worker and stays stable during a strike. Do not hit through walls or turn toward distant stones unexpectedly.

UI touches must not steer the worker. Losing a touch, opening a menu, or pausing the app clears movement and mining input. Consider an optional hold-to-mine control only if testing shows that automatic mining causes accidental actions.

### Pickaxe strikes

Use an animation sequence with preparation, swing, impact, and recovery. One validated impact event applies one hit. A canceled swing before contact does no damage; a completed contact retains its damage even if the player then moves away. Animation speed and damage timing must remain aligned after upgrades.

At contact, show a restrained chip burst and play an appropriate sound. Stronger damage stages appear on the stone, followed by a clear final break. Cosmetic fragments can disappear; valuable material cannot disappear because an effect ends.

The worker's hands must align with the pickaxe grip, and the tool must contact the intended stone area. Test different worker facings and deposit sizes. The current generic interaction clip is not a finished mining animation.

### Deposits

Each deposit has a stable identity, health, hardness, material yield, and visual damage state. Keep the initial set small and readable. Proposed launch content is one common material and one later valuable ore, with clear distinctions between raw and processed forms.

Newly unlocked harder stones should communicate their difficulty before a player spends a long time striking them. Keep earlier profitable work available while the player earns the next upgrade. Essential progress must never require a purchase the player can no longer afford to earn.

## 6. Backpack and wheelbarrow

### Equipment roles

| Property | Backpack | Wheelbarrow |
|---|---|---|
| Availability | Equipped from the beginning | Unlocked and earned later in Sprint 1 |
| Best use | Mining while on foot, small loads, short trips | Collecting and delivering larger loads |
| Capacity | Smaller, with upgrades | Higher, with a later capacity improvement |
| Movement | Free walking and turning | Worker pushes the barrow; slightly wider turning space |
| Mining | Available when standing near a stone | Stop and leave the wheelbarrow before mining |
| Inventory | Remains with the worker | Remains in the wheelbarrow when parked |

Higher capacity is the primary benefit. Do not make handling frustrating just to justify the backpack. Both must fit the essential paths and station areas.

### Using the wheelbarrow

Approach its handle area and tap **Use wheelbarrow**. Move the worker and barrow as one controlled unit with hands aligned to the handles. Show wheelbarrow capacity as the main cargo readout while pushing.

Tap **Leave wheelbarrow** to stop and release it into a valid nearby position. Restore on-foot movement, backpack readout, and mining. Preserve both inventories. Clear held input during the transition so neither actor continues moving accidentally.

Provide a recover option if the wheelbarrow becomes stuck. Recovery moves it to a valid nearby checkpoint and retains its cargo; it must not grant extra materials. Save its location and load.

### Collection and cargo ownership

Proposed collection behavior is automatic within a small, visible reach:

- On foot, material goes into the backpack.
- While pushing, material goes into the wheelbarrow.
- A parked wheelbarrow does not pull materials from distant mining activity.
- Switching equipment does not merge, swap, unload, or erase either inventory.
- Stations transfer only from the active carrier. Hidden backpack stock is not silently sold while pushing the wheelbarrow.
- No direct backpack-to-wheelbarrow transfer UI is required for launch; each carrier can collect and unload independently.

Show a small secondary badge when the inactive carrier still holds material, so players can find it. Raw and processed material must have different icons and names.

### Full capacity

When the backpack is full, do not begin another mining swing. Explain the full state and guide the player toward unloading. An already committed strike may complete; any yield that cannot fit remains available on the ground. While pushing a full wheelbarrow, movement and unloading remain available, but further collection stops.

Valuable drops persist until collected. Save their logical quantity and location; nearby drops can be combined into a bounded visual pile without losing units. Never expire valuable material to solve performance problems. A partially accepted pile retains the remainder.

## 7. Processing and sales

Keep processing input, processing output, backpack contents, wheelbarrow contents, and sold quantities separate. A transfer succeeds only when the destination has space and accepts that material.

Use distinct marked areas for **Raw material input**, **Finished material pickup**, and **Sell**. Stand in an area to transfer automatically at a readable rate. Show exactly what moved. The transfer stops when the player leaves, the source empties, or the destination fills.

Processing should be brief enough to support active play. A full output buffer pauses processing without consuming additional input. Partial loads are valid. A fixed sale station is sufficient for launch; variable truck arrivals and capacity queues are deferred.

Money is granted when a sale transaction succeeds, not when a coin animation finishes. Repeated trigger callbacks, interrupted effects, pause/resume, and reload must not pay twice. Resource conversion and sale values should live in editable data so balance can change without rewriting the interaction code.

Launch does not promise offline earnings. Saved processing resumes consistently when the player returns; later offline progress would require a separate design.

## 8. Tools and economy

### Proposed launch equipment ladder

| Track | Initial content target | Visible benefit |
|---|---|---|
| Pickaxe | Basic model plus three earned upgrade levels | Stronger hits, improved strike cadence where appropriate, and a clear visual change at major milestones |
| Backpack | Starting capacity plus two upgrades | More units per trip and a readable change to the carrying setup |
| Wheelbarrow | One unlockable barrow plus one capacity upgrade | A larger haul and a visible storage improvement |

These counts are content targets, not a fixed economy. Sprint 1 does not require another tool type such as a drill or sledgehammer. Add a new tool type only when it solves a distinct problem and its animation and interactions are ready. Avoid offering several confusing alternatives at once.

Use one spendable currency from sales and contracts. Chapter or worker level reflects completed milestones; it is not another currency to grind. Upgrades persist throughout the first site. The wheelbarrow is owned after acquisition, with no rental timer, fuel, stamina, or repair tax in launch scope.

### Balance rules

- Demonstrate an upgrade on comparable work so its benefit is obvious.
- Tune early prices around a small number of enjoyable deliveries, not a long repetitive grind.
- Give a clear before/after preview, such as fewer expected hits on the highlighted stone or increased cargo capacity.
- Preserve earning opportunities if the player buys an optional upgrade before the recommended one.
- Keep larger orders reasonable for backpack users; wheelbarrow use offers efficiency rather than access to otherwise impossible rewards.
- Avoid hard countdowns or penalties in the first site. Early difficulty should encourage improvement without threatening progress loss.
- Define contract progress precisely: delivered material, sold output, or cleared deposits. Do not label all of these as an unexplained percentage.

Exact capacities, health values, prices, rewards, and unlock timing remain balance data. Do not copy the reference's values. Validate the mining-to-hauling relationship before assigning final numbers.

## 9. Environment and content

Proposed Sprint 1 size: one original compact site with three connected work areas introduced through the chapter sequence. They are parts of one coherent journey, not three mandatory separate worlds.

The opening area places a few soft stones, the processor, and sale point within easy sight. A later area adds larger orders and comfortable wheelbarrow routes. The final area introduces harder deposits and a visible completion objective. Opening access or a shortcut should make subsequent travel more convenient.

Use clear landmarks, generous paths, comfortable corners, and room to park at every station. Decoration must not hide material or create collision traps. Avoid copying the reference's layout, stone grid, station placement, colors, signage, or camera framing exactly.

Start with installed Kenney nature and factory assets as licensed building blocks. Adapt a coherent selection in Blender; preserve source files and export new versions. A wheelbarrow, pickaxe, backpack variations, and appropriate hand/handle attachment points need dedicated asset preparation. Their acquisition and license status must be checked before production use.

## 10. Presentation and accessibility

Use a cohesive stylized low-poly look with readable silhouettes. Distinguish worker, deposits, loose pickups, raw material, and processed output at the intended phone size. Choose an original palette and interface treatment during the first visual pass.

Required worker animation set: idle, walk, faster movement if used, pickaxe strike, carrying locomotion, wheelbarrow engage/release, pushing idle, pushing movement, and turning. Loading/unloading can use short contextual motion and bounded item effects without a separate animation for every unit. Hand alignment and foot sliding must be reviewed in gameplay, not only in an animation preview.

Audio should distinguish a normal strike, a final break, pickup, deposit, processing completion, sale, and upgrade. Vary repetitive impacts subtly and limit overlapping sounds. Add a quiet ambient bed and optional music. Include independent music and effects controls, haptics toggle, and reduced camera shake.

### Main interface

| Screen or HUD area | Essential information |
|---|---|
| Main menu | Continue, New Game with reset confirmation, Settings |
| Gameplay HUD | One main objective, currency, active carrier capacity, movement control, relevant context action |
| Equipment panel | Current level, next improvement, price, availability condition, and purchase result |
| Station feedback | Accepted material, quantity transferred, processing status, and reason a transfer stopped |
| Pause and settings | Resume, audio, haptics, reduced motion, and return to menu |
| Chapter completion | What was completed, reward, visible site improvement, and the next available activity |

Use icon plus text cues; do not communicate full cargo or material type through color alone. Keep labels legible, controls comfortably sized, and HUD content inside safe areas. Avoid placing important information behind thumbs or notches. Use portrait-only presentation on Android and iOS. Tune camera framing and thumb reach through representative mining and wheelbarrow control tests within that orientation.

## 11. Saves and interruption

Save chapter progress, claimed rewards, currency, owned equipment, upgrade levels, deposit damage/depletion, loose valuable resources, both carrier inventories, processing input/output, and player/wheelbarrow positions.

Use versioned saves and a recoverable backup. Save after important transactions and at suitable checkpoints; do not rely solely on application exit. Purchases, sales, material transfers, and chapter rewards must restore consistently as completed transactions or unchanged prior state.

On pause or loss of focus, stop input and mining safely. On resume, restore a valid actor state and explain the current objective. If a saved location is invalid, restore at a safe nearby point without losing cargo. New Game requires confirmation and must not be triggered by a routine navigation action.

## 12. Sprint 1 release scope

| Included in the first release | Deferred |
|---|---|
| Original playable environment and first chapter sequence | Bulldozer and other heavy machinery |
| Pickaxe mining and tool levels | Large tool catalogs with overlapping roles |
| Backpack collection and upgrades | Complex inventory management screens |
| Optional wheelbarrow, separate cargo, and hauling upgrades | Rentals, fuel, stamina, and equipment breakdown |
| Processing, sales, contracts, and one currency | Variable truck schedules and multiple currencies |
| Polished controls, UI, animation, audio, and save/resume | Worker management, multiplayer, live events |
| Android/iOS builds, device QA, and release materials | Offline earnings and server-dependent features |

Monetization is undecided. The design baseline adds no ads or purchase SDKs. If monetization is requested for launch, define its placement, economy effects, disclosures, and testing before release; never insert it as an unplanned dependency or use paid relief to compensate for tedious balance.

The launch chapter needs a satisfying completion state and a small set of optional repeat supply jobs. Make a basic repeat job and its clearly designated replenishing work patch available after the first sale, so players can recover from optional spending before completing the chapter. Finished story areas and restoration progress must stay complete. Do not silently reset the site. The main chapter has a finite completion goal even when optional replay remains available.

## 13. Sprint 2 and later progression

Sprint 2 is a post-release expansion informed by the first release's playtests and player feedback. It should continue the same pattern: a new job reveals a manageable limitation, one new capability addresses it, and the player enjoys the improvement.

Proposed Sprint 2 focus:

1. Improve first-release control clarity, pacing, save reliability, and performance where evidence shows problems.
2. Add a second story chapter or site with a new route or material decision.
3. Introduce the next justified hand-tool or hauling tier. A heavier hand tool or improved cart is a candidate, not a locked commitment.
4. Add contracts that use the new capability and preserve meaningful uses for previous equipment.
5. Carry forward equipment and earned progression with an explicit save migration.

Do not promise a bulldozer in Sprint 2 by default. Heavy machinery gets its own future milestone after the smaller equipment progression is established. Define its purpose, cost, control changes, cargo rules, and compatible jobs before scheduling it. A bigger model alone is not a meaningful progression step.

## 14. Improvements over the reference experience

The reference establishes the hand-tool-to-larger-equipment journey we want to preserve. The project's recorded observations inform the following original design responses; they are not claims that every reference build has the same issue.

| Observation or design concern | Our response | How to judge the result |
|---|---|---|
| Equipment interaction can be unclear | Explicit Use/Leave controls and a brief first-use objective | A new player can load and unload the wheelbarrow without explanation from the developer |
| Equipment and personal cargo can be confused | Separate inventories, an active-carrier display, and an inactive-load reminder | Switching never appears to lose or magically move items |
| Recommendations can compete with the current lesson | Contextual guidance and recognized alternate upgrade paths | Players understand why the next suggested purchase helps |
| Repeated hauling can feel routine | Capacity relief, useful shortcuts, and a choice of carriers | Players notice fewer trips and make a reasoned carrier choice |
| Effects and large loads can obscure play | Bounded visual piles and restrained impact effects | Worker, stone contact, and destination stay readable |
| Progress indicators can be ambiguous | Named chapter objectives and concrete before/after site changes | Players can explain what advances the current objective |
| New content can only increase numerical effort | Add a new capability or route decision gradually | Later jobs feel different without erasing prior upgrade benefits |

The game must use original creative content and tuning. Reference footage and any previously inspected APK information are research only; they are not sources for shipping models, textures, animations, code, levels, or branding.

## 15. Technical foundation and performance

Use the existing Unity URP project and modular responsibilities for input, movement, targeting, mining, inventory, processing, transactions, progression, saves, and presentation. Editable equipment/deposit/contract data should be independent from runtime save state.

Keep one active input owner. Gameplay transactions remain authoritative when animations, DOTween, Feel, particles, or audio are disabled. Reuse existing tools deliberately rather than importing duplicate motors, joysticks, tween libraries, or loggers.

Initial performance target: stable 30 FPS on named baseline Android and iPhone devices. An earlier modest Android 3–4 GB planning target does not establish minimum supported hardware. Frame budget is 33.3 ms; aim for headroom and validate after a sustained session.

Use pooled cosmetic effects, compact resource data, simple collisions, limited transparent overdraw, shared materials, and restrained lighting. Review around 150k visible triangles or 120 batches as starting authoring thresholds, not universal limits. Prefer few material slots and a small active skinned-character count. Do not create a rigidbody for every inventory unit.

Run a sustained session of at least 20 minutes on each chosen baseline device. Record device/OS, graphics settings, frame-time percentiles, memory trend, allocations, and thermal slowdown. Include repeated mining, full cargo, wheelbarrow turns, station transfers, menu use, pause/resume, and save reload. Validate release builds as well as development profiling builds.

## 16. Implementation milestones within Sprint 1

These are development steps inside the first release, not separate store sprints.

| Milestone | Deliverable | Exit condition |
|---|---|---|
| Foundation repair | Resolve editor exception and input integration; prepare mobile toolchains | Test scene and intended input setup operate without blocking errors |
| Mining slice | Worker, pickaxe, one deposit, impact timing, pickups, backpack | A player can mine and collect; canceling a swing cannot double-hit |
| Earning slice | Processor input/output, sale, first upgrade, transaction saves | A complete earning loop survives interruption and restart |
| Hauling slice | Wheelbarrow, Use/Leave, independent inventories, station support | Both carriers complete the loop without lost or duplicated resources |
| Progression chapter | Upgrade ladder, contextual lessons, contracts, site access | A fresh player progresses from basic gear to better hauling without a dead end |
| Art and experience pass | Original site, production animation, audio, HUD, accessibility settings | Entire launch chapter is understandable and visually consistent on phones |
| Release validation | Device QA, balanced chapter, saves, store materials and configured builds | All critical acceptance checks pass and owner-controlled release details are ready |

Build and test the small mining and earning slice before polishing the full site. The environment is part of Sprint 1, but its final art should support proven interaction distances and hauling space.

## 17. Acceptance criteria

### Gameplay and clarity

- A fresh player can mine, process, sell, buy an improvement, and resume play using in-game guidance.
- Damage is applied once at pickaxe contact. Movement before contact cancels the hit, and continued movement prevents unintended mining.
- Tool improvements have a visible, measurable benefit on familiar deposits.
- Backpack and wheelbarrow both remain practical for ordinary work after the wheelbarrow unlock.
- Leaving, reusing, unloading, recovering, or reloading the wheelbarrow preserves both inventories.
- Full capacity never destroys valuable drops, hides the reason collection stopped, or blocks unloading.
- Processing with full output pauses safely. Reentering a sale area cannot duplicate payment.
- Buying an optional improvement does not leave the chapter permanently unaffordable or impossible.
- Chapter rewards are awarded once; the final job has a clear completion state.

### Mobile quality

- HUD and controls work on selected small/large screens and safe-area configurations.
- Touch cancellation, app backgrounding, menus, and resume clear stale movement and mining input.
- Save recovery is tested during purchases, transfers, processing, sales, and chapter completion.
- The selected Android and iPhone baseline devices sustain the performance target without growing memory or recurring blocking errors.
- Audio and haptics settings work and persist; reducing effects does not change game results.
- Both platform builds are verified on devices; Editor playback alone is insufficient.

### Release completion

The first release needs final identity, icon, screenshots from the actual build, descriptions, support details, asset-license records, accurate privacy/data disclosures, configured signing, and tested distribution builds. Verify applicable store requirements at submission time. Store approval is external and is not guaranteed by this document. Publishing requires the owner's release authorization.

## 18. Current implementation status

The existing work is preparation: project audit, reference research, asset/license register, installed Cinemachine and Android Logcat, and an isolated CC0 worker test with four simple custom Blender clips. It is not the playable first release.

The existing technical notes report Unity 6000.3.12f1, URP 17.3.0, a legacy-input integration mismatch, an editor hierarchy-icons exception, and missing mobile build modules at the time of inspection. Recheck these before implementation. The worker test requires production mining/pushing animations and material optimization. No environment, production pickaxe system, backpack loop, wheelbarrow gameplay, or complete chapter is claimed as implemented here.

## 19. Decisions to resolve through production

| Decision | When to settle it |
|---|---|
| Public game title, setting names, and story dialogue | Before final UI and store artwork |
| Exact camera framing and input layout within portrait orientation | During the mining and hauling control slice |
| Specific baseline phones and supported OS/device range | Before performance sign-off and release configuration |
| Wheelbarrow, pickaxe, backpack models and animation sources | Before importing production assets; record licenses |
| Exact resource types, chapter details, prices, capacities, and unlock pacing | After the first complete earning and hauling loops can be played |
| Monetization and any analytics | Before related SDKs, economy changes, or release disclosures |
| Next tool type and eventual heavy machinery milestone | After first-release progression is validated |

## 20. Project references

- [Existing gameplay observations and lessons](Codex-Handoff/Bulldozer_Video_Addendum_2026-10-26/03_GAMEPLAY_LESSONS.md): reference behavior, including separate hauling and mining responsibilities. Its older sprint recommendations are superseded by this document.
- [Detailed local recording observations](Codex-Handoff/Bulldozer_Video_Addendum_2026-10-26/02_OBSERVATIONS.md): evidence for backpack/equipment cargo and processing states.
- [Project audit](Sprint1-Foundation/PROJECT_AUDIT.md): reusable assets, packages, and previously observed technical gaps.
- [Asset register](Sprint1-Foundation/ASSET_REGISTER.md): source, license, and acquisition status. A listed asset is not automatically downloaded or approved for production.
- [Tooling and animation validation](Sprint1-Foundation/TOOLS_AND_VALIDATION.md): completed technical checks and their limits.
- [Earlier mobile budget research](Codex-Handoff/Bulldozer_Free_Assets_Mobile_2026-10-26/04_LOW_END_MOBILE_BUDGET.md): initial performance guardrails; vehicle-specific scope is deferred.

The governing progression is **a manageable job, a useful earned improvement, easier familiar work, and then a new challenge**. Every new tool or equipment unlock must serve that progression.
