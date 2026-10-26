# Acquisition and first playable slice

The user wants both third-person walking and driving. Include a minimal version of each in the first slice. Keep multiple vehicles, crowds, a big town, elaborate seat-climbing animation and large factory chains for later.

## Small acquisition order

| Step | Action | Result to check |
|---|---|---|
| 1 | Reuse existing Kenney rocks/cliffs, joystick, safe-area helper, DOTween, Feel and selected effects | Simple mobile worksite without adding another framework |
| 2 | Obtain one Quaternius worker and the 200097 bulldozer; keep originals/licenses outside runtime folders | Correct scale and readable silhouettes together |
| 3 | Choose one on-foot motor: assess installed KCC first; official Unity controller is an alternative | Smooth touch walking, one camera and one input owner |
| 4 | Add enter/exit state transitions and a simple arcade vehicle motor | Walk → enter → drive → stop → safe exit → walk |
| 5 | Select one ore model and one depot building; primitives can stand in until then | Work → cargo → delivery loop using the chosen earlier economy spec |
| 6 | Add a few contact/reward sounds, particles and UI cues | Responsive feedback within device budget |
| 7 | Profile the combined loop on the baseline phone | Recorded acceptance data before expanding content |

Acquire only free variants through the linked official pages. Store pages may change; recheck price/license when acquiring. This is a short confirmation at acquisition, not a need to repeat the whole research. No downloads were started by this report.

## Suggested ownership model

```text
OnFoot -> EnteringVehicle -> Driving -> ExitingVehicle -> OnFoot
```

One player-session state owns the active actor, camera target, input routing and HUD mode. The worker and vehicle have separate movement implementations, but only one consumes player motion at a time. Enter/exit checks proximity, obstruction and valid exit ground. Lock repeated action presses during transitions. Reset held input when changing ownership.

The vehicle's visible blade/cargo presentation is separate from authoritative work/cargo state. Keep transactions and saving independent of animation or tween completion. Preserve the active mode and a valid position in saves, or define a consistent safe spawn rule. Handle background/resume during a transition.

Use a simple seat marker and a controlled fade/pose for the first transition if needed. A full climbing animation is polish; the two playable modes themselves are required. A single worker also avoids the cost of unnecessary NPC animation and navigation.

## Suggested folder layout for future implementation

```text
ArtSource/                 original downloads, licenses and editable Blend files
Assets/Game/Art/           selected exports, palettes and icons
Assets/Game/Prefabs/       project-owned worker, vehicle, resource and depot wrappers
Assets/Game/Scenes/        small mobile slice and isolated asset validation scene
Assets/Game/ThirdPartyNotices/
```

Adapt to the project's existing conventions before creating these folders. Keep imported vendor packages distinct and preserve their metadata. ArtSource contains licensed originals; repository visibility and ignore rules must reflect those licenses. Do not publish raw Unity Asset Store files as part of a public asset collection.

## Definition of ready

- Worker and dozer share style and scale; body/blade/helmet remain readable on a small screen.
- Both movement modes work entirely by touch, with safe entry/exit, one active camera and no duplicate input.
- The chosen cargo/work/delivery loop functions without effects determining its results.
- Selected meshes/materials/audio have recorded measured import costs, license records and credits.
- A warmed-up, 20-minute baseline-phone run meets the agreed frame target without growing memory or repeated stalls.

Earlier handoff packs contain two differing Sprint 1 economy proposals. This document does not resolve cargo size, resource types, upgrade requirements or win conditions. Follow the owner's chosen proposal; the common asset and control plan here works with either.
