# Sources, evidence, and limits

Research date: **23 October 2026**, Asia/Karachi. Store information can change by region, account, platform, and update. The same Google Play URL was supplied twice; it was treated as one source.

## Source register

- **S1 — [Google Play — Bulldozer Master: Crush & Mine](https://play.google.com/store/apps/details?id=io.supercent.bulldozermasters&hl=en)**. Read directly with web tool on 2026-10-23; US English response. Contains publisher claims and individual reviews.
- **V1 — [Bulldozer Master: Crush and Mine - Tips, Cheats, and Strategy Guide!](https://www.youtube.com/watch?v=IwMD7SOjFTs)**. YouTube browser UI; selected visual samples and chapter list. Transcript panel did not load; no complete transcript obtained.
- **V2 — [Bulldozer Master: Crush & Mine #1 | Gameplay](https://www.youtube.com/watch?v=R_zuyy1o2UE)**. YouTube browser UI; selected visual samples across the recording. In-game text in Spanish. Video opened without the incomplete supplied playlist parameter.
- **S2 — [Apple App Store — official listing and version history](https://apps.apple.com/us/app/bulldozer-master-crush-mine/id6747710694?platform=ipad)**. Search-index excerpts from the official listing; direct page fetch failed. Indexed histories varied in freshness. 11.2.0 / Oct 12 appeared in the newer result; do not assume Android parity.
- **U1 — [Unity ScriptableObject manual](https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html)**. Read documentation search result; shared configuration versus runtime state.
- **U2 — [Unity Input System on-screen controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/OnScreen.html)**. Opened official documentation; use installed package-compatible docs when implementing.
- **U3 — [Unity Application.persistentDataPath](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html)**. Opened official API documentation; verify target-platform behavior.
- **U4 — [Unity persistent game data guide](https://unity.com/blog/games/persistent-data-how-to-save-your-game-states-and-settings)**. Read official guide excerpt; JsonUtility and persistence responsibilities.

- **A1 — User-supplied `Bulldozer.apk`**, inspected 2026-10-23. Version 11.3.0, version code 102, SHA-256 `b8c79b5afd638cef9536b9239ab0fa695135dc46f8bfe26e4df8c2078e981414`. Static inspection only; incomplete base requiring ABI split; publisher signing identity not independently authenticated. [Findings](08_APK_FINDINGS.md), [machine-readable evidence](apk_findings.json), [selected table schemas](apk_table_catalog.json).
- **T1 — [Android App Bundle format](https://developer.android.com/guide/app-bundle/app-bundle-format)**. Official documentation supporting the base/configuration APK distinction and CPU-specific native-library delivery.
- **T2 — [UnityPy project](https://pypi.org/project/UnityPy/)**. Parser documentation; locally used version 1.25.2, with incomplete custom MonoBehaviour decoding recorded.

## Coverage

Both supplied videos were opened successfully in the browser after ordinary web fetching failed. Research used selected gameplay states across their duration, plus visible metadata and the first video's chapter list. This is **sampled observation**, not a claim that every second was watched or every mechanic tested. Audio was not independently transcribed or analyzed. The first video's transcript panel remained unavailable. A YouTube advertising interruption was excluded from the evidence set.

The game itself was not installed or played. After the video research, the user supplied `Bulldozer.apk`; its manifest, metadata, serialized settings and selected TextAssets were inspected locally. No extracted production assets, original balance tables or compiled code are included in this handoff. See [08_APK_FINDINGS.md](08_APK_FINDINGS.md) for the static findings and parser limits. Screenshots are research references, not reusable game assets. Do not infer audio design, collision implementation, ad cadence or exact tuning from frames or identifiers.

The supplied second URL contained a playlist parameter `PLeWEdw2WOkRc`. The individual video ID was valid and used; the playlist's completeness and contents were not verified.

## Timestamped frame index

The PNG files are browser captures of the video player. Some have player controls, captions, letterboxing, or source recording overlays. They are not original-resolution exports from the game. Each link below seeks to the recorded point; a video's later edits could shift that point.

| ID | Source / time | Evidence | Local frame |
|---|---|---|---|
| E01 | [V2 00:20](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=20s) | The opening tutorial directs raw material through a refinery before sale. | [Processing tutorial](references/e01_v2_0020s.png) |
| E02 | [V1 00:30](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=30s) | Refinery input/output, carried-load count, a sale area and truck are visible. | [Processing and delivery area](references/e02_v1_0030s.png) |
| E03 | [V1 01:33](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=93s) | Roles, star ratings, equipment, four stat labels and a stage-only hiring notice. | [Worker management](references/e03_v1_0093s.png) |
| E04 | [V1 03:12](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=192s) | A worker building and ground interaction marker are visible. | [Worker facility](references/e04_v1_0192s.png) |
| E05 | [V1 04:54](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=294s) | A roller-crusher offer illustrates a separate heavy-equipment layer. | [Heavy equipment rental](references/e05_v1_0294s.png) |
| E06 | [V1 05:41](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=341s) | Rental signage, a gauge and large machinery are visible; exact fuel rules are unverified. | [Fuel-powered machinery](references/e06_v1_0341s.png) |
| E07 | [V1 08:32](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=512s) | Mining and carrying tabs with locked equipment progression. | [Equipment progression](references/e07_v1_0512s.png) |
| E08 | [V2 09:59](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=599s) | Rock corridors, fragments, cargo-full feedback, task card and direction cue. | [Mining and full cargo](references/e08_v2_0599s.png) |
| E09 | [V2 13:08](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=788s) | Tall carried material and a large vehicle occupy the cleared route. | [Cargo and vehicle scale](references/e09_v2_0788s.png) |
| E10 | [V2 19:42](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=1182s) | A later sample still shows the mining corridor, full load and persistent task UI. | [Repeated mining loop](references/e10_v2_1182s.png) |
| E11 | [V2 26:16](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=1576s) | Loaded character, direction marker and distance indicator on a longer route. | [Return route](references/e11_v2_1576s.png) |
| E12 | [V2 29:33](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=1773s) | The later view includes vehicles, walkways and an upgrade-related task. | [Equipment and upgrade task](references/e12_v2_1773s.png) |

## First video's published chapter navigation

| Time | Chapter topic |
|---|---|
| [00:00](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=0s) | Introduction / basics |
| [01:05](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=65s) | Hiring and workers |
| [02:19](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=139s) | Active and passive play |
| [03:04](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=184s) | Recovery and facilities |
| [03:40](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=220s) | Machinery and rentals |
| [05:35](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=335s) | Fuel and vehicles |
| [07:21](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=441s) | Advanced crushing |
| [07:54](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=474s) | Upgrades and progression |

These are navigation aids from the creator's chapter list, not independently verified conclusions.

## Conflict register

| Question | Evidence | Handling in this pack |
|---|---|---|
| Does the reference generate offline income? | Store description claims it; a Play review and a video comment question it | Unresolved; excluded from Sprint 1 |
| Are store screenshots a complete representation of play? | Reviewer expresses mismatch; video includes manual work and management | Base observations on footage; do not promise machine-only play |
| Are Android and iOS feature sets identical? | Store histories differ in date and coverage | Treat advanced release-note features as platform/version-specific claims |
| Are video prices and percentages permanent balance rules? | UI numbers vary across frames and progression | Do not transfer these numbers; our JSON contains proposed values |

## Follow-up research when it becomes relevant

Before a refining sprint, record a complete raw-input-to-sale cycle in a current build. Before worker AI, inspect task assignment and stage transitions. Before offline simulation, test real close/reopen behavior. Before monetization, inspect actual entitlement behavior and interruption placement. None of those unknowns prevents the original first-sprint prototype.

## APK evidence boundaries

A1 adds engine/version, configuration and schema evidence; it does not resolve offline earnings, current Android/iOS parity, active remote balance, purchase entitlements or device performance. Its 11.3.0 label does not establish the versions used in V1/V2. Source-file integrity and extracted-schema checks passed; the future game acceptance tests remain unrun. APK internal evidence IDs are listed in [apk_findings.json](apk_findings.json).
