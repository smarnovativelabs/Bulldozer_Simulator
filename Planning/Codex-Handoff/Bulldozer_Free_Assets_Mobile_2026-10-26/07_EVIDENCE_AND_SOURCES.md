# Evidence and source record

Research date: **26 October 2026**. Files are a dated reference, not a claim of permanent availability.

## What was checked

- Official asset/author pages for free status, license labels, stated content and versions.
- Live Sketchfab rendered previews and model-information panels for the three shortlisted vehicles/mining models. Direct web fetches returned 403 on some Sketchfab pages; their browser UI supplied the observations instead.
- Local Unity version, package manifest, existing plugin reference and Kenney license/model filenames.
- Selected local Kenney OBJ files by text parsing, without opening Unity or Blender.

No new asset downloads, model-file inspections of external candidates, animation retargeting, Unity imports/builds or real-device profiling were performed. No video or package-size label is treated as performance proof. Some Unity description details came from the indexed official page when the live text extraction omitted its description; the JSON marks that distinction.

## Local geometry observations

| Existing OBJ basename | Position vertices | Estimated triangles | Unique usemtl names |
|---|---:|---:|---:|
| `rock_largeA` | 42 | 80 | 2 |
| `rock_smallA` | 10 | 16 | 2 |
| `cliff_block_rock` | 8 | 12 | 2 |
| `cliff_rock` | 18 | 32 | 1 |
| `tree_simple` | 33 | 62 | 2 |
| `tree_pineSmallA` | 84 | 164 | 2 |
| `ground_pathRocks` | 60 | 102 | 3 |
| `log_stack` | 92 | 184 | 2 |

Method: sum `(number of vertices in each face − 2)` for ordinary polygon triangulation. Position vertices do not include UV/normal seam duplication; FBX import and Unity rendering may yield different vertex counts. Material-name count is not draw-call count. SHA-256 hashes, paths and OBJ byte sizes are in `local_nature_inspection.json`. Only a small representative subset was inspected.

Local evidence paths relative to the Unity project:

- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `Assets/1_Plugins/kenney_nature-kit/License.txt`
- `Assets/1_Plugins/kenney_nature-kit/Models/OBJ format/`
- `Assets/1_Plugins/kenney_nature-kit/Models/FBX format/`

## Source index

Exact asset URLs, creators, licenses, listed counts, evidence modes and open checks are recorded per asset in [asset_catalog.json](asset_catalog.json). Rejected candidates are retained so an agent does not mistake outdated free lists or conflicting licenses for approved choices.

Technical and license references:

- [Unity 6.3 URP performance settings](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html)
- [Unity profiling on the target device](https://docs.unity3d.com/6000.3/Documentation/Manual/profiling-target-device.html)
- [Blender Decimate modifier, stable concept reference in 4.0 manual](https://docs.blender.org/manual/en/4.0/modeling/modifiers/generate/decimate.html)
- [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
- [CC0](https://creativecommons.org/publicdomain/zero/1.0/)
- [Unity Asset Store terms/EULA](https://unity.com/legal/as-terms)
- [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license)

The Blender reference describes the operation, not the exact UI in the user's installed version. All budgets, palette choices, asset roles and sprint integration steps are recommendations by this review, not claims made by asset authors.

## What needs refreshing later

At acquisition, confirm the chosen page still offers the asset free under the recorded license and save the downloaded license/version. After import, replace unknown geometry/material/texture/rig values with measured results. Revisit performance budgets only when a new device, content change or profiler evidence warrants it. Keep this research and append the actual import/device results instead of repeating broad discovery.
