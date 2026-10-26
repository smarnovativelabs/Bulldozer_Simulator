# Low-end mobile performance and third-person controls

The user accepted an initial **30 FPS / modest Android / 3–4 GB RAM** target. This is a planning baseline, not a certified minimum specification. Name a real baseline phone (SoC/GPU/OS as well as RAM) before performance sign-off. Walking and driving have equal importance. Portrait/landscape remains open; test the chosen orientation early.

## Initial budgets to measure against

These numbers are proposed first-slice guardrails, not Unity limits or claims that every phone can sustain them. They are also in [mobile_budget.json](mobile_budget.json).

| Item | Starting target / review threshold |
|---|---|
| Frame rate | 30 FPS gives 33.3 ms per frame; aim for CPU and GPU each below about 25 ms in the busy loop for headroom |
| Hero vehicle | Roughly 2–6k triangles, 1–2 materials; the selected 1.9k base is already acceptable |
| Worker | Roughly 3–6k triangles after inspection; aim for 1 skinned renderer and a shared material |
| Small prop / hub building | About 50–500 / 500–3k triangles where the silhouette permits |
| Whole visible scene | Review above 150k triangles or 120 render batches; lower counts can still be slow |
| Textures | Shared 256–512 pixel palette textures; start at no more than 1024 for a hero texture if needed |
| Bursts | Start with caps of 24 cosmetic chunks, 150 live short particles and 8 one-shot sound voices; tune from captures |
| Memory | Measure process/texture/mesh/audio memory on the actual phone; no universal RAM allowance inferred from installed RAM |

Mesh triangles, imported vertices, materials, draw calls and package megabytes describe different things. Texture expansion, overdraw, shadows, skinning, scripts and physics may dominate. Review budgets when actual device evidence disagrees.

## Start with an inexpensive render setup

Use one main camera and a simple URP Forward setup. Begin with one sun; disable additional-light shadows and unnecessary post-processing. Try short-distance hard shadows or a simple blob for moving actors on the lowest tier. Keep far dressing sparse. Disable unused HDR, opaque/depth textures and expensive full-screen effects; first check whether selected particles require scene depth. Test no MSAA versus 2× MSAA and a proposed render scale of 0.75–1.0 on the target phone. Unity's [URP performance guide](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html) supports reducing these features when they are not needed.

Use shared materials and a small shader set. SRP Batcher reduces state-setup cost; it is not a promise to merge draw calls. Avoid large transparent dust clouds and layered transparent vegetation. Keep distant shadows and LOD cross-fades off on the lowest tier unless measurements justify them. Texture compression must match the selected Android device formats; check fallback behavior and memory after import.

## Walking, driving and touch ownership

- Give walking and driving the same left-thumb movement area, an obvious context action, and a predictable camera recenter button. A right-side drag area can orbit the view if needed. Reserve touch areas so dragging the camera does not trigger a button.
- On foot, show **Enter** only near a reachable vehicle entry point. Use a short distance and obstruction check. In the vehicle, show **Exit** and the work/cargo UI. SafeArea and comfortable button sizing matter on small screens.
- Keep exactly one gameplay input owner. During entry/exit, suppress locomotion, switch camera target, then enable the destination controller. Switching must not leave the inactive worker or dozer moving from a held joystick value.
- Exit to a checked, unoccupied ground position beside the vehicle. If blocked, choose another valid side or show a clear message. Do not place the worker inside rocks or under the vehicle.
- While driving, disable the hidden worker's locomotion/physics/animation as appropriate, or use one inexpensive visible seated pose. Do not leave two fully simulated player actors running unnecessarily.
- Frame the dozer so the blade, obstacle edge and upcoming route are visible. For walking, keep the worker readable without occupying the interaction target. Camera transition/collision behavior needs testing in both modes.
- Handle interrupted touches, app pause/resume, notches and aspect ratios. Desktop play-mode input does not satisfy mobile acceptance.

## Make work effects cheap and reliable

Keep damage, cargo and transaction logic separate from cosmetic pieces. Pool pickup visuals and short particles. Limit simultaneous physical rubble; do not represent every inventory unit as an active rigidbody. Use simple vehicle collision and a character capsule; avoid per-track physical links.

Reuse DOTween and Feel, with cleanup when pooled objects disable. Sample or rate-limit continuous contact feedback instead of spawning a new burst every physics tick. Play reward sounds through a small voice limit. Select only required clips and verify their runtime memory. Avoid frequent object creation or managed allocations in movement/contact loops.

## Real-phone acceptance session

Build a development Android player, connect the Unity Profiler and capture on the phone. Unity documents the [target-device profiling workflow](https://docs.unity3d.com/6000.3/Documentation/Manual/profiling-target-device.html). Use Deep Profile only for a focused investigation because it changes timing; validate the final experience again in a release build.

Run the following for at least **20 minutes**, including warmed-up device conditions:

1. Walk around the site; rotate/recenter the camera and interact near obstacles.
2. Enter/exit repeatedly, including while a finger is holding the joystick and when one exit side is blocked.
3. Drive, contact a dense deposit, collect with full cargo, deliver at the hub and trigger reward feedback.
4. Repeat the loop many times; compare memory after each cycle. Look for increasing pooled objects, audio voices or retained tweens.
5. Pause/resume, interrupt a touch and reload the save. Confirm the correct active actor, camera and controls recover.

Record phone model, SoC/GPU, OS, resolution, quality tier, build type, CPU/GPU frame times, frame-time percentiles, GC allocations, process/texture/audio memory and heat-related slowdown. If GPU timing is unavailable on the device/backend, record that and use supported platform tools rather than inventing a value. Investigate sustained misses of the 33.3 ms frame budget and repeatable stutters; lowering polygon count is useful only if geometry is the bottleneck.

No assets in this shortlist have been performance-certified in this project. Approval requires the real-device results above.
