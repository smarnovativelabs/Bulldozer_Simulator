using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class PolishPreview {
 public static string Prepare(){if(Application.isPlaying)throw new Exception("Stop Play first");string path=SessionState.GetString("Bulldozer.Mining.TestSavePath","");if(string.IsNullOrEmpty(path))throw new Exception("Isolated save required");var state=MiningSnapshot.Create(0,100);state.guidanceSeenMask=15;state.wheelbarrowUnlocked=true;state.barrowItems=new[]{0,1,4,6,7,10};state.barrowPosition=new Vector3(.2f,.05f,-2);state.refineryInput=new[]{0,1,2,4};state.refineryOutput=new[]{6,7,8,9,10,11};state.loadingQueue=new[]{7,10};state.groundDrops=Enumerable.Range(0,6).Select(i=>new GroundOreState{id="preview-"+i,item=i,position=new Vector3(1.2f+i*.32f,.13f,.75f)}).ToArray();if(!new MiningProgressStore(path).TryCommit(state))throw new Exception("Fixture save failed");return "Isolated visual fixture saved";}
 public static string Main(){if(!Application.isPlaying)throw new Exception("Play required");new GameObject("Final visual smoke test").AddComponent<PolishPreviewRunner>();return "Final visual capture and fracture smoke checks running";}
}
public class PolishPreviewRunner:MonoBehaviour {
 IEnumerator Start(){
 var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var site=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();var guide=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();
 p.autoMine=false;guide.enabled=false;site.enabled=false;p.GuidanceInputBlocked=true; var stock=e.Snapshot;stock.refineryInput=new[]{0,1,2,4};stock.refineryOutput=new[]{6,7,8,9,10,11};stock.loadingQueue=new[]{7,10};e.Commit(stock);site.SendMessage("Refresh");
 var cam=p.followCamera.GetComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(6,6.5f,-7),Quaternion.LookRotation(new Vector3(3,.45f,-1.5f)-new Vector3(6,6.5f,-7)));
 yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/VisualPolish/refinery-preview.png");
 cam.transform.SetPositionAndRotation(new Vector3(2.4f,2.6f,-4.1f),Quaternion.LookRotation(cart.transform.position+Vector3.up*.55f-new Vector3(2.4f,2.6f,-4.1f)));
 yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/VisualPolish/wheelbarrow-preview.png");
 var fracture=e.drops.fracture;fracture.Burst(new Bounds(new Vector3(1.5f,.6f,0),Vector3.one),Vector3.back);yield return new WaitForSeconds(.2f);
 bool alive=fracture.ActiveCount==8&&fracture.GetComponent<MeshRenderer>().enabled&&fracture.HighestLift>0; string detail=" count="+fracture.ActiveCount+" visible="+fracture.GetComponent<MeshRenderer>().enabled+" lift="+fracture.HighestLift;
 cam.transform.SetPositionAndRotation(new Vector3(3,3,-4),Quaternion.LookRotation(new Vector3(1.5f,.6f,0)-new Vector3(3,3,-4)));
 yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/VisualPolish/fracture-preview.png");
 yield return new WaitForSeconds(1.3f);bool pooled=fracture.ActiveCount==0&&!fracture.GetComponent<MeshRenderer>().enabled;
 // Verify the introductory camera supports an early skip, restoring only its own lock.
 p.GuidanceInputBlocked=false;guide.enabled=true;var state=e.Snapshot;state.guidanceSeenMask&=~8;e.Commit(state);
 bool started=guide.TryShow(8,p.transform.position,"WELCOME TO YOUR QUARRY","Tap to begin your first shift.");yield return new WaitForSeconds(.5f);guide.Skip();yield return new WaitForSeconds(.8f);bool released=!guide.Playing&&!p.GuidanceInputBlocked&&(e.Snapshot.guidanceSeenMask&8)!=0;
 File.WriteAllLines("Maintenance/VisualPolish/final-smoke-checks.txt",new[]{(alive?"PASS":"FAIL")+" Eight angular fracture pieces animate and lift"+detail,(pooled?"PASS":"FAIL")+" Fracture pool retires and disables its single renderer",(started&&released?"PASS":"FAIL")+" Intro can be skipped and returns player controls"});Destroy(gameObject);
 }
}

