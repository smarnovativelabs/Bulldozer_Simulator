using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player.Editor
{
    public static class QuarryGuidanceVerification
    {
        public static string Run()
        {
            if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
            var report=new List<string>();Action<bool,string> check=(ok,text)=>report.Add((ok?"PASS ":"FAIL ")+text);
            var g=UnityEngine.Object.FindFirstObjectByType<QuarryGuidance>();var p=g.player;var e=g.equipment;g.production.enabled=false;p.autoMine=false;
            var state=e.Snapshot;state.items=Array.Empty<int>();state.refineryInput=Array.Empty<int>();state.refineryOutput=Array.Empty<int>();state.loadingQueue=Array.Empty<int>();state.truckItems=Array.Empty<int>();state.groundDrops=Array.Empty<GroundOreState>();state.coins=0;state.damageLevel=0;state.trucksCompleted=0;e.Commit(state);g.RefreshObjective();
            check(g.CurrentGoal==QuarryGuidance.Goal.Mine&&g.HasRoute,"Fresh player gets a complete route to a reachable rock");
            state=e.Snapshot;state.groundDrops=new[]{new GroundOreState{id="guide-test",item=0,position=new Vector3(0,.13f,1)}};e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.PickUp&&g.HasRoute,"Loose ore is prioritized before refinery delivery");
            state=e.Snapshot;state.groundDrops=Array.Empty<GroundOreState>();state.items=new[]{0,1};e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Refine&&g.HasRoute,"Raw cargo points to the refinery input");
            state=e.Snapshot;state.items=new[]{6};e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Load&&g.HasRoute,"Refined cargo points to the loading pad");
            state=e.Snapshot;state.items=Array.Empty<int>();state.refineryOutput=new[]{6};e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Collect&&g.HasRoute,"Ready output points to collection");
            state=e.Snapshot;state.refineryOutput=Array.Empty<int>();state.refineryInput=new[]{1};e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Mine,"Guide encourages mining while the refinery works");
            state=e.Snapshot;state.refineryInput=Array.Empty<int>();state.coins=25;state.trucksCompleted=1;e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Upgrade&&g.HasRoute,"First affordable pickaxe upgrade is suggested");
            state=e.Snapshot;state.damageLevel=1;e.Commit(state);g.RefreshObjective();check(g.CurrentGoal==QuarryGuidance.Goal.Mine,"Guide resumes production after the upgrade");
            string before=JsonUtility.ToJson(e.Snapshot);g.RefreshObjective();check(before==JsonUtility.ToJson(e.Snapshot),"Guidance does not mutate inventory or cash");
            check(!g.dialogGroup.blocksRaycasts&&!g.dialogGroup.interactable,"Dialog does not intercept movement input");
            check(g.pathRenderer.sharedMaterial==g.pathMaterial&&g.PathTriangles<=256,"Arrows and marker use one shared material and <=256 triangles");
            // Start east of the refinery: the route must avoid the housing, not draw through it.
            var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(7,.1f,0);cc.enabled=true;Physics.SyncTransforms();state=e.Snapshot;state.items=new[]{0};e.Commit(state);g.RefreshObjective();
            check(g.CurrentGoal==QuarryGuidance.Goal.Refine&&g.HasRoute,"Refinery input is reachable from behind the machine");
            cc.enabled=false;p.transform.position=new Vector3(0,.1f,0);cc.enabled=true;state=e.Snapshot;state.items=Array.Empty<int>();state.coins=0;state.trucksCompleted=0;state.damageLevel=0;e.Commit(state);g.RefreshObjective();
            report.Add("EDITOR ONLY: batches="+UnityStats.batches+", SetPass="+UnityStats.setPassCalls+". No device FPS measurement.");File.WriteAllLines("Maintenance/Miner/guidance-verification.txt",report);return string.Join("\n",report);
        }
    }
}
