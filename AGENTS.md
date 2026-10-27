# Project instructions

## UI artwork workflow

- Use dummy images and placeholder UI assets for now.
- The user will later provide a UI reference image, a sprite sheet, or both.
- When given a UI reference image, create the needed sprites from that reference and assemble them into a sprite sheet for use in the UI.
- When given a sprite sheet, use the supplied sprites; when a UI reference is also supplied, implement the UI according to that reference.
- Keep placeholder assets easy to replace when the user supplies the final references or sprite sheet.
- Follow this rule for future UI work in this project unless the user changes it.

## Low-end device performance

- Design all gameplay, art, animation and effects for low-end devices.
- Control rendering batches and material count, reuse shared materials and atlases, and pool repeated objects/effects.
- Keep the delivery truck below 5,000 triangles. Preserve its single-renderer wheel rig unless a measured alternative is better.
- Check relevant mesh, particle and batching budgets when adding features. Distinguish editor measurements from actual device profiling; do not promise device FPS without testing it.
