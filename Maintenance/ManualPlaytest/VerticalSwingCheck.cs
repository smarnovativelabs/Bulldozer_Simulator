using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
public static class VerticalSwingCheckSetup {
 public static string Main(){
  if(!Application.isPlaying||!UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","").Contains("ManualPlaytest"))throw new Exception("Isolated test required");
  File.Copy("Maintenance/ManualPlaytest/completed-playthrough.json","Maintenance/ManualPlaytest/vertical-progress.json",true);
  UnityEditor.SessionState.SetString("Bulldozer.Mining.TestSavePath",Path.GetFullPath("Maintenance/ManualPlaytest/vertical-progress.json"));
  var go=new GameObject("Vertical swing test recording");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<VerticalSwingCheck>();return "Animation regression and front recording started";
 }
}
public class VerticalSwingCheck:MonoBehaviour {
 int previous;MinerController p;List<string> log=new List<string>();
 IEnumerator Start(){
  Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().path);yield return new WaitForSeconds(2);
  p=FindFirstObjectByType<MinerController>();while(p.InputBlocked)yield return null;p.autoMine=false;
  var devices=UnityEngine.InputSystem.InputSystem.devices.Where(d=>d.enabled&&(d is UnityEngine.InputSystem.Keyboard||d is UnityEngine.InputSystem.Mouse)).ToArray();UnityEditor.SessionState.SetString("ManualPlaytest.InputDevices",string.Join(",",devices.Select(d=>d.deviceId)));
  foreach(var d in devices)UnityEngine.InputSystem.InputSystem.DisableDevice(d);
  var rock=FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!r.Depleted&&!r.requiresCopperRidge&&r.transform.position.z<3).OrderBy(r=>r.transform.position.x).First();
  var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=rock.transform.position+new Vector3(0,.03f,-.95f);cc.enabled=true;p.visual.rotation=Quaternion.identity;Physics.SyncTransforms();
  p.touchControls.AddLook(new Vector2(Mathf.DeltaAngle(p.followCamera.Yaw,165)/.13f,Mathf.DeltaAngle(55,p.followCamera.transform.eulerAngles.x)/.13f));
  yield return new WaitForSeconds(.3f);previous=Time.captureFramerate;Time.captureFramerate=20;
  var mining=p.visual.GetComponent<MinerMining>();var a=p.visual.GetComponent<Animator>();var driver=p.visual.GetComponent<MinerAnimator>();int hits=mining.SuccessfulHits;bool wasMining=false;int swings=0;float maxSupport=0;int supportFrames=0;Vector3 up=mining.headProbes[0].localPosition.normalized;
  Directory.CreateDirectory("Maintenance/ManualPlaytest/vertical-frames");p.autoMine=true;
  log.Add("Animation test fixture positioned beside actual rock: "+rock.blockId+" health="+rock.Health+" damage="+mining.equipment.Damage);
  for(int i=0;i<200;i++){
   yield return new WaitForEndOfFrame();
   if(p.IsMining&&!wasMining)swings++;wasMining=p.IsMining;
   if(p.IsMining&&!a.IsInTransition(0)) {maxSupport=Mathf.Max(maxSupport,Vector3.Distance(a.GetBoneTransform(HumanBodyBones.LeftHand).position,driver.toolGrip.TransformPoint(-up*.22f)));supportFrames++;}
   var tex=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("Maintenance/ManualPlaytest/vertical-frames/frame-"+i.ToString("D4")+".png",tex.EncodeToPNG());Destroy(tex);
  }
  p.autoMine=false;p.CancelMining();
  log.Add((mining.SuccessfulHits-hits>=2?"PASS ":"FAIL ")+"Repeated physical hits="+(mining.SuccessfulHits-hits)+" swings="+swings);
  log.Add((rock.Depleted?"PASS ":"FAIL ")+"Actual rock broke from animation contact");
  log.Add("Maximum retargeted support-hand error="+maxSupport+" over "+supportFrames+" frames");
  File.WriteAllLines("Maintenance/ManualPlaytest/vertical-checks.txt",log);Destroy(gameObject);
 }
 void OnDestroy(){Time.captureFramerate=previous;if(p&&p.touchControls)p.touchControls.Move=Vector2.zero;}
}
