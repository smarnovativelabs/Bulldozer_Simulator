using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class FirstSessionChecks {
public static string Main(){
if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
var go=new GameObject("First Session Test Runner");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<FirstSessionRunner>().Begin();return "Running first-session checks";
}}
public class FirstSessionRunner:MonoBehaviour {
const string Report="Maintenance/VisualPolish/progression-checks.txt";
readonly List<string> log=new List<string>();
MinerEquipment e;MinerController player;MiningProduction production;ExcavationRounds rounds;QuarryCameraGuidance cameraGuide;MineableRock[] rocks;
void Flush(){using(var f=new FileStream(Report,FileMode.Create,FileAccess.Write,FileShare.ReadWrite))using(var w=new StreamWriter(f))foreach(var line in log)w.WriteLine(line);}
void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);Flush();if(!ok)throw new Exception(name);}
public void Begin(){StartCoroutine(Guard(Run()));}
IEnumerator Guard(IEnumerator source){var stack=new Stack<IEnumerator>();stack.Push(source);while(stack.Count>0){object value=null;bool more=false;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;else stack.Pop();}catch(Exception ex){log.Add("FAILED "+ex);Flush();yield break;}if(more){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}}log.Add("COMPLETE "+log.Count+" checks; frame="+Time.frameCount);Flush();Destroy(gameObject);}
void Bind(){e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();player=UnityEngine.Object.FindFirstObjectByType<MinerController>();production=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();rounds=UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>();cameraGuide=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)&&r.Available(e.Snapshot)).OrderBy(r=>r.maxHealth).ThenBy(r=>r.blockId).ToArray();player.autoMine=false;player.touchControls.Move=Vector2.zero;cameraGuide.enabled=false;}
void Disk(){Check(JsonUtility.ToJson(new MiningProgressStore(e.SavePath).Current)==JsonUtility.ToJson(e.Snapshot),"Runtime matches durable save");}
IEnumerator Reload(){int id=e.GetInstanceID();SceneManager.LoadSceneAsync(rounds.gameObject.scene.path);float end=Time.realtimeSinceStartup+40;while(!UnityEngine.Object.FindFirstObjectByType<MinerEquipment>()||UnityEngine.Object.FindFirstObjectByType<MinerEquipment>().GetInstanceID()==id){if(Time.realtimeSinceStartup>end)throw new Exception("Reload timed out");yield return null;}yield return null;yield return null;Bind();}
IEnumerator Run(){
Bind();int frame=Time.frameCount;yield return null;yield return null;Check(Time.frameCount>frame,"Live player frames advance");
Check(e.drops.PoolCapacity==36,"First shift prewarms only 36 ore slots");
Check(rocks.Length==12&&rocks.All(r=>r.gameObject.activeInHierarchy),"Fresh first shift contains 12 active rocks");
Check(rocks.Count(r=>r.maxHealth==2)==3&&rocks.Count(r=>r.maxHealth==4)==5&&rocks.Count(r=>r.maxHealth==6)==4,"Easy, medium and tough starter rocks");
Check(!e.Snapshot.firstRefineryDeposit&&!e.Snapshot.wheelbarrowUnlocked,"Fresh onboarding and wheelbarrow state");
Check(!production.DepositRaw()&&!e.Snapshot.firstRefineryDeposit,"Empty deposit cannot unlock mining");
// Mine the first two rocks without buying anything; the second also proves partial damage.
foreach(var rock in rocks.Take(2)){for(int h=0;h<2;h++)Check(rock.TryHit(e,rock.HitCollider.bounds.center),"Starter hit "+rock.blockId+" / "+h);}
Check(e.Cash>=e.DamageCost&&e.Damage==1,"Starter bonus makes first upgrade affordable but does not buy it");
foreach(var drop in e.Snapshot.groundDrops.Take(5).ToArray())Check(e.TryCollectDrop(drop.id,Vector3.zero),"Collect first bag");
var target=rocks.First(r=>!r.Depleted);int health=target.Health;
Check(e.IsFull&&e.MiningBlockedByCapacity&&!target.TryHit(e,target.HitCollider.bounds.center)&&target.Health==health,"First full bag blocks hits without changing rock health");
Check(!e.TryCollectDrop(e.Snapshot.groundDrops[0].id,Vector3.zero),"Full bag never exceeds pickup capacity");
// Show the first cue, inspect a live blend, skip, and verify one-time persistence.
cameraGuide.enabled=true;Vector3 home=player.followCamera.transform.position;float yaw=player.followCamera.Yaw;
Check(cameraGuide.TryShow(1,production.inputZone.transform.position,"REFINERY INPUT","Place ore here. Tap to continue."),"Refinery Cinemachine cue starts");
Check(player.GuidanceInputBlocked&&cameraGuide.Playing,"Camera owns its own temporary input lock");
yield return new WaitForSecondsRealtime(.9f);
Check(Vector3.Distance(home,player.followCamera.transform.position)>1,"Cinemachine moved the real camera toward refinery");
cameraGuide.Skip();yield return new WaitForSecondsRealtime(.9f);
Check(!cameraGuide.Playing&&!player.GuidanceInputBlocked&&Mathf.Abs(yaw-player.followCamera.Yaw)<.001f,"Skip restores controls and original camera yaw");
Check(!cameraGuide.TryShow(1,production.inputZone.transform.position,"Repeat","Repeat"),"Seen refinery look never repeats");cameraGuide.enabled=false;
Check(production.DepositRaw()&&e.Snapshot.firstRefineryDeposit,"Successful refinery deposit permanently unlocks full-bag mining");
foreach(var drop in e.Snapshot.groundDrops.ToArray())e.TryCollectDrop(drop.id,Vector3.zero);
Check(e.IsFull&&!e.MiningBlockedByCapacity,"Second full bag permits mining");
Check(target.TryHit(e,target.HitCollider.bounds.center),"Full-bag partial/normal strike succeeds after deposit");
string partialId=target.blockId;int remaining=target.Health;
Disk();yield return Reload();
Check(e.IsFull&&!e.MiningBlockedByCapacity&&e.Snapshot.firstRefineryDeposit,"Deposit permission survives scene reload");
Check(rocks.First(r=>r.blockId==partialId).Health==remaining,"Partial mining survives reload");
Check((e.Snapshot.guidanceSeenMask&1)!=0,"Camera one-time state survives reload");
var tough=rocks.First(r=>r.maxHealth==6);Check(tough.TryHit(e,tough.HitCollider.bounds.center)&&tough.Health==5,"Tough rock can be hit without upgrading");
Check(e.TryUpgrade(false)&&e.Damage==2,"Optional first upgrade doubles damage");
Check(tough.TryHit(e,tough.HitCollider.bounds.center)&&tough.Health==3,"Upgrade removes two HP per actual hit");
// Animate a full-bag hit through the real controller/contact pipeline, not only TryHit.
var animated=rocks.First(r=>r.maxHealth==2&&!r.Depleted);var motor=player.GetComponent<CharacterController>();motor.enabled=false;player.transform.position=animated.transform.position+Vector3.back*1.0f+Vector3.up*.1f;motor.enabled=true;player.visual.rotation=Quaternion.identity;
player.autoMine=true;int hits=player.visual.GetComponent<MinerMining>().SuccessfulHits;float until=Time.realtimeSinceStartup+8;
while(player.visual.GetComponent<MinerMining>().SuccessfulHits==hits&&Time.realtimeSinceStartup<until)yield return null;
player.autoMine=false;player.CancelMining();Check(player.visual.GetComponent<MinerMining>().SuccessfulHits>hits,"Actual animation contact mines while bag is full");
// Finish all starter rocks with a full bag, preserving every extra drop.
foreach(var rock in rocks)while(!rock.Depleted)Check(rock.TryHit(e,rock.HitCollider.bounds.center),"Full-bag mining "+rock.blockId);
Check(e.Snapshot.groundDrops.Length+e.CargoCount+e.RefineryInputCount+e.RefineryOutputCount==36,"All 36 starter ore conserved");
Check(!rounds.TryComplete(),"Loose ore and undelivered cargo prevent premature completion");
Disk();yield return Reload();
Check(e.drops.ActiveCount==e.Snapshot.groundDrops.Length,"Ground ore restored visually after reload");
// Drain via real inventory/refinery/loading APIs; trucks run live.
float deadline=Time.realtimeSinceStartup+160;
while(!e.Snapshot.roundBonusClaimed){
 foreach(var d in e.Snapshot.groundDrops.ToArray()){if(e.IsFull)break;e.TryCollectDrop(d.id,Vector3.zero);}
 while(production.DepositRaw()){}
 while(production.ProcessOne()){}
 while(production.CollectOutput()){}
 while(production.LoadTruck()){}
 rounds.Evaluate();if(Time.realtimeSinceStartup>deadline)throw new Exception("Delivery drain timeout");yield return new WaitForSecondsRealtime(.1f);
}
Check(e.Snapshot.wheelbarrowUnlocked&&e.DamageLevel==1,"Short shift grants permanent wheelbarrow and keeps upgrade");
Check(!rounds.TryComplete(),"Completion cannot reward twice");Disk();
int previous=e.GetInstanceID();rounds.NextRound();float nextDeadline=Time.realtimeSinceStartup+40;while(!UnityEngine.Object.FindFirstObjectByType<MinerEquipment>()||UnityEngine.Object.FindFirstObjectByType<MinerEquipment>().GetInstanceID()==previous){if(Time.realtimeSinceStartup>nextDeadline)throw new Exception("Next scene timeout");yield return null;}yield return new WaitForSecondsRealtime(1);Bind();
Check(e.RoundIndex==1&&rocks.Length==78&&e.Snapshot.wheelbarrowUnlocked,"Second shift restores normal 78 rocks and unlocked cart");
Check(e.Snapshot.firstRefineryDeposit&&(e.Snapshot.guidanceSeenMask&1)!=0,"Tutorial state persists between shifts");
// The automatic reveal may already have fired during scene loading on a slow Editor.
// Reset only the isolated fixture flag to test the manual skip/return path deterministically.
var cueFixture=e.Snapshot;cueFixture.guidanceSeenMask&=~4;Check(e.Commit(cueFixture),"Prepare isolated cart-camera fixture");
while(player.InputBlocked)yield return null;cameraGuide.enabled=true;
Check(cameraGuide.TryShow(4,cameraGuide.cart.transform.position,"YOUR WHEELBARROW","Carry 12 ore. Tap to continue."),"Wheelbarrow location reveal starts once unlocked");
yield return new WaitForSecondsRealtime(3.6f);Check(!cameraGuide.Playing&&!player.GuidanceInputBlocked,"Camera returns automatically");
Check(cameraGuide.TryShow(2,cameraGuide.guide.upgrades.zone.transform.position,"WORKBENCH","Optional upgrades."),"Workbench reveal starts");
player.ExcavationInputBlocked=true;yield return null;
Check(!cameraGuide.Playing&&!player.GuidanceInputBlocked&&player.ExcavationInputBlocked,"Interrupted guidance preserves another owner's input lock");player.ExcavationInputBlocked=false;cameraGuide.enabled=false;Disk();yield return Reload();
Check((e.Snapshot.guidanceSeenMask&7)==7,"All three one-time camera flags survive reload");
// Stress the largest excavation without collecting: enough pool for every rock.
var large=e.Snapshot;large.roundIndex=3;large.copperRidgeUnlocked=true;large.firstRefineryDeposit=true;large.items=new int[5];large.depletedBlocks=Array.Empty<string>();large.damagedBlocks=Array.Empty<RockDamageState>();large.groundDrops=Array.Empty<GroundOreState>();large.roundBonusClaimed=false;Check(e.Commit(large),"Prepare largest excavation stress fixture");yield return Reload();
Check(rocks.Length==84&&e.drops.PoolCapacity==252,"Largest excavation has 84 rocks and 252 pooled ore slots");
foreach(var rock in rocks)while(!rock.Depleted){if(!rock.TryHit(e,rock.HitCollider.bounds.center))throw new Exception("Pool blocked "+rock.blockId);}
Check(e.Snapshot.groundDrops.Length==252&&e.drops.ActiveCount==252,"Entire 84-rock quarry can be mined with a full bag; 252 drops retained");Disk();yield return Reload();Check(e.Snapshot.groundDrops.Length==252&&e.drops.ActiveCount==252,"All 252 ore survive reload");
}
}





