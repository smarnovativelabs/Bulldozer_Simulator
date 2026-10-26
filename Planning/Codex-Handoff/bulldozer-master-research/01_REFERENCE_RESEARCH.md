# Reference research

## Identity and current snapshot

**OFFICIAL CLAIM — S1:** The supplied package is `io.supercent.bulldozermasters`, titled **Bulldozer Master: Crush & Mine**, by Supercent. On 23 October 2026, Google Play displayed 5M+ downloads, 4.3 stars, about 139K headline reviews, ads and in-app purchases, and an update date of 22 October. The phone-specific review count differed from the headline. These numbers are a dated, locale-dependent snapshot.

The listing describes mining, selling, upgrading, workers, facilities, and offline rewards. Its update notes mention worker recruitment, stamina, a lounge, equipment, and HR improvements. Visible reviews criticize interruptions and promotions; one disputes the idle framing. These are individual reports, not measured ad frequency or proof that offline progression never works. [Google Play](https://play.google.com/store/apps/details?id=io.supercent.bulldozermasters&hl=en)

## The core loop supported by footage

**OBSERVED — V2 00:20; V1 00:30:** Breakable material, a capacity-limited character, a refining station, and a truck/sale area form the visible supply chain. The Spanish tutorial at V2 00:20 tells the player that raw ore needs processing before sale. Translation is summarized here rather than reproduced. [Opening tutorial](https://www.youtube.com/watch?v=R_zuyy1o2UE&t=20s), [processing area](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=30s).

```text
Intact deposit → broken pieces → carried material
              → refinery → output / delivery → cash
              → equipment and operational improvements → repeat
```

**INFERENCE:** Improving one part of this chain can move the bottleneck elsewhere: stronger mining produces little benefit if hauling or refining cannot keep up. The exact production rules and rates were not measured.

## Mechanics and presentation inventory

| Area | Evidence and observation | Practical design lesson |
|---|---|---|
| World | V2 09:59: tall square rock cells, embedded colored material, scattered fragments, cleared corridors | Make depletion and newly opened space easy to read |
| Player | V2 13:08: a small worker with visible carried material; V1 05:41: heavy machinery nearby | Player role can grow from manual work to machinery; no need to ship every form initially |
| Cargo | V2 09:59 and 26:16: stacked load, full indicator, and home-direction marker | Show why collection stops and where the player should go |
| Tasks | V2 samples show changing task cards with progress and rewards | Teach through short actions in the world |
| Stage | Stage label and percentage appear at top center | One persistent progress indicator supports a long activity |
| Stations | V1 00:30 shows separate processing and sale surroundings | Station placement defines the return-trip rhythm |
| Workers | V1 01:33 shows roles, stars, equipment, and DMG/SPD/CAP/STA stats | Automation introduces staff decisions beyond simply adding NPCs |
| Worker lifetime | V1 01:33 explicitly marks hires as limited to the current stage | Keep stage-specific state separate from permanent progression |
| Facilities | V1 03:12 shows a worker building with an entry marker | Management can be presented as spatial stations |
| Heavy equipment | V1 04:54 shows a roller-crusher rental offer | Machinery can be an upgrade in interaction and throughput |
| Fuel | V1 05:41 shows fuel-powered rental signage, a gauge, and a heavy vehicle | Fuel adds another constraint; it is unnecessary for the first prototype |
| Equipment tree | V1 08:32 shows separate mining/carrying tabs and locked progression | Collection and damage can progress independently |
| Meta UI | V1 frames show shop, company, timed offers, several currency counters | The mature game's interface is much larger than its first playable loop |

Use the frame links in [07_SOURCES_AND_EVIDENCE.md](07_SOURCES_AND_EVIDENCE.md) to inspect each observation. A screenshot supports only what is visible at that moment; it does not establish hidden formulas or unlock conditions.

## How the game feels visually

**OBSERVED:** Portrait presentation, an elevated oblique camera, brown quarry ground, chunky gray deposits with colored ore, construction machinery, strong shadows, white ground interaction outlines, outlined labels, and bright reward/offer icons. Carried objects form a conspicuous vertical stack. Dense UI sits around an open central movement area.

**INFERENCE:** Satisfaction comes from several synchronized signals: a hit lands, the rock changes, fragments appear, cargo grows, and the player gains a visible resource. Repeated travel then creates a reason to improve efficiency. The exact camera projection, gesture handling, physics, audio mix and haptics were not verified. Later static APK inspection identifies the engine version and some data structures; see [08_APK_FINDINGS.md](08_APK_FINDINGS.md). It does not verify runtime algorithms.

## Later systems found in official version history

**OFFICIAL CLAIM — S2:** The indexed App Store history lists R&D, sales, walkways, auto-miners, truck variations, secretary teams, and limited events. A later update describes a Lost Mine mode with randomized cards/skills, weekly goals, secretary affinity/skins, mystery ores, and additional equipment. Other entries describe worker recruitment/stamina and progression support. Treat these as version/platform-specific feature claims. The history is useful as a breadth reference, not a requirement list for your game. [Official App Store listing](https://apps.apple.com/us/app/bulldozer-master-crush-mine/id6747710694?platform=ipad)

## What remains unknown

- The formula behind stage completion, ore hardness, prices, worker stars, fuel consumption, and rentals.
- Which currencies and upgrades persist between stages; only the on-screen worker-lifetime message is directly evidenced.
- Whether offline earnings work in every current build, and how they are calculated.
- Complete late-game content, event rules, ads cadence, purchase entitlements, and current Android/iOS parity.
- Whether tutorial interactions are touch gestures, proximity actions, or context-dependent input in every mode.
- Performance on real devices and precise session timing. Video elapsed time is not a clean measure of player progression speed.

## Implications for your Sprint 1

Preserve the readable action/reward/upgrade loop. Propose a direct delivery sale initially, then add processing as a deliberate second-sprint expansion. Use one currency and one upgrade. Test input, collision, pickup ownership, and persistence before introducing worker AI. Build the machinery fantasy immediately with an original rover silhouette instead of reproducing the reference's character or interface.

## Additional source: supplied APK

**STATIC — A1:** The supplied Android base package identifies version 11.3.0 and Unity 2022.3.76f1. Separate ore, stat, drop, cargo, smelter, sale, equipment, quest and worker tables support the broad system map above. It also contains identifiers for pooling, saving and onboarding. This is a separate versioned source: do not assume both videos show this exact build. Details, schemas and inspection limits are in [08_APK_FINDINGS.md](08_APK_FINDINGS.md); first-sprint implications are in [09_APK_LESSONS_FOR_SPRINT_1.md](09_APK_LESSONS_FOR_SPRINT_1.md). Offline earnings, active remote settings, exact formulas and device performance remain untested.
