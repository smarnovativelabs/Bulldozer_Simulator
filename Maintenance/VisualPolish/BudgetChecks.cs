using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Bulldozer.Player;
public static class PolishBudgets {
 public static object Main(){var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
 var bodies=new[]{p.delivery.body,p.secondDelivery.body}; if(bodies.Any(b=>!b||b.sharedMesh.GetIndexCount(0)/3>=5000||b.sharedMaterials.Length!=1))throw new Exception("Truck renderer budget changed");
 var active=cart.GetComponentsInChildren<MeshFilter>(true).Where(f=>!f.name.StartsWith("Caddy cargo")&&f.sharedMesh).ToArray();
 var report=new{rawTrianglesPerItem=(int)e.resourceMeshes[0].GetIndexCount(0)/3,maxLooseOreTriangles=252*(int)e.resourceMeshes[0].GetIndexCount(0)/3,oreInstancedSubmissions=6,sharedItemMaterials=e.resourceMaterials.Distinct().Count(),refinedTrianglesPerItem=(int)e.resourceMeshes[6].GetIndexCount(0)/3,fractureTriangles=RockFractureEffects.Capacity*(int)e.drops.fracture.sourceMesh.GetIndexCount(0)/3,fractureRendererCount=1,cartTriangles=active.Sum(f=>(int)f.sharedMesh.GetIndexCount(0)/3),cartRenderers=active.Length,cartMaterials=active.Select(f=>f.GetComponent<Renderer>().sharedMaterial).Distinct().Count(),truckTriangles=4400,truckBodyRenderersPerVehicle=1,truckMaterialPerBody=1,transferPool=12,smokeMax=p.effects.smoke.Sum(s=>s.main.maxParticles),deviceProfiled=false};
 File.WriteAllText("Maintenance/VisualPolish/budgets.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}

