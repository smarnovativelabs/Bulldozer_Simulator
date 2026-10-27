using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player.Persistence;

namespace Bulldozer.Player.Editor
{
    public static class TruckDeliveryVerification
    {
        static readonly List<string> results=new List<string>();
        static MiningProduction production; static TruckDelivery truck; static MinerEquipment equipment;
        static Vector3 home; static Quaternion wheel; static float began;
        static int instance, cycles, samples, batchTotal, maxBatches, maxSetPass;
        static TruckDelivery.DeliveryPhase last;
        static void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);}
        public static string Start()
        {
            if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play session required");
            EditorApplication.update-=Tick;results.Clear();cycles=samples=batchTotal=maxBatches=maxSetPass=0;
            production=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();truck=production.delivery;equipment=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();
            if(truck.IsBusy)throw new Exception("Truck must be parked");
            home=truck.transform.position;wheel=truck.wheels[0].rotation;instance=truck.GetInstanceID();
            var state=equipment.Snapshot;state.items=new[]{6,7,8,9,10};state.truckItems=Array.Empty<int>();state.trucksCompleted=0;state.coins=0;state.refineryInput=Array.Empty<int>();state.refineryOutput=Array.Empty<int>();equipment.Commit(state);
            Check(production.LoadTruck()&&!truck.IsBusy&&equipment.Coins==0,"Partial load stays parked without payment");
            Check(production.LoadTruck()&&truck.IsBusy&&equipment.Coins==25,"Full load begins delivery and pays exactly once");
            Check(!production.LoadTruck()&&equipment.CargoCount==3,"Loading delay rejects additional cargo");
            Check(truck.ShowDepartingCargo&&production.truckStack.Cast<Transform>().Count(t=>t.gameObject.activeSelf)==2,"Full cargo remains visible after saved inventory is cleared");
            Check(truck.body.sharedMesh.triangles.Length/3==4400&&truck.body.sharedMaterials.Length==1&&truck.body.bones.Length==7,"Truck retains 4400 triangles, one material and seven rigid bones");
            began=Time.time;last=truck.Phase;EditorApplication.update+=Tick;return "Delivery verification running through two full cycles";
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||!truck){EditorApplication.update-=Tick;return;}
            if(Time.time-began>40){Check(false,"Delivery completed before timeout");Finish();return;}
            samples++;batchTotal+=UnityStats.batches;maxBatches=Math.Max(maxBatches,UnityStats.batches);maxSetPass=Math.Max(maxSetPass,UnityStats.setPassCalls);
            if(truck.Phase==last)return;
            last=truck.Phase;
            if(last==TruckDelivery.DeliveryPhase.Departing)Check(!truck.parkingCollider.enabled,"Moving truck disables parking collision");
            if(last==TruckDelivery.DeliveryPhase.Away)
            {
                Check(!truck.body.enabled&&!truck.cargo.gameObject.activeSelf,"Off-screen truck and delivered cargo are hidden before recycling");
                Check(Vector3.Distance(home,truck.transform.position)>31,"Truck drives away beyond the loading area");
                Check(Quaternion.Angle(wheel,truck.wheels[0].rotation)>.1f,"Wheel bones rotate during travel");
                Check(Mathf.Abs(truck.WheelDegrees-truck.DistanceTravelled/truck.wheelRadius*Mathf.Rad2Deg)<.2f,"Wheel rotation follows travelled distance and radius");
                Check(!production.LoadTruck(),"Absent truck rejects loading");
            }
            if(last==TruckDelivery.DeliveryPhase.Arriving)Check(!truck.cargo.gameObject.activeSelf&&!production.LoadTruck(),"Arriving truck is empty and cannot load before parking");
            if(last==TruckDelivery.DeliveryPhase.Parked)
            {
                Check(Vector3.Distance(home,truck.transform.position)<.001f&&truck.parkingCollider.enabled,"Replacement parks exactly at the loading bay");
                Check(truck.GetInstanceID()==instance&&production.truckStack.childCount==8,"Truck and eight cargo slots are reused across deliveries");
                Check(equipment.Coins==(cycles==0?25:100),"Animation never changes the committed payment");
                cycles++;
                if(cycles==1){Check(production.LoadTruck()&&production.LoadTruck()&&production.LoadTruck()&&truck.IsBusy,"Replacement accepts its larger next order");last=truck.Phase;}
                else
                {
                    var disk=new MiningProgressStore(equipment.SavePath).Current;
                    Check(disk.coins==100&&disk.trucksCompleted==2&&disk.truckItems.Length==0,"Disk reload retains both payments with no delivered cargo");
                    Finish();
                }
            }
        }
        static void Finish()
        {
            EditorApplication.update-=Tick;
            results.Add("EDITOR OBSERVATION: average batches="+(samples==0?0:batchTotal/samples)+", max batches="+maxBatches+", max SetPass="+maxSetPass+". Not a device performance measurement.");
            File.WriteAllLines("Maintenance/ArtPolish/truck-delivery-verification.txt",results);Debug.Log(string.Join("\n",results));
        }
    }
}
