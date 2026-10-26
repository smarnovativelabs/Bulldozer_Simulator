# Unity mining-game research and Sprint 1 handoff

Prepared 23 October 2026. Research reference: **Bulldozer Master: Crush & Mine**, Supercent. Your project: an **original game in the same broad mining / hauling genre**, developed in Unity, one sprint at a time.

## What to do on your laptop

1. Copy or unzip this entire folder onto your laptop.
2. Give your AI agent `LAPTOP_AGENT_PROMPT.txt` and access to the folder.
3. Tell it where your existing Unity project is, if you already have one.
4. Build **Sprint 1 only**. Use the later roadmap to preserve ideas, not to expand this sprint.

The first sprint is a playable loop: **drive → break → collect → deliver → upgrade → repeat**, with a small completion objective and reliable local saving.

## Reading order

| File | Purpose |
|---|---|
| [LAPTOP_AGENT_PROMPT.txt](LAPTOP_AGENT_PROMPT.txt) | Ready-to-paste implementation request |
| [01_REFERENCE_RESEARCH.md](01_REFERENCE_RESEARCH.md) | What the reference actually shows, and what remains uncertain |
| [02_ORIGINAL_GAME_DIRECTION.md](02_ORIGINAL_GAME_DIRECTION.md) | Proposed identity, controls, art direction, and differentiation |
| [03_SPRINT_1_BUILD_PLAN.md](03_SPRINT_1_BUILD_PLAN.md) | Scope, ordered tasks, dependencies, and completion gate |
| [04_UNITY_TECHNICAL_HANDOFF.md](04_UNITY_TECHNICAL_HANDOFF.md) | Scene structure, script responsibilities, transactions, and saving |
| [05_ACCEPTANCE_AND_PLAYTEST.md](05_ACCEPTANCE_AND_PLAYTEST.md) | Checks the laptop agent must run and report |
| [06_LATER_SPRINT_ROADMAP.md](06_LATER_SPRINT_ROADMAP.md) | Incremental expansion after the first loop works |
| [07_SOURCES_AND_EVIDENCE.md](07_SOURCES_AND_EVIDENCE.md) | Sources, timestamp links, screenshot index, research limits |
| [08_APK_FINDINGS.md](08_APK_FINDINGS.md) | Static findings from the supplied Android package, with limits |
| [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md) | Concrete refinements to the existing sprint and tests |
| [apk_findings.json](apk_findings.json) / [apk_table_catalog.json](apk_table_catalog.json) | Machine-readable package evidence and selected schemas |
| [prototype_config.json](prototype_config.json) | Proposed starter values; these are not extracted game values |
| [sprint_1_backlog.json](sprint_1_backlog.json) | Machine-readable task order and acceptance criteria |
| [original_layout.svg](original_layout.svg) | Original level and HUD schematic, not reference artwork |
| [START_HERE.html](START_HERE.html) | Offline visual overview and evidence gallery |

## Decisions and assumptions

**Confirmed by you:** Unity; similar genre rather than an exact copy; gradual feature development; first sprint now.

**Proposed defaults:** stylized 3D; portrait mobile layout; Android as the eventual first device target; keyboard and mouse support in the Editor; one local single-player scene; no monetization or backend in Sprint 1. These are planning assumptions, not additional user requirements. Reuse an existing project's established version and tooling when suitable. Do not install or upgrade an Editor just to match this document.

**Working codename:** Strata Salvage. It is a temporary project label, not a cleared commercial name.

## Evidence labels

- **STATIC:** verified presence in the supplied APK; not proof of runtime behavior.
- **OBSERVED:** visible in sampled video frames or the page UI.
- **OFFICIAL CLAIM:** publisher/store description or release notes, not independently tested.
- **REPORTED:** a reviewer or video narrator's account.
- **INFERENCE:** a plausible interpretation requiring confirmation.
- **PROPOSAL:** our original design or engineering recommendation.

This folder is a research and implementation brief. It contains no playable Unity project, extracted production assets, original balance tables, source-code reconstruction, or verified reproduction of the original game's balance. Video research used selected timestamps across both supplied videos, not an uninterrupted playthrough or a test install.

Reference screenshots belong in research only. Keep `references/` outside the Unity `Assets/` tree and exclude it from game builds. Create your own models, UI, names, audio, and level designs. These design precautions are not a guarantee against third-party complaints.

## APK update

The supplied file identifies version **11.3.0** and **Unity 2022.3.76f1**. Static inspection added useful data-structure evidence, implementation lessons and three acceptance checks. The file requires a separate ABI split and was not installed or run. Start with [08_APK_FINDINGS.md](08_APK_FINDINGS.md) for evidence and [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md) for what changes in the build plan. Sprint scope and original tuning remain unchanged.
