# APK-informed changes to the first sprint

These are **original implementation recommendations** informed by [static APK findings](08_APK_FINDINGS.md). They tighten the existing sprint; they do not authorize extra gameplay features. All numeric tuning in [prototype_config.json](prototype_config.json) remains our proposed starting point.

## 1. Keep definitions small and state explicit

Use one material definition with a stable ID, display name and original view references. Keep deposit HP/drop count and sale value in the existing small tuning configuration. Runtime deposits have stable IDs, remaining HP and destroyed state. Cargo stores quantity and material identity; it does not count visible meshes.

There is no need to recreate the reference's database schema or 5,605 upgrade rows. A single ScriptableObject plus simple serializable state can cover the prototype. Do not copy any reference table values, content names or asset paths.

## 2. Make every transfer accountable

The intended ownership chain is:

```text
Deposit breaks once
  → three uniquely identified fragments exist on the ground
  → accepted fragments leave the ground and enter cargo once
  → delivered fragments leave cargo and add credits + deliveries once
```

Commit authoritative state before starting a decorative pickup flight, unload animation or cash effect. At capacity, do not remove a fragment or its collider. A visual animation may be interrupted without losing or duplicating material. With the one material in Sprint 1, a scalar quantity is acceptable if the material ID is explicit at definition/transaction boundaries.

This restates the existing transaction requirement; it is not a claim that the reference implements identical guarantees.

## 3. Destroy obstacles completely and reset reused objects

When HP reaches zero, mark destruction once, disable the obstacle collider and generate the three drops once. Save the resulting world state. On reload, a destroyed deposit must not leave an invisible wall or regenerate drops already collected.

Pool only where it simplifies frequent short effects or repeated views. If a pooled object is reused, reset logical ID bindings, HP where applicable, collected/destroyed flags, collision state, target references, hit cooldown, animation state and particles. Do not use an object's runtime instance ID as a save key. A stable scene deposit may keep its logical state while a separate view is reused.

## 4. Keep drilling cadence deterministic

Separate target eligibility, damage application and hit feedback. Use the existing range, forward cone and interval. Apply the first valid hit immediately; the baseline three hits take two intervals after that first hit. Re-evaluate target validity after movement, destruction or closing a modal. Avoid a frame-dependent damage loop.

The reference has fields for resistance, mid-hit drops, heat and many modifiers. Leave those out. Our single upgrade remains a direct damage change from 20 to 30 against 60 HP.

## 5. Teach successful actions

Use a short action-driven hint sequence: movement → deposit broken → material collected → material delivered → upgrade purchased. Advance each step once after a valid success, not after a button press, entering a menu or waiting a fixed time. Keep cargo-full guidance visible when needed without resetting the main sequence.

Persist the current hint step or derive it from saved milestones so reloading does not replay completed instructions. Failed or canceled purchases cannot mark the upgrade step complete. This can be a small enum/state field; do not build a quest framework.

## 6. Save the world and economy together

Retain the coherent snapshot in [04_UNITY_TECHNICAL_HANDOFF.md](04_UNITY_TECHNICAL_HANDOFF.md): scene/layout version, rover position, partial HP, destroyed deposits, remaining drop IDs, cargo, wallet, upgrade, delivered total, completion and minimal hint progress. Never restore only the wallet and repopulate the world with fresh ore.

Check conservation before and after reload: delivered fragments + cargo + loose fragments + potential drops from unbroken deposits = 90 for this fixed layout. Each unbroken deposit still contributes three potential drops, even when damaged. This is a test invariant for our break-only design, not an original-game formula.

## 7. Use the existing Unity plan

The reference's serialized settings are consistent with the Built-in pipeline. That does not change our proposed URP setup for a new project or justify migrating an existing project. Its input evidence also does not require matching its input implementation. Preserve the laptop project's version and conventions when suitable.

Use a safe-area HUD over a world view that can fill the screen. Test portrait and tall-screen layouts, two touches, canceled input, application pause, and UI modals. Paid shaders, vibration packages, ad SDKs and analytics are unnecessary for the first loop.

## 8. Report the extra checks

The main acceptance file now includes A19–A21:

- Destroy and reload a deposit: the path stays open; no duplicate reward or ghost collider.
- Reuse pooled views/effects, if implemented: no stale ID, state or reward. If not pooled, record that this branch is not applicable and test ordinary spawn/despawn cleanup.
- Follow hints across successful and failed actions and reload: steps advance once on success and restore coherently.

The task backlog adds these criteria to existing tasks. It adds no new feature sprint. Keep refining for Sprint 2, the first carrier for Sprint 3, and fuel, stamina, combo systems, heat, card events, recruitment and monetization later.
