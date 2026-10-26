# Unity technical handoff

This is proposed architecture for the original prototype. It does not describe Supercent's source code or verified runtime implementation. Static evidence is separately documented in [08_APK_FINDINGS.md](08_APK_FINDINGS.md); apply its practical lessons through [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md).

## Project baseline

Inspect the laptop workspace before creating anything. Preserve an existing project, its supported Editor version, render pipeline, and working conventions. For a new project, use an installed supported Unity LTS with a 3D URP template. Record the actual Editor and package versions; do not assume a specific version is installed or silently upgrade. Choose matching package versions through Unity Package Manager.

Proposed new-project stack: C#, URP, Unity Input System, uGUI Canvas with TextMeshPro, and the Unity Test Framework for a few core rules. Add no third-party framework unless the project already uses it. A small follow camera is sufficient; Cinemachine is optional. There is no need for ECS, a service backend, dependency-injection infrastructure, or NavMesh in Sprint 1.

Use the current pipeline's supported materials. Check the Game view and Editor Console for missing shaders, references, and compile errors. Preserve generated `.meta` files with their assets. Exclude `Library`, `Temp`, `obj`, and build outputs from source control.

## Suggested scene

```text
SalvageSite
  Systems
    GameSession                  # authoritative mutable run state
    SaveService
    AudioController
  Environment
    Ground / BoundaryColliders
    DepositClusters
      Deposit_001 ... Deposit_030
    Depot
    Workshop
    Beacon
  Rover
    MovementBody / RoverView
    DrillOrigin / PickupSensor
    CargoSockets_01 ... _08
  MainCamera
  Lighting
  Canvas
    SafeArea
      ContractStrip / Wallet
      CargoMeter / Hint
      TouchStick
      WorkshopPanel / PausePanel / CompletionPanel
  EventSystem
```

Suggested code folders: `Core`, `Player`, `Mining`, `Economy`, `UI`, `Persistence`, and `Tests`. Prefer the project's existing organization if there is one. The JSON in this pack is design data, not an automatically importable Unity asset; translate or explicitly load it rather than pretending it already configures a scene.

## Responsibilities

| Component | Owns | Does not own |
|---|---|---|
| PlayerInputReader | Current movement vector, input enabled state, UI/touch ownership | Wallet or movement physics |
| RoverMotor | Camera-relative movement, collision, rotation | Mining rewards |
| FollowCamera | Follow smoothing and framing | Player control |
| DrillController | Target query, visibility/range/cone validation, strike timer | Cash or inventory mutations |
| Deposit | Stable ID, runtime HP, one-shot destruction | Upgrade purchase UI |
| FragmentPickup | Stable drop ID, pickup request, visual animation | Independent wallet changes |
| Inventory | Capacity and accepted amounts | Visual prefab lifetime |
| DepotZone | Requests one authoritative sale transaction | Cash animations as source of truth |
| UpgradeService | Eligibility and atomic purchase | Direct UI text manipulation |
| ContractTracker | Cumulative delivery and upgrade requirements | Current wallet as a proxy for deliveries |
| GameSession | Consistent run state and transaction ordering | Unity rendering |
| SaveService | Versioned snapshots, disk persistence, validation/recovery | Changing authored tuning assets |
| HUDPresenter | Displays events/state; forwards deliberate button requests | Recalculating authoritative balances |

These are responsibilities, not a requirement for thirteen separate scripts. Combine small presentation components where simpler. Avoid one giant script that owns every system.

## Authored data versus runtime state

Use a small `GameTuning` ScriptableObject for designer-editable values and a serializable runtime snapshot for progress. Do not mutate an authored ScriptableObject as the player's save. Unity documents ScriptableObjects as shared asset data; a deployed game's mutable progress needs its own persistence. [Unity ScriptableObject manual](https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html)

Each handcrafted deposit has a stable unique ID that does not change when the scene reloads. Validate duplicates in the Editor. Each drop ID can be derived as `depositId + slotIndex`, with three slots. Never use a runtime instance ID for persistence.

## Movement, target selection, and interaction

Use one movement authority. A CharacterController is a reasonable new-project choice; a correctly configured Rigidbody controller is also acceptable if already established. Do not move a dynamic Rigidbody with unrelated Transform writes. Keep gameplay on the XZ plane, resolve collision against boundaries and intact deposits, and disable a destroyed deposit's blocking collider promptly.

Flatten the camera forward/right vectors onto XZ, normalize them, and construct movement from the input vector. Clamp the input magnitude to one. Rotate toward the last nonzero movement direction. Mining chooses the closest visible live deposit inside the drill's range and forward cone; ignore targets behind another blocking object. Keep a current target until it becomes invalid to prevent jitter.

Tick damage at the configured interval, not once per rendered frame. One target per strike is enough. Pause and modal UI disable movement and damage. The first valid strike is immediate; subsequent strikes follow the interval. A broken target cannot generate a second reward if two callbacks arrive in the same frame.

The Input System provides on-screen sticks/buttons that feed input controls; their visuals are authored separately. Bind desktop and touch to the same movement action and test canceled touches and UI interactions. [Unity on-screen controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/OnScreen.html)

## Economic invariants

Use integer fragment counts and credits. All mutations happen on the main gameplay thread through the session's transaction methods. Visual animations respond to a completed transaction.

```text
TryCollect(dropId):
  reject if already collected, unavailable, or cargo is full
  mark drop collected and increment inventory by one together
  emit CargoChanged; play pickup animation

SellCargo():
  quantity = inventory count
  if quantity == 0: return without side effects
  inventory = 0
  wallet += quantity * unitPrice
  deliveredTotal += quantity
  evaluate contract; mark save dirty; emit one sale result

TryBuyDrill():
  reject if upgrade owned, modal state invalid, player out of range,
            or wallet < upgrade price
  subtract price and set upgrade level to one together
  emit upgrade result; mark save dirty; reevaluate contract
```

The depot may request a transfer on entry, and again when inventory becomes nonempty while inside; the transaction remains safe because it only consumes current inventory. Do not award money every frame while the player remains in the zone. Use an explicit purchase button at the workshop so merely walking past it cannot spend credits.

If capacity is seven of eight and three fragments are nearby, accept one and leave two. A pickup visual may fly toward the rover after acceptance, but disable its interaction immediately. Decorative debris must not be counted as material.

## Saving without duplication

The snapshot should contain:

- Schema version, run ID, scene/layout version, player position and facing.
- Credits, inventory count, cumulative deliveries, upgrade level, contract completion, tutorial step.
- Every deposit's stable ID and remaining HP.
- Collected drop IDs. A destroyed deposit recreates only its uncollected drops on load.
- Audio setting and optional accessibility preference.

For this small static map, spawn uncollected drops at deterministic safe offsets around the source deposit. Presentation bounce is cosmetic. This avoids needing to persist arbitrary physics positions. If the implementation allows pickups to move permanently, save their positions instead.

Store one coherent snapshot, rather than saving money separately from inventory. Save after purchases and deliveries, on pause/background, and after a short debounce when mining/collection changes state. Do not rely only on application quit. An abrupt process kill can lose changes since the last checkpoint; never claim perfect crash-proof storage.

Write to a temporary file, validate it, then replace the primary file while retaining a last-good backup. Verify the replacement strategy on the chosen platform. Load primary → backup → recoverable fresh state with a clear message; never silently overwrite an unreadable save before preserving it. Clamp impossible numeric data and reject incompatible layout/schema versions deliberately.

Use `Application.persistentDataPath` for the target platform's save location. `JsonUtility` works for a simple serializable snapshot, but does not support every JSON data structure; use serializable arrays/lists rather than assuming dictionaries work. These API facts are documented by Unity. [Persistent path](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html), [Unity persistence guide](https://unity.com/blog/games/persistent-data-how-to-save-your-game-states-and-settings).

## Performance and presentation

Thirty deposits and at most ninety gameplay fragments are a deliberately small test scene. Pool repeated debris and hit effects where useful; a complex world streaming system is unnecessary. Keep logical pickups lightweight, avoid per-frame full-scene searches, and avoid allocating a new collection during every mining query. Query only relevant physics layers. Cosmetic particles do not need gameplay colliders.

A readable baseline is more valuable than heavy effects: one light, simple shadows, restrained dust, a short hit flash, a small mesh recoil, and clear cargo feedback. Camera shake is optional and should be subtle or disableable. Cap effect lifetime and simultaneous count. Profile on the actual target when available; Editor frame rate does not certify mobile performance.

## Acceptance evidence to return

Record exact versions, changed files, scene to open, controls, build path, test results, actual device or Editor used, measured frame behavior if available, unresolved issues, and any tuning changes from this brief. Distinguish tested results from intended behavior.

## Additional implementation notes from APK research

Treat material identity and cargo quantity as authoritative data; meshes and pickup flights are presentation. Disable destroyed deposit colliders immediately and restore that state on load. Reset reusable views before rebinding them to a new stable logical ID. Complete hints from success events, with a minimal saved hint step or derived milestones. Keep the current small architecture: the reference's large data catalog and third-party integrations are not dependencies. Its Built-in-compatible settings do not change the pipeline recommendation for our project. See [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md) for ownership and conservation checks.
