# Blender preparation for a consistent mobile game

This is a future editing workflow; no Blender asset edits were performed. Ordinary local mesh editing is the intended route. License obligations remain attached to source assets; see [the license guide](05_LICENSES_AND_ATTRIBUTION.md).

## Establish one visual direction

Use chunky silhouettes, simple flat-color surfaces, opaque windows and readable details. A proposed palette is teal/navy machinery with a warm orange accent; it is a design suggestion, not an approved final art direction. Match the worker's vest/helmet accents to the vehicle and depot. Judge all assets at the intended phone camera distance.

Change the dozer's cabin/roof proportions, blade profile and rear utility rack as well as color. Create original logos and icons. Give the worker a simple hardhat, high-visibility vest and recognizable shoulder silhouette. Keep hands/boots readable while walking. Do not trace the reference game's distinctive machine, UI or map layout.

## Inspect before editing

1. Preserve original source and license in an art-source folder outside Unity `Assets`; work on a copy.
2. Record triangulated faces, material slots, texture dimensions, object count, scale and rig/animation presence. Compare these with the dated listing values; unknowns are not zero.
3. Check normals, hidden duplicate meshes, internal faces, extreme transforms and transparent surfaces. Inspect selected worker bone weights and clips.
4. Identify parts that must move independently: blade and its linkage, tracks/wheels if animated, worker skeleton. Source separations and useful pivots are unverified.

## Vehicle pass

- Keep cabin/body as a simple rigid assembly. Separate the blade only where animation needs it; set its pivot at the hinge. Use a common vehicle root at a consistent ground reference.
- Preserve the main silhouette. The selected vehicle is already economical; do not automatically decimate it to an arbitrary lower count.
- Consolidate material slots through a shared palette/atlas when practical. Keep windows opaque initially. Bake visual detail into color instead of adding tiny bolts or transparent glass.
- Use a cheap visual track motion solution later. Do not build physical rigidbody track links for the first mobile slice.
- Make collider proxies separately: a few boxes/convex shapes for the body and a trigger/query region for the work blade. Detailed render geometry is not the physics shape.

## Worker pass

- Choose one body/clothing combination rather than shipping all modular combinations for Sprint 1. The pack's actual selected-character triangle count must be measured.
- Add a low-detail helmet and vest. Keep the rig and vertex weights intact; apply transforms with care around a bound armature.
- Where practical, combine selected skinned parts using the same skeleton and atlas. Verify animation and bounds after merging. One skinned renderer is an initial target, not a reason to break the rig.
- Retain idle/walk/run clips first. For entering/exiting, a short controlled pose/fade is acceptable for the prototype; a full climbing animation can follow later.
- Check foot sliding, scale, root motion policy and hand/body intersection at the vehicle. Do not assume Humanoid retargeting works until tested in Unity.

## Props, level pieces and optional LODs

Use existing rocks/cliffs at consistent scale. Remove unseen interior geometry only when safe. Keep ore pickup meshes small and share materials. The original mining-set display arrangement should not become one large runtime object.

For repeated medium-size props, test a simpler distant mesh or a shorter visibility distance. Tiny 20–40 triangle gems rarely need a separate LOD system. If simplification is useful, inspect the result from gameplay angles and preserve UV/material boundaries and silhouette. Blender's [Decimate documentation](https://docs.blender.org/manual/en/4.0/modeling/modifiers/generate/decimate.html) explains its reduction modes; it is not a substitute for checking the result.

## Export and Unity acceptance

Keep editable `.blend` files outside `Assets` and export explicit FBX files. Export only selected objects; avoid cameras/lights and unused animation takes. Verify meters, orientation and scale with a known one-meter reference in Unity, rather than assuming a universal preset works for every rig. Confirm final Unity forward/up axes and animated pivots. Keep one stable export path so future updates preserve references.

Create project-owned prefab wrappers and materials around imported meshes. In URP, start with Simple Lit or Unlit where visually suitable. Match colors on the device; imported source materials are not automatically mobile-ready. Check surface normals, shadow artifacts, animation bounds, collision footprint and body/blade alignment.

Accept the asset only after both walking and driving camera views look correct, attribution is recorded, and the device capture shows an acceptable cost. Keep the source mesh's original credit even after edits. Record our changes in the final credit entry.
