using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class TraversalReview {
 public static string Main(){
  var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();
  if(!Application.isPlaying||!e.SavePath.Contains("CartReleaseFix"))throw new Exception("Isolated save required");
  var s=e.Snapshot;s.roundIndex=3;s.copperRidgeUnlocked=true;s.wheelbarrowUnlocked=true;s.firstRefineryDeposit=true;s.guidanceSeenMask=15;s.usingWheelbarrow=false;s.roundBonusClaimed=false;
  s.items=s.backpackItems=s.barrowItems=s.refineryInput=s.refineryOutput=s.loadingQueue=s.truckItems=Array.Empty<int>();s.depletedBlocks=Array.Empty<string>();s.damagedBlocks=Array.Empty<RockDamageState>();s.groundDrops=Array.Empty<GroundOreState>();s.barrowPosition=Vector3.zero;QuarryProgressionRules.ResetContract(s);
  if(!e.Commit(s))throw new Exception(e.SaveError);
  var go=new GameObject("Traversal review runner");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<TraversalReviewRunner>().Begin();return "Started live joystick traversal scenarios";
 }
}
public class TraversalReviewRunner:MonoBehaviour {
 MinerController p;QuarryWheelbarrow c;MinerEquipment e;List<string> lines=new List<string>(); float maxGrip; int gripFrames; void LateUpdate(){if(c&&c.Pushing&&p&&p.visual.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Cart")&&!p.visual.GetComponent<Animator>().IsInTransition(0)){maxGrip=Mathf.Max(maxGrip,Mathf.Max(c.LeftGripError,c.RightGripError));gripFrames++;}}
 void Log(string s){lines.Add(s);File.WriteAllLines("Maintenance/CartReleaseFix/traversal.txt",lines);}
 public void Begin(){StartCoroutine(Run());}
 void Place(Vector3 pos,Vector3 forward){p.touchControls.Move=Vector2.zero;p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=pos;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(forward);p.enabled=true;Physics.SyncTransforms();}
 void Input(Vector3 world){var v=Quaternion.Euler(0,-p.followCamera.Yaw,0)*world;p.touchControls.Move=new Vector2(v.x,v.z);}
 IEnumerator Drive(Vector3 dir,float duration){float end=Time.time+duration;while(Time.time<end){Input(dir);yield return null;}p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.2f);}
 IEnumerator Go(Vector3 target,string name){float end=Time.time+16;while(Time.time<end){var delta=target-p.transform.position;delta.y=0;if(delta.magnitude<.25f)break;Input(delta.normalized*Mathf.Clamp01(delta.magnitude));yield return null;}p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.3f);float error=Vector2.Distance(new Vector2(target.x,target.z),new Vector2(p.transform.position.x,p.transform.position.z));Log((error<.5f?"PASS ":"FAIL ")+name+" target="+target+" actual="+p.transform.position+" error="+error.ToString("F2"));}
 IEnumerator Snap(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/CartReleaseFix/"+name+".png");yield return null;}
 string Overlaps(){return string.Join(",",Physics.OverlapBox(c.transform.position+Vector3.up*.55f,new Vector3(.335f,.32f,.55f),c.transform.rotation,~0,QueryTriggerInteraction.Ignore).Where(x=>!x.transform.IsChildOf(p.transform)&&!x.transform.IsChildOf(c.transform)).Select(x=>x.name));}
 IEnumerator Attach(Vector3 pos,Vector3 dir){if(c.Pushing){c.Interact();yield return new WaitForSeconds(.5f);}Place(pos,dir);c.transform.position=pos-dir*2.2f;Physics.SyncTransforms();yield return new WaitForSeconds(.6f);c.Interact();yield return new WaitForSeconds(.5f);Log("ATTACH "+c.Pushing+" player="+p.transform.position+" cart="+c.transform.position);}
 IEnumerator Run(){
  yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().path);yield return new WaitForSeconds(2);
  p=UnityEngine.Object.FindFirstObjectByType<MinerController>();c=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();e=c.equipment;p.autoMine=false;p.CancelMining();
  while(p.InputBlocked)yield return null;
  Log("START frame="+Time.frameCount+" live joystick input; fixture excavation 4, intact rocks; no production code changed");
  foreach(var r in UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(x=>x.requiresCopperRidge))Log("ROCK "+r.name+" bounds="+r.HitCollider.bounds);
  yield return Attach(new Vector3(19.2f,.05f,1.9f),Vector3.forward);
  yield return Drive(Vector3.forward,3);Vector3 hit=p.transform.position;Log("CONTACT position="+hit+" overlaps="+Overlaps());yield return Snap("cart-rock-contact");
  Vector3 back=Vector3.back*.2f;Quaternion same=p.visual.rotation;c.ConstrainMovement(ref back,ref same);Log("PROBE backward sweep allowed="+back+" facing="+same.eulerAngles);
  yield return Drive(Vector3.back,4);Log("REVERSE displacement="+Vector3.Distance(hit,p.transform.position).ToString("F3")+" position="+p.transform.position+" facing="+p.visual.eulerAngles+" overlaps="+Overlaps());yield return Snap("cart-rock-reverse");
  yield return Drive(Vector3.right,3);Log("SIDE escape position="+p.transform.position+" facing="+p.visual.eulerAngles+" overlaps="+Overlaps());
  if(c.Pushing){c.Interact();yield return new WaitForSeconds(.5f);}Place(new Vector3(20.45f,.05f,9.4f),Vector3.back);
  yield return Go(new Vector3(20.45f,.05f,2.2f),"Foot ridge centre aisle exit");yield return Go(new Vector3(20.45f,.05f,-3.8f),"Foot through south ridge entrance");yield return Go(new Vector3(12,.05f,-3.8f),"Foot return west below side bank");
  Place(new Vector3(16.7f,.05f,8),Vector3.back);yield return Go(new Vector3(16.7f,.05f,-3.8f),"Foot exterior west ridge corridor south");yield return Go(new Vector3(12,.05f,-3.8f),"Foot exterior corridor to quarry");
  yield return Attach(new Vector3(20.45f,.05f,9.4f),Vector3.back);yield return Go(new Vector3(20.45f,.05f,2.2f),"Cart ridge centre aisle exit");yield return Go(new Vector3(20.45f,.05f,-3.8f),"Cart south entrance exit");yield return Go(new Vector3(12,.05f,-3.8f),"Cart return west below side bank");yield return Snap("ridge-return-route");
  if(c.Pushing){c.Interact();yield return new WaitForSeconds(.5f);}yield return Attach(new Vector3(16.7f,.05f,8),Vector3.back);yield return Go(new Vector3(16.7f,.05f,-3.8f),"Cart exterior west ridge corridor south");yield return Go(new Vector3(12,.05f,-3.8f),"Cart exterior corridor to quarry");
  p.touchControls.Move=Vector2.zero;Log("ANIMATION gripFrames="+gripFrames+" maxGrip="+maxGrip.ToString("F3"));Log("COMPLETE frame="+Time.frameCount);Destroy(gameObject);
 }
}



