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
public static class VisualGameplayChecks {
 public static string Main(){if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play required");var go=new GameObject("Visual polish test runner");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<VisualPolishRunner>().Begin();return "Running live polish checks";}
}
public class VisualPolishRunner:MonoBehaviour {
 readonly List<string> log=new List<string>();MinerController p;MinerEquipment e;QuarryWheelbarrow cart;QuarryCameraGuidance guide;MiningProduction production;Animator animator;
 void Flush(){using(var f=new FileStream("Maintenance/VisualPolish/gameplay-checks.txt",FileMode.Create,FileAccess.Write,FileShare.ReadWrite))using(var w=new StreamWriter(f))foreach(var line in log)w.WriteLine(line);}
 void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);Flush();if(!ok)throw new Exception(name);}
 public void Begin(){StartCoroutine(Guard(Run()));}
 IEnumerator Guard(IEnumerator source){var stack=new Stack<IEnumerator>();stack.Push(source);while(stack.Count>0){object value=null;bool more=false;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;else stack.Pop();}catch(Exception ex){log.Add("FAILED "+ex);Flush();yield break;}if(more){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}}log.Add("COMPLETE "+log.Count+" checks; frame="+Time.frameCount);Flush();Destroy(gameObject);}
 void Bind(){p=UnityEngine.Object.FindFirstObjectByType<MinerController>();e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();guide=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();production=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();animator=p.visual.GetComponent<Animator>();p.autoMine=false;}
 IEnumerator Reload(){int id=e.GetInstanceID();SceneManager.LoadSceneAsync(p.gameObject.scene.path);float until=Time.realtimeSinceStartup+40;while(!UnityEngine.Object.FindFirstObjectByType<MinerEquipment>()||UnityEngine.Object.FindFirstObjectByType<MinerEquipment>().GetInstanceID()==id){if(Time.realtimeSinceStartup>until)throw new Exception("Reload timeout");yield return null;}Bind();}
 void Warp(Vector3 pos,float heading=0){var motor=p.GetComponent<CharacterController>();motor.enabled=false;p.transform.position=pos;motor.enabled=true;p.visual.rotation=Quaternion.Euler(0,heading,0);p.touchControls.Move=Vector2.zero;p.CancelMining();Physics.SyncTransforms();}
 Vector2 Input(float worldHeading){float radians=(worldHeading-p.followCamera.Yaw)*Mathf.Deg2Rad;return new Vector2(Mathf.Sin(radians),Mathf.Cos(radians));}
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/VisualPolish/"+name+".png");}
 IEnumerator Run(){
 Bind();Check(e.Commit(MiningSnapshot.Create(0,0)),"Prepare isolated fresh profile");yield return Reload();
 int frames=0;bool airborne=false,sawIntro=false,locked=true,captured=false;float start=Time.realtimeSinceStartup;
 while(Time.realtimeSinceStartup-start<6){frames++;airborne|=animator.GetCurrentAnimatorStateInfo(0).IsName("Airborne")||animator.GetNextAnimatorStateInfo(0).IsName("Airborne");if(guide.Playing){sawIntro|=guide.ActiveCue==8;locked&=p.InputBlocked;if(!captured&&Time.realtimeSinceStartup-start>1){yield return Capture("player-intro");captured=true;}}yield return null;}
 Check(frames>10,"Real gameplay frames advance");Check(!airborne&&p.Grounded,"Scene load stays grounded without airborne/jump animation");Check(sawIntro&&locked&&guide.ShotsStarted==1,"Fresh profile plays one intro and holds input");Check(!guide.Playing&&!p.InputBlocked,"Intro automatically returns control");Check((e.Snapshot.guidanceSeenMask&8)!=0,"Intro completion flag persisted");
 yield return Reload();yield return new WaitForSecondsRealtime(3);Check(guide.ShotsStarted==0&&!guide.Playing&&!p.InputBlocked,"Reload does not repeat intro or lock controls");
 // Verify an intentional jump still leaves the ground and lands.
 float floor=p.transform.position.y;bool left=false;p.Jump();float end=Time.time+1.8f;while(Time.time<end){left|=!p.Grounded&&p.transform.position.y>floor+.2f;yield return null;}Check(left&&p.Grounded,"Intentional jump and landing still work");
 var state=e.Snapshot;state.wheelbarrowUnlocked=true;state.guidanceSeenMask=15;state.items=new[]{0,1};state.barrowItems=new[]{7,8,9};Check(e.Commit(state),"Prepare cart inventory fixture");guide.enabled=false;production.enabled=false;
 Warp(new Vector3(0,.03f,-2));cart.transform.position=p.transform.position+Vector3.forward*.95f;yield return new WaitForSeconds(.5f);cart.Interact();Check(cart.Pushing&&e.Snapshot.items.SequenceEqual(new[]{7,8,9})&&e.Snapshot.backpackItems.SequenceEqual(new[]{0,1}),"Attach preserves backpack and cart cargo");
 foreach(int fps in new[]{30,60}){int oldTarget=Application.targetFrameRate;Application.targetFrameRate=fps;
 foreach(float heading in new[]{0f,23f,67f,135f,225f,300f}){
  Warp(new Vector3(0,.03f,-2),heading-80);yield return null;p.touchControls.Move=Input(heading);float until=Time.time+1.1f;float maximumSlip=0;Vector3 last=p.transform.position;
  while(Time.time<until){yield return null;Vector3 delta=p.transform.position-last;delta.y=0;if(delta.magnitude>.002f)maximumSlip=Mathf.Max(maximumSlip,Vector3.Angle(delta,p.visual.forward));last=p.transform.position;}
  p.touchControls.Move=Vector2.zero;Check(Mathf.Abs(Mathf.DeltaAngle(p.visual.eulerAngles.y,heading))<7,"Continuous cart turn to "+heading+" degrees at target "+fps);Check(maximumSlip<12,"Cart follows facing without sideways movement at "+heading+" / "+fps);
 }Application.targetFrameRate=oldTarget;}
 p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.5f);Check(p.Speed<.08f,"Cart brakes after input release");Check(cart.LeftGripError<.12f&&cart.RightGripError<.12f,"Narrow cart retains reachable hand grips");
 Warp(new Vector3(0,.03f,-2));yield return new WaitForSeconds(.2f);var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Temporary cart collision test";obstacle.transform.position=new Vector3(0,.55f,.3f);obstacle.transform.localScale=new Vector3(2,1.1f,.2f);Physics.SyncTransforms();p.touchControls.Move=Input(0);yield return new WaitForSeconds(2);p.touchControls.Move=Vector2.zero;Check(p.transform.position.z<-.9f,"Cart sweep stops before solid obstacle");UnityEngine.Object.Destroy(obstacle);yield return new WaitForSeconds(.5f);
 cart.Interact();Check(!cart.Pushing&&e.Snapshot.items.SequenceEqual(new[]{0,1})&&e.Snapshot.barrowItems.SequenceEqual(new[]{7,8,9}),"Parking restores exact backpack and retains cart cargo");
 // Populate all mineral types; no gameplay item IDs or save positions are changed by art.
 state=e.Snapshot;state.items=Array.Empty<int>();state.groundDrops=Enumerable.Range(0,6).Select(i=>new GroundOreState{id="polish-ore-"+i,item=i,position=new Vector3(1.4f+i*.35f,.13f,1.5f)}).ToArray();state.refineryInput=new[]{0,1,2};state.refineryOutput=new[]{6,7,8,9,10,11};state.loadingQueue=new[]{6,10};Check(e.Commit(state),"Seed isolated visual/counter fixture");e.drops.Sync(state.groundDrops,Vector3.zero,false);production.enabled=false;
 production.SendMessage("Refresh");Check(production.inputLabel.text.Contains("3 RAW")&&production.truckLabel.text.Contains("2 QUEUED")&&production.outputLabel.text.Contains("6 READY"),"Boundary counters show actual raw, ready and queued inventories");
 for(int i=0;i<3;i++){var zone=new[]{production.inputZone,production.outputZone,production.sellZone}[i];var label=new[]{production.inputLabel,production.outputLabel,production.truckLabel}[i];var local=zone.transform.InverseTransformPoint(label.transform.position);Check(Mathf.Abs(local.x)+label.rectTransform.sizeDelta.x*.5f<zone.size.x*.5f&&Mathf.Abs(local.z)+label.rectTransform.sizeDelta.y*.5f<zone.size.z*.5f,"Counter rectangle remains inside boundary "+i);}
 Check(e.resourceMaterials.Distinct().Count()==2&&e.resourceMeshes.Take(6).Max(m=>(int)m.GetIndexCount(0)/3)<=32,"Ore uses two shared materials and 32 triangles per raw chunk");Check(e.resourceMeshes.Skip(6).All(m=>m.GetIndexCount(0)/3==44),"Finished products have bevelled 44-triangle meshes");
 var poses=e.drops.GetComponentsInChildren<MeshFilter>().ToDictionary(m=>m.transform.parent.name,m=>m.transform.localRotation);
 yield return Capture("ore-and-refinery");yield return Reload();guide.enabled=false;production.enabled=false;production.enabled=false;yield return null;
 Check(e.Snapshot.groundDrops.Length==6&&e.drops.ActiveCount==6,"Ground ore retained after scene reload");Check(e.drops.GetComponentsInChildren<MeshFilter>().All(m=>poses.TryGetValue(m.transform.parent.name,out var rotation)&&Quaternion.Angle(rotation,m.transform.localRotation)<.01f),"Ore resting rotations reproduce exactly on reload");
 Check(e.Snapshot.barrowItems.SequenceEqual(new[]{7,8,9})&&e.Snapshot.refineryOutput.Length==6,"Cart and refined inventory retained on reload");
 // Keep a useful visual inspection fixture active at the end.
 Warp(new Vector3(1,.03f,-2));yield return new WaitForSeconds(.3f);yield return Capture("final-gameplay");
 }
}


