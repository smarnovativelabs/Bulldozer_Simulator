using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player.Persistence;
using Newtonsoft.Json.Linq;

public static class SaveCompatibilityChecks {
static List<string> log=new List<string>();
static void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);if(!ok)throw new Exception(name);}
public static string Main(){
string root=Path.GetFullPath("Temp/Sprint1SaveChecks/"+DateTime.UtcNow.Ticks);Directory.CreateDirectory(root);
try{
for(int version=1;version<=8;version++){
var s=MiningSnapshot.Create(2,4321);s.items=new[]{2,8};s.damageLevel=3;s.capacityLevel=4;s.speedLevel=2;
s.refineryInput=new[]{1,4};s.refineryOutput=new[]{6,10};s.truckItems=new[]{9};s.loadingQueue=new[]{7};
s.depletedBlocks=new[]{"old-rock-1"};s.trucksCompleted=8;s.refiningSeconds=.7f;
s.roundIndex=4;s.taskRocks=2;s.taskRefined=3;s.taskDeliveries=1;s.taskClaimedMask=1;
s.wheelbarrowUnlocked=true;s.backpackItems=new[]{1};s.barrowItems=new[]{10,11};s.barrowPosition=new Vector3(-5,.05f,-2);
s.groundDrops=new[]{new GroundOreState{id="old-drop",item=3,position=new Vector3(1,.13f,2)}};
s.removeAds=true;s.fulfilledPurchases=new[]{"verified-old-receipt"};
s.damagedBlocks=new[]{new RockDamageState{id="partial",damage=2}};
var json=JObject.Parse(JsonUtility.ToJson(s));json["version"]=version;
if(version==1){json=new JObject{{"format","bulldozer-mining"},{"version",1},{"cargo",2},{"coins",4321}};}
else{
if(version<3)json.Remove("groundDrops");
if(version<4)foreach(var k in new[]{"roundIndex","roundBonusClaimed"})json.Remove(k);
if(version<5)foreach(var k in new[]{"speedLevel","loadingQueue","wheelbarrowUnlocked","usingWheelbarrow","backpackItems","barrowItems","barrowPosition","taskRocks","taskDense","taskOreMask","taskRefined","taskDeliveries","taskClaimedMask"})json.Remove(k);
if(version<6){json.Remove("fulfilledPurchases");json.Remove("removeAds");}
if(version<7)json.Remove("damagedBlocks"); if(version<8)foreach(var k in new[]{"copperRidgeUnlocked","contractKind","contractProgress","contractClaimed"})json.Remove(k);
}
string file=Path.Combine(root,"v"+version+".json"),raw=json.ToString();File.WriteAllText(file,raw);
var store=new MiningProgressStore(file);var got=store.Current;
Check(store.Error==null&&got.version==8&&got.coins==4321&&got.cargo==2,"v"+version+" loads and migrates wallet/cargo");
Check(File.ReadAllText(file)==raw,"v"+version+" load does not overwrite original");
if(version>=2)Check(got.items.SequenceEqual(s.items)&&got.damageLevel==3&&got.capacityLevel==4&&got.trucksCompleted==8&&got.refineryInput.SequenceEqual(s.refineryInput)&&got.refineryOutput.SequenceEqual(s.refineryOutput)&&got.truckItems.SequenceEqual(s.truckItems)&&got.depletedBlocks.SequenceEqual(s.depletedBlocks)&&got.refiningSeconds==.7f,"v"+version+" preserves upgrades and production");
if(version>=3)Check(got.groundDrops.Length==1,"v"+version+" retains ground pickups");
if(version>=4)Check(got.roundIndex==4,"v"+version+" retains excavation");
if(version>=5)Check(got.speedLevel==2&&got.wheelbarrowUnlocked&&got.backpackItems.Length==1&&got.barrowItems.Length==2&&got.loadingQueue.Length==1&&got.taskClaimedMask==1,"v"+version+" retains cart, queues and claimed tasks");
if(version>=6)Check(got.removeAds&&got.fulfilledPurchases.SequenceEqual(s.fulfilledPurchases),"v"+version+" retains purchases");
if(version>=7)Check(got.damagedBlocks.Length==1,"v7 retains partial rock damage");
Check(store.TryCommit(got),"v"+version+" migrated state commits");
Check(JsonUtility.ToJson(new MiningProgressStore(file).Current)==JsonUtility.ToJson(got),"v"+version+" second launch preserves exact migrated state");
}
var path=Path.Combine(root,"recovery.json");var good=new MiningProgressStore(path);
Check(good.TryCommit(MiningSnapshot.Create(0,100)),"first save");
Check(good.TryCommit(MiningSnapshot.Create(0,200)),"backup save");
File.WriteAllText(path,"{broken");
var recovered=new MiningProgressStore(path);
Check(recovered.RecoveredBackup&&recovered.Current.coins==100,"corrupt primary recovers backup");
Check(recovered.TryCommit(recovered.Current)&&new MiningProgressStore(path).Current.coins==100,"recovered primary repairs safely");
File.Delete(path);
Check(new MiningProgressStore(path).Current.coins==100,"missing primary recovers backup");
var future=MiningSnapshot.Create(0,555);future.version=9;File.WriteAllText(path,JsonUtility.ToJson(future));
var blocked=new MiningProgressStore(path);Check(blocked.Error!=null&&!blocked.TryCommit(MiningSnapshot.Create(0,0))&&File.ReadAllText(path).Contains("555"),"future save cannot be overwritten or rolled back to old backup");
File.WriteAllText(path,"broken");File.WriteAllText(path+".bak","broken");
blocked=new MiningProgressStore(path);Check(blocked.Error!=null&&!blocked.TryCommit(MiningSnapshot.Create(0,0)),"both corrupt copies preserved");
var folder=Path.Combine(root,"unwritable.json");Directory.CreateDirectory(folder);
blocked=new MiningProgressStore(folder);Check(!blocked.TryCommit(MiningSnapshot.Create(0,999))&&blocked.Current.coins==0,"failed write leaves in-memory balance unchanged");
var invalid=MiningSnapshot.Create(0,0);invalid.damagedBlocks=new[]{new RockDamageState{id="a",damage=1},new RockDamageState{id="a",damage=2}};
Check(!good.TryCommit(invalid),"duplicate rock damage rejected");
invalid=MiningSnapshot.Create(0,0);invalid.refiningSeconds=float.NaN;Check(!good.TryCommit(invalid),"invalid processing timer rejected");
var round=MiningSnapshot.Create(0,1000);round.damageLevel=2;round.capacityLevel=3;round.speedLevel=1;round.wheelbarrowUnlocked=true;round.removeAds=true;round.fulfilledPurchases=new[]{"purchase-1"};
for(int i=0;i<10;i++){
round.depletedBlocks=new[]{"a","b"};round.damagedBlocks=Array.Empty<RockDamageState>();var before=round.coins;
Check(ExcavationRules.TryComplete(round,new[]{"a","b"},out var paid)&&paid.coins==before+ExcavationRules.Bonus(i),"round "+(i+1)+" pays once");
Check(!ExcavationRules.TryComplete(paid,new[]{"a","b"},out _),"round "+(i+1)+" duplicate completion rejected");
Check(ExcavationRules.TryAdvance(paid,new[]{"a","b"},out round)&&round.roundIndex==i+1&&round.damageLevel==2&&round.capacityLevel==3&&round.speedLevel==1&&round.wheelbarrowUnlocked&&round.removeAds&&round.fulfilledPurchases.Length==1&&round.depletedBlocks.Length==0&&round.damagedBlocks.Length==0,"round "+(i+1)+" advance retains permanent progress");
}
Check(!ExcavationRules.TryAdvance(round,new[]{"a","b"},out _),"cannot advance uncleared round");
log.Add("COMPLETE "+log.Count+" checks; fixtures="+root);
} catch(Exception ex){log.Add("FAILED "+ex);throw;}
finally{File.WriteAllLines("Maintenance/Sprint1Release/save-checks.txt",log);}
return log.Last();
}}

