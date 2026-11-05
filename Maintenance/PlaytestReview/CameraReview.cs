using System;
using System.IO;
using System.Collections;
using UnityEngine;
using Bulldozer.Player;
public static class CameraReview {
 public static string Main(){var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();if(!e.SavePath.Contains("PlaytestReview"))throw new Exception("Isolated save required");new GameObject("Camera review").AddComponent<CameraReviewRunner>().Begin();return "Isolated workbench cue check started";}
}
public class CameraReviewRunner:MonoBehaviour {
 public void Begin(){StartCoroutine(Run());}
 IEnumerator Run(){
 var g=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();var e=g.guide.equipment;g.Finish();
 var s=e.Snapshot;bool alreadySeen=(s.guidanceSeenMask&2)!=0;s.guidanceSeenMask&=~2;e.Commit(s);g.enabled=true;
 yield return new WaitForSeconds(.5f);
 bool started=g.TryShow(2,g.guide.upgrades.zone.transform.position,"WORKBENCH TEST","Optional upgrade");
 float deadline=Time.realtimeSinceStartup+8;while(g.Playing&&Time.realtimeSinceStartup<deadline)yield return null;
 bool released=!g.Playing&&!g.guide.player.GuidanceInputBlocked;bool repeat=g.TryShow(2,g.guide.upgrades.zone.transform.position,"REPEAT","REPEAT");
 File.WriteAllText("Maintenance/PlaytestReview/camera-check.txt","Previously seen="+alreadySeen+"\nStarted="+started+"\nControls released="+released+"\nRepeat suppressed="+(!repeat)+"\nSaved flag="+((e.Snapshot.guidanceSeenMask&2)!=0));Destroy(gameObject);
 }
}
