# Original direction and Sprint 2 plan

## Game direction

Build an original mobile reclamation and construction game: a small crew clears a damaged industrial site, sorts recoverable material, and restores useful infrastructure. Retain the satisfaction of machinery, visible cargo and short upgrades, while creating original layouts, vehicle silhouettes, station designs, UI and progression values.

The reference's [official listing](https://play.google.com/store/apps/details?id=io.supercent.bulldozermasters&hl=en) describes mining, machinery and growth. The project's [video lessons](../Codex-Handoff/Bulldozer_Video_Addendum_2026-10-26/03_GAMEPLAY_LESSONS.md) give more specific evidence: visible cargo, separate processing areas, expanded equipment and some confusing purchase/interaction sequences. These inform design choices rather than dictate a copy.

## Improvements to prototype after environment preparation

| Area | Original proposal | Acceptance signal |
|---|---|---|
| Work choices | Clear debris to open a shorter return route, or recover a higher-value material first. Begin with one route choice. | Players can explain why they chose a route; no corridor traps the vehicle. |
| Walking and driving | Same left-thumb move area, explicit Enter/Exit, one context action and camera recenter. One active input owner. | Holding the stick during a mode switch never moves both actors; blocked exits are rejected visibly. |
| Cargo | Compact visual rack plus numeric capacity. Separate character and vehicle inventories. | Full state explains the next action; mounting never deletes or duplicates cargo. |
| Full capacity | Proposed first-loop rule: do not commit a deposit hit unless its entire yield fits. Leave the deposit unchanged and show return guidance. | Boundary and burst tests conserve resources; no almost-broken dead end. This replaces conflicting older proposals only when gameplay implementation adopts it. |
| Progression | One currency; meaningful choices between blade width, carrying capacity and handling. Reveal upgrades gradually. | First upgrade changes a visible behavior, not only a hidden multiplier. |
| Rewards | Short restoration contracts unlock a bridge, depot module or cosmetic crew item. | Progress counts the stated contract objective; reload cannot award completion twice. |
| Onboarding | One current task and contextual prompt; recommended purchases support that task. | A new player understands the first delivery without a modal tutorial chain. |
| UI | Original shapes/icons; readable material names, optional large text, color plus icon cues, adjustable haptics/shake. | Labels and context controls remain usable on small notched screens. |
| Audio | Distinct contact, break, pickup and delivery sounds; subtle machinery load variation. | Repeated work remains readable without a wall of overlapping sounds. |
| Environment | Warm stone, desaturated teal machinery accents and sparse vegetation; clear landmark silhouettes. | Depot remains recognizable from both walking and driving camera heights. |

Separate inventory and transaction data from DOTween/Feel effects. Disabling feedback must not change resources, money or progression. Keep cosmetic debris pooled and bounded. Avoid forced ads during the first prototype; monetization, rentals, workers and live events are later product decisions.

## Sprint 2 environment creation

1. **Lock constraints.** Target Android and iOS; name a baseline device for each. Carry forward the earlier 30 FPS / modest Android 3–4 GB planning baseline without treating RAM as a certified minimum. Decide portrait versus landscape through a camera/control check before final layout.
2. **Choose a compact asset set.** Reuse installed Kenney Nature and Factory Kit. Use the tested worker base after material consolidation. Acquire the selected dozer only after confirming its exact license, download payload and separable blade/track geometry. If unsuitable, author an original low-poly vehicle in Blender.
3. **Prepare in Blender.** Use meter units, ground-level pivots and stable names. Preserve source models. Export selected pieces as new FBX files; apply transforms, verify normals, create simple collider meshes, and keep moving vehicle parts separate. Consolidate worker/material palettes where feasible. No requirement to import every pack asset.
4. **Build one original site.** Depot, small work area, generous turn-around space, one clear return lane, one optional shortcut and a few landmarks. Do not reproduce the reference's grid, station arrangement or UI. Keep decorative geometry out of collision/navigation layers.
5. **Validate in Unity.** Import into new modular folders and prefabs. Check FBX scale, material appearance in URP, LOD behavior, capsule/vehicle clearances, entry/exit space and camera occlusion. Use Cinemachine with restrained damping and optional shake.
6. **Hand off to gameplay.** Stable deposit IDs and station sockets, collider layers, navigation boundaries, spawn/exit points and contract anchors. Record the selected full-capacity rule before implementing resource transactions. Keep authoring data separate from save state.

## Mobile performance guardrails

Treat these as initial review thresholds, not measured results: 30 FPS gives 33.3 ms/frame; aim for CPU and GPU work below roughly 25 ms for headroom. Review above 150k visible triangles or 120 batches. Start near 2–6k vehicle triangles, 3–6k worker triangles, 1–2 materials per hero and shared 256–512px palettes where appropriate. The current worker's 13 material slots need improvement despite its acceptable triangle count.

Use simple collisions, pooled effects, one main light and limited shadow distance. Profile transparency, skinning, shadows and render scale before assuming geometry is the bottleneck. Start with at most 24 cosmetic chunks, 150 short particles and eight one-shot audio voices, then tune from measurements. Match texture compression to supported devices and verify actual memory. The existing adaptive-performance engine module does not prove a thermal provider is configured.

After the first gameplay loop exists, run at least a 20-minute session on a baseline Android phone and iPhone: walk, drive, switch modes, fill cargo, unload repeatedly, pause/resume, interrupt touches and reload a save. Record device/OS, quality tier, frame-time percentiles, CPU/GPU timings where supported, allocations, memory trend and thermal slowdown. Use [Unity target-device profiling](https://docs.unity3d.com/6000.3/Documentation/Manual/profiling-target-device.html); Simulator and Editor FPS cannot substitute for these checks.

Sprint 2 is accepted when the original modular site is navigable on foot and by the planned vehicle footprint, selected assets have provenance, materials render correctly, and a baseline build can be profiled. No environment was created during Sprint 1.
