# Plugin integration checklist

Research and documentation checks have been completed separately in `VALIDATION.txt`. Everything below is a **future implementation check**, not a test already passed.

## Baseline and compile

- [ ] Open the intended project in its pinned Editor and record the actual first Console error, if any.
- [ ] Resolve package/assembly references without importing duplicate DOTween, TMP, UIEffect or haptic libraries.
- [ ] Verify the new game asmdef can see the chosen DOTween modules and the actual Feel Tools runtime assembly.
- [ ] Verify Android input module, Nice Vibrations define if used, and native plugin settings.
- [ ] Fix the Joystick AxisOptions backing-field getter in a reviewed code change if using that API; verify read/write behavior.

## First playable loop

- [ ] Complete movement → damage → collect → delivery → upgrade with effects disabled.
- [ ] Joystick release returns to zero, screen taps reach UI, and keyboard fallback uses the agreed input route.
- [ ] Safe area works on tall/notched screens and the chosen orientation.
- [ ] A rejected damage/pickup/purchase produces no success feedback or extra resource grant.
- [ ] One accepted delivery produces one transaction even with multiple listeners/colliders nearby.

## DOTween and UI

- [ ] Open/close the upgrade panel repeatedly, including interrupting halfway through.
- [ ] Trigger an icon pulse repeatedly without accumulating scale or producing competing tweens.
- [ ] Pause the game and verify unscaled menu transitions where intended.
- [ ] Return a cosmetic pickup to its pool mid-flight; its next spawn has fresh target/state.
- [ ] Kill a visual sequence without losing or duplicating already-committed resources.
- [ ] A hidden panel does not intercept taps unless deliberately used as a blocking overlay.
- [ ] TMP text remains correct after rapid counter updates; per-character animation is optional.

## Feel, particles, sound and camera

- [ ] `MMF_Player` initializes before play, and every required target is assigned.
- [ ] Script-position particle effects appear at the accepted hit point.
- [ ] Break effects finish after a deposit is disabled or returned to its pool.
- [ ] Pool exhaustion/reuse is bounded and pooled particles never destroy their pool slots unexpectedly.
- [ ] Stop/restore returns transforms, colors and camera offsets to known baselines.
- [ ] The follow camera and shake system do not overwrite each other's transform.
- [ ] Effect prefabs render correctly in Android URP with intended sorting and depth behavior.
- [ ] Looping vehicle sounds stop on disable/scene exit; one hit does not play two copies of the same sound.
- [ ] Reduced motion, muted sound and disabled haptics preserve readable gameplay feedback.

## Actual device and performance

- [ ] Build and launch on the target Android device; record model, graphics API, orientation and build settings.
- [ ] Exercise repeated Play Mode with domain reload settings used by the project.
- [ ] Test background/resume, scene reload and repeated delivery loops.
- [ ] Record frame-time spikes, managed allocations, peak particles/audio sources and memory trend.
- [ ] If used, confirm one haptic provider on device, with throttling and preference respected.
- [ ] Run the reference checker after changes; update only affected guide sections and fingerprints.

The feedback-disabled run is the key regression check: the game must remain correct when presentation is interrupted or absent.
