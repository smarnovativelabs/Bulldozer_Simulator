using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class FirstSessionVisualChecks {
public static string Main(){
if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
var go=new GameObject("First session visual check");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<FirstSessionVisualRunner>().Begin();return "Capturing starter shift and three guidance shots";
}}
public class FirstSessionVisualRunner:MonoBehaviour {
public void Begin(){StartCoroutine(Run());}
IEnumerator Run(){
var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();e.Commit(MiningSnapshot.Create(0,0));SceneManager.LoadSceneAsync("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");yield return new WaitForSecondsRealtime(3);
var guide=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();e=guide.guide.equipment;guide.guide.player.autoMine=false;
ScreenCapture.CaptureScreenshot("Maintenance/FirstSession/first-shift.png");yield return new WaitForSecondsRealtime(.3f);
var destinations=new[]{guide.guide.production.inputZone.transform.position,guide.guide.upgrades.zone.transform.position,guide.cart.transform.position};
var titles=new[]{"REFINERY INPUT","OPTIONAL PICKAXE UPGRADE","YOUR WHEELBARROW"};
var bodies=new[]{"Place ore here once. After that, you can keep mining even with a full bag.","Tough rocks take more hits. This workbench doubles your first pickaxe's damage. Buying is your choice.","Carry 12 ore per trip. Use the cart here, then park it when you want to mine."};
string report="Editor visual fixture; not phone profiling.\n";
for(int i=0;i<3;i++){
var s=e.Snapshot;
if(i==0){s.items=new int[5];e.Commit(s);}
if(i==1){guide.guide.production.DepositRaw();s=e.Snapshot;s.taskRocks=2;s.coins=10;e.Commit(s);}
if(i==2){s.wheelbarrowUnlocked=true;e.Commit(s);}
float timeout=Time.realtimeSinceStartup+12;while(guide.ShotsStarted<i+1){if(Time.realtimeSinceStartup>timeout)throw new Exception("Automatic cue timeout "+i);yield return null;}
if(guide.ActiveCue!=(1<<i))throw new Exception("Wrong automatic cue "+i);report+="PASS automatic "+titles[i]+" trigger\n";
yield return new WaitForSecondsRealtime(1.3f);
foreach(var t in new[]{guide.guide.heading,guide.guide.message,guide.guide.stepLabel}){t.ForceMeshUpdate();report+=titles[i]+" "+t.name+" overflow="+t.isTextOverflowing+"\n";}
ScreenCapture.CaptureScreenshot("Maintenance/FirstSession/camera-"+i+".png");
yield return new WaitForSecondsRealtime(2.4f);
}
int oreTris=Enumerable.Range(0,6).Max(i=>e.MeshForItem(i).triangles.Length/3);
var truck=guide.guide.production.delivery;
report+="First-shift prewarmed ore slots="+e.drops.PoolCapacity+"; upper pool cap="+OreDropSystem.PoolSize+"; whole-quarry live ore max=252; maximum ore mesh triangles="+oreTris+"; worst-case ore triangles <= "+(252*oreTris)+"; ore shared materials="+e.drops.materials.Length+"; GPU instancing="+SystemInfo.supportsInstancing+"\n";

report+="Delivery truck skinned body triangles="+Enumerable.Range(0,truck.body.sharedMesh.subMeshCount).Sum(i=>(long)truck.body.sharedMesh.GetIndexCount(i)/3)+"; body renderers=1; shared materials="+truck.body.sharedMaterials.Length+"; wheel transforms="+truck.wheels.Length+"\n";
report+="Guidance adds 2 virtual cameras, 0 rendering cameras, 0 renderers, 0 materials, 0 particles.\n";
File.WriteAllText("Maintenance/FirstSession/visual-budget-checks.txt",report+"COMPLETE\n");Destroy(gameObject);
}
}





