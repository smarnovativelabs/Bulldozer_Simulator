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
public static class RemainingSessionChecks {
public static string Main(){
if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
var go=new GameObject("First Session Test Runner");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<RemainingSessionRunner>().Begin();return "Running first-session checks";
}}
public class RemainingSessionRunner:MonoBehaviour {
const string Report="Maintenance/FirstSession/remaining-checks.txt";
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
Bind();
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
Check(e.Snapshot.guidanceSeenMask==7,"All three one-time camera flags survive reload");
// Stress the largest excavation without collecting: enough pool for every rock.
var large=e.Snapshot;large.roundIndex=3;large.copperRidgeUnlocked=true;large.firstRefineryDeposit=true;large.items=new int[5];large.depletedBlocks=Array.Empty<string>();large.damagedBlocks=Array.Empty<RockDamageState>();large.groundDrops=Array.Empty<GroundOreState>();large.roundBonusClaimed=false;Check(e.Commit(large),"Prepare largest excavation stress fixture");yield return Reload();
Check(rocks.Length==84,"Largest excavation has 84 rocks");
foreach(var rock in rocks)while(!rock.Depleted){if(!rock.TryHit(e,rock.HitCollider.bounds.center))throw new Exception("Pool blocked "+rock.blockId);}
Check(e.Snapshot.groundDrops.Length==252&&e.drops.ActiveCount==252,"Entire 84-rock quarry can be mined with a full bag; 252 drops retained");Disk();yield return Reload();Check(e.Snapshot.groundDrops.Length==252&&e.drops.ActiveCount==252,"All 252 ore survive reload");
}
}



