# Production flow update

Implemented in the Gameplay scene and MiningProductionSite prefab:
- Two reusable 4,400-triangle trucks, each with one skinned body renderer, one body material and six wheel bones. Separate parallel routes avoid vehicle overlap.
- Billboard load/capacity counters follow each truck. Partial orders persist; delivery animation never awards money.
- Raw stone lands on the input pad before feeding into the machine. A machine label shows the current resource and remaining processing time.
- Refined stone lands on a loading pad. A persistent 100-item buffer lets the player unload even when both trucks are away; available trucks load automatically.
- Cash HUD, cash upgrade prices and pooled green banknote rewards. The legacy serialized `coins` field is deliberately retained to preserve existing balances.
- Every third completed truck grants an additional $30 in the same saved transaction as its base payout. The world label shows progress toward this bonus.
- Brighter ambient/daylight, two baked yard fill lights, 32 static receivers and one baked lightmap atlas. Moving trucks, rocks, cargo and player are excluded from static lightmapping.

Validation: `production-flow-verification.txt` records 20 passing Play-mode checks, including two simultaneous delivery cycles, buffered unloading, exact rewards, save reload, the countdown, and resource identity. `production-store-verification.txt` records six compatibility and failed-save checks. These were run against isolated saves, not the player's save. Older single-truck verification scripts assume immediate loading and are superseded by ProductionFlowVerification for the new staging flow.

Editor observation: peak 162 batches and 18 SetPass calls in this test. This is not device profiling or an FPS promise. Transfer pool remains limited to 12 objects and reward graphics to eight notes.

UI artwork remains replaceable placeholder geometry/text. `ProductionLabel.mat` is shared across the new billboard text. Reference reviewed for the production/economy loop: https://play.google.com/store/apps/details?id=com.KisekiGames.smart&hl=en

The overview image uses an isolated test save and a temporary elevated review camera; the regular gameplay camera and production speed are unchanged.
