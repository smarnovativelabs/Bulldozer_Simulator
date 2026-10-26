# Bulldozer Simulator — plugin reference

Research date: **26 October 2026**. This pack describes the files actually present in the project, with official documentation and sampled video references. It is a reference for future implementation, not a report that the game has been built or tested.

## Read in this order

1. [Compatibility and known issues](05_COMPATIBILITY_AND_UPDATES.md).
2. [Installed plugin catalog](03_INSTALLED_PLUGIN_CATALOG.md).
3. [DOTween Pro](01_DOTWEEN_PRO.md) and [Feel](02_FEEL.md).
4. [Sprint 1 integration recipes](04_SPRINT_1_RECIPES.md).
5. [Validation checklist](07_INTEGRATION_CHECKLIST.md).

The [laptop-agent prompt](LAPTOP_AGENT_PLUGIN_PROMPT.txt) is ready to paste. [Source and video notes](06_SOURCES_AND_VIDEOS.md) distinguish local inspection, current online information, sampled video content, and proposed design choices.

## Project-specific decisions

| Need | Recommended starting point | Reason |
|---|---|---|
| UI fades, counters, panel motion, cosmetic pickup flight | DOTween | One code-controlled tween system with explicit lifetime handling |
| Coordinated hit, break, delivery and upgrade feedback | Feel `MMF_Player` | Inspector tuning of sound, particles, scale, camera and optional haptics |
| Mobile movement input | Input System + one Joystick Pack control | Direct `Direction` input; keep one EventSystem and input route |
| UI safe area | Crystal `SafeArea` | Existing small utility for notches and screen changes |
| World particles | A few adapted Epic Toon FX prefabs | Broad existing library; select only what the gameplay needs |
| Screen-space particles | Particle Image, later | Useful for reward presentation, not essential to proving the loop |
| Sound | A small set of AudioSources first; MMSoundManager if mixing/pooling is needed | Both routes are available; avoid duplicate playback |
| Haptics | One adapter, preferably Feel/Nice Vibrations when ready | CandyCoded is another installed option; do not trigger both |
| Character locomotion | Decide from the actual vehicle design | KCC is a character motor; it does not supply bulldozer physics |
| Debug logs | Editor/device logs first | Installed Reporter uses legacy input and needs an integration check |

These are proposed project conventions, not features observed in a finished game. Keep authoritative gameplay state in game code: damage, inventory, transactions, saves and completion must not depend on effects finishing.

## Versions that matter

| Item | Local evidence |
|---|---|
| Unity | `6000.3.13f1` in ProjectVersion.txt |
| URP | `17.3.0` in manifest and lock |
| Input System | `1.19.0`; Active Input Handling is new system only |
| DOTween core | `1.2.420` product-version string inside local DLL |
| DOTween Pro | `1.0.244` product-version string inside local DLL |
| Feel | `5.6.1` in local readme |
| Nice Vibrations | `4.1.2`, Lofelt SDK `1.3.4` in bundled readme |
| LeanTween | `2.50` source header |
| Particle Image | `1.2.0` editor source header |
| UIEffect | `4.0.0-preview.9` embedded package.json |
| CandyCoded HapticFeedback | `1.0.3` embedded package.json |

DLL strings and readmes are version evidence, not proof every loose script is an untouched vendor release. Exact local source and hashes take precedence over online examples.

**Folder names can be misleading:** JMO Assets, Sparkles and Particlecollection Free samples are partial imports containing only scripts and metadata. Their effect prefab/material/texture libraries are absent. The catalog records actual file types to prevent planning around missing assets.

**Do not blindly update Feel to the latest release.** Feel 6.3 currently requires Unity 6000.5.0f1+, which is newer than this project's Editor. The local 5.6.1 readme names 6000.0.23f1 as its setup baseline. See the [official release history](https://feel.moremountains.com/feel-releases).

## Use this pack without repeating discovery

`plugin_inventory.json` records plugin roots, purposes, versions and evidence files. `installed_api_index.json` lists actual local API signatures and feedback menu entries. `source_fingerprints.json` records SHA-256 hashes of the code, metadata and configuration used by this reference.

Run the included read-only check from any folder:

```sh
python3 check_reference.py --project "/path/to/Bulldozer_Simulator"
```

If tracked files match, use these guides directly for the covered features. If they differ, review the changed plugin/setting rather than researching every tool again. The check does not detect newly added files, compile Unity or validate every prefab, material or DLL dependency; newly installed tools still need their own inventory entry.

## Scope and limits

- Examined local plugin code, bundled readmes, XML API documentation, package settings and assembly configuration.
- Read official DOTween/Feel pages and inspected all six supplied YouTube pages with selected paused samples. These are not full-video transcripts or full watch-throughs.
- Unity was not opened or imported, and no build, runtime profiling, device haptics test or shader validation was performed.
- No packages, scenes or gameplay scripts were changed. No assets were deleted or upgraded.
- This pack contains original notes and metadata, not redistributed plugin source, binaries, art or video downloads.
- The earlier project plan and research pack propose different first sprints. Follow the latest human-approved design; these plugin recipes do not resolve or combine those proposals.
