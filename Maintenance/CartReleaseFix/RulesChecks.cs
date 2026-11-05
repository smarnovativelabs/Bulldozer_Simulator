using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class CartRulesChecks {
 public static string Main(){
  var log=new List<string>();Action<bool,string> check=(ok,s)=>{log.Add((ok?"PASS ":"FAIL ")+s);File.WriteAllLines("Maintenance/CartReleaseFix/rules-checks.txt",log);if(!ok)throw new Exception(s);};
  var rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)).ToArray();
  var s=MiningSnapshot.Create(0,0);check(s.gradualShifts,"Fresh saves opt into gradual shifts");
  int[] expected={12,24,48,84,84};for(int round=0;round<5;round++){s.roundIndex=round;s.copperRidgeUnlocked=round>=3;int count=rocks.Count(r=>r.Available(s));check(count==expected[round],"Shift "+(round+1)+" rocks="+count);check(rocks.Where(r=>r.Available(s)).Select(r=>r.resourceType).Distinct().Count()==6,"All six ore types remain available in shift "+(round+1));}
  var old=new MiningProgressStore(Path.GetFullPath("Maintenance/CartReleaseFix/user-save-backup.json"));check(old.Error==null&&!old.Current.gradualShifts,"Existing v10 save reads with original progression");check(rocks.Count(r=>r.Available(old.Current))==78,"Existing excavation 2 retains 78 rocks");
  var copy=old.Current.Copy();check(JsonUtility.ToJson(old.Current)==JsonUtility.ToJson(copy),"Clone retains every saved field");
  string temp=Path.GetFullPath("Maintenance/CartReleaseFix/roundtrip-save.json");var store=new MiningProgressStore(temp);check(store.TryCommit(copy),"Existing save commits to isolated roundtrip file");check(JsonUtility.ToJson(new MiningProgressStore(temp).Current)==JsonUtility.ToJson(copy),"Roundtrip retains cargo, cash, partial rocks and purchases");
  var source=old.Current.Copy();source.roundBonusClaimed=true;source.items=source.backpackItems=source.barrowItems=source.refineryInput=source.refineryOutput=source.loadingQueue=source.truckItems=Array.Empty<int>();source.cargo=0;source.groundDrops=Array.Empty<GroundOreState>();source.depletedBlocks=rocks.Where(r=>r.Available(source)).Select(r=>r.blockId).ToArray();check(ExcavationRules.TryAdvance(source,source.depletedBlocks,out var advanced)&&!advanced.gradualShifts,"Advancing legacy progress never opts into reduced rock sets");
  var clips=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/0_ProjectData/Player/MinerActions.controller").animationClips;
  check(new[]{"CartIdle","CartPush","CartPull"}.All(n=>clips.Any(c=>c.name==n)),"Animator references idle, push and pull clips");
  check(UnityEngine.Object.FindFirstObjectByType<MinerAnimator>().cartAnimations,"Authored cart animations enabled");
  var lane=GameObject.Find("Copper return lane");check(lane&&lane.GetComponentsInChildren<MeshFilter>().Sum(m=>(long)m.sharedMesh.GetIndexCount(0)/3)<500,"Return lane geometry below 500 triangles");
  check(UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>().sign.fontSize<=1.1f,"Copper sign compact");log.Add("COMPLETE "+log.Count+" checks");File.WriteAllLines("Maintenance/CartReleaseFix/rules-checks.txt",log);return log.Last();
 }
}
