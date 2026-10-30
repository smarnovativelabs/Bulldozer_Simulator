using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class ProgressionChecks {
public static string Main(){
if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
var go=new GameObject("Progression Test Runner");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<ProgressionRunner>().Begin();return "Running progression, gate and reload checks";
}}
public class ProgressionRunner:MonoBehaviour {
const string Report="Maintenance/Sprint1Release/progression-checks.txt";
readonly List<string> log=new List<string>();
MinerEquipment e;ExcavationRounds rounds;QuarryExpansion area;MineableRock[] rocks;
void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(Report,log);if(!ok)throw new Exception(name);}
public void Begin(){StartCoroutine(Guard(Run()));}
IEnumerator Guard(IEnumerator source){var stack=new Stack<IEnumerator>();stack.Push(source);while(stack.Count>0){object value=null;bool more=false;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;else stack.Pop();}catch(Exception ex){log.Add("FAILED "+ex);File.WriteAllLines(Report,log);yield break;}if(more){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}}log.Add("COMPLETE at live frame "+Time.frameCount);File.WriteAllLines(Report,log);Destroy(gameObject);}
void Bind(){rounds=UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>();e=rounds.equipment;area=UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>();rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)).ToArray();rounds.player.autoMine=false;rounds.player.touchControls.Move=Vector2.zero;}
IEnumerator Reload(bool advance){int id=rounds.GetInstanceID();if(advance){rounds.NextRound();rounds.NextRound();}else SceneManager.LoadSceneAsync(rounds.gameObject.scene.path);float until=Time.realtimeSinceStartup+40;do{if(Time.realtimeSinceStartup>until)throw new Exception("Reload timeout");yield return null;}while(!UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>()||UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>().GetInstanceID()==id);yield return null;yield return null;Bind();}
void Disk(){Check(JsonUtility.ToJson(new MiningProgressStore(e.SavePath).Current)==JsonUtility.ToJson(e.Snapshot),"Disk and runtime match");}
IEnumerator Run(){
Bind();int frame=Time.frameCount;yield return null;yield return null;Check(Time.frameCount>frame,"Player loop advances");
Check(rocks.Length==84&&rocks.Select(r=>r.blockId).Distinct().Count()==84,"84 unique IDs including six ridge rocks");
for(int i=0;i<3;i++){
Check(e.RoundIndex==i,"Excavation "+(i+1)+" loaded");
Check(area.gate.activeSelf&&area.deposits.All(r=>!r.enabled&&!r.HitCollider.enabled),"Locked ridge cannot be mined or entered");
var available=rocks.Where(r=>r.Available(e.Snapshot)).ToArray();Check(available.Length==78,"Only original 78 rocks count toward completion");
Check(Physics.Raycast(new Vector3(20.5f,1,0),Vector3.forward,out var hit,3)&&hit.collider.gameObject==area.gate,"Entrance physically blocked before unlock");
var s=e.Snapshot;int target=QuarryProgressionRules.Target(s),reward=QuarryProgressionRules.Reward(s),cash=s.coins;
// Exercise transactional collection API for the cart contract; delivery rules receive real item IDs.
if(i==1){s.usingWheelbarrow=true;s.groundDrops=Enumerable.Range(0,target).Select(n=>new GroundOreState{id="contract-drop-"+n,item=1,position=Vector3.zero}).ToArray();Check(e.Commit(s),"Seed isolated cart contract pickup fixture");foreach(var d in s.groundDrops)Check(e.TryCollectDrop(d.id,Vector3.zero),"Cart pickup recorded "+d.id);s=e.Snapshot;Check(s.contractClaimed&&s.coins==cash+reward,"Cart contract pays exactly once");s.items=Array.Empty<int>();s.cargo=0;s.usingWheelbarrow=false;Check(e.Commit(s),"Clear test cargo");}
else {QuarryProgressionRules.Delivered(s,Enumerable.Repeat(i==2?7:6,target-1).ToArray());Check(!s.contractClaimed&&s.contractProgress==target-1,"Contract waits for required amount");Check(e.Commit(s),"Save partial contract");yield return Reload(false);s=e.Snapshot;Check(s.contractProgress==target-1&&!s.contractClaimed,"Partial contract survives scene reload");QuarryProgressionRules.Delivered(s,new[]{i==2?7:6});Check(s.contractClaimed&&s.coins==cash+reward,"Delivery contract reward");Check(e.Commit(s),"Persist awarded contract");}
s=e.Snapshot;cash=s.coins;QuarryProgressionRules.Add(s,s.contractKind,100);Check(s.coins==cash,"Repeated contract event cannot duplicate payout");Disk();
// Completion fixture skips the already-tested mining/refining loop, exercising actual scene completion/transition.
s=e.Snapshot;s.depletedBlocks=available.Select(r=>r.blockId).ToArray();s.damagedBlocks=Array.Empty<RockDamageState>();Check(e.Commit(s),"Seed cleared excavation fixture");rounds.Evaluate();s=e.Snapshot;
Check(s.roundBonusClaimed&&s.wheelbarrowUnlocked,"Completion unlocks cart and awards completion");
Check(s.copperRidgeUnlocked==(i==2)&&!QuarryProgressionRules.AreaAvailable(s),"Ridge opens only on next round after third completion");
Check(rounds.summary.text.Contains("NEXT: EXCAVATION "+(i+2)),"Completion displays next-round preview");
Check(!rounds.TryComplete(),"Completion reward cannot duplicate");Disk();yield return Reload(true);
Check(e.RoundIndex==i+1&&e.Snapshot.contractProgress==0&&!e.Snapshot.contractClaimed,"Double next press advances once and resets contract");
}
Check(e.RoundIndex==3&&!area.gate.activeSelf&&area.deposits.All(r=>r.enabled&&r.HitCollider.enabled),"Fourth excavation opens gate and enables six deposits");
Check(rocks.Count(r=>r.Available(e.Snapshot))==84,"Fourth excavation includes all 84 rocks");
Check(!Physics.Raycast(new Vector3(20.5f,1,0),Vector3.forward,3),"Unlocked entrance is unobstructed");
yield return null;yield return null;
var nav=new UnityEngine.AI.NavMeshPath();var filter=new UnityEngine.AI.NavMeshQueryFilter{agentTypeID=UnityEngine.Object.FindFirstObjectByType<QuarryGuidance>().agentTypeId,areaMask=-1};
Check(UnityEngine.AI.NavMesh.CalculatePath(new Vector3(12,0,-3),new Vector3(20.5f,0,4),filter,nav)&&nav.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Baked guide route reaches new area through entrance");
var ridge=area.deposits[0];Check(ridge.TryHit(e,ridge.HitCollider.bounds.center),"Unlocked deposit can be mined");int health=ridge.Health;string block=ridge.blockId;yield return Reload(false);
Check(!area.gate.activeSelf&&area.deposits.First(r=>r.blockId==block).Health==health,"Unlock and ridge damage survive reload");Disk();
var old=MiningSnapshot.Create(0,456);old.version=7;old.roundIndex=2;old.roundBonusClaimed=true;old.damageLevel=3;old.removeAds=true;old.fulfilledPurchases=new[]{"existing-receipt"};string path=Path.Combine(Path.GetDirectoryName(e.SavePath),"legacy.json");File.WriteAllText(path,JsonUtility.ToJson(old));var migrated=new MiningProgressStore(path);Check(migrated.Error==null&&migrated.Current.coins==456&&migrated.Current.damageLevel==3&&migrated.Current.removeAds&&migrated.Current.fulfilledPurchases.Length==1&&migrated.Current.copperRidgeUnlocked&&migrated.Current.wheelbarrowUnlocked,"Completed v7 migration retains wallet/upgrades/purchases and earned unlocks");
}
}
