using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class BoundaryChecks {
 public static string Main(){var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();if(!Application.isPlaying||!e.SavePath.Contains("BoundaryAnimationFix"))throw new Exception("Isolated test required");var s=e.Snapshot;s.roundIndex=3;s.copperRidgeUnlocked=true;s.wheelbarrowUnlocked=true;s.firstRefineryDeposit=true;s.guidanceSeenMask=15;e.Commit(s);var go=new GameObject("Boundary animation checks");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<BoundaryRunner>().Begin();return "Live checks running";}
}
public class BoundaryRunner:MonoBehaviour {
 MinerController p;MinerEquipment e;MinerAnimator anim;QuarryCameraGuidance guide;List<string> log=new List<string>();float maxDistanceError;
 void Check(bool ok,string message){log.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines("Maintenance/BoundaryAnimationFix/checks.txt",log);if(!ok)throw new Exception(message);}
 public void Begin(){StartCoroutine(Run());}
 void Place(Vector3 pos,Vector3 forward){p.touchControls.Move=Vector2.zero;p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=pos;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(forward);p.enabled=true;p.CancelMining();Physics.SyncTransforms();}
 void Input(Vector3 dir){var v=Quaternion.Euler(0,-p.followCamera.Yaw,0)*dir;p.touchControls.Move=new Vector2(v.x,v.z);}
 IEnumerator Drive(Vector3 direction,float seconds){float end=Time.time+seconds;while(Time.time<end){Input(direction);yield return new WaitForEndOfFrame();float d=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);maxDistanceError=Mathf.Max(maxDistanceError,Mathf.Abs(d-p.followCamera.distance));}p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.3f);}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/BoundaryAnimationFix/"+name+".png");yield return null;}
 IEnumerator Run(){
  yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().path);yield return new WaitForSeconds(2);
  p=UnityEngine.Object.FindFirstObjectByType<MinerController>();e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();anim=p.visual.GetComponent<MinerAnimator>();guide=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();while(p.InputBlocked)yield return null;p.autoMine=false;
  Check(p.GetComponentsInChildren<DitzelGames.FastIK.FastIKFabric>(true).Length==0,"No FastIK solver on player hierarchy");
  var a=anim.GetComponent<Animator>();int layer=a.GetLayerIndex("Pickaxe Carry");Check(layer>=0&&a.runtimeAnimatorController.animationClips.Count(c=>c.name.StartsWith("PickaxeCarry"))==3,"Three carry clips and masked animation layer wired");
  var hand=a.GetBoneTransform(HumanBodyBones.RightHand);Check(anim.toolGrip.IsChildOf(hand),"Pickaxe socket belongs to animated hand");
  foreach(var label in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).Where(t=>t.name=="Return lane label")){label.ForceMeshUpdate();Check(Mathf.Abs(label.GetComponent<Renderer>().bounds.center.y-.006f)<.004f,"Exit lettering sits on ground at "+label.transform.position);}
  Place(new Vector3(24.7f,.05f,0),Vector3.forward);yield return Drive(Vector3.forward,2);Check(p.transform.position.z<1.1f,"Player blocked at marked exterior entrance");Check(UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.name=="Cut Rock"&&r.forceRenderingOff),"Occluding boundary scenery hidden without moving camera");yield return Shot("blocked-gap");
  p.Jump();yield return Drive(Vector3.forward,1.5f);Check(p.transform.position.z<1.1f,"Gap blocker cannot be jumped");
  Place(new Vector3(20.45f,.05f,0),Vector3.forward);yield return Drive(Vector3.forward,.8f);Check(p.transform.position.z>2.4f,"Intended copper entrance remains open");
  Place(new Vector3(16.7f,.05f,0),Vector3.forward);yield return Drive(Vector3.forward,.8f);Check(p.transform.position.z>2.4f,"Marked west return corridor remains open");
  Place(new Vector3(0,.05f,-3),Vector3.right);yield return Drive(Vector3.right,.7f);yield return Shot("pickaxe-running");yield return Drive(Vector3.zero,.6f);Check(a.GetLayerWeight(layer)>.99f,"Carry pose restored on foot");yield return Shot("pickaxe-idle");
  Check(maxDistanceError<.003f,"Camera distance unchanged near walls and movement; error="+maxDistanceError.ToString("F6"));
  var rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)&&!r.requiresCopperRidge&&!r.Depleted).OrderBy(r=>r.transform.position.z).ThenBy(r=>Mathf.Abs(r.transform.position.x)).First();
  Place(rock.transform.position+Vector3.back*1.05f+Vector3.up*.08f,Vector3.forward);int hits=anim.GetComponent<MinerMining>().SuccessfulHits;p.autoMine=true;float deadline=Time.time+9;bool released=false;while(Time.time<deadline&&anim.GetComponent<MinerMining>().SuccessfulHits==hits){yield return null;if(p.IsMining&&a.GetLayerWeight(layer)<.01f)released=true;}
  Check(anim.GetComponent<MinerMining>().SuccessfulHits>hits,"Real animated mining contact still damages a rock");Check(released,"Carry layer releases during authored mining swing");yield return Shot("pickaxe-mining");p.autoMine=false;p.CancelMining();
  Place(new Vector3(0,.05f,-3),Vector3.forward);yield return new WaitForSeconds(1.3f);
  var state=e.Snapshot;state.guidanceSeenMask&=~8;Check(e.Commit(state),"Prepare isolated intro cue replay check");Vector3 home=p.followCamera.transform.position;
  Check(guide.TryShow(8,p.transform.position,"WELCOME","Test intro"),"One-time intro starts");yield return new WaitForSeconds(.9f);Check(Vector3.Distance(home,p.followCamera.transform.position)>.5f,"Intentional Cinemachine shot still moves camera");guide.Skip();yield return new WaitForSeconds(1);
  Check(!guide.Playing&&!p.InputBlocked,"Intro returns control");Check(!guide.TryShow(8,p.transform.position,"Repeat","Repeat"),"Saved intro does not replay");yield return new WaitForEndOfFrame();Check(Mathf.Abs(Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f)-p.followCamera.distance)<.003f,"Configured follow distance restored after intro");
  Check(e.SaveError==null,"No save errors");log.Add("COMPLETE "+log.Count+" checks");File.WriteAllLines("Maintenance/BoundaryAnimationFix/checks.txt",log);Destroy(gameObject);
 }
 void OnDestroy(){if(p)p.touchControls.Move=Vector2.zero;}
}
