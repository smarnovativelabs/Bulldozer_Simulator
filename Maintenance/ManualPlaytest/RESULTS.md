# Playthrough and vertical mining verification

## Follow-up: repeated-hit idle rotation

User identified a visual regression not covered by contact-only checks: the .22-second controller cooldown returned the Animator to locomotion/carry, twisting the pickaxe between strikes. MinerAnimator now holds the non-looping mining end pose for .35 seconds after activity and restarts the matching clip on the next swing. Movement, blocked input, airborne state and cart usage release that hold. Damage and save logic are unchanged.

`repeat-fixed-checks.txt`: 5 physical hits / 5 swings; zero idle/carry frames across 20 recovery frames; moving away restores carry. Support hand error stayed below .009m. Compile succeeded without errors/warnings. `repeat-fixed-front.mp4` is the updated normal-speed cropped front view. Original save hash unchanged; test overrides and input devices restored.

Disk filled during this follow-up. Removed only 560 generated PNG recording frames from front-frames, vertical-frames and vertical-block-frames; their encoded MP4 videos are retained. New recording uses compressed JPG intermediates. The older individual raw frames referenced below are no longer retained.

2026-11-05. Unity 6000.3.13f1. All gameplay tests used isolated saves under this directory. Original progress SHA256 remained A04C56CEB39DECB593D459275E653E8658812F3BCF5ACFE3AD1488A382EF7618.

## Input-driven playthrough

Desktop capture timed out, so this was a screenshot-guided live controller-input / UI-pointer-event playthrough, not a verified physical mouse/keyboard or phone test. No player teleports, direct damage calls, cash injection or progression shortcuts were used in the initial playthrough.

- Fresh save: 12 first-shift rocks; initial full bag stops mining until first refinery deposit.
- First deposit, refinery processing, output pickup and truck loading/sales worked.
- Pickaxe and cargo upgrades bought through workbench UI; power 1 -> 2 and cargo 5 -> 6.
- Mining continued with full bag after first deposit; extra drops stayed on ground.
- All 12 rocks cleared, final partial truck dispatched, 11 truck deliveries completed.
- Next Excavation UI opened shift 2 with 24 rocks; cash 1010, upgrades and wheelbarrow unlock carried forward.
- Full action/screenshot telemetry: actions.txt, view-001.png onward. Tool latency is not representative player duration or device performance.

## Reproduced issues fixed

- Empty hands + ready refinery output + loose drops: guidance now prioritizes collecting processed output. Authentic save fixture reproduced the old misleading route. First bag/optional upgrade/load priorities remain intact.
- Workbench roof could hide player from some camera angles: split its original 12 triangles into its own renderer and temporarily hide it only when obstructing the camera ray. Same material, same total 108 environment-metal triangles, one additional renderer. No camera zoom.
- Pickaxe steel head was sideways across the player during the original swing. New Ground/Wall clips preserve the authored overhead body motion, bake corrected wrist orientation and supporting-hand poses into humanoid animation. No runtime IK components were added. Original source clips retained.

## Checks

- Recompile: zero errors, zero warnings.
- regression-checks.txt: 9 passes, including guidance priority, canopy hide/restore, unchanged camera position/distance, original triangle/material budget, no active FastIK.
- vertical-checks.txt: actual boulder, 2 physical hits / 2 swings, rock broke; max support-hand error 0.008821m outside Animator transitions.
- vertical-block-checks.txt: actual quarry blocks, starter-pickaxe test fixture, 5 physical hits / 5 swings, rock broke; max support-hand error 0.008821m outside Animator transitions.
- Animation regression fixtures explicitly position the player beside a rock. They are distinct from the input-driven playthrough. Hits/damage still use real animation events and physical head probes.
- Console since cursor 244: no new errors; ground-truth console errors 0. Older historical test/editor-network errors are not current game errors.
- Controls restored, Play stopped, test save override removed, capture frame rate reset, background override disabled. TMP dynamic atlas test churn restored to baseline.

## Video

- vertical-mining-front.mp4: normal-speed front view.
- vertical-mining-front-slow.mp4: cropped half-speed view of consecutive hits.
- vertical-boulder-slow.mp4: cropped half-speed boulder view.
- Frame capture uses a fixed 20 game frames per second for consistent animation review; this is not device FPS evidence. Crop/slow playback does not change gameplay camera zoom.

## Remaining release work

Actual low-end Android build/device profiling and touch/pause/resume tests remain necessary. Production service configuration is unfinished: Firebase config, ad IDs, IAP products, privacy URL, consent and SDK startup integration. QuarryServicesRoot.InitializeServices currently has no startup caller, so adding IDs alone is insufficient. Then validate purchases/restores, rewarded outcomes/offline behavior, signed AAB, native plugin compatibility and store submissions. No release-ready device performance claim is made by this Editor test.
