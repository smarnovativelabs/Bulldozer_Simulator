using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class ManualRegressionChecks {
 public static object Main(){
  var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();
  if(!Application.isPlaying||!e.SavePath.Contains("ManualPlaytest"))throw new Exception("Isolated test required");
  var log=new List<string>();Action<bool,string> check=(ok,msg)=>{log.Add((ok?"PASS ":"FAIL ")+msg);File.WriteAllLines("Maintenance/ManualPlaytest/regression-checks.txt",log);if(!ok)throw new Exception(msg);};
  var g=UnityEngine.Object.FindFirstObjectByType<QuarryGuidance>();
  check(e.Snapshot.items.Length==0&&e.RefineryOutputCount>0&&e.Snapshot.groundDrops.Length>0,"Reproduced empty bag, finished output and loose drops fixture");
  g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Collect,"Ready refinery output takes priority over more loose raw stone");
  var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var c=p.followCamera;var roof=c.overheadOccluder;
  check(roof&&roof.name=="Workbench canopy","Separate canopy renderer wired");
  var method=typeof(MinerCamera).GetMethod("ShowPlayerThroughScenery",BindingFlags.Instance|BindingFlags.NonPublic);
  var b=roof.bounds;float d=c.distance;var original=c.transform.position;
  method.Invoke(c,new object[]{new Vector3(b.center.x,b.min.y-.4f,b.center.z),Vector3.up});
  check(roof.forceRenderingOff,"Canopy hides when between player and camera");
  check(c.transform.position==original&&c.distance==d,"Occlusion does not move or zoom camera");
  method.Invoke(c,new object[]{new Vector3(b.center.x,b.min.y-.4f,b.center.z),Vector3.down});
  check(!roof.forceRenderingOff,"Canopy restores when view clears");
  check(UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>("Assets/0_ProjectData/Player/Environment/WorkbenchCanopy.asset").triangles.Length==36,"Canopy asset keeps original twelve triangles (runtime static batching may combine meshes)");
  var metal=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Dark Metal");
  check(metal.GetComponent<Renderer>().sharedMaterial==roof.sharedMaterial,"Canopy reuses existing material");
  check(!UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(x=>x&&x.GetType().Name.Contains("FastIK")&&x.isActiveAndEnabled),"No active FastIK on player or other live tools");
  return string.Join("\n",log);
 }
}
