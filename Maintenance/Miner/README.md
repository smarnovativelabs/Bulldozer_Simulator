# Miner player

## Animation/equipment update (27 October 2026)

- Installed Kevin Iglesias Human Crafting Animations FREE from https://kevdev.itch.io/human-villager-animations-free and Human Basic Motions FREE from https://kevdev.itch.io/basic-motions-free (official Unity packages). Publisher usage terms: https://keviniglesias.com/ . Imported assets and their original metadata are under Assets/Kevin Iglesias.
- Installed Daniel Erdmann Fast IK from https://github.com/ditzel/SimpleIK (MIT license retained in Assets/ThirdParty/FastIK/LICENSE.txt). One small adapter exposes Solve() so our driver evaluates the solver exactly once after setting grip targets.
- Removed the complete Ch17_Hair beard-card mesh. Body: 6,549 triangles. Total character, lamp, separate pickaxe and wooden carrying rack: 7,217 triangles. Five optional cargo placeholders add 40 triangles.
- Equipment has separate renderers: body, lamp, rack and pickaxe. The pickaxe is still skinned to the right hand but is NOT part of the body mesh. MinerEquipment.SetPickaxeVisible(false/true) hides/shows its independent GameObject and cancels mining when hidden. SetCargoCount(0..5) fills the rack; this is a visual cargo API, not an inventory system.
- MinerActions.controller uses breathing idle, genuine walk/run/sprint clips, jump/fall/landing, and the pack's right-handed ground/wall mining clips. The FREE mining clips are one-handed; left-hand placement follows their authored animation. MineWall selects the mining variant. E / gamepad X / SWING performs a swing; movement pauses during mining. No ore damage or harvesting system is implemented by this animation update.
- Fast IK stabilizes the carried right arm, blends back to the authored mining wrist path, and closes the fingers around the handle using humanoid finger muscles. The independent right grip target and elbow pole are evaluated before the explicit solver call.
- Regeneration: run optimize_worker.py, then consolidate_equipment.py in Blender. Unity menu Tools > Bulldozer > Install Mining Animations and Equipment rebuilds the scene player and saves the prefab. This setup intentionally replaces the generated player visual; preserve any manual customizations before rerunning it.
- Verification: Editor.MinerActionVerification.Start() in Play mode exercises movement, ground/wall mining, hiding, cargo and jump/landing. Results and close-up renders are in this folder.

The pickaxe palm pivot/orientation was corrected after visual review of the authored mining swing. Carry pose holds the head downward; the strike animation raises the head above the gripping hand. This correction is included in optimize_worker.py; correct_pickaxe_grip.py records the one-off migration and should not be rerun on an already corrected file.

## Historical notes (superseded by the update above)

### Original first pass

Source character: the existing I_Am_Janitor project, 0_Project Data/3_Arts/FBX/NPC Characters/Man 1/Man 1-1.fbx and Man 1 Atlas.png. Reused at the project owner's request; the original source asset licence still applies. No external asset download was used.

The character texture was rebuilt in Blender, preserving the source face/hands and adding navy workwear and an orange reflective shirt. Hard hat, lamp, pickaxe and ore-filled backpack were authored locally. Miner.fbx includes the rig and equipment. Separate backpack/pickaxe FBXs retain character-space placement for matching attachments. MinerAtlas.png is replaceable. Maintenance/Miner/Miner.blend and build_miner.py preserve the editable source and reproducible export.

Gameplay: Assets/0_ProjectData/0_Scenes/1_Gameplay.unity. The initial scene had only a camera and light; PlayerTestYard supplies a floor, step, ramp and camera-collision wall and can be replaced with the actual level.

Controls: WASD/arrows or left gamepad stick to move; Shift/left stick click to sprint; Space/gamepad south to jump; E/gamepad west to swing; hold RMB and drag or right gamepad stick to orbit. Mobile: left joystick, right look area, hold RUN, tap JUMP/SWING. Touch controls use temporary replaceable uGUI images and safe-area anchors.

Locomotion is a first-pass procedural bone animation. Pickaxe swing is visual only; mining targets, damage, inventory and backpack capacity are future gameplay work. No authored animation clips or mobile-device build verification are claimed.

Setup menu: Tools > Bulldozer > Set Up Miner Player (only for a gameplay scene without MinerPlayer). Existing player setups are intentionally not overwritten. Scene references connect the player, camera and touch-controls prefab assets; a standalone prefab needs those scene references assigned.

Verification: Unity Play mode movement/jump/landing/swing check passed (28 advancing frames, 2.206 m movement, 0.754 m observed jump rise). Camera obstruction reduced follow distance to 1.185 m against the wall. Exactly one active EventSystem was found. No new gameplay runtime errors appeared during these checks. See the verification text files. Mobile hardware performance and touch feel still need device testing.

### Earlier optimization

Selected the textured worker from `Assets/0_ProjectData/Breathing Idle.fbx` (53,738 triangles), instead of `Downloads/construction-worker/source/complete.fbx` (26,766 triangles; external textures missing). The complete file contributes its Walk animation through `WorkerMotions.fbx`; its other clips are retained in that animation-only export. The earlier Janitor-based miner remains available in Art/Miner.fbx (6,678 triangles including equipment).

Active model: Art/Optimized/MinerMobile.fbx, 7,415 triangles TOTAL including hard hat, simplified beard, backpack, ore, headlamp, and original locally modelled pickaxe. One skinned renderer, three material slots. The supplied rig, UVs and skin weights are preserved through per-part reduction. Both source rigs were validated as Humanoid in Unity. The body diffuse is reduced from 4096 to 2048; hair diffuse from 2048 to 1024. Equipment shares a 64px palette. Blender source: Maintenance/Miner/MinerMobile.blend. Original user FBXs are not overwritten.

The active Animator blends the supplied breathing idle and retargeted Walk. Sprint currently plays Walk faster; pickaxe swing remains additive/procedural. Dedicated running/mining clips, finger grip polish, inventory and resource damage remain future work. The user-provided models and motions retain their original source licences.

### Earlier asset review

These links were reviewed, not downloaded/imported. No matching packages were present in the local cache. Browser access for downloading is disabled in the current Work settings. The listed Unity store pages currently show free pricing; acquisition/import still needs the relevant files/account workflow.

- [Fast IK](https://assetstore.unity.com/packages/tools/animation/fast-ik-139972): candidate for hand-to-tool positioning. Do not run it over an Animator without deliberately ordering IK after animation. It is not installed.
- [Human Crafting Animations FREE](https://assetstore.unity.com/packages/3d/animations/human-crafting-animations-free-157920): highest-priority next animation import. The [publisher lists one-handed mining](https://kevdev.itch.io/human-villager-animations-free), idle and other crafting actions. The current Humanoid setup is ready for retargeting; no package animation has been integrated yet.
- [Gold Mining Game UI](https://assetstore.unity.com/packages/2d/gui/gold-mining-game-2d-mine-ui-tilset-263856): possible icon/UI source. Keep existing placeholder controls until supplied/imported sprites are available; no redesign in this player task.
- [PBR Crystals Pack](https://assetstore.unity.com/packages/3d/props/exterior/pbr-crystals-pack-290970): possible ore targets; listing reports URP compatibility. Verify mesh and material cost after import.
- [Stylized Pickaxe](https://assetstore.unity.com/packages/3d/props/tools/low-poly-stylized-pickaxe-hand-painted-game-ready-3d-asset-368916): promising stylized replacement; URP supported on its listing. Triangle count was not exposed in retrieved page text.
- [Basic Pickaxe](https://assetstore.unity.com/packages/3d/props/tools/basic-pickaxe-33718): another candidate; listing reports Built-in/URP/HDRP compatibility. Compare actual mesh scale and hand grip after import.
- [Pixel Art Rock Pile](https://assetstore.unity.com/packages/2d/environments/pixel-art-rock-pile-pack-271760): 2D sprites, suited to inventory/UI rather than third-person world geometry.
- [Pandazole Nature](https://assetstore.unity.com/packages/3d/environments/pandazole-nature-environment-low-poly-pack-212621): candidate low-poly environment set for a later level pass.
- [Forest Survival Starter Lite](https://assetstore.unity.com/packages/3d/props/low-poly-forest-survival-starter-pack-lite-334794): candidate resource/prop set; listing reports URP support on Unity 6000.3.8f1. Not imported.
- [Sketchfab lowpoly pickaxe by Ritsumei](https://sketchfab.com/3d-models/pickaxe-lowpoly-aa0f40cabcbf4496810a397f335f9833): page lists 680 triangles and CC Attribution, plus a NoAI notice. Reviewed metadata only; not downloaded or used in our model.
- The other three supplied Sketchfab pages returned 403 to the web reader: [726bd104](https://sketchfab.com/3d-models/pickaxe-726bd1041790439cba610fd8be337a42), [c6b1b5a8](https://sketchfab.com/3d-models/pickaxe-c6b1b5a8d6fd406084df9ad8bf56a54e), and [Old Pickaxe](https://sketchfab.com/3d-models/old-pickaxe-1583447683c94ed482a71e92bac4056a). Their actual geometry and current download terms were not verified.

Latest optimized-player verification: 51 advancing frames; 2.271 m movement; 0.778 m observed jump rise; grounded after landing; swing peak 1.0. Live walk blend weight 0.999999 and idle weight 1.0 observed separately. Final mesh count confirmed by Unity: 7,415. UV-channel correction verified in Blender and imported into Unity.



## Mining gameplay connection

MinerMining opens a contact window from events on local copies of the ground/wall animation clips (30%-65% of their duration). Three contact probes are derived from the steel head's bound mesh vertices and parented to its hand bone. After animation and Fast IK, swept head contact can damage only the selected, visible, reachable rock once per swing. Source animation FBXs remain unchanged.

Six mineable rocks use the project's Kenney nature-kit rock mesh. Each takes three successful strikes. The final strike reserves one of five rack slots before removing the rock's collision and visual; the ore then travels to that slot. CargoCount is the authoritative runtime count; SetCargoCount supports resetting/unloading by a future depot. A full rack prevents further damage. Inventory persistence and a depot are not implemented in this step.

MinerMiningSetup.Apply (Tools > Bulldozer > Connect Mining Rocks and Cargo) wires existing player/HUD assets without rebuilding the character and adds the demonstration rocks only when MineableRocks is absent. MiningVerification.Start in Play mode exercises timing, one-hit gating, collection, hidden tool, wall mining, full capacity and misses.

HierarchyComponentIcons now checks for destroyed Unity objects before accessing enabled state and invalidates stale cached component rows between hierarchy callbacks.

Verification outcome: all 17 recorded mining checks passed over 390 advancing frames; destroyed-component hierarchy cache regression passed. Ground and wall contact were exercised. Unity reported no new runtime errors after these checks. See mining-verification.txt and hierarchy-regression.txt.

## Depositing ore and persistent balances

Walk into the yellow OreDepositArea pad to sell all carried ore automatically. Default price is 10 coins per ore, configured by OreDepositZone.coinsPerItem. Empty/repeated trigger calls do not award coins. The HUD displays cargo and coins; sold items disappear from the rack, including any pickup still flying toward a slot.

Persistence reuses SaveFileWriter.cs from the owner's I_Am_Janitor project with its namespace adapted. A small versioned JSON snapshot holds both cargo and coins at Application.persistentDataPath/Mining/progress.json. Collection and deposit commit immediately through a temporary file and replacement; .bak retains the preceding good state. No quit callback is required to retain a completed transaction. A failed save leaves both live balances unchanged. An unreadable primary can recover from backup; unreadable or newer-version data is preserved rather than overwritten.

Tests use the Editor-only SessionState key Bulldozer.Mining.TestSavePath, pointing to a unique file under Maintenance/Miner/SaveTests. DepositVerification.Prepare sets it before entering Play mode. Erase this key after stopping Play mode to return to the real game save. Existing destructive player verification helpers now require this isolation. Do not use SetCargoCount to test against a real player save: it now persists changes.

Cargo and coins are local to this game installation; this step does not add cloud saves or persistence for rock depletion.

Deposit/save verification: 10 save-store checks passed (including backup recovery, protected future/unreadable data and failed writes); 8 live deposit checks passed; a new Play session restored 1 cargo, 20 coins and one visible rack item. Tests used an isolated save; normal game data was not changed.

## Resource blocks, refinery, trucks and upgrades (27 October 2026)

Active scene: 1_Gameplay. The previous six test rocks and direct raw-ore selling pad are retained inactive; the active route is mine -> REFINING input -> COLLECT INGOTS output -> SELL truck. Thirty beveled blocks have six independent resource IDs, matching the visible gray/orange/blue/white/gold/purple reference appearances. Each broken block contributes one matching raw item; raw and refined items use different rack meshes. Cracks appear after damage. Depleted block IDs persist with their awarded cargo.

Reference images were recovered, not missing:
- Planning/Codex-Handoff/bulldozer-master-research/references/
- Planning/Codex-Handoff/Bulldozer_Video_Addendum_2026-10-26/local-recording-evidence/
- The latter folder's wheelbarrow-refinery-sequence.jpg shows separate input/output loads and truck dispatch; sheet_0.jpg shows the block variants. The 02_OBSERVATIONS.md file records exact observation limits and timestamps.
- User's YouTube reference: https://www.youtube.com/watch?v=qKD38PeG2q4 . Its earlier sampled observations are in the same addendum. Direct web fetching could not replay it during this implementation, so the recovered frames and recorded observations were used.

This is a functional reference-inspired implementation, not a claim of exact hidden reference rules. Color names are descriptive, not verified mineral names. One raw item -> one matching ingot in 1.5 seconds, prices 10/15/20/25/30/35, damage +1 per level, rack +1 per level, upgrade prices, and the deterministic truck-capacity cycle 2/3/4/5 are prototype tuning. The footage confirms variable truck capacities and separate processing stocks, but does not verify these exact formulas. No extracted reference-game artwork is shipped. Meshes are generated locally; UI uses replaceable placeholder colors/buttons.

Walk inside the upgrade pad to reveal two explicit purchase buttons. Damage starts at 1, costs 20 + 20 per purchased level, maximum level 10. Rack starts at 5, costs 15 + 15 per purchased level, maximum capacity 20. Buttons never spend merely by entering the pad.

Schema 2 saves typed rack contents, coins, both upgrade levels, refinery input/output, partial truck load, completed trucks and depleted block IDs in one atomic snapshot. V1 saves migrate their cargo to gray raw material and retain coins. Each transfer/purchase/break commits before presentation. Full trucks pay once on dispatch; partial truck cargo persists. Processing pauses while closed and resumes with a fresh interval for the pending item; no offline processing is awarded. The original Janitor-derived atomic file writer and backup/future-version protection remain in use.

Setup: Tools/Bulldozer/Build Mining Production and Upgrades is intentionally initial creation only. Edit the existing scene/site afterwards. Runtime station rates and resource palette/prices are exposed in MiningProduction / MiningResources; scene-generated materials and meshes live in Player/Production. Keep the scene's upgrade-panel reference when reusing the site prefab (it points to the player's controls canvas).

Verification: 62 passing checks across save corruption/recovery, v1 migration, animation contact events, typed inventory, refinery conversion, truck transactions, damage/capacity purchases, full-rack handling, restart restoration, real timed area transfers, and the upgrade button. The final Play session reported zero console errors. An intermediate script hot reload exposed the existing nonserialized IK handle issue; MinerAnimator and MinerEquipment now reinitialize their transient state on enable. Testing used an isolated save under Maintenance/Miner/SaveTests; real player progress was not modified. No device build/performance test was run. production-overview.png uses a temporary overview camera; the scene keeps the existing third-person camera. upgrade-gameplay.png shows the actual placeholder purchase panel.

## Mobile art polish (27 October 2026)

Replaced the primitive truck with the user's MAN TGA FBX. The source in Downloads is unchanged. Blender's reproducible script is Maintenance/ArtPolish/optimize_truck.py; editable source is TruckMobile.blend. The optimized FBX is Assets/0_ProjectData/Player/Art/Truck/TruckMobile.fbx. Mesh audit: 25,464 -> 4,400 triangles, one renderer/material, one 1024px baked atlas from the supplied truck/wheel/glass images. Preserve the imported FBX root rotation and scale when positioning it; the scene is normalized to 3.8m length. No wheel or tipper rig is included in this static mobile variant.

New original generated QuarryAtlas.png contains stone, quarry dirt, worn steel and orange paint. Atlas imports are capped at 1024px with mipmaps, Read/Write off, compressed, and Android ASTC 6x6 overrides. Ground uses mirrored tile UVs for continuous joins. Material changes and locally generated mineral geometry distinguish copper veins, blue crystals, brilliant-cut white diamonds, irregular gold nuggets and amethyst points. Raw rack items and processed diamond/amethyst products use corresponding silhouettes. Stable resource IDs, prices and recipes are retained.

Refinery panels are textured and have vents, braces, rivets and tray detailing. Stationary surfaces are combined into four material groups; inventory props are separate. Processing smoke uses the existing Kenney particle pack. Effects are presentation-only: inventory is committed before flights or bursts. Raw items arc into the hopper; finished products fly out of the glowing mouth, with a brief spark burst; output pickup and truck loading also animate. Effects are pooled: 12 flying items, at most 24 smoke particles total and 10 sparks. Smoke stops when input is empty/output is full. Visible stock props are capped at 8 per station while labels show full inventory. Furnace light casts no shadows. A 100-flight stress check retained 12 objects and left inventory unchanged.

Validation: 17 mining-contact checks, 21 production/upgrade checks and 6 migration/save checks passed during this art pass. Unity confirmed 4,400 truck triangles and one material. Screenshots truck-gameplay.png and refinery-final.png in Maintenance/ArtPolish are temporary inspection-camera views, not a permanent camera change. Low-end Android FPS, GPU cost and thermal behavior have NOT been measured on a device; these are bounded asset/effect budgets, not a device-performance guarantee.

## Truck delivery and batching (27 October 2026)

The scene now uses Art/Truck/TruckDelivery.fbx: the same 4,400 triangles and 1024px atlas, with one SkinnedMeshRenderer, one shared material, six wheel bones plus a chassis bone, and one bone influence per vertex. No Animator, WheelCollider, Rigidbody or vehicle spawning is needed. Maintenance/ArtPolish/rig_truck.py maps the original decimated wheel vertices onto the baked mesh without rebaking or changing UVs; TruckDelivery.blend is the editable source. The static TruckMobile.fbx remains available as a source variant.

TruckDelivery waits .85 seconds for the last cargo flight, accelerates 32m forward over five seconds, hides for .7 seconds, resets off screen and decelerates into the same bay from the opposite side. The same vehicle is reused. The full visual load stays aboard until departure; the replacement is empty. Wheel angle derives from world distance / measured radius (.2643251m), around the physical axle. The parking collider is disabled while moving. Loading is rejected until fully parked. Pausing time pauses movement. Delivery is cosmetic: the existing atomic transaction pays once on dispatch and clears delivered inventory, so restarting during travel restores an empty parked truck without replaying payment.

Stock slots are prewarmed (8 per station); mesh/material updates only run when inventory changes. Stock uses shared per-resource materials instead of property blocks, and rock hit flashes clear their temporary property block when finished to restore SRP batching eligibility. Only fixed scenery has Batching Static enabled, never truck, ore blocks or inventory. SRP batching remains enabled.

27 delivery checks passed across two complete live cycles, including full-load visuals, rolling distance, forward departure, hidden reset, empty arrival, parking, loading rejection, next-order acceptance, pool reuse and persisted payment. See Maintenance/ArtPolish/truck-delivery-verification.txt. The game advanced from frame 1 to frame 750 during validation. Editor observations for the default game view: 139 parked batches; test average 142, peak 146; peak 18 SetPass calls. These figures are view/platform dependent and are not device FPS measurements. Current console errors: zero. Real player saves were not used. Preview: Maintenance/ArtPolish/truck-delivery.png.

## Quarry road and environment (27 October 2026)

Added a continuous eastbound road along the existing truck lane (z=-6.2), through covered rock cuttings around the off-screen recycle positions. The truck follows its original straight route without crossing dirt or the pedestrian area. Marked loading footprint, safety hatching, bollards, concrete equipment pads and a connecting walkway retain the existing SELL/refinery/upgrade trigger positions. Added safety railings, quarry benches and side banks, sparse faceted pines and scree, and a workbench shelter. Sky and distance haze soften the horizon; no extra lights or particles were added.

Environment is saved in Assets/0_ProjectData/Player/Environment/QuarryEnvironment.prefab and the gameplay scene: 7,136 triangles, nine shared-material mesh renderers, simple box colliders, no runtime scenery scripts, and no new realtime shadow casters. Existing quarry atlas and shared materials are reused. GroundSurface.asset is the final flat ground mesh; it replaced the intermediate QuarryDirt mesh to ensure fresh GPU buffers after simplification. The editor-only QuarryEnvironmentSetup builder is guarded against overwriting an existing environment. Final look is in the saved scene/prefab; edit these in place.

Validation: full truck bounds and ground support checked at 65 points from x=-28 to x=36 with no obstacles; 27 live checks passed across two full deliveries with the new environment. Final default-view observation: average 144 batches, peak 150 batches, peak 18 SetPass calls. These are editor observations, not device FPS or thermal measurements. All test cargo/coins used an isolated save; the normal save path and third-person camera were restored. Previews: Maintenance/ArtPolish/loading-bay.png and quarry-road-overview.png.

## Truck coin reward feedback (27 October 2026)

MiningProduction calls RewardCoinFeedback only after the full-truck transaction successfully commits. The existing balance/save path is unchanged; animation completion never awards currency. The effect locates the COINS value in the existing CargoStatus TMP text, projects the truck to the overlay (or the lower safe edge when behind the camera), and flies a fixed maximum of eight coins toward the balance. A repeated burst reuses the same eight-element array. No DOTween or practice-project dependency was imported.

The placeholder artwork is drawn in one MaskableGraphic with the default UI material and white texture. coinSprite can replace it later without changing the flight logic. A nested canvas isolates rebuilds; no raycaster or raycast targets are added. Maximum placeholder geometry is 312 vertices / 288 triangles, falling to zero when idle. No per-coin objects, instantiate/destroy cycle, per-frame inventory access or save writes occur. CanvasRenderer is explicitly required and saved in the scene/prefab.

Eleven checks passed: rejected/partial/full payments, saved-before-animation balance, repeated busy loading, 100 burst object reuse, visual-only balance safety, cancellation, zero payment, input passthrough and mesh budget. A live same-view comparison measured 144 batches idle versus 145 with the effect active, returning to 144 afterward. This is an editor measurement; device FPS remains unprofiled. See Maintenance/ArtPolish/reward-feedback-checks.txt, reward-render-budget.txt and reward-coins.png. Verification uses an isolated save, not the player's progress.

Fresh-session follow-up: the saved renderer initialized correctly, 25 coins restored with zero replayed bursts, all eleven checks passed again, and current console errors/warnings were zero. Normal save override was cleared afterward.
