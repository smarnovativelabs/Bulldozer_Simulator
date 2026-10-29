var report=new System.Collections.Generic.List<string>();System.Action<bool,string> check=(ok,text)=>report.Add((ok?"PASS ":"FAIL ")+text);
string path=System.IO.Path.GetFullPath("Maintenance/Miner/SaveTests/rounds-"+System.DateTime.UtcNow.Ticks+"/progress.json");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
var s=Bulldozer.Player.Persistence.MiningSnapshot.Create(0,70);s.damageLevel=2;s.capacityLevel=3;s.speedLevel=2;s.wheelbarrowUnlocked=true;s.depletedBlocks=new[]{"a","b"};var ids=new[]{"a","b"};
var old=s.Copy();old.version=3;string json=JsonUtility.ToJson(old).Replace("\"roundIndex\":0,","").Replace("\"roundBonusClaimed\":false,","");System.IO.File.WriteAllText(path,json);var store=new Bulldozer.Player.Persistence.MiningProgressStore(path);
check(store.Current.version==5&&store.Current.coins==70&&store.Current.damageLevel==2&&store.Current.capacityLevel==3&&store.Current.speedLevel==2&&store.Current.wheelbarrowUnlocked,"Sprint 1 version 3 save migrates without losing upgrades or ownership");
check(store.Current.roundIndex==0&&!store.Current.roundBonusClaimed,"Legacy save starts first excavation unclaimed");
check(!Bulldozer.Player.Persistence.ExcavationRules.TryComplete(s,new[]{"a","b","c"},out _),"Missing rock blocks completion");
check(!Bulldozer.Player.Persistence.ExcavationRules.TryComplete(s,System.Array.Empty<string>(),out _),"Empty quarry configuration cannot award bonus");
var modifiers=new System.Action<Bulldozer.Player.Persistence.MiningSnapshot>[] {x=>x.items=new[]{0},x=>x.backpackItems=new[]{0},x=>x.barrowItems=new[]{0},x=>x.refineryInput=new[]{0},x=>x.refineryOutput=new[]{6},x=>x.loadingQueue=new[]{6},x=>x.truckItems=new[]{6},x=>x.groundDrops=new[]{new Bulldozer.Player.Persistence.GroundOreState{id="loose",item=0,position=Vector3.zero}}};
for(int i=0;i<modifiers.Length;i++){var pending=s.Copy();modifiers[i](pending);check(!Bulldozer.Player.Persistence.ExcavationRules.TryComplete(pending,ids,out _),"Pending cargo buffer "+i+" blocks completion");}
check(!Bulldozer.Player.Persistence.ExcavationRules.TryAdvance(s,ids,out _),"Cannot advance an unclaimed round");
bool completed=Bulldozer.Player.Persistence.ExcavationRules.TryComplete(s,ids,out var awarded);check(completed&&awarded.coins==120&&awarded.roundBonusClaimed&&s.coins==70,"Bonus transaction adds $50 and leaves source untouched");
check(store.TryCommit(awarded),"Completed round saved");store=new Bulldozer.Player.Persistence.MiningProgressStore(path);
check(store.Current.roundBonusClaimed&&store.Current.coins==120&&!Bulldozer.Player.Persistence.ExcavationRules.TryComplete(store.Current,ids,out _),"Restart cannot duplicate completion bonus");
bool advanced=Bulldozer.Player.Persistence.ExcavationRules.TryAdvance(store.Current,ids,out var next);check(advanced&&next.roundIndex==1&&!next.roundBonusClaimed&&next.depletedBlocks.Length==0,"Next round resets only excavation progress");
check(next.coins==120&&next.damageLevel==2&&next.capacityLevel==3&&next.speedLevel==2&&next.wheelbarrowUnlocked,"Next round preserves cash, all upgrades and purchased cart");
check(store.TryCommit(next)&&new Bulldozer.Player.Persistence.MiningProgressStore(path).Current.roundIndex==1,"Advanced round survives disk restart");
check(!Bulldozer.Player.Persistence.ExcavationRules.TryAdvance(next,ids,out _),"Repeated next click cannot skip a round");
var overflow=s.Copy();overflow.coins=int.MaxValue;check(!Bulldozer.Player.Persistence.ExcavationRules.TryComplete(overflow,ids,out _),"Cash overflow cannot consume completion");
bool bounded=true;for(int r=1;r<10000;r++){int health=Bulldozer.Player.Persistence.ExcavationRules.Health(9,r,"a",false);bounded&=health>=4&&health<=16&&Bulldozer.Player.Persistence.ExcavationRules.Health(2,r,"a",true)<=3;}
check(bounded&&Bulldozer.Player.Persistence.ExcavationRules.Bonus(10000)==150,"Late rounds keep difficulty and bonuses bounded");
check(Enumerable.Range(1,8).Select(r=>Bulldozer.Player.Persistence.ExcavationRules.Health(9,r,"a",false)).Distinct().Count()>1,"Round variations change without unbounded difficulty");
var future=next.Copy();future.version=6;string futurePath=path+".future";System.IO.File.WriteAllText(futurePath,JsonUtility.ToJson(future));var protectedStore=new Bulldozer.Player.Persistence.MiningProgressStore(futurePath);check(!protectedStore.TryCommit(next),"Future version remains protected");
string blocked=path+".directory";System.IO.Directory.CreateDirectory(blocked);var failing=new Bulldozer.Player.Persistence.MiningProgressStore(blocked);check(!failing.TryCommit(awarded)&&!failing.Current.roundBonusClaimed&&failing.Current.coins==0,"Failed disk write cannot claim bonus in memory");
System.IO.File.WriteAllLines("Maintenance/Miner/excavation-store-verification.txt",report);return string.Join("\n",report);

