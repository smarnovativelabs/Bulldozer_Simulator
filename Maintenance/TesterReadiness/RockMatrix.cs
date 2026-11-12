using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class RockMatrixSetup {
 public static string Main(){
  if(!Application.isPlaying||!UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","").Contains("TesterReadiness"))throw new Exception("Isolated test required");
  Application.runInBackground=true;new GameObject("Rock matrix checks").AddComponent<RockMatrix>();return "Rock geometry and transition matrix started";
 }
}
public class RockMatrix:MonoBehaviour {
 MinerController p;MinerMining mining;Animator anim;MineableRock[] all;bool[] enabledColliders;int oldCapture;List<string> report=new List<string>();
 void Check(bool ok,string message){report.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines("Maintenance/TesterReadiness/rock-matrix.txt",report);}
 void Place(Vector3 pos,Vector3 direction){p.CancelMining();p.touchControls.Move=Vector2.zero;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=pos;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(direction);Physics.SyncTransforms();}
 IEnumerator Start(){
  yield return new WaitForSeconds(2);p=FindFirstObjectByType<MinerController>();while(p.InputBlocked)yield return null;p.autoMine=false;mining=p.visual.GetComponent<MinerMining>();anim=p.visual.GetComponent<Animator>();
  var devices=UnityEngine.InputSystem.InputSystem.devices.Where(d=>d.enabled&&(d is UnityEngine.InputSystem.Keyboard||d is UnityEngine.InputSystem.Mouse)).ToArray();UnityEditor.SessionState.SetString("TesterReadiness.InputDevices",string.Join(",",devices.Select(d=>d.deviceId)));foreach(var d in devices)UnityEngine.InputSystem.InputSystem.DisableDevice(d);
  oldCapture=Time.captureFramerate;Time.captureFramerate=30;
  all=FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)&&r.Available(mining.equipment.Snapshot)).ToArray();enabledColliders=all.Select(r=>r.HitCollider.enabled).ToArray();foreach(var r in all)r.HitCollider.enabled=false;
  var cases=all.Where(r=>r.naturalBoulder).Concat(all.Where(r=>!r.naturalBoulder&&!r.requiresCopperRidge).GroupBy(r=>r.resourceType).Select(g=>g.OrderByDescending(r=>r.maxHealth).First())).Concat(all.Where(r=>r.requiresCopperRidge)).Distinct().ToArray();
  report.Add("Isolated geometry fixture: real rock temporarily placed on clear spawn pad; other rock colliders disabled. This tests animation reach, not navigation. " +cases.Length+" rock models, four directions each.");
  foreach(var rock in cases){
   Vector3 saved=rock.transform.position;int health=rock.Health,maximum=rock.maxHealth;rock.maxHealth=100;rock.transform.position=Vector3.zero;rock.HitCollider.enabled=true;
   for(int side=0;side<4;side++){
    typeof(MineableRock).GetProperty("Health").SetValue(rock,100);Vector3 direction=Quaternion.Euler(0,side*90,0)*Vector3.forward;
    Place(-direction*1.05f+Vector3.up*.03f,direction);yield return new WaitForSeconds(.65f);
    int hits=mining.SuccessfulHits;p.Mine();yield return new WaitForSeconds(p.miningDuration+.65f);
    Check(mining.SuccessfulHits-hits==1,rock.blockId+" ore="+rock.resourceType+" natural="+rock.naturalBoulder+" side="+side*90+" contact hits="+(mining.SuccessfulHits-hits));
    Check(!p.IsMining&&!anim.GetBool("Mining")&&anim.GetLayerWeight(anim.GetLayerIndex("Pickaxe Carry"))>.99f,"Mining returns to carry idle: "+rock.blockId+" side="+side*90);
   }
   rock.HitCollider.enabled=false;rock.transform.position=saved;rock.maxHealth=maximum;typeof(MineableRock).GetProperty("Health").SetValue(rock,health);
  }
  var sample=cases[0];Vector3 original=sample.transform.position;sample.maxHealth=100;sample.transform.position=Vector3.zero;sample.HitCollider.enabled=true;typeof(MineableRock).GetProperty("Health").SetValue(sample,100);Place(new Vector3(0,.03f,-1.05f),Vector3.forward);yield return new WaitForSeconds(.6f);
  int before=mining.SuccessfulHits;p.Mine();yield return new WaitForSeconds(.1f);float until=Time.time+.4f;while(Time.time<until){var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*Vector3.back;p.touchControls.Move=new Vector2(local.x,local.z);yield return null;}p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.5f);
  Check(!p.IsMining&&!anim.GetBool("Mining")&&mining.SuccessfulHits==before,"Movement cancels wind-up without a delayed ghost hit");
  sample.transform.position=original;sample.HitCollider.enabled=false;Place(new Vector3(0,.03f,-2),Vector3.forward);
  for(int i=0;i<all.Length;i++)all[i].HitCollider.enabled=enabledColliders[i];
  report.Add("COMPLETE failures="+report.Count(s=>s.StartsWith("FAIL"))+" checks="+report.Count(s=>s.StartsWith("PASS")||s.StartsWith("FAIL")));File.WriteAllLines("Maintenance/TesterReadiness/rock-matrix.txt",report);Destroy(gameObject);
 }
 void OnDestroy(){Time.captureFramerate=oldCapture;if(p){p.autoMine=false;p.touchControls.Move=Vector2.zero;}if(all!=null&&enabledColliders!=null)for(int i=0;i<all.Length;i++)if(all[i]&&all[i].HitCollider)all[i].HitCollider.enabled=enabledColliders[i];}
}
