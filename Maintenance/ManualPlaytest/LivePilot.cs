using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Bulldozer.Player;
public static class LivePilotSetup {
 public static string Main(){if(!Application.isPlaying)throw new Exception("Play mode required");if(!UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","").Contains("ManualPlaytest"))throw new Exception("Isolated save required");Application.runInBackground=true;var old=GameObject.Find("Live input pilot");if(old)UnityEngine.Object.Destroy(old);var go=new GameObject("Live input pilot");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<LiveInputPilot>();return "Input-only live pilot ready";}
}
public class LiveInputPilot:MonoBehaviour {
 bool busy; int shot; void Awake(){shot=Directory.GetFiles("Maintenance/ManualPlaytest","view-*.png").Length;}
 public void Act(string command){if(busy)throw new Exception("Previous input still active");StartCoroutine(Batch(command));}
 IEnumerator Batch(string command){busy=true;foreach(var step in command.Split(';'))yield return Perform(step);busy=false;}
 IEnumerator Perform(string command){busy=true;var args=command.Split('|');var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();
  File.AppendAllText("Maintenance/ManualPlaytest/actions.txt",DateTime.Now.ToString("s")+" "+command+"\n");
  if(args[0]=="move"){
   if(!p)throw new Exception("Player missing");var world=new Vector3(float.Parse(args[1],System.Globalization.CultureInfo.InvariantCulture),0,float.Parse(args[2],System.Globalization.CultureInfo.InvariantCulture));float duration=float.Parse(args[3],System.Globalization.CultureInfo.InvariantCulture);float end=Time.time+duration;
   while(Time.time<end){var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*world;p.touchControls.Move=new Vector2(local.x,local.z);yield return null;}p.touchControls.Move=Vector2.zero;
  }else if(args[0]=="click"){
   var buttons=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name==args[1]&&b.gameObject.activeInHierarchy&&b.interactable).ToArray();if(buttons.Length!=1)throw new Exception("Expected one active button "+args[1]+" got "+buttons.Length);
   ExecuteEvents.Execute(buttons[0].gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
  }else if(args[0]!="wait")throw new Exception("Unknown input");
  yield return new WaitForSecondsRealtime(args[0]=="wait"?float.Parse(args[1],System.Globalization.CultureInfo.InvariantCulture):.7f);
  yield return new WaitForEndOfFrame();shot++;ScreenCapture.CaptureScreenshot("Maintenance/ManualPlaytest/view-"+shot.ToString("D3")+".png");
  p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var prod=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
  string summary="shot="+shot+" frame="+Time.frameCount+" time="+Time.time;
  if(p)summary+=" position="+p.transform.position.ToString("F2")+" facing="+p.visual.forward.ToString("F2")+" blocked="+p.InputBlocked;
  if(e)summary+=" round="+e.RoundIndex+" cargo="+e.Snapshot.items.Length+" cash="+e.Cash+" depleted="+e.Snapshot.depletedBlocks.Length+" raw="+e.RefineryInputCount+" output="+e.RefineryOutputCount+" queue="+e.Snapshot.loadingQueue.Length+" trucks="+e.Snapshot.trucksCompleted;
  File.WriteAllText("Maintenance/ManualPlaytest/latest.txt",summary);File.AppendAllText("Maintenance/ManualPlaytest/actions.txt",summary+"\n");busy=false;
 }
 void OnDestroy(){var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();if(p&&p.touchControls)p.touchControls.Move=Vector2.zero;}
}
