using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player.Editor
{
    public static class ProductionFlowVerification
    {
        static readonly List<string> checks=new List<string>();
        static MiningProduction p;static MinerEquipment e;static float began;static int phase,startFrame,maxBatches,maxSetPass;
        static void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);}
        public static string Begin()
        {
            if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
            checks.Clear();p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();p.enabled=false;
            var s=e.Snapshot;s.items=new[]{0,1};s.coins=0;s.refineryInput=Array.Empty<int>();s.refineryOutput=Array.Empty<int>();s.loadingQueue=Array.Empty<int>();s.truckItems=Array.Empty<int>();s.trucksCompleted=0;e.Commit(s);
            Check(p.DepositRaw()&&p.DepositRaw()&&e.CargoCount==0&&e.RefineryInputCount==2,"Raw stone moves onto persistent input buffer");
            Check(p.ProcessOne()&&p.ProcessOne()&&e.Snapshot.refineryOutput.SequenceEqual(new[]{6,7}),"Refinery preserves resource identities");
            Check(p.CollectOutput()&&p.CollectOutput()&&e.CargoCount==2,"Finished stone can be collected");
            Check(p.LoadTruck()&&p.LoadTruck()&&e.CargoCount==0&&e.Cash==0&&e.Snapshot.loadingQueue.Length==2,"Loading pad accepts goods without paying before truck transfer");
            Check(new MiningProgressStore(e.SavePath).Current.loadingQueue.SequenceEqual(new[]{6,7}),"Loading pad survives disk reload");
            Check(p.TransferToTruck()&&e.Cash==0&&e.Snapshot.truckItems.Length==1,"Partial truck load has no payout");
            Check(p.TransferToTruck()&&p.delivery.IsBusy&&e.Cash==25,"First full truck pays exactly $25");
            s=e.Snapshot;s.items=new[]{8,9,10};e.Commit(s);p.LoadTruck();p.LoadTruck();p.LoadTruck();
            Check(p.TransferToTruck()&&p.TransferToTruck()&&p.TransferToTruck()&&p.secondDelivery.IsBusy&&e.Cash==100,"Second truck loads and pays while first is delivering");
            s=e.Snapshot;s.items=new[]{6,6,6,6};e.Commit(s);
            Check(p.LoadTruck()&&p.LoadTruck()&&p.LoadTruck()&&p.LoadTruck()&&e.CargoCount==0,"Player can unload while both trucks are away");
            Check(!p.TransferToTruck()&&e.Cash==100&&e.Snapshot.loadingQueue.Length==4,"Both busy vehicles leave buffered goods intact");
            foreach(var t in new[]{p.delivery,p.secondDelivery})Check(t.body.sharedMesh.triangles.Length/3<5000&&t.body.sharedMaterials.Length==1&&t.GetComponentsInChildren<SkinnedMeshRenderer>().Length==1&&t.wheels.Length==6,t.name+" retains <5000 triangles, one body renderer/material and six wheel bones");
            startFrame=Time.frameCount;began=Time.time;phase=0;maxBatches=maxSetPass=0;EditorApplication.update-=Tick;EditorApplication.update+=Tick;return string.Join("\n",checks)+"\nTimed return and refinery tests running";
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||!p){EditorApplication.update-=Tick;return;}
            maxBatches=Math.Max(maxBatches,UnityStats.batches);maxSetPass=Math.Max(maxSetPass,UnityStats.setPassCalls);
            if(Time.time-began>20){Check(false,"Timed test completed within 20 seconds");Finish();return;}
            if(phase==0&&!p.delivery.IsBusy&&!p.secondDelivery.IsBusy)
            {
                Check(Time.frameCount>startFrame+10,"Player loop advances during delivery test");
                Check(p.delivery.DistanceTravelled>60&&p.secondDelivery.DistanceTravelled>60,"Both reusable trucks depart and return");
                Check(p.TransferToTruck()&&p.TransferToTruck()&&p.TransferToTruck()&&p.TransferToTruck()&&e.Cash==170,"Third delivery pays $40 plus $30 milestone bonus");
                Check(!p.TransferToTruck()&&e.Cash==170,"Empty buffer cannot duplicate reward");
                var disk=new MiningProgressStore(e.SavePath).Current;Check(disk.coins==170&&disk.trucksCompleted==3&&disk.loadingQueue.Length==0,"Cash and milestone persist atomically");
                var s=e.Snapshot;s.refineryInput=new[]{2};s.refineryOutput=Array.Empty<int>();e.Commit(s);p.enabled=true;began=Time.time;phase=1;
            }
            else if(phase==1&&Time.time-began>1.1f)
            {
                Check(p.refineryTimer&&p.refineryTimer.text.Contains("s")&&e.RefineryOutputCount==0,"Machine displays countdown before production completes");phase=2;
            }
            else if(phase==2&&Time.time-began>3.5f)
            {
                Check(e.RefineryInputCount==0&&e.Snapshot.refineryOutput.SequenceEqual(new[]{8}),"Timed refinery produces exactly one correct stone");
                Check(p.effects.ActiveFlights<=12,"Transfers stay within fixed 12-item pool");Finish();
            }
        }
        static void Finish()
        {
            EditorApplication.update-=Tick;
            checks.Add("EDITOR ONLY: max batches="+maxBatches+", max SetPass="+maxSetPass+"; no device FPS claim.");
            File.WriteAllLines("Maintenance/Miner/production-flow-verification.txt",checks);Debug.Log(string.Join("\n",checks));
        }
    }
}
