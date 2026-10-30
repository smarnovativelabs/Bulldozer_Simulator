using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Bulldozer.Player;
public static class CaptureRidge {
public static string Main(){
var area=UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>();
var meshes=area.GetComponentsInChildren<MeshFilter>(true);var renderers=area.GetComponentsInChildren<Renderer>(true);
var report="Added area: "+meshes.Sum(m=>m.sharedMesh?m.sharedMesh.triangles.Length/3:0)+" mesh triangles, "+renderers.Length+" renderers, "+renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count()+" shared materials, "+area.GetComponentsInChildren<Light>(true).Length+" lights, "+area.GetComponentsInChildren<ParticleSystem>(true).Length+" particle systems. Editor asset counts; not device profiling.";
File.WriteAllText("Maintenance/Sprint1Release/progression-budget.txt",report);
var go=new GameObject("Temporary ridge review camera");var cam=go.AddComponent<Camera>();cam.transform.position=new Vector3(20,20,-10);cam.transform.LookAt(new Vector3(19,0,5));cam.orthographic=true;cam.orthographicSize=12;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.18f,.2f,.22f);
var rt=new RenderTexture(1200,900,24);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1200,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,900),0,0);tex.Apply();File.WriteAllBytes("Maintenance/Sprint1Release/copper-ridge.png",tex.EncodeToPNG());RenderTexture.active=old;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);return report;
}
}
