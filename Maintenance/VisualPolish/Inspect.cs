using UnityEngine;
using UnityEditor;
using Bulldozer.Player;
using System.Linq;
public static class VisualPolishInspect {
 public static object Main(){
 var cart=Object.FindFirstObjectByType<QuarryWheelbarrow>(); var p=Object.FindFirstObjectByType<MiningProduction>(); var e=Object.FindFirstObjectByType<MinerEquipment>(); var c=Object.FindFirstObjectByType<MinerController>();
 return new {playing=Application.isPlaying,cart=cart.GetComponentsInChildren<Transform>(true).Select(t=>new{name=t.name,pos=t.localPosition.ToString(),scale=t.localScale.ToString(),rot=t.localEulerAngles.ToString(),mesh=t.GetComponent<MeshFilter>()?AssetDatabase.GetAssetPath(t.GetComponent<MeshFilter>().sharedMesh):"",mat=t.GetComponent<Renderer>()?string.Join(",",t.GetComponent<Renderer>().sharedMaterials.Select(m=>AssetDatabase.GetAssetPath(m))):""}).ToArray(),
 labels=new[]{p.inputLabel,p.outputLabel,p.truckLabel,p.refineryTimer}.Select(t=>new{name=t.name,pos=t.transform.position.ToString(),rotation=t.transform.eulerAngles.ToString(),scale=t.transform.lossyScale.ToString(),font=t.fontSize,size=t.rectTransform.sizeDelta.ToString(),kind=t.GetType().Name}).ToArray(),
 pads=new[]{p.inputZone,p.outputZone,p.sellZone}.Select(z=>new{name=z.name,position=z.transform.position.ToString(),size=z.size.ToString(),center=z.center.ToString()}).ToArray(),
 stacks=new[]{p.inputStack,p.outputStack,p.loadingStack}.Select(t=>new{name=t.name,pos=t.position.ToString()}).ToArray(),meshes=e.resourceMeshes.Select(m=>new{name=m.name,path=AssetDatabase.GetAssetPath(m),bounds=m.bounds.ToString()}).ToArray(),materials=p.stockMaterials.Select(m=>AssetDatabase.GetAssetPath(m)).ToArray(),
 controller=new{pos=c.transform.position.ToString(),center=c.GetComponent<CharacterController>().center.ToString(),height=c.GetComponent<CharacterController>().height,skin=c.GetComponent<CharacterController>().skinWidth},
 floors=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.bounds.size.x>15&&r.bounds.size.z>15).Select(r=>new{name=r.name,mat=AssetDatabase.GetAssetPath(r.sharedMaterial)}).ToArray()}; }
}
