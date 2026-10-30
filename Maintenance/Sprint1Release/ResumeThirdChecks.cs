using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;

public static class ResumeThirdChecks {
public static string Main(){
if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
var go=new GameObject("Sprint1 third resume runner");UnityEngine.Object.DontDestroyOnLoad(go);
go.AddComponent<ResumeThirdRunner>().Begin();return "Three-round gameplay and reload validation started";
}}
public class ResumeThirdRunner:MonoBehaviour {
const string Report="Maintenance/Sprint1Release/third-gameplay-checks.txt";
MinerEquipment e;MinerController p;MiningProduction prod;ExcavationRounds rounds;MineableRock[] rocks;
readonly List<string> report=new List<string>();float deadline;
void Log(string message){report.Add(message);if(message.StartsWith("COMPLETE")||message.StartsWith("FAILED"))File.WriteAllLines(Report,report);}
void Check(bool ok,string name){Log((ok?"PASS ":"FAIL ")+name);if(!ok)throw new Exception(name);}
public void Begin(){File.WriteAllText(Report,"START live frame="+Time.frameCount+"\n");StartCoroutine(Guard(Run()));}
IEnumerator Guard(IEnumerator source){
var stack=new Stack<IEnumerator>();stack.Push(source);
while(stack.Count>0){
object value=null;bool has=false;
try{has=stack.Peek().MoveNext();if(has)value=stack.Peek().Current;else stack.Pop();}
catch(Exception ex){Log("FAILED "+ex);Time.timeScale=1;yield break;}
if(has){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}
}
Time.timeScale=1;Log("COMPLETE third excavation resumed from checkpoint; all 78 rocks refined/delivered; fourth round reloaded, frame="+Time.frameCount);Destroy(gameObject);
}
void Bind(){
rounds=UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>();e=rounds.equipment;p=rounds.player;prod=rounds.production;
rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)&&r.Available(e.Snapshot)).OrderBy(r=>r.blockId).ToArray();
p.autoMine=false;p.CancelMining();p.touchControls.Move=Vector2.zero;
var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(0,.04f,-9);cc.enabled=true;p.enabled=false;
}
IEnumerator Reload(){
int id=rounds.GetInstanceID();SceneManager.LoadSceneAsync(rounds.gameObject.scene.path);
float until=Time.realtimeSinceStartup+35;
do{if(Time.realtimeSinceStartup>until)throw new Exception("Scene reload timeout");yield return null;}
while(!UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>()||UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>().GetInstanceID()==id);
yield return null;yield return null;Bind();
}
int Stored(MiningSnapshot s)=>s.items.Length+s.backpackItems.Length+s.barrowItems.Length+s.groundDrops.Length+s.refineryInput.Length+s.refineryOutput.Length+s.loadingQueue.Length+s.truckItems.Length;
void DiskMatches(string where){
var disk=new MiningProgressStore(e.SavePath);
Check(disk.Error==null&&JsonUtility.ToJson(disk.Current)==JsonUtility.ToJson(e.Snapshot),"Disk matches runtime at "+where);
}
void Transfer(){
var before=e.Snapshot;prod.TransferToTruck();var after=e.Snapshot;
if(after.trucksCompleted>before.trucksCompleted)Check(after.trucksCompleted==before.trucksCompleted+1&&after.truckItems.Length==0&&after.coins>before.coins,"Atomic delivery payment/removal "+after.trucksCompleted);
}
IEnumerator Drain(){
var s=e.Snapshot;
foreach(var drop in s.groundDrops){
while(e.IsFull){yield return SendCargo();}
Check(e.TryCollectDrop(drop.id,drop.position),"Pickup "+drop.id);
Check(!e.TryCollectDrop(drop.id,drop.position),"Pickup cannot repeat "+drop.id);
}
yield return SendCargo();
while(e.RefineryInputCount>0){Check(prod.ProcessOne(),"Refine raw item");yield return null;}
while(e.RefineryOutputCount>0){
while(!e.IsFull&&e.RefineryOutputCount>0)Check(prod.CollectOutput(),"Collect refined item");
yield return SendCargo();
}
}
IEnumerator SendCargo(){
while(e.Snapshot.items.Any(x=>x<6))Check(prod.DepositRaw(),"Deposit raw item");
while(e.Snapshot.items.Any(x=>x>=6)){
while(e.Snapshot.loadingQueue.Length>=95){Transfer();Timed();yield return null;}
Check(prod.LoadTruck(),"Queue refined item");
}
Transfer();yield return null;
}
void Timed(){if(Time.realtimeSinceStartup>deadline)throw new Exception("Gameplay watchdog expired");}
IEnumerator Run(){
Bind();Check(e.RoundIndex==2,"Resume third excavation after report file-lock interruption");
Time.timeScale=6;bool upgrades=true;int expectedDamage=e.DamageLevel,expectedCapacity=e.CapacityLevel,expectedSpeed=e.SpeedLevel;
for(int cycle=2;cycle<3;cycle++){
deadline=Time.realtimeSinceStartup+240;
Check(e.RoundIndex==cycle,"Excavation index "+(cycle+1));
prod.enabled=false;
yield return Drain();
foreach(var rock in rocks){
while(!rock.Depleted){Timed();Check(rock.TryHit(e,rock.HitCollider.bounds.center),"Mine "+rock.blockId);yield return null;}
yield return Drain();
if(!upgrades&&e.Cash>=e.DamageCost+e.CapacityCost+e.SpeedCost){
Check(e.TryUpgrade(false)&&e.TryUpgrade(true)&&e.TryUpgradeSpeed(),"Buy all upgrade types with earned cash");
upgrades=true;expectedDamage=e.DamageLevel;expectedCapacity=e.CapacityLevel;expectedSpeed=e.SpeedLevel;
}
}
while(Stored(e.Snapshot)>0){
Transfer();prod.TryDispatchFinalLoad();Timed();yield return null;
}
rounds.Evaluate();
var complete=e.Snapshot;Check(complete.roundBonusClaimed&&complete.depletedBlocks.Length==rocks.Length&&complete.taskRefined==rocks.Length*3,"All rocks refined and delivered in excavation "+(cycle+1));
Check(complete.damagedBlocks.Length==0,"No stale partial damage after depletion");
Check(!rounds.TryComplete()&&e.Cash==complete.coins,"No duplicate completion bonus");
DiskMatches("completed excavation "+(cycle+1));
if(cycle==0){
Time.timeScale=1;yield return Reload();prod.enabled=false;Time.timeScale=6;
Check(e.Cash==complete.coins&&e.Snapshot.roundBonusClaimed&&rounds.completionPanel.gameObject.activeSelf,"Completed excavation reload retains payout and completion UI");
}
int oldId=rounds.GetInstanceID();int cash=e.Cash;
rounds.nextButton.onClick.Invoke();rounds.nextButton.onClick.Invoke();
Check(e.RoundIndex==cycle+1&&e.Cash==cash,"Double next click advances only once");
float loadDeadline=Time.realtimeSinceStartup+40;
do{yield return null;if(Time.realtimeSinceStartup>loadDeadline)throw new Exception("Next excavation load timeout");}
while(!UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>()||UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>().GetInstanceID()==oldId);
yield return null;yield return null;Bind();prod.enabled=false;
while(p.InputBlocked){yield return null;if(Time.realtimeSinceStartup>loadDeadline)throw new Exception("Input remains blocked after round load");}
Check(e.RoundIndex==cycle+1&&e.Cash==cash&&e.DamageLevel==expectedDamage&&e.CapacityLevel==expectedCapacity&&e.SpeedLevel==expectedSpeed,"Next excavation retains wallet and upgrades");
Check(rocks.All(r=>!r.Depleted&&r.HitCollider.enabled)&&e.Snapshot.depletedBlocks.Length==0&&e.Snapshot.damagedBlocks.Length==0,"New excavation restores every rock and collider");
Check(!e.Snapshot.roundBonusClaimed&&e.Snapshot.taskClaimedMask==0&&Stored(e.Snapshot)==0,"New excavation resets only round progress");
DiskMatches("fresh excavation "+(cycle+2));
}
Check(upgrades,"Earned upgrade path exercised");
Time.timeScale=1;
}
}


