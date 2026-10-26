# Free asset register

Checked 26 October 2026. Download links below are official acquisition pages or author-linked files. A listing check is distinct from downloading, importing and testing. Preserve creator, source, license, modification notes and file hashes when acquiring anything else.

## Selected assets

| Asset and acquisition link | License and formats | Intended use | Actual status |
|---|---|---|---|
| [Quaternius Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html), [specific Worker FBX](https://drive.google.com/file/d/10dPAOOrlH6m6ASa3hOcc-ElclT_cIQjU/view) | Official page: CC0; FBX/OBJ/Blend and animated pack. Downloaded individual humanoid FBX has a rig, colored materials, no clips. | Worker base; keep original, optimize selected appearance later. | Worker downloaded, imported and Humanoid Avatar validated. New custom motion FBX retargeted and played. No textures needed for the test's flat-color URP materials. |
| [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) | CC0; local FBX plus other interchange formats and palette textures. Local license identifies 2.1. | Rocks, cliffs, sparse trees. | Already installed before this sprint; license and paths checked. No environment assembled. |
| [Kenney Factory Kit](https://kenney.nl/assets/factory-kit) | CC0; installed 3.0 model pack, including FBX. Website advertises animation; individual clips not validated here. | Conveyor/depot components and industrial props. | Already installed; local license checked. Select a few pieces in Sprint 2. |
| [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0; local OGG audio files. | Break/contact/delivery cues. | Already installed; not auditioned in this sprint. |
| [Low poly Bulldozer — 200097](https://sketchfab.com/3d-models/low-poly-bulldozer-f536793b630d4137a701a68ab7b565ce) | Indexed listing: CC Attribution, ~1.9k triangles. Existing project inspection records FBX, one texture, four materials, no rig/animation. | Candidate vehicle base; restyle and author separate blade/track pivots. | Not downloaded. Direct fetch was blocked; indexed license/name/count corroborate earlier research. Confirm license version, downloadable payload and moving parts before acquisition. |
| [Free Low Poly Mining Assets — purepoly](https://sketchfab.com/3d-models/free-low-poly-mining-assets-fb8c435dc8644a1ea4555f59a17313fc) | Indexed listing: CC Attribution; 16 assets, shared 256×256 texture, ~7.7k triangles for set. Exact downloadable format not independently verified this sprint; FBX preferred. | Optional ore/gem shapes; avoid redundant rustic props. | Listing reviewed, not downloaded/imported. Obtain archive and verify FBX/texture contents before selecting. |

Quaternius's separate `License.txt` and animated master FBX downloads returned Google Drive quota-error HTML. These responses were not imported or recorded as successful asset downloads. The official pack page independently states CC0 and personal/commercial permission; its HTML snapshot and the downloaded Worker's SHA-256 are retained. The four motion clips are newly authored Blender test animations, not Mixamo clips or downloaded Quaternius animations.

## Optional fallbacks

| Source | Verified offer and license | Decision |
|---|---|---|
| [Human Basic Motions FREE](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271) | FREE, version 2.4.2; Standard Unity Asset Store EULA, Extension Asset; listed compatibility includes Unity 6000.0.59f2 and URP. Unity package; exact internal model/clip formats not inspected. | Missing production locomotion only; not acquired or tested on 6000.3.12f1. Do not import another complete controller. |
| [Kenney City Kit Industrial](https://kenney.nl/assets/city-kit-industrial) | Free CC0 3D pack, 40 files. Exact archive formats not inspected this sprint. | Defer because Factory Kit is installed. |
| [Mixamo](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) | Adobe states free with Adobe ID and royalty-free use in commercial games; humanoid bipeds only. FBX animation workflow. | Optional production idle/walk/run/interactions; no account login, upload, download or Mixamo retarget test performed. Save provenance and confirm clip-specific fit before replacing the lab motions. |
| [GrabCAD usage guidance](https://help.grabcad.com/article/246-how-can-models-be-used-and-shared) | Community models generally private-use; commercial use needs the creator's explicit permission and attribution. CAD payloads are not automatically game-ready FBX. | Interpreted the requested “Grab3D” as GrabCAD for this review. Excluded from commercial shortlist without specific permission; no downloads. |

## License handling

[CC0](https://creativecommons.org/publicdomain/zero/1.0/) permits commercial modification and distribution without attribution requirements, while trademark and other third-party rights remain separate. Keeping author credits is useful provenance.

For a CC BY asset, preserve title, creator, source link, exact license link and a note describing modifications. Example pending actual acquisition: “Low poly Bulldozer by 200097, source [Sketchfab](https://sketchfab.com/3d-models/low-poly-bulldozer-f536793b630d4137a701a68ab7b565ce), CC BY [confirm version]; modified materials and pivots.” Do not ship this incomplete credit until the version is confirmed.

Unity Asset Store assets use their stated EULA, not CC0. Check seat/license requirements and distribute only as permitted within the game; do not redistribute raw vendor packs. Existing paid plugins do not count as free acquisitions. Noncommercial Sketchfab assets and ambiguous license listings remain excluded.

Before a future import: inspect archive paths and file types outside Assets, retain the license, verify geometry/scale/pivots/rig/materials, then import only required content into a unique folder. Check Unity materials and animation on both platform build paths. No reference-game assets, levels or branding are included.
