using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
using TMPro;
public static class ApplyVisuals {
const string Folder="Assets/0_ProjectData/Player/Production/LaunchPolish/";
static List<Vector3> verts;static List<int> tris;static List<Vector2> uv;static int resource;
static void Face(Vector3 a,Vector3 b,Vector3 c,int shade){int n=verts.Count;verts.AddRange(new[]{a,b,c});tris.AddRange(new[]{n,n+1,n+2});for(int i=0;i<3;i++)uv.Add(new Vector2((shade+.5f)/8,(resource+.5f)/8));}
static Mesh MakeMesh(int id){
 resource=id%6;verts=new List<Vector3>();tris=new List<int>();uv=new List<Vector2>();
 bool raw=id<6;int sides=raw?6:8;var rings=new Vector3[3][];
 for(int r=0;r<3;r++){rings[r]=new Vector3[sides];for(int j=0;j<sides;j++){
  Vector3 v;
  if(raw){float angle=j*Mathf.PI*2/sides;float rough=1+Mathf.Sin(j*13.7f+resource*5.3f+r)*.16f;float radius=r==1?1:r==0?.72f:.57f;
   v=new Vector3(Mathf.Cos(angle)*.125f*rough*radius,new[]{-.085f,-.012f,.105f}[r]+(r==1?Mathf.Sin(j*3+resource)*.026f:0),Mathf.Sin(angle)*.155f*rough*radius);
  }else{
   // Eight-sided rectangle gives a visible chamfer, with a broad, clean top face.
   Vector2[] outline={new Vector2(-.085f,-.17f),new Vector2(.085f,-.17f),new Vector2(.12f,-.135f),new Vector2(.12f,.135f),new Vector2(.085f,.17f),new Vector2(-.085f,.17f),new Vector2(-.12f,.135f),new Vector2(-.12f,-.135f)};
   float scale=r==1?1:r==0?.91f:.80f;
   v=new Vector3(outline[j].x*scale,new[]{-.065f,-.04f,.065f}[r],outline[j].y*scale);
  }rings[r][j]=v;
 }}
 for(int r=0;r<2;r++)for(int j=0;j<sides;j++){int k=(j+1)%sides;int shade=raw?(j+r+resource)%7:4+(j%3);Face(rings[r][j],rings[r+1][j],rings[r+1][k],shade);Face(rings[r][j],rings[r+1][k],rings[r][k],shade);}
 for(int j=1;j<sides-1;j++){Face(rings[0][0],rings[0][j],rings[0][j+1],raw?0:3);Face(rings[2][0],rings[2][j+1],rings[2][j],raw?2+j%5:7);}
 var m=new Mesh{name=(raw?"Rough ore ":"Bevelled product ")+resource};m.SetVertices(verts);m.SetTriangles(tris,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();
 string path=Folder+"Item"+id+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(m,path);return m;
}
static Material Mat(string name,Color color,float metal,float smooth,Texture texture=null){string path=Folder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.mainTexture=texture;m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
static Texture2D Palette(){var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){int type=Mathf.Min(5,y/8),shade=x/8;Color ore=MiningResources.Color(type),stone=new Color(.40f,.43f,.42f);Color col=shade<2?stone*(.8f+shade*.18f):shade==2?Color.Lerp(stone,ore,.45f):Color.Lerp(ore,Color.white,(shade-3)*.1f);texture.SetPixel(x,y,col);}texture.Apply();string path=Folder+"MineralPalette.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
public static object Main(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
 var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();
 Undo.RecordObjects(new UnityEngine.Object[]{e,p,cart,e.drops},"Launch visual polish");
 var palette=Palette();var raw=Mat("Rough mineral",Color.white,.05f,.16f,palette);var product=Mat("Finished mineral",Color.white,.35f,.48f,palette);
 e.resourceMeshes=Enumerable.Range(0,12).Select(MakeMesh).ToArray();e.resourceMaterials=Enumerable.Range(0,12).Select(i=>i<6?raw:product).ToArray();
 p.stockMaterials=e.resourceMaterials;cart.stockMaterials=e.resourceMaterials;e.drops.materials=Enumerable.Repeat(raw,6).ToArray();
 EditorUtility.SetDirty(e);EditorUtility.SetDirty(p);EditorUtility.SetDirty(cart);EditorUtility.SetDirty(e.drops);
 var assembly=cart.transform.Find("Slim cart assembly");if(!assembly){assembly=new GameObject("Slim cart assembly").transform;Undo.RegisterCreatedObjectUndo(assembly.gameObject,"Slim cart assembly");assembly.SetParent(cart.transform,false);foreach(var t in cart.transform.Cast<Transform>().ToArray())if(t!=assembly&&t!=cart.counter.transform)Undo.SetTransformParent(t,assembly,"Group cart visuals");}assembly.localScale=new Vector3(.88f,1,1);
 var enamel=Mat("Caddy enamel",new Color(.95f,.59f,.13f),.15f,.3f);var rubber=Mat("Caddy rubber",new Color(.065f,.08f,.085f),0,.12f);var steel=Mat("Caddy steel",new Color(.34f,.41f,.44f),.55f,.34f);
 foreach(var r in assembly.GetComponentsInChildren<MeshRenderer>(true)){if(r.name.StartsWith("Caddy cargo"))continue;r.sharedMaterial=r.name=="Base"||r.name=="Raised side storage"?enamel:r.name=="Extended ergonomic handle"?steel:rubber;EditorUtility.SetDirty(r);}
 var body=cart.GetComponent<BoxCollider>();Undo.RecordObject(body,"Narrow cart collider");body.size=new Vector3(.748f,.8f,1.2f);EditorUtility.SetDirty(body);
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
 return new {rawTriangles=e.resourceMeshes[0].triangles.Length/3,refinedTriangles=e.resourceMeshes[6].triangles.Length/3,sharedItemMaterials=2,cartWidthScale=assembly.localScale.x};
}
public static object Counters(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play first");var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
 var labels=new[]{p.inputLabel,p.outputLabel,p.truckLabel};var zones=new[]{p.inputZone,p.outputZone,p.sellZone};
 for(int i=0;i<3;i++){var t=labels[i];Undo.RecordObjects(new UnityEngine.Object[]{t,t.rectTransform},"Place boundary counter");t.transform.position=zones[i].transform.position+new Vector3(0,.065f,-.43f);t.rectTransform.sizeDelta=new Vector2(1.48f,.60f);t.fontSize=2.35f;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.NoWrap;t.color=new Color(1,.97f,.85f);t.lineSpacing=0;EditorUtility.SetDirty(t);}
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);return "Three compact counters within boundary, away from stacks";
}
public static object Debris(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
 var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var f=e.drops.fracture;
 Undo.RecordObjects(new UnityEngine.Object[]{f,f.GetComponent<MeshRenderer>()},"Angular fracture pieces");
 f.sourceMesh=e.resourceMeshes[0];f.GetComponent<MeshRenderer>().sharedMaterial=e.resourceMaterials[0];EditorUtility.SetDirty(f);EditorUtility.SetDirty(f.GetComponent<MeshRenderer>());
 var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();Undo.RecordObject(cart.counter.transform,"Separate parked cart label");cart.counter.transform.localPosition=new Vector3(0,1.65f,.7f);
 EditorSceneManager.MarkSceneDirty(e.gameObject.scene);EditorSceneManager.SaveScene(e.gameObject.scene);return "24 pooled angular fracture pieces, 768 triangles, one draw; parked cart counter offset";
}
public static object Ground(){
 var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/0_ProjectData/Player/Production/QuarryDirt.mat");string path=Folder+"Quiet quarry ground.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(source);AssetDatabase.CreateAsset(mat,path);}
 var target=RenderTexture.GetTemporary(256,256,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var previous=RenderTexture.active;Graphics.Blit(source.mainTexture,target);RenderTexture.active=target;var texture=new Texture2D(256,256,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,256,256),0,0);RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);
 var pixels=texture.GetPixels();for(int y=0;y<256;y++)for(int x=0;x<256;x++){int at=y*256+x;Color center=pixels[at];float l=center.grayscale;pixels[at]=Color.Lerp(center,new Color(.47f,.40f,.30f),.58f);}texture.SetPixels(pixels);texture.Apply();string texturePath=Folder+"QuietGround.png";File.WriteAllBytes(texturePath,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texturePath);var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.maxTextureSize=256;importer.mipmapEnabled=true;importer.SaveAndReimport();mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);EditorUtility.SetDirty(mat);
 int count=0;foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(r.sharedMaterial==source){Undo.RecordObject(r,"Quieter ground");r.sharedMaterial=mat;EditorUtility.SetDirty(r);count++;}
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Ground renderers="+count;
}
}
