# Original direction — Strata Salvage

Everything in this document is a **PROPOSAL**, not a description of the reference game. The codename can change later.

## One-sentence pitch

Pilot a compact magnetic salvage rover around a weathered coastal worksite, recover mineral fragments, and improve your drill to restore a small outpost.

The first scene is one contained site. An original six-wheeled or tracked rover with a short center drill and rear cargo tray gives the player a clear machine to operate from the start. A placeholder mesh is acceptable during implementation, but the final Sprint 1 silhouette should read as a rover rather than an untouched cube.

## Creative direction

| Dimension | Our direction |
|---|---|
| Theme | Coastal salvage and outpost restoration |
| Player | Compact unmanned rover; no copied worker portrait or avatar |
| Environment | Sandy limestone, dark slate edges, wind-worn props, irregular rock clusters |
| Palette | Deep navy `#163541`, turquoise `#43C6B6`, warm coral `#F08768`, sand `#E9DEC6`, cream `#FFF8EA` |
| Materials | Broad flat color areas, soft highlights, modest roughness; limited texture noise |
| UI | Cream cards and navy text; one upper objective strip; lower cargo meter; compact depot panel |
| Cargo | Rear tray with eight visible sockets; fill sockets instead of copying a tall material tower |
| Progress | A delivery contract that powers a beacon; no copied stage percentage layout |
| World layout | Three uneven clusters around an open route; depot and workshop together on the south side |
| Audio | Original or licensed short drilling pulses, stone ticks, pickup chimes, and a warm sale sound |

The long-term differentiator is **restoration**: recovered material gradually brings the outpost to life. Sprint 1 expresses this cheaply through one beacon turning on when the contract completes. Rich restoration construction is deferred.

## First-session sequence

1. Spawn facing a nearby deposit. One hint says “Move toward a deposit.”
2. In drill range, the rover automatically targets a reachable rock in its forward cone.
3. Impacts show damage. A broken rock yields three recoverable fragments.
4. Nearby fragments enter available cargo slots; the world retains excess fragments.
5. At capacity, show “Cargo full — return to depot” and a small direction marker.
6. Enter the depot zone to unload and receive credits. This direct-sale rule is intentionally simpler than the reference's refining chain.
7. At the adjacent workshop, purchase one drill upgrade using an explicit button.
8. Deliver at least 60 fragments and own the drill upgrade to activate the beacon and complete the first contract.

## Controls and camera

- World: XZ plane, Y up. Camera-relative movement, normalized to avoid faster diagonals.
- Desktop/Editor: WASD or arrow keys. Mouse operates UI.
- Touch: one lower-left analog stick, no separate attack button in this sprint.
- Mining: automatic while a valid target is in front and within range. Continue breaking when cargo is full, but do not collect beyond capacity; display a full-cargo hint.
- UI ownership: a touch that begins on a button must not steer. Releasing/canceling the movement touch immediately returns input to zero.
- Camera: start with an orthographic oblique view, approximately 50° down and 35° yaw, smoothly following the rover. This is our choice, not a measurement of the reference.
- Portrait layout is the initial design target; keep the Editor view usable at desktop sizes. Physical Android validation remains pending until a device is available.

## Asset brief for the laptop agent

Create one rover, one deposit with damaged/broken states, one fragment, a depot, a workshop, a beacon, ground/boundary pieces, and simple feedback effects. Use primitives, your own meshes, or properly licensed assets; record provenance in the project. Keep a consistent silhouette and palette even when using placeholders. All HUD icons can begin as original geometric symbols.

Reference images are comparison material only. Do not trace their UI, import them as sprites, reproduce their characters, copy their dialogue, or use their branding in the product. A name or color change alone is not the design goal; change the setting, shapes, layout, onboarding, cargo display, and progression framing together.

See [original_layout.svg](original_layout.svg) for a planning schematic. It is not finished art, a screenshot, or an exact placement requirement.
