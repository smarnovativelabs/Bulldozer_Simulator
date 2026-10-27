using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Bulldozer.Player.Editor
{
 public static class MinerActionVerification
 {
  static MinerController p;static MinerAnimator driver;static Animator a;static MinerEquipment e;
  static float start,stageStart,height;static int stage,firstFrame;static Vector3 miningPosition;static List<string> lines;static HashSet<string> clips;
  public static void Start(){if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.InvalidOperationException("Prepare an isolated save with DepositVerification.Prepare before running destructive player tests.");p=UnityEngine.Object.FindFirstObjectByType<MinerController>();a=p.visual.GetComponent<Animator>();driver=p.visual.GetComponent<MinerAnimator>();e=driver.equipment;stage=0;firstFrame=Time.frameCount;start=stageStart=Time.time;height=p.transform.position.y;lines=new List<string>();clips=new HashSet<string>();p.touchControls.Move=Vector2.up;p.touchControls.Sprinting=true;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
  static void Capture(string name,bool rear=false)
  {
   var wall=GameObject.Find("CameraCollisionWall")?.GetComponent<Renderer>();bool wallWasVisible=wall && wall.enabled;if(wall)wall.enabled=false; var go=new GameObject("Verification Camera");var c=go.AddComponent<Camera>();c.clearFlags=CameraClearFlags.Skybox;c.fieldOfView=48;
   c.transform.position=p.transform.position+new Vector3(rear?2:2.4f,1.65f,rear?-3:1.8f);c.transform.LookAt(p.transform.position+Vector3.up*.95f);
   var rt=new RenderTexture(900,900,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(900,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes("Maintenance/Miner/"+name+".png",tex.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);if(wall)wall.enabled=wallWasVisible;
  }
  static void Tick()
  {
   if(!EditorApplication.isPlaying||!p){EditorApplication.update-=Tick;return;}
   foreach(var c in a.GetCurrentAnimatorClipInfo(0))if(c.weight>.15f)clips.Add(c.clip.name);
   float elapsed=Time.time-stageStart;
   if(stage==0 && elapsed>.8f){lines.Add("Run speed="+p.Speed);Capture("running-front");p.touchControls.Move=Vector2.zero;p.touchControls.Sprinting=false;stage++;stageStart=Time.time;}
   else if(stage==1 && elapsed>.5f){Capture("clean-face-front");e.SetCargoCount(5);Capture("cargo-rack",true);e.SetCargoCount(0);p.Mine();miningPosition=p.transform.position;stage++;stageStart=Time.time;}
   else if(stage==2 && elapsed>.40f){Capture("mining-ground");lines.Add("Ground mining="+p.IsMining+"; movement="+Vector3.Distance(miningPosition,p.transform.position)+"; grip error="+Vector3.Distance(a.GetBoneTransform(HumanBodyBones.RightHand).position,driver.gripTarget.position));stage++;stageStart=Time.time;}
   else if(stage==3 && elapsed>1){p.MineWall=true;p.Mine();stage++;stageStart=Time.time;}
   else if(stage==4 && elapsed>.4f){Capture("mining-wall");e.SetPickaxeVisible(false);lines.Add("Hide cancels mining="+(!p.IsMining)+"; tool active="+e.pickaxe.activeInHierarchy);Capture("hidden-pickaxe");p.Mine();stage++;stageStart=Time.time;}
   else if(stage==5 && elapsed>.3f){lines.Add("Hidden tool blocks mining="+(!p.IsMining));e.SetPickaxeVisible(true);p.MineWall=false;p.Jump();stage++;stageStart=Time.time;}
   else if(stage==6){height=Mathf.Max(height,p.transform.position.y);if(elapsed>1.4f){lines.Add("Landed="+p.Grounded+"; peakY="+height);lines.Add("Frames="+(Time.frameCount-firstFrame));lines.Add("Clips="+string.Join(", ",clips));lines.Add("Triangles="+p.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(r=>r.sharedMesh.triangles.Length/3));File.WriteAllLines("Maintenance/Miner/action-verification.txt",lines);Debug.Log(string.Join("\n",lines));EditorApplication.update-=Tick;}}
  }
 }
}




