# Pickaxe force pass
Updated OneHandGround.anim and OneHandWall.anim using ForcefulOneHandAuthor.cs.
Preserves one-handed grip, fixed tool orientation, total cycle duration and carry clips.
Lift: 271ms, 8cm higher and 6cm farther back.
Downswing: 145ms (was 232ms), accelerating quadratic motion instead of easing to zero.
Contact stop: 24ms (was 116ms), then 7cm upward / 4.5cm backward rebound over 82ms.
Slightly stronger body contribution; ground and upright poses remain separate.
Contact events retimed to 34%-49% of the clip.
No added meshes, materials, particles, renderers or runtime IK; existing 120-sample bake retained.

Editor fixture verification (isolated save; actual probe collisions, no direct damage):
- Wall: 5 hits / 5 swings, target broke.
- Ground: 3 hits / 3 swings, target broke.
- Both: fixed angle error 0, left hand clear, no carry/idle between repeated strikes; movement restores carry.
- Console errors: 0.
Captured 200 advancing frames per case at fixed 20fps for review, not device profiling.
Reviewed wind-up / strike / contact frames. forceful-pickaxe.mp4 is a normal-speed cropped wall recording.
Stopped Play mode, restored controls and prior test-save override.

## Stronger follow-up
Raised wind-up another 14cm and pulled back another 8cm.
Downstroke 106ms (previous 145ms), exponent 2.3; impact stop 48ms.
Rebound 12cm up / 7.5cm back over 116ms.
Body blend increased from .45/.12 to .70/.32 for ground/wall.
Retimed contact events to 35%-50%; same duration, rendering budgets and runtime components.
Editor verification: wall 5/5 hits, ground 3/3; both break, fixed angle, clear left hand, clean repeat recovery and movement exit.
Normal-speed preview: stronger-pickaxe.mp4.
Temporary wall JPG frames removed after encoding to conserve disk space.

## Overhead arc correction
Root cause: MinerAnimator.LateUpdate forced the tool to a fixed world-facing rotation even during authored mining poses. Earlier timing changes could not create a striking arc.
Now the angle lock applies only outside mining states/transitions. Carry remains fixed; the wrist drives mining.
Baked 105-degree overhead wind-up and accelerating 155ms drive, with chest coil/unwind and braced contact. No new renderers, materials, particles or runtime IK; same sample budget.
Measured real Play-mode arc: 102.29 degrees.
Wall: 5/5 physical hits. Ground: 3/3. Targets break; free hand remains clear; no between-swing idle/carry; movement restores carry.
Console errors 0. Checks are Editor fixtures, not device performance measurements.
Preview: overhead-pickaxe.mp4. Raw JPGs removed after encoding to conserve disk.
