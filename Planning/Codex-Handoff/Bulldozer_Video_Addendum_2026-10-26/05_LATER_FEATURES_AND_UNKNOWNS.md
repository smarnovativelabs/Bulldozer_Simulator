# Later features and focused follow-up questions

These candidates preserve what is useful from the expanded footage. Each becomes relevant only after the preceding playable loop is reliable.

| Order | Feature candidate | Evidence | Design work needed when selected |
|---|---|---|---|
| 1 | A refinery with raw input and processed output | [Part 1 06:25](https://www.youtube.com/watch?v=ovlJLqLo9I8&t=385s) | Queue ownership, capacities, processing time, save/reload, blocked output and one clear transfer per station |
| 2 | A second cargo or mining upgrade | [Guide 08:32](https://www.youtube.com/watch?v=IwMD7SOjFTs&t=512s), [Part 2 12:46](https://www.youtube.com/watch?v=ZvYnKFfRzRo&t=766s) | One understandable bottleneck at a time; avoid requiring an upgrade that makes the only objective impossible |
| 3 | One alternative vehicle and clear enter/exit interaction | [Part 1 14:58](https://www.youtube.com/watch?v=ovlJLqLo9I8&t=898s), [episode 10 22:59](https://www.youtube.com/watch?v=mka1GPEnH-w&t=1379s) | Input ownership, camera transition, cargo transfer policy, turning radius, parking and persistent vehicle state |
| 4 | One helper performing a bounded job | Worker facilities in [Part 2 16:25](https://www.youtube.com/watch?v=ZvYnKFfRzRo&t=985s); earlier worker menu in original pack | Resource reservation, blocked paths, full cargo, job cancellation and stage lifetime; do not assume offline operation |
| 5 | A second original site with one new route or material decision | [episode 14 20:45](https://www.youtube.com/watch?v=lXuiGjgeTSM&t=1245s), [episode 17 22:07](https://www.youtube.com/watch?v=NGAMLxMZhbs&t=1327s) | Stable site IDs, sufficient reachable resources, distinct silhouette/palette and defined carry-over at completion |
| 6 | Fuel if it improves the chosen vehicle loop | [episode 15 25:57](https://www.youtube.com/watch?v=7DGYwDg_-Hc&t=1557s) | Consumption basis, refill costs, empty-tank recovery, save behavior and prevention of a stranded save |
| Much later | Time-limited alternate jobs or events | [episode 9 22:43](https://www.youtube.com/watch?v=Apa6GFfilGk&t=1363s) | Entry/exit, clock policy, pause/offline time, rewards, accessibility and expiry behavior |

## Questions the footage does not settle

| Question | Why it matters | Focused verification if the feature is selected |
|---|---|---|
| How is stage percentage calculated? | Visible damage does not map obviously to the percentage | Observe one complete action cycle and compare broken, gathered, processed and sold quantities |
| What persists after stage completion? | Upgrades, helpers and stock may have different lifetimes | Capture values before and after one transition; independently define our persistence policy |
| What exactly happens at full capacity? | The two first-sprint proposals differ | Observe the same action immediately before and after full cargo, tracking valuable output separately from effects |
| What triggers refinery or truck transfer? | Proximity, pad entry and continuous transfer produce different behavior | Observe entry, dwell, exit, partial load, empty load and blocked destination |
| Do workers recover at facilities and operate offline? | Facilities are visible; rates and offline behavior are not proved | Test assignment, resting, app close/reopen and the same saved interval if implementing helpers |
| Which machines break, collect, carry or refuel? | Similar-looking vehicles may have different responsibilities | Compare each selected machine against the same deposit and pickup setup |
| How does fuel reach zero and recover? | Essential to avoid a trapped progression state | Test use, idle, refill, zero fuel and reloading while empty |
| What causes the timed-site transition? | Countdown and site numbering alone do not establish the rules | Inspect entry, task completion, timer expiry and return to the main site |

No additional video research is necessary to start the agreed small prototype. Reopen a specific timestamp or run a focused test only when a selected feature depends on an unresolved behavior. Do not treat online comments, thumbnails or store claims as measurements of implementation or balance.

## Original direction choices

Retain the broad satisfaction of clearing, collecting and improving a machine. Create different level topology, material silhouettes, machinery styling, HUD composition, narrative purpose and progression rules. A compact restoration objective is one possible distinction if the user selects the bridge proposal; the salvage-contract proposal offers another.

Do not copy the reference's maps, names, icons, character designs, audio, extracted models or exact interface arrangement. These are practical design boundaries, not a legal clearance or a guarantee against complaints. Asset-license verification remains part of the existing project handoff.
