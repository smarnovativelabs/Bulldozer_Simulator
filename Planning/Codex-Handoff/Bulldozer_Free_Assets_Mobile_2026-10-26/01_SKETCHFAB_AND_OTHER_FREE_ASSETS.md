# Sketchfab and other free assets

Checked 26 October 2026. Recommended uses below are our design judgments; listed counts and licenses are source observations. A downloadable mesh is not a working Unity vehicle or character controller.

## Vehicle and mining shortlist

| Asset and source | Verified listing facts | Recommended use / preparation |
|---|---|---|
| [Low poly Bulldozer — 200097](https://sketchfab.com/3d-models/low-poly-bulldozer-f536793b630d4137a701a68ab7b565ce) | CC BY 4.0; about 1.9k triangles / 1.1k vertices; 4 materials, 1 texture; FBX; no rig or animations | First vehicle. Preview is a chunky yellow tracked dozer with cyan windows. Make our own cabin/roof/blade details; aim for one shared palette material plus an optional second material. Inspect moving-part separation after download. |
| [Free Low Poly Mining Assets — purepoly](https://sketchfab.com/3d-models/free-low-poly-mining-assets-fb8c435dc8644a1ea4555f59a17313fc) | CC BY 4.0; 16 assets; about 7.7k triangles across the WHOLE set; one material and 256×256 texture stated by author | Use ore/gems selectively. Author lists ore stone at 270 triangles and gems at 20–40 each. The lantern is 2,294 triangles; do not assume all props are tiny. Rustic horse cart/wood styling may not suit the depot. |
| [Low Poly Excavator — FSkala](https://sketchfab.com/3d-models/low-poly-excavator-9be8a258f34f4a08b50414791b921d66) | CC BY 4.0; about 5.6k triangles / 3.4k vertices; 1 material, 1 texture; FBX; no rig/animations | Alternative vehicle later. Despite the name, the inspected preview is a tracked front-bucket machine, not a long-boom excavator. Pixel-art surface detail needs restyling to match a flat-color world. |

All three rendered previews and model-information panels were inspected. Mesh files were not downloaded. Source pivots, child-object structure, texture dimensions where unlisted, collisions and Unity shading still need inspection. Sketchfab rounds geometry counts; these are not profiler measurements.

## Environment, worker and presentation

| Free source | License / listing | Use in this project |
|---|---|---|
| [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) | CC0; 330 files on current page | **Reuse installed copy.** Rocks, cliffs, sparse trees and logs cover the first worksite. |
| [Kenney City Kit Industrial](https://kenney.nl/assets/city-kit-industrial) | CC0; 40 files | Pick one depot building and perhaps one background structure. Avoid building a complete factory simulation in this sprint. |
| [Kenney City Kit Roads](https://kenney.nl/assets/city-kit-roads) | CC0; 90 files | Short approach road and signs if needed; a simple ground strip is enough initially. |
| [Quaternius Ultimate Modular Men Pack](https://quaternius.com/packs/ultimatemodularcharacters.html) | CC0; 11 characters, 24 animations; four interchangeable parts per character; includes Blend/FBX | **First-slice worker base**, because walking matters equally. Select one character; build a hardhat and vest, simplify chosen parts and test animation import. Polygon count is not listed. |
| [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0; 130 files | Audition a few contact/break/delivery sounds. Engine-loop coverage has not been verified. |
| [Kenney UI Pack](https://kenney.nl/assets/ui-pack) | CC0; 430 files | Use a few buttons/panels; draw original enter, exit and cargo icons. This is artwork, not joystick functionality. |

Use individual free pack downloads. Kenney's paid all-in-one bundle and Quaternius's optional paid Source/Patreon offerings are separate from these selected free packs. Do not assume every asset on either site has the same offer or contents.

## Already available: actual local nature files

Existing folder: `Assets/1_Plugins/kenney_nature-kit/`. Its `License.txt` identifies Nature Kit 2.1 under CC0. Start with the following FBX names in `Models/FBX format/`:

- `rock_largeA.fbx`, `rock_smallA.fbx`: mine deposits and loose rock silhouettes.
- `cliff_block_rock.fbx`, `cliff_rock.fbx`: small level boundary and quarry face.
- `tree_simple.fbx`, `tree_pineSmallA.fbx`: sparse distance dressing.
- `ground_pathRocks.fbx`, `log_stack.fbx`: optional site details.

Companion OBJ files were counted offline. See the table in [the evidence guide](07_EVIDENCE_AND_SOURCES.md) and full paths/hashes in [local_nature_inspection.json](local_nature_inspection.json). Those counts do not establish FBX import vertex counts, draw calls or phone performance.

## Results deliberately excluded from the first acquisition

| Candidate | Reason |
|---|---|
| [GOGA Construction Equipment collection](https://sketchfab.com/3d-models/construction-equipment-lowpoly-free-3d-models-dcd9c8edd2cd4ed3bee650a82e3f091d) | Approximately 1.6 million triangles for the collection. “Low poly” in a title is not a budget. Individual models might be salvageable, but this needs more inspection. |
| [semyon.kan Bulldozer](https://sketchfab.com/3d-models/bulldozer-b06a715d23a7450babac383b8bb7fb0a) | 111.5k listed triangles and realistic PBR styling; substantially more work than the selected hero base. |
| [Lluc Guardiolaa Stylized Excavator](https://sketchfab.com/3d-models/stylized-excavator-ab77d4892fbb4743a638e29df7f51e7a) | CC BY-NC; omit from a potentially commercial game shortlist. Editing does not remove the noncommercial restriction. |
| [RgsDev vehicle pack](https://sketchfab.com/3d-models/free-low-poly-vehicles-pack-cb7640039e7a40679a53be705ebff50e) | Page license and description disagree (CC Attribution versus CC0). Resolve that discrepancy first. |

The best first batch is one dozer, one worker, selected ore and one depot. Use the existing environment and effects. Add more vehicles only when the core loop and device budget are proven.
