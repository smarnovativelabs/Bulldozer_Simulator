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
public static class PluginChecks {
 public static string Main(){if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play required");var go=new GameObject("Plugin polish checks");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<PluginCheckRunner>().Begin();return "Plugin checks running";}
}
public class PluginCheckRunner:MonoBehaviour {
 List<string> log=new List<string>();QuarryPluginPolish fx;MinerController p;MinerEquipment e;MiningProduction production;
 void Flush(){using(var f=new FileStream("Maintenance/PluginPolish/checks.txt",FileMode.Create,FileAccess.Write,FileShare.ReadWrite))using(var w=new StreamWriter(f))foreach(var x in log)w.WriteLine(x);}
 void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);Flush();if(!ok)throw new Exception(name);}
 void Bind(){fx=UnityEngine.Object.FindFirstObjectByType<QuarryPluginPolish>();p=fx.guide.player;e=fx.guide.equipment;production=fx.guide.production;p.autoMine=false;production.enabled=false;UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>().enabled=false;}
 public void Begin(){StartCoroutine(Guard(Run()));}
 IEnumerator Guard(IEnumerator r){while(true){bool more;object current;try{more=r.MoveNext();current=more?r.Current:null;}catch(Exception x){log.Add("FAILED "+x);Flush();yield break;}if(!more)break;yield return current;}log.Add("COMPLETE "+log.Count+" checks");Flush();Destroy(gameObject);}
 IEnumerator Run(){
 Bind();int frame=Time.frameCount;yield return new WaitForSeconds(.3f);Check(Time.frameCount>frame,"Live frames advance");
 Check(fx.shinyShader&&fx.shinyShader.isSupported,"UI Shiny shader available and supported");Check(fx.RewardSweeps==0&&fx.CartSweeps==0,"Restored profile does not trigger reward effects");
 Check(fx.indicatorPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).All(g=>!g.raycastTarget)&&!fx.indicatorPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>(),"Objective overlay never intercepts input");
 var state=e.Snapshot;state.items=new int[5];Check(e.Commit(state),"Seed full first bag");fx.guide.RefreshObjective();p.followCamera.enabled=false;
 var cam=p.followCamera.GetComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(0,3,-4),Quaternion.Euler(25,180,0));yield return new WaitForSeconds(.3f);
 Check(fx.IndicatorVisible&&fx.caption.text.StartsWith("REFINERY"),"Off-screen first-deposit objective gets labelled direction arrow");
 Vector3 screen=RectTransformUtility.WorldToScreenPoint(null,fx.marker.position);Check(float.IsFinite(screen.x)&&screen.x>=Screen.safeArea.xMin&&screen.x<=Screen.safeArea.xMax&&screen.y>=Screen.safeArea.yMin&&screen.y<=Screen.safeArea.yMax,"Arrow remains finite and inside safe area");
 yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Maintenance/PluginPolish/offscreen-objective.png");
 cam.transform.SetPositionAndRotation(new Vector3(3,5,-5),Quaternion.LookRotation(fx.guide.Destination-new Vector3(3,5,-5)));yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();Check(!fx.IndicatorVisible,"On-screen objective uses existing ground guidance without duplicate marker");
 cam.transform.rotation=Quaternion.Euler(25,180,0);p.ExcavationInputBlocked=true;yield return null;yield return null;Check(!fx.IndicatorVisible,"Blocking modal/camera input hides indicator");
 state=e.Snapshot;state.coins+=25;state.wheelbarrowUnlocked=true;Check(e.Commit(state),"Seed earned reward and cart unlock while blocked");yield return new WaitForSeconds(.25f);yield return new WaitForEndOfFrame();Check(fx.RewardSweeps==0&&fx.CartSweeps==0,"Effects defer while input is blocked");
 p.ExcavationInputBlocked=false;yield return new WaitForSeconds(.25f);yield return new WaitForEndOfFrame();Check(fx.RewardSweeps==1&&fx.CartSweeps==1,"Reward and cart unlock each trigger one sweep after control returns");
 var shiny=fx.rewardShine.GetComponent<UnityEngine.UI.Image>().materialForRendering;Check(shiny&&shiny.shader==fx.shinyShader&&!shiny.shader.name.Contains("Error"),"Reward image renders with the intended UI Shiny shader");
 yield return new WaitForEndOfFrame();Check(fx.rewardShine.GetComponent<UnityEngine.UI.Image>().canvasRenderer.GetMesh().uv.All(v=>v.x>=0&&v.x<=1&&v.y>=0&&v.y<=1),"Shine preserves original sprite UV range without packed atlas coordinates");ScreenCapture.CaptureScreenshot("Maintenance/PluginPolish/reward-shine.png");
 string baseline=JsonUtility.ToJson(e.Snapshot);yield return new WaitForSeconds(1.1f);Check(!fx.rewardShine.enabled&&!fx.cartShine.enabled,"Shine components disable after the short sweep");Check(JsonUtility.ToJson(e.Snapshot)==baseline,"Plugin effects leave progress and inventory unchanged");Check(JsonUtility.ToJson(new MiningProgressStore(e.SavePath).Current)==baseline,"Saved progress matches runtime after effects");
 Check(fx.rewardShine.GetComponent<UnityEngine.UI.Image>().materialForRendering.shader.name=="UI/Default","Idle image restores original UI shader/batching");
 int sweeps=fx.RewardSweeps;yield return new WaitForSeconds(3.1f);Check(fx.RewardSweeps==sweeps&&fx.CartSweeps==1,"Effects do not loop while idle");
 state=e.Snapshot;state.coins-=5;Check(e.Commit(state),"Seed spending transaction");yield return new WaitForSeconds(.3f);Check(fx.RewardSweeps==sweeps,"Spending does not replay earned-cash shine");
 fx.enabled=false;yield return null;Check(!fx.IndicatorVisible&&!fx.rewardShine.enabled&&!fx.cartShine.enabled,"Disabling controller cleans up presentation");fx.enabled=true;
 int id=e.GetInstanceID();SceneManager.LoadSceneAsync(p.gameObject.scene.path);float deadline=Time.realtimeSinceStartup+30;while(!UnityEngine.Object.FindFirstObjectByType<MinerEquipment>()||UnityEngine.Object.FindFirstObjectByType<MinerEquipment>().GetInstanceID()==id){if(Time.realtimeSinceStartup>deadline)throw new Exception("Reload timeout");yield return null;}Bind();yield return new WaitForSeconds(.6f);
 Check(fx.RewardSweeps==0&&fx.CartSweeps==0&&e.WheelbarrowUnlocked,"Reload keeps earned cart without replaying celebration");Check(e.Cash==20&&e.CargoCount==5,"Reload retains exact cash and cargo");
 Check(UnityEngine.Object.FindObjectsByType<QuarryPluginPolish>(FindObjectsSortMode.None).Length==1&&fx.indicatorPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Length==3,"Reload retains one controller and only three overlay graphics");
 Check(!p.InputBlocked,"Plugin never owns or leaves an input lock");
 }
}


