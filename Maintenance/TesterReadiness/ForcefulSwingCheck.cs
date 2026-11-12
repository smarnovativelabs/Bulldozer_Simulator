using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
public static class ForcefulSwingCheckSetup {
 public static string Main(){Application.runInBackground=true;
  if(!Application.isPlaying||!UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","").Contains("TesterReadiness"))throw new Exception("Isolated test required");
  File.Copy("Maintenance/ManualPlaytest/completed-playthrough.json","Maintenance/TesterReadiness/forceful-progress.json",true);
  UnityEditor.SessionState.SetString("Bulldozer.Mining.TestSavePath",Path.GetFullPath("Maintenance/TesterReadiness/forceful-progress.json"));
  var go=new GameObject("Vertical swing test recording");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<ForcefulSwingCheck>();return "Animation regression and front recording started";
 }
}
public class ForcefulSwingCheck:MonoBehaviour {
 int previous;MinerController p;PlayerCargoBadge badge;List<string> log=new List<string>();
 IEnumerator Start(){
  Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().path);yield return new WaitForSeconds(2);
  p=FindFirstObjectByType<MinerController>();while(p.InputBlocked)yield return null;p.autoMine=false;
  var devices=UnityEngine.InputSystem.InputSystem.devices.Where(d=>d.enabled&&(d is UnityEngine.InputSystem.Keyboard||d is UnityEngine.InputSystem.Mouse)).ToArray();UnityEditor.SessionState.SetString("ManualPlaytest.InputDevices",string.Join(",",devices.Select(d=>d.deviceId)));
  foreach(var d in devices)UnityEngine.InputSystem.InputSystem.DisableDevice(d);
  var rock=FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!r.Depleted&&!r.naturalBoulder&&!r.requiresCopperRidge&&r.transform.position.z<3).OrderBy(r=>r.transform.position.x).First();
  var equipment=p.visual.GetComponent<MinerEquipment>();var fixture=equipment.Snapshot;fixture.damageLevel=0;equipment.Commit(fixture);var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=rock.transform.position+new Vector3(0,.03f,-.95f);cc.enabled=true;p.visual.rotation=Quaternion.identity;Physics.SyncTransforms();
  p.touchControls.AddLook(new Vector2(Mathf.DeltaAngle(p.followCamera.Yaw,165)/.13f,Mathf.DeltaAngle(55,p.followCamera.transform.eulerAngles.x)/.13f));
  yield return new WaitForSeconds(.3f);previous=Time.captureFramerate;Time.captureFramerate=20;
  var mining=p.visual.GetComponent<MinerMining>();var a=p.visual.GetComponent<Animator>();var driver=p.visual.GetComponent<MinerAnimator>();int hits=mining.SuccessfulHits;bool wasMining=false;int swings=0;float maxSupport=0,maxToolRotation=0,minLeftDistance=100;int supportFrames=0;int gapIdle=0,pendingIdle=0,gapSamples=0,pendingSamples=0;Vector3 up=mining.headProbes[0].localPosition.normalized;
  badge=FindFirstObjectByType<PlayerCargoBadge>();if(badge)badge.gameObject.SetActive(false);Directory.CreateDirectory("Maintenance/TesterReadiness/forceful-wall-frames");p.autoMine=true;
  log.Add("Animation test fixture positioned beside actual rock: "+rock.blockId+" health="+rock.Health+" damage="+mining.equipment.Damage);
  for(int i=0;i<200;i++){
   yield return new WaitForEndOfFrame();
   maxToolRotation=Mathf.Max(maxToolRotation,Quaternion.Angle(Quaternion.Inverse(p.visual.rotation)*driver.toolGrip.rotation,driver.pickaxeFrontRotation));if(p.IsMining)minLeftDistance=Mathf.Min(minLeftDistance,Vector3.Distance(a.GetBoneTransform(HumanBodyBones.LeftHand).position,driver.toolGrip.position));if(i==70)p.touchControls.AddLook(new Vector2(Mathf.DeltaAngle(p.followCamera.Yaw,85)/.13f,Mathf.DeltaAngle(40,p.followCamera.transform.eulerAngles.x)/.13f));if(p.IsMining&&!wasMining){if(swings>0){gapIdle+=pendingIdle;gapSamples+=pendingSamples;}pendingIdle=pendingSamples=0;swings++;} if(!p.IsMining&&swings>0){pendingSamples++;if(a.GetCurrentAnimatorStateInfo(0).IsName("Locomotion")||a.GetLayerWeight(a.GetLayerIndex("Pickaxe Carry"))>.01f)pendingIdle++;} wasMining=p.IsMining;
   if(p.IsMining&&!a.IsInTransition(0)&&(a.GetCurrentAnimatorStateInfo(0).IsName("Mine Wall")||a.GetCurrentAnimatorStateInfo(0).IsName("Mine Ground"))&&a.GetCurrentAnimatorStateInfo(0).normalizedTime>.12f&&a.GetLayerWeight(a.GetLayerIndex("Pickaxe Carry"))<.01f) {maxSupport=Mathf.Max(maxSupport,Vector3.Distance(a.GetBoneTransform(HumanBodyBones.LeftHand).position,driver.toolGrip.TransformPoint(-up*.22f)));supportFrames++;}
   var tex=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("Maintenance/TesterReadiness/forceful-wall-frames/frame-"+i.ToString("D4")+".jpg",tex.EncodeToJPG(85));Destroy(tex);
  }
  p.autoMine=false;p.CancelMining();
  log.Add((mining.SuccessfulHits-hits>=2?"PASS ":"FAIL ")+"Repeated physical hits="+(mining.SuccessfulHits-hits)+" swings="+swings);
  log.Add((rock.Depleted?"PASS ":"FAIL ")+"Actual rock broke from animation contact");
  log.Add((maxToolRotation<.1f?"PASS ":"FAIL ")+"Fixed pickaxe angle: max rotation="+maxToolRotation);log.Add((minLeftDistance>.15f?"PASS ":"FAIL ")+"Left hand stays away from pickaxe: min distance="+minLeftDistance);
  log.Add((gapSamples>0&&gapIdle==0?"PASS ":"FAIL ")+"No carry/idle pose between swings: idle frames="+gapIdle+" / recovery frames="+gapSamples);float end=Time.time+.5f;while(Time.time<end){var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*Vector3.back;p.touchControls.Move=new Vector2(local.x,local.z);yield return null;}p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.4f);log.Add((!a.GetBool("Mining")&&a.GetLayerWeight(a.GetLayerIndex("Pickaxe Carry"))>.99f?"PASS ":"FAIL ")+"Moving away releases recovery and restores carry pose");File.WriteAllLines("Maintenance/TesterReadiness/forceful-wall-checks.txt",log);Destroy(gameObject);
 }
 void OnDestroy(){if(badge)badge.gameObject.SetActive(true);Time.captureFramerate=previous;if(p&&p.touchControls)p.touchControls.Move=Vector2.zero;}
}









