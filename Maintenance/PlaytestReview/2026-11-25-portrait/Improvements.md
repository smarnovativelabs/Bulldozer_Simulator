# Portrait playtest aur improvement guide — 25 Nov 2026

Scripted Editor review (`Bulldozer/Run Full Gameplay Review`), portrait 1080x1920 Game view, isolated save.
Result: **COMPLETE, sab checks PASS** (`FullGameplay.txt`). Editor ki ginti: max 120 batches, 26 SetPass
(pehle run mein 147 / 31). Ye device FPS nahi hai; Android phone par profile karna baqi hai.

## Is round mein kya badla

| Tester point | Change |
|---|---|
| Controls | Floating joystick: screen ke neeche 55% hisse mein kahin bhi touch karo; base ungli ke saath chalta hai. Joystick 180 se 220 units. |
| Hit animation | Swing 1.6x tez (`MinerController.swingSpeed`), swing ke beech ka gap 0.22s se 0.06s. |
| Machine time | Har stone ~2.15s se ~1.0s. Truck loading interval 0.8s se 0.3s. |
| Portrait | Player settings portrait-only. Canvas 720x1280 (match 0.5). Camera distance 5.8 se 8.5, pitch 46 se 50. |
| Truck filling | Blocks poore truck bed par 3x2 grid mein, koi block doosre ke oopar nahi; har block apne slot par ur kar girta hai. |

## Agle improvements (feel ke hisaab se, pehle sab se zyada asar wale)

### 1. Har hit ka "juice" (sab se bara farq) — 26 Nov ko lagaya
Haptics (NiceVibrations, default ON), hit-stop 45/75 ms, rock squash, break par camera shake, pickup aur cash haptic.
Values `MinerMining` ke "Impact feel" Inspector fields mein tune hoti hain. Device par feel check karna baqi hai.
Editor test 26 Nov (`../2026-11-26-hitfeel/`): har hit par hit-stop aur rock squash (~6%) chala; camera sirf impact par hila
(frame jump max ~39 mm, baaqi waqt 0); swing ~0.7s par ek, contact swing ke ~0.3s baad. Full Gameplay Review COMPLETE, 118 PASS.
- **Haptics:** abhi Vibration default **OFF** hai, aur sirf UI confirm par lambi `Handheld.Vibrate` buzz chalti hai.
  NiceVibrations installed hai: har hit par light haptic, rock tootne par medium, aur cash milne par success haptic. Default ON rakhen.
- **Hit-stop:** contact par 40-60 ms ka freeze (sirf player animator aur rock), taake hit bhaari lage.
- **Camera shake:** rock tootne par chhota sa shake (0.1s). Har hit par nahi.
- **Rock punch:** `DOPunchPosition(Vector3.up*.025f)` bahut halka hai. Scale punch (0.92 → 1) zyada nazar aayega.
- **Damage numbers:** hit point par "-1" ya cracks badhne ka feedback.

### 2. Frame rate — 26 Nov ko lagaya
Default ab 60 FPS hai, aur game shuru hote hi (pehle scene se pehle) lagta hai. `QuarryFrameRateGuard` gameplay ke frame time naapta hai:
12s warm-up ke baad agar do lagataar 8s windows mein 45 FPS se kam aaye (kamzor phone ya garmi), to khud 30 FPS par aa jata hai
aur ye yaad rakhta hai. Settings mein label "AUTO / 30 FPS" dikhata hai. Player ki apni choice hamesha chalti hai, aur Reset se
phone dobara naapa jata hai. Android "Optimized Frame Pacing" (Swappy) ON kiya. Guard Editor mein band rehta hai.
Editor test (`../2026-11-26-fps/`): default 60; ~22 FPS simulate karne par 16s mein 30; choice aur reset sahi.
Phone par garmi aur battery ka asar abhi naapna baqi hai.
- Default performance **30 FPS (Battery Saver)** hai. 30 FPS par joystick aur camera bhaari lagte hain.
  Default 60 rakhen, aur low-end phone ya garam hone par 30 par aayen (Adaptive Performance package pehle se mojood hai).

### 3. Ore pickup — 26 Nov ko lagaya
Magnet pickup: zameen par tiki ore 2 m ke andar ho to ur kar player (ya cart) tak aati hai, sirf utni jitni bag mein jagah ho.
Ore usi saved `TryCollectDrop` raste se collect hoti hai, aur ek frame mein ek hi save hota hai. Bag beech mein bhar jaye to ore wapas
apni saved jagah par gir jati hai. Inspector: `OreDropSystem` → "Magnet pickup" (`magnetRadius` 2, `magnetSpeed` 9; 0 = band).
Editor test (`../2026-11-26-magnet/`): rock tootne ke 0.87s baad teeno ore bag mein; bag mein 1 jagah par sirf 1 ore aayi;
ur rahi ore bag bharne par wapas zameen par. Full Gameplay Review COMPLETE, 118 PASS.
- `pickupRadius = .72` hai, yani har stone ke bilkul oopar chalna parta hai. Is genre mein **magnet pickup** standard hai:
  ~2 m ke andar ore khud player ki taraf ur kar aaye. Ye akela change mining loop ko bahut smooth kar dega.

### 4. Rewards aur progress
- **Excavation Complete screen** bahut dim aur grey hai; "NEXT EXCAVATION" button low-contrast brown hai.
  Bright button, cash count-up ($ ginti badhti hui), confetti aur success sound ke saath celebration banayen.
- **Truck order size** `2 + trucks % 4` se 2,3,4,5,2... chakkar mein ghoomta hai; 5 ke baad 2 hona ulta lagta hai.
  Badhti hui ya clearly dikhayi gayi "order" sizes rakhen.
- Truck jaate waqt cash coins ka truck se HUD cash tak urna (RewardCoinFeedback pehle se hai) — isay har delivery par zaroor chalayen.

### 5. Portrait HUD saaf karna
- **Tasks panel** top-right mein screen ka ~40% chaura aur bahut khali jagah ke saath hai. Ise collapsible banayen
  (sirf 1 line: agla task + reward), tap par poora khule.
- **Guidance dialog** typewriter text screenshots mein aadha kata reh jata hai ("upgrade your p"). Tap par poora text dikhayen.
- **"REWARD VIDEO LOADING"** toast default font mein aur joystick/cart button ke oopar aata hai. Game font use karen aur upar le jayen.
- World labels "CADDY 0/12" aur player cargo badge ek doosre par chadh jate hain. Ek label chhupa den ya offset karen.
- Popups ke peeche halka dim background (Excavation Complete ki tarah), taake focus rahe.

### 6. Rock health
- Baad ki shifts mein rocks 16 HP tak jate hain, aur level 0 pickaxe 1 damage deta hai: 16 swings (~10s) ek rock par.
  Pickaxe upgrade ka faida zyada mehsoos karwane ke liye pehli 2-3 upgrades par damage jump bara rakhen.

## Abhi verify nahi hua
- Android device par feel, FPS aur garmi.
- Notch wale phones par safe area aur HUD (Editor mein sirf 1080x1920 dekha).
- Lambe phones (20:9) par camera framing.
