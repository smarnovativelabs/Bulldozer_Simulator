# Findings from the supplied APK

**Inspected 23 October 2026.** The user-supplied `Bulldozer.apk` identifies itself as **Bulldozer Master 11.3.0**, package `io.supercent.bulldozermasters`, built with **Unity 2022.3.76f1**. It contains useful data schemas and gameplay identifiers. This was **static file inspection**: the APK was not installed, executed, modified, or playtested. Its publisher signing identity was not independently authenticated.

The practical outcome is a stronger Unity handoff: separate material identity from appearance, make inventory and transfers authoritative, reset reusable objects carefully, save world progress coherently, and advance tutorial hints from successful actions. These are recommendations for our original project, not a reconstruction of the reference's source code. Apply them through [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md).

## File identity and installation limits

| Field | Static finding |
|---|---|
| Source ID | A1 |
| File size | 188,182,527 bytes, approximately 179.46 MiB |
| Package / version | `io.supercent.bulldozermasters` / `11.3.0` |
| Version code | 102 |
| Minimum / target / compile SDK | 24 / 36 / 36 |
| Engine version | Unity `2022.3.76f1`, from serialized headers; also present in Android metadata |
| Scripting evidence | IL2CPP metadata, header version 31 |
| Orientation | UnityPlayerActivity declares portrait |
| Required split | Manifest declares `requiredSplitTypes="base__abi"` and split-required metadata |
| Native libraries in this file | No `lib/` entries |

SHA-256:

```text
b8c79b5afd638cef9536b9239ab0fa695135dc46f8bfe26e4df8c2078e981414
```

**This is an incomplete base APK, not a self-contained installation package.** It requires a CPU-specific split that was not supplied. Android App Bundles can deliver native libraries through configuration APKs alongside the base. The precise target CPU architecture and complete installed size therefore remain unconfirmed. The manifest and archive contents support this conclusion; no installation failure was induced. [Android App Bundle format](https://developer.android.com/guide/app-bundle/app-bundle-format)

A matching complete package would be needed for any later runtime investigation. That is not necessary to implement our first sprint. Do not transplant the reference's package ID, SDK settings, signing material, or build identity into the new game.

## How to read the evidence

- **STATIC:** a manifest value, serialized setting, table field, object type, or identifier is present in this exact file.
- **INFERENCE:** a likely purpose suggested by those structures; function bodies and runtime behavior were not verified.
- **PROPOSAL:** an original implementation choice for our project.

The machine-readable [findings](apk_findings.json) include source IDs and APK entry paths. The [table catalog](apk_table_catalog.json) records selected schemas, row counts and Unity object path IDs. Original table rows, balance values, images, audio, models and compiled code are not included in the handoff. Names below are evidence references, not names the new game needs to adopt.

## Material flow and data separation

**STATIC — A1-TABLES:** ore definitions, ore statistics, drop rules, carried items, smelting recipes and sales have separate tables. Particularly useful examples are:

| Table | Rows | Relevant fields | Design lesson |
|---|---:|---|---|
| `OreData_40` | 33 | `OreCategory`, `PrefabPath`, `SkinPath`, `DamageableType` | Separate material identity and presentation |
| `OreStatData_18` | 76 | `HP`, `Resistance`, `GameModeType` | Keep balance editable without changing artwork |
| `MineBlockDropData_2` | 65 | `DropCount`, `MidDropAmount`, `FinalDropAmount` | Drop timing and quantity are explicit rules |
| `StackingItemData_4` | 67 | `OreCategory`, `Weight`, `PrefabName` | Cargo data and its visual representation can differ |
| `SmelterData_2` | 33 | `InputItemID`, `OutputItemID`, `RefiningTime` | Treat processing as an explicit input/output conversion |
| `SalesStandData_8` | 62 | `StackingItemID`, `PriceAmount`, `StatType` | Price belongs to an economy rule, not a rendered object |

Row counts describe packaged records, not the number of resources a player can currently access. The numeric suffixes are part of the stored asset names; their meaning was not established.

**INFERENCE:** this separation supports several material stages and different tuning by game mode. It fits the raw ore → refining → sale chain observed in the videos. The table structure does not establish actual smelting throughput, precise transfer timing, or every recipe's active status.

**PROPOSAL:** our first sprint keeps one material, break-only drops and direct sale. Give it a stable material ID and separate view references. Preserve a clean point for a recipe in Sprint 2. Do not implement mid-hit drops, weight-based cargo or 33 recipes simply because those fields exist.

## Mining, cargo and object reuse

**STATIC — A1-SYMBOLS:** identifiers include `MineBlockActor`, `MineBlockInst`, `MineBlockDamager`, `MineBlockInRangeController`, `MineBlockActorPoolManager`, `MineBlockSkinPoolManager`, and `MineBlockBreakRewardDropPool`. Cargo-related identifiers include `StackingInventory`, `CargoBase`, `CargoMulti`, `StackingFormation`, `GridStackingFormation`, and sender/receiver concepts for buildings and mining units. The scan also found many packaged particle systems and collision components.

**INFERENCE:** the reference separates some mining responsibilities, uses reusable representations, and has explicit material transfer concepts. This is not proof of its collision algorithm, pooling correctness, inventory transaction guarantees, or actual number of active objects.

**PROPOSAL:** the important design rule is that a fragment has one logical owner: ground, cargo or delivered. A flying pickup effect and a tray decoration are views of that state. Do not award material again when an animation completes. Keep HP and stable IDs outside any resettable visual state. If pooling is used, reset IDs, hit flags, colliders, timers, effects and references before reuse. For 30 deposits and 90 possible fragments, a modest implementation is sufficient.

## Tutorial and objective structure

**STATIC — A1-TABLES / A1-SYMBOLS:** `QuestData_58` contains 156 records with condition, guide, reward and next-step fields. `ConditionData_32` contains 149 records. Onboarding identifiers include mining, smelting, getting an ingot, selling and opening the company. Conditions include `CargoAlmostFull`, `HasOreNearby`, `StageOreDestroyCount`, `HasUnlockedEquipment`, and station-stack thresholds.

**INFERENCE:** these structures are compatible with action-based guidance and contextual prompts. They do not reveal exactly when each tutorial is triggered or skipped.

**PROPOSAL:** use five small hints: move, break, collect, deliver, upgrade. Complete a hint after its action succeeds. A failed purchase must not advance an upgrade hint. Cargo-full feedback should point to the depot. Restore or derive hint progress on reload. A general quest editor or condition-expression framework would be excessive for Sprint 1.

## Saving and state ownership

**STATIC — A1-SYMBOLS:** save-category identifiers exist for drop items, mine blocks, break rewards, mining units, equipment, buildings, stages, stage progression, items, guide quests, onboarding, worker hiring and cafeteria systems.

**INFERENCE:** world state and progression are treated as distinct categories. This does not prove atomic saving, a particular file format, crash recovery or offline earnings. Save names alone cannot settle the earlier disagreement between store claims and player reports about offline progression.

**PROPOSAL:** keep the existing Sprint 1 snapshot design: partially damaged deposits, destroyed IDs, remaining drop IDs, cargo, wallet, purchased upgrade and contract progress must agree. Test “break → pick up one of three → reload” and “sell → immediately background → reload.” Never save just the wallet while reconstructing all ore as fresh.

## Workers and navigation: useful later

**STATIC:** mining and carrying worker state-machine names are present, including idle, moving, mining, moving to dropped material, moving to a smelter, recovery and waiting. Cafeteria identifiers describe moving to a seat, waiting in line and eating. Recruitment is split across profile, stat and recruitment-timing tables. `Unity.AI.Navigation` is referenced; the serialized inventory includes 21 NavMeshAgent and 136 NavMeshObstacle objects.

These are packaged definitions, not counts of simultaneously active workers. The presence of navigation components does not demonstrate how the player moves or how routes recover from failure.

**PROPOSAL:** when Sprint 3 introduces one carrier, use explicit job ownership, reservation/release and recoverable movement states. Defer hiring rarity, stamina, recovery facilities and staff UI. Compare the proposed carrier state machine in [06_LATER_SPRINT_ROADMAP.md](06_LATER_SPRINT_ROADMAP.md); it is our design, not a copied state machine.

## Progression and advanced systems

**STATIC — A1-TABLES:** packaged data includes 46 equipment records, 117 equipment/stat mappings, 5,605 upgrade-level records, 58 stage records and 129 management-tree records. Fields cover tiers, unlock conditions, research duration, stage multipliers and multiple stat categories. These counts span configurations and possibly modes; they are not evidence of 5,605 player levels or 58 main stages.

Other tables describe mining combos, overheat, triggered actions, buffs, a Lost Mine mode and card drafting. Combo reset delays, heat/cooling fields, event time limits, card grades and draw weights exist in data. Fuel-related stat identifiers also exist. None establishes the precise behavior when fuel empties, a heat threshold is crossed or a card is selected.

**PROPOSAL:** keep these as future experiments. Our own distinctive restoration objective and rover design matter more than reproducing a mature game's content volume. The one-upgrade first sprint remains unchanged.

## Rendering, input and integrations

**STATIC — A1-UNITY:** GraphicsSettings and the single quality level have no custom render pipeline reference assigned. This is **consistent with Built-in pipeline configuration**. BuildSettings lists two configured scenes: a loader and a main game scene. This is not a requirement to create two scenes in our prototype.

**STATIC — A1-ASSEMBLIES:** references include uGUI, TextMeshPro, AI Navigation, a soft-mask package, Nice Vibrations, Toony Colors Pro and Kino Bloom. Legacy input modules and touch-zone identifiers are present; `Unity.InputSystem.dll` was not listed. Exact player gestures and all runtime input paths were not established. Fullscreen and rendering outside the safe area are enabled; these settings do not prove that HUD elements handle notches correctly.

**PROPOSAL:** keep the proposed URP/Input System stack for a suitable new project, or preserve an existing project's working stack. Do not downgrade Unity or buy these packages to match the reference. Use original materials and a small camera script. Explicitly test touch cancellation, simultaneous UI touches, modal input blocking and safe areas.

**STATIC:** packaged integration references include Unity Purchasing, Google Mobile Ads, MAX, Firebase analytics/remote config/crash reporting, AppsFlyer, Facebook and a web-view package. Android permission declarations also include billing, networking, notifications and advertising-related entries. Presence is not a measurement of active behavior, data collection, ad frequency or entitlement correctness. These integrations are unnecessary for our local first sprint.

Several bundled remote-config tables exist. The active version and any server override are unknown. Configuration values, service identifiers and keys have deliberately not been copied into this pack.

## Package size and inspection coverage

The ZIP contains 6,063 entries. Its 16 root DEX entries account for 109,395,888 bytes, about 104.33 MiB, stored without ZIP compression. Unity data accounts for 163,313,308 uncompressed bytes, about 155.75 MiB. Java/Kotlin code and libraries share the DEX area: it would be incorrect to attribute all of it to ads. The missing native split prevents a complete app-size estimate.

The scan examined **3,589 Unity serialized files**, found **117 TextAssets**, and parsed **110 JSON payloads**. Inventory counts include 1,390 Texture2D, 1,254 Sprite, 370 Mesh, 384 Material, 122 AudioClip, 515 AnimationClip and 2,020 ParticleSystem objects. These are packaged objects, including prefab content. They say nothing reliable about simultaneous draw calls, GPU memory, frame rate or on-device loading time.

**16,161 custom MonoBehaviour reads were incomplete.** Standard object headers and many names remained enumerable, but much custom component data could not be decoded. No native program library was available in this base APK. We did not recover source-level logic or inspect runtime network responses. Two Unity `.split0`/`.split1` asset chunks were joined for parsing; those asset-file chunks are unrelated to the missing Android ABI split.

Tools: Python ZIP/hash inspection, a local Android binary-XML parser cross-checked with pyaxmlparser 0.3.31, and UnityPy 1.25.2. [UnityPy project](https://pypi.org/project/UnityPy/)

The input's ZIP integrity passed and its SHA-256 was checked again after inspection. Our original tuning and visual direction remain proposals. The next validation that matters for the new game is a real playable Unity loop on the laptop, followed by the acceptance checks in this pack.
