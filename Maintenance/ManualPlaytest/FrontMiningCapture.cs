using System;
using System.IO;
using System.Collections;
using UnityEngine;
using Bulldozer.Player;
public static class FrontMiningCaptureSetup {
 public static string Main(){
  if(!Application.isPlaying || !UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath", "").Contains("ManualPlaytest"))throw new Exception("Isolated play session required");
  var go=new GameObject("Temporary front mining recording");go.AddComponent<FrontMiningCapture>();return "Recording normal input and mining at 20 frames per game second";
 }
}
public class FrontMiningCapture:MonoBehaviour {
 int previous; MinerController p;
 IEnumerator Start(){
  p=FindFirstObjectByType<MinerController>();previous=Time.captureFramerate;Time.captureFramerate=20;
  Directory.CreateDirectory("Maintenance/ManualPlaytest/front-frames");
  for(int i=0;i<160;i++){
   Vector3 world=i<9?Vector3.forward:Vector3.zero;
   var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*world;p.touchControls.Move=new Vector2(local.x,local.z);
   yield return new WaitForEndOfFrame();
   var tex=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("Maintenance/ManualPlaytest/front-frames/frame-"+i.ToString("D4")+".png",tex.EncodeToPNG());Destroy(tex);
  }
  File.WriteAllText("Maintenance/ManualPlaytest/front-recording.txt","Complete: 160 frames, 20fps; normal movement + automatic mining; round="+FindFirstObjectByType<MinerEquipment>().RoundIndex);
  Destroy(gameObject);
 }
 void OnDestroy(){Time.captureFramerate=previous;if(p&&p.touchControls)p.touchControls.Move=Vector2.zero;}
}
