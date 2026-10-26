# Free assets for the mobile bulldozer game

Research checked **26 October 2026**. This is a curated selection and preparation plan for the existing Unity project. No new vendor assets were downloaded, imported, modified or device-tested during this review.

The user confirmed **walking and driving are equally important**. The initial performance target is **30 FPS on a modest Android phone with 3–4 GB RAM**, subject to testing on a named device. Mobile is the shipping priority; any earlier desktop-first assumption is superseded. Keyboard controls are only a development convenience. Screen orientation and exact minimum phone remain undecided.

## What I recommend starting with

| Role | First choice | Why this combination |
|---|---|---|
| Player bulldozer | [200097's Low poly Bulldozer](https://sketchfab.com/3d-models/low-poly-bulldozer-f536793b630d4137a701a68ab7b565ce) | Simple tracked silhouette; modify cabin, blade and palette |
| Player worker | [Quaternius Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html) | Animated base; add a simple original hardhat and vest |
| Mine environment | Existing Kenney Nature Kit | Already in the project; selected rocks/cliffs are enough |
| Ore details | [purepoly Mining Assets](https://sketchfab.com/3d-models/free-low-poly-mining-assets-fb8c435dc8644a1ea4555f59a17313fc) | Select only ore, gems and a tool or two |
| Delivery hub | [Kenney City Kit Industrial](https://kenney.nl/assets/city-kit-industrial) | One building can establish the depot |
| UI and contact sounds | [Kenney UI Pack](https://kenney.nl/assets/ui-pack) and [Impact Sounds](https://kenney.nl/assets/impact-sounds) | Small selections, customized to the game |

The two Sketchfab choices require attribution. Kenney and the selected Quaternius pack are CC0. The [license guide](05_LICENSES_AND_ATTRIBUTION.md) includes conditional credit text. Blender modifications do not erase source-license obligations or guarantee freedom from third-party claims.

## Read by task

1. [Sketchfab and other free sources](01_SKETCHFAB_AND_OTHER_FREE_ASSETS.md): exact links, selection rationale and known model costs.
2. [Separate Unity Asset Store guide](02_UNITY_FREE_ASSETS.md): seven free options, when to use them and what overlaps existing tools.
3. [Blender preparation](03_BLENDER_PREPARATION.md): consistent art, pivots, mesh/material checks and export.
4. [Low-end mobile guide](04_LOW_END_MOBILE_BUDGET.md): proposed budgets, touch/camera design and real-phone acceptance.
5. [Licenses and credits](05_LICENSES_AND_ATTRIBUTION.md).
6. [First-slice acquisition and integration order](06_FIRST_SLICE_PLAN.md): walking → entering → driving → exiting, then the work loop.

For the laptop agent, paste [LAPTOP_AGENT_ASSET_PROMPT.txt](LAPTOP_AGENT_ASSET_PROMPT.txt). Machine-readable files are [asset_catalog.json](asset_catalog.json), [mobile_budget.json](mobile_budget.json) and [local_nature_inspection.json](local_nature_inspection.json). Evidence boundaries and sources are in [07_EVIDENCE_AND_SOURCES.md](07_EVIDENCE_AND_SOURCES.md).

The pack adds asset guidance to the earlier gameplay, APK, repository, plugin and video research. It does not silently choose between their different cargo/economy Sprint 1 proposals. This asset set supports either proposal.
