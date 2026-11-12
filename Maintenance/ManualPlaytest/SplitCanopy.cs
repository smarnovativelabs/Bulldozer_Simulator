using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
public static class SplitCanopy {
 const string Folder="Assets/0_ProjectData/Player/Environment/";
 static Mesh Subset(Mesh source,List<int> triangles,string name){
  var map=new Dictionary<int,int>();var order=new List<int>();var remapped=new List<int>();
  foreach(int old in triangles){if(!map.TryGetValue(old,out int value)){value=order.Count;map.Add(old,value);order.Add(old);}remapped.Add(value);}
  var mesh=new Mesh{name=name,indexFormat=source.indexFormat};var v=source.vertices;mesh.vertices=order.Select(i=>v[i]).ToArray();
  var n=source.normals;if(n.Length==v.Length)mesh.normals=order.Select(i=>n[i]).ToArray();
  var uv=source.uv;if(uv.Length==v.Length)mesh.uv=order.Select(i=>uv[i]).ToArray();
  var uv2=source.uv2;if(uv2.Length==v.Length)mesh.uv2=order.Select(i=>uv2[i]).ToArray();
  var c=source.colors32;if(c.Length==v.Length)mesh.colors32=order.Select(i=>c[i]).ToArray();
  var t=source.tangents;if(t.Length==v.Length)mesh.tangents=order.Select(i=>t[i]).ToArray();
  mesh.SetTriangles(remapped,0);mesh.RecalculateBounds();return mesh;
 }
 public static object Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");
  var existing=GameObject.Find("Workbench canopy");if(existing){UnityEngine.Object.FindFirstObjectByType<MinerCamera>().overheadOccluder=existing.GetComponent<Renderer>();EditorSceneManager.MarkSceneDirty(existing.scene);EditorSceneManager.SaveScene(existing.scene);return "Existing canopy wired";}
  var filter=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>AssetDatabase.GetAssetPath(f.sharedMesh)==Folder+"Dark Metal.asset");
  var source=filter.sharedMesh;if(source.subMeshCount!=1)throw new Exception("Unexpected submesh layout");var vertices=source.vertices;var indices=source.triangles;
  var inverse=Matrix4x4.TRS(new Vector3(-4.5f,2.55f,-1.65f),Quaternion.Euler(6,0,0),Vector3.one).inverse;
  Func<int,bool> inside=i=>{var p=inverse.MultiplyPoint3x4(filter.transform.TransformPoint(vertices[i]));return Mathf.Abs(p.x)<1.526f&&Mathf.Abs(p.y)<.061f&&Mathf.Abs(p.z)<1.401f;};
  var canopy=new List<int>();var rest=new List<int>();for(int i=0;i<indices.Length;i+=3){var target=inside(indices[i])&&inside(indices[i+1])&&inside(indices[i+2])?canopy:rest;target.Add(indices[i]);target.Add(indices[i+1]);target.Add(indices[i+2]);}
  if(canopy.Count!=36)throw new Exception("Expected 12 canopy triangles, got "+canopy.Count/3);
  var roofMesh=Subset(source,canopy,"Workbench canopy");var remainder=Subset(source,rest,"Environment metal without canopy");
  AssetDatabase.CreateAsset(roofMesh,Folder+"WorkbenchCanopy.asset");AssetDatabase.CreateAsset(remainder,Folder+"MetalWithoutCanopy.asset");
  filter.sharedMesh=remainder;PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
  var go=new GameObject("Workbench canopy",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(filter.transform.parent,false);go.transform.localPosition=filter.transform.localPosition;go.transform.localRotation=filter.transform.localRotation;go.transform.localScale=filter.transform.localScale;go.GetComponent<MeshFilter>().sharedMesh=roofMesh;
  var original=filter.GetComponent<Renderer>();var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=original.sharedMaterial;renderer.shadowCastingMode=original.shadowCastingMode;renderer.receiveShadows=original.receiveShadows;renderer.motionVectorGenerationMode=original.motionVectorGenerationMode;
  GameObjectUtility.SetStaticEditorFlags(go,GameObjectUtility.GetStaticEditorFlags(filter.gameObject));
  var camera=UnityEngine.Object.FindFirstObjectByType<MinerCamera>();camera.overheadOccluder=renderer;EditorUtility.SetDirty(camera);
  EditorSceneManager.MarkSceneDirty(go.scene);EditorSceneManager.SaveScene(go.scene);AssetDatabase.SaveAssets();
  return new{canopyTriangles=canopy.Count/3,totalBefore=indices.Length/3,totalAfter=(rest.Count+canopy.Count)/3,extraRenderers=1,extraMaterials=0};
 }
}
