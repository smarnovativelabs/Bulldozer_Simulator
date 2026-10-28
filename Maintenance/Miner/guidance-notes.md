# Quarry guidance

The Gameplay scene now includes a non-blocking objective dialog and animated chevrons on the ground. The destination ring pulses; dialog changes slide in, reveal their text, and show short completion messages. Artwork is placeholder UI and can be replaced.

Selection follows the current saved inventory: loose pickups, raw stone input, finished stone collection, truck loading, and the first affordable pickaxe upgrade. Mining continues to be recommended while processing or deliveries run. No guide code awards money, moves the player, or writes inventory.

Navigation uses the existing default humanoid agent settings and a saved modern AI Navigation surface covering the quarry. No movement agent or navigation links were added. Thirty stationary carving obstacles follow mineable rocks and turn off after depletion; two stationary truck obstacles follow their reused vehicles. A complete route is required before arrows are drawn. Re-bake the Quarry Guidance surface if the fixed environment layout changes.

The arrows and ring share one mesh/material, with at most 48 arrows and a 32-segment ring: 256 triangles maximum. Path decisions run at 2.5 Hz, mesh animation at 20 Hz. No new particles or vehicle meshes. The UI uses the existing safe-area parent and never intercepts touch input.

Validation: guidance-verification.txt has 12 passing Play-mode checks against an isolated save. A separate visual route check returned a complete six-corner path around the refinery housing. Console had no runtime errors. Editor observation was 147 batches and 17 SetPass calls, not device profiling.
