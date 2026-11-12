using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class LoadedChecks {
 public static string Main(){var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();if(!e.SavePath.Contains("TesterReadiness"))throw new Exception("Test save required");new GameObject("Loaded cart checks").AddComponent<LoadedCartRunner>().Begin();return "Running loaded reverse and animation checks";}
}
public class LoadedCartRunner:MonoBehaviour {
 MinerController p;QuarryWheelbarrow c;List<string> log=new List<string>();int originalRate;int penetration;float grip;
 void Check(bool ok,string s){log.Add((ok?"PASS ":"FAIL ")+s);File.WriteAllLines("Maintenance/TesterReadiness/loaded-checks.txt",log);if(!ok)throw new Exception(s);}
 public void Begin(){p=UnityEngine.Object.FindFirstObjectByType<MinerController>();c=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();originalRate=Application.targetFrameRate;StartCoroutine(Run());}
 void Input(Vector3 d){var v=Quaternion.Euler(0,-p.followCamera.Yaw,0)*d;p.touchControls.Move=new Vector2(v.x,v.z);}
 IEnumerator Drive(Vector3 d,float seconds){float end=Time.time+seconds;while(Time.time<end){Input(d);yield return new WaitForEndOfFrame();if(Physics.OverlapBox(c.transform.position+Vector3.up*.55f,new Vector3(.33f,.31f,.54f),c.transform.rotation,~0,QueryTriggerInteraction.Ignore).Any(x=>!x.transform.IsChildOf(p.transform)&&!x.transform.IsChildOf(c.transform)))penetration++;grip=Mathf.Max(grip,c.LeftGripError,c.RightGripError);}p.touchControls.Move=Vector2.zero;}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("Maintenance/TesterReadiness/"+name+".jpg",texture.EncodeToJPG(85));Destroy(texture);}
 IEnumerator Run(){
  foreach(int rate in new[]{30,60}){
   Application.targetFrameRate=rate;if(c.Pushing){c.Interact();yield return new WaitForSeconds(.5f);}
   p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(19.2f,.05f,1.9f);cc.enabled=true;p.visual.rotation=Quaternion.identity;c.transform.position=new Vector3(19.2f,.05f,-.3f);p.enabled=true;p.autoMine=false;Physics.SyncTransforms();yield return new WaitForSeconds(.6f);c.Interact();yield return new WaitForSeconds(.5f);Check(c.Pushing,"Cart attaches at target "+rate);
   var s=c.equipment.Snapshot;s.items=Enumerable.Range(0,12).Select(i=>i%6).ToArray();Check(c.equipment.Commit(s),"Load 12 mixed ore");penetration=0;grip=0;
   yield return Drive(Vector3.forward,1);Check(p.CartSignedSpeed>.1f,"Forward push signed animation speed");yield return Shot("push-"+rate);var contact=p.transform.position;yield return Drive(Vector3.back,1);Check(p.CartSignedSpeed<-.1f,"Reverse pull signed animation speed");yield return Shot("pull-"+rate);
   Check(Vector3.Distance(contact,p.transform.position)>.35f,"Loaded reverse escapes contact at target "+rate);
   Check(p.CartSignedSpeed<-.05f||p.Speed<.05f,"Reverse never faces away from handles");
   var animator=p.visual.GetComponent<Animator>();Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Cart"),"Cart animation state active");
   yield return Drive(Vector3.right,2);Check(p.transform.position.x>19.8f,"Steer clear after reverse at target "+rate);
   var braking=p.transform.position;yield return Drive(Vector3.zero,.5f);Check(Vector3.Distance(braking,p.transform.position)<.25f&&p.Speed<.05f,"Release input brakes without forward kick");
   Check(penetration==0,"Zero cart penetration frames at target "+rate);Check(grip<.09f,"Baked hand contact max error="+grip.ToString("F3"));Check(c.equipment.Snapshot.items.Length==12,"Loaded cargo unchanged by reverse/collision");
   Check(!c.GetComponentsInChildren<DitzelGames.FastIK.FastIKFabric>().Any(x=>x.enabled),"No active cart IK components");
   c.Interact();yield return new WaitForSeconds(.5f);Check(!c.Pushing&&c.equipment.Snapshot.barrowItems.Length==12,"Parking retains all twelve cargo");
   Check(!animator.GetBool("Cart")&&c.equipment.PickaxeVisible,"Parking restores on-foot animation and pickaxe");
  }
  Application.targetFrameRate=originalRate;log.Add("COMPLETE "+log.Count+" checks; target frame rates are editor settings, not device measurements");File.WriteAllLines("Maintenance/TesterReadiness/loaded-checks.txt",log);Destroy(gameObject);
 }
 void OnDestroy(){Application.targetFrameRate=originalRate;if(p)p.touchControls.Move=Vector2.zero;}
}


