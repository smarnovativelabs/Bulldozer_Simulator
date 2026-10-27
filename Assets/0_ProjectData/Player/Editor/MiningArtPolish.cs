using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
 public static class MiningArtPolish
 {
  const string F="Assets/0_ProjectData/Player/Production/";
  static Texture2D atlas;
  static Material stone,metal,orange,trim;
  static Mesh raw,ingot;
  public static string Apply()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
   var production=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();var player=UnityEngine.Object.FindFirstObjectByType<MinerController>();if(!production||!player)throw new InvalidOperationException("Open gameplay scene");
   if(production.transform.Find("ArtPolishApplied"))throw new InvalidOperationException("Art pass already applied; edit existing assets");
   atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(F+"QuarryAtlas.png");ConfigureTexture(F+"QuarryAtlas.png",1024);ConfigureTexture("Assets/0_ProjectData/Player/Art/Truck/TruckAtlas.png",1024);
   stone=Mat("PolishedStone",new Color(.85f,.84f,.79f),0,.08f,0);metal=Mat("WornSteel",Color.white,.35f,.22f,2);orange=Mat("PaintedOrange",new Color(.82f,.7f,.56f),.2f,.2f,3);trim=Mat("EdgeSteel",new Color(.43f,.49f,.48f),.55f,.45f,-1);
   raw=AssetDatabase.LoadAssetAtPath<Mesh>(F+"RawChunk.asset");ingot=AssetDatabase.LoadAssetAtPath<Mesh>(F+"Ingot.asset");
   var gems=new Mesh[6];gems[0]=raw;gems[1]=raw;gems[2]=Gem("BlueCrystalMesh",6,false);gems[3]=Gem("DiamondMesh",8,true);gems[4]=Nugget();gems[5]=Gem("AmethystMesh",6,false);
   var materials=new[]{stone,Mat("CopperVein",new Color(.6f,.3f,.13f),.5f,.35f,-1),Mat("BlueMineral",new Color(.16f,.48f,.58f),.4f,.6f,-1),Mat("Diamond",new Color(.72f,.90f,.98f),.3f,.8f,-1),Mat("Gold",new Color(1,.67f,.12f),.75f,.65f,-1),Mat("Amethyst",new Color(.48f,.16f,.68f),.35f,.65f,-1)};
   foreach(var rock in UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None))
   {
    var view=rock.rockRenderer.transform;var source=view.GetComponent<MeshFilter>().sharedMesh;var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(F+"TexturedOre.asset");
    if(!mesh){mesh=UnityEngine.Object.Instantiate(source);mesh.name="Chipped Textured Ore";UV(mesh);AssetDatabase.CreateAsset(mesh,F+"TexturedOre.asset");}view.GetComponent<MeshFilter>().sharedMesh=mesh;rock.rockRenderer.sharedMaterial=stone;
    foreach(Transform child in view.Cast<Transform>().ToArray())if(child.name!="Damage Cracks")UnityEngine.Object.DestroyImmediate(child.gameObject);
    int type=rock.resourceType;if(type==0)continue;
    var combine=new List<CombineInstance>();
    for(int j=0;j<(type==1?7:4);j++)
    {
     float x=(j%3-1)*.29f,z=(j/3-.5f)*.40f;
     var pos=new Vector3(x,.55f,z);var scale=type==1?new Vector3(1.25f,.13f,1.6f):type==4?new Vector3(1.2f,.8f,1.15f):new Vector3(.85f,1.35f,.85f);
     combine.Add(new CombineInstance{mesh=gems[type],transform=Matrix4x4.TRS(pos,Quaternion.Euler(0,j*47,j*7-12),scale)});
     if(j<2)combine.Add(new CombineInstance{mesh=gems[type],transform=Matrix4x4.TRS(new Vector3(x,.05f+j*.29f,-.55f),Quaternion.Euler(78,j*55,15),scale)});
    }
    var ore=new Mesh{name="Embedded "+type};ore.CombineMeshes(combine.ToArray(),true,true);UV(ore);var existing=AssetDatabase.LoadAssetAtPath<Mesh>(F+"Embedded"+type+".asset");if(existing){UnityEngine.Object.DestroyImmediate(ore);ore=existing;}else AssetDatabase.CreateAsset(ore,F+"Embedded"+type+".asset");ObjectMesh("Embedded Minerals",view,ore,materials[type]);
   }
   var equipment=player.visual.GetComponent<MinerEquipment>();equipment.resourceMeshes=new Mesh[12];for(int i=0;i<6;i++){equipment.resourceMeshes[i]=gems[i];equipment.resourceMeshes[i+6]=(i==3||i==5)?gems[i]:ingot;}
   // A tiled mesh repeats each atlas quadrant without a custom shader or texture copies.
   var ground=GameObject.Find("PlayerTestYard/Ground");ground.GetComponent<Renderer>().enabled=false;
   var dirt=Mat("QuarryDirt",new Color(.86f,.83f,.76f),0,.05f,1);var groundMesh=Ground();var floor=ObjectMesh("Textured Quarry Ground",production.transform,groundMesh,dirt);floor.transform.position=new Vector3(0,.012f,0);
   var furnace=production.transform.Find("Refinery");
   foreach(var r in furnace.GetComponentsInChildren<MeshRenderer>())
   {
    if(r.name=="Glowing Mouth")continue;
    bool accent=r.name=="Furnace Housing"||r.name=="Foundation";r.sharedMaterial=accent?orange:metal;
    var filter=r.GetComponent<MeshFilter>();if(filter.sharedMesh.uv.Length==0){var m=UnityEngine.Object.Instantiate(filter.sharedMesh);UV(m);string path=F+"Textured"+filter.sharedMesh.name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!saved){AssetDatabase.CreateAsset(m,path);saved=m;}else UnityEngine.Object.DestroyImmediate(m);filter.sharedMesh=saved;}
   }
   var detail=new GameObject("Refinery Detail");detail.transform.SetParent(furnace,false);
   Box("Control Panel",detail.transform,new Vector3(.78f,.9f,-.25f),new Vector3(.09f,.58f,.6f),metal);
   for(int i=0;i<5;i++)Box("Vent",detail.transform,new Vector3(.83f,.72f+i*.08f,-.27f),new Vector3(.03f,.025f,.4f),trim);
   foreach(float x in new[]{-.66f,.66f})Box("Corner Brace",detail.transform,new Vector3(x,.88f,-.65f),new Vector3(.09f,1.4f,.06f),trim);
   for(int i=0;i<6;i++){var bolt=Box("Rivet",detail.transform,new Vector3((i%3-1)*.48f,.25f+(i/3)*1.18f,-.69f),new Vector3(.055f,.055f,.035f),trim);bolt.transform.localRotation=Quaternion.Euler(0,0,45);}
   for(int i=0;i<4;i++)Box("Cooling Fin",detail.transform,new Vector3(-.82f,.6f+i*.17f,-.15f),new Vector3(.13f,.05f,.7f),metal);
   Box("Tray Inlay",detail.transform,new Vector3(0,.18f,-1.85f),new Vector3(1.08f,.025f,.78f),trim);
   // Consolidate stationary furnace surfaces by shared material while retaining collision objects.
   foreach(var group in furnace.GetComponentsInChildren<MeshRenderer>().GroupBy(r=>r.sharedMaterial).ToArray())
   {
    var instances=group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=furnace.worldToLocalMatrix*r.localToWorldMatrix}).ToArray();var m=new Mesh{name="Furnace "+group.Key.name};m.CombineMeshes(instances,true,true);string path=F+"Furnace_"+group.Key.name+".asset";AssetDatabase.CreateAsset(m,path);ObjectMesh("Combined "+group.Key.name,furnace,m,group.Key);foreach(var r in group)r.enabled=false;
   }
   var truck=production.truckVisual;
   foreach(Transform t in truck.Cast<Transform>().ToArray())if(t!=production.truckStack)UnityEngine.Object.DestroyImmediate(t.gameObject);
   var truckAsset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/0_ProjectData/Player/Art/Truck/TruckMobile.fbx");var model=(GameObject)PrefabUtility.InstantiatePrefab(truckAsset);model.name="MAN Truck 4400 triangles";model.transform.SetParent(truck,false);
   var bounds=model.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});float truckScale=3.8f/Mathf.Max(bounds.size.x,bounds.size.z);model.transform.localScale*=truckScale;model.transform.localRotation=Quaternion.Euler(0,90,0)*model.transform.localRotation;
   bounds=model.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});model.transform.position+=new Vector3(truck.position.x-bounds.center.x,-bounds.min.y,truck.position.z-bounds.center.z);
   var truckMat=Mat("MobileTruck",Color.white,.15f,.35f,-1);truckMat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/0_ProjectData/Player/Art/Truck/TruckAtlas.png");foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=truckMat;
   var col=truck.gameObject.GetComponent<BoxCollider>();if(!col)col=truck.gameObject.AddComponent<BoxCollider>();col.center=new Vector3(0,.8f,0);col.size=new Vector3(3.8f,1.6f,1.4f);production.truckStack.localPosition=new Vector3(-.65f,1.27f,-.28f);
   var effects=production.gameObject.AddComponent<ProductionEffects>();effects.equipment=equipment;effects.itemMaterial=Mat("TransferMinerals",Color.white,.4f,.55f,-1);production.effects=effects;
   effects.hopper=Anchor("Hopper Intake",furnace,new Vector3(.2f,2.25f,.1f));effects.mouth=Anchor("Production Exit",furnace,new Vector3(0,.75f,-.85f));effects.tray=Anchor("Product Landing",furnace,new Vector3(0,.45f,-1.85f));
   effects.smoke=new[]{Smoke(furnace,new Vector3(-.85f,2.48f,.3f),false),Smoke(furnace,new Vector3(-.52f,2.22f,.55f),false)};effects.completionSparks=Smoke(furnace,new Vector3(0,.7f,-.85f),true);
   production.furnaceLight.shadows=LightShadows.None;production.furnaceLight.range=2.2f;
   new GameObject("ArtPolishApplied").transform.SetParent(production.transform,false);
   PrefabUtility.SaveAsPrefabAsset(player.gameObject,"Assets/0_ProjectData/Player/MinerPlayer.prefab");PrefabUtility.SaveAsPrefabAsset(production.gameObject,F+"MiningProductionSite.prefab");EditorSceneManager.MarkSceneDirty(production.gameObject.scene);EditorSceneManager.SaveScene(production.gameObject.scene);AssetDatabase.SaveAssets();
   int tris=model.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3);return "Truck triangles="+tris+"; materials=1; transfer pool=12; smoke cap=24; sparks cap=10";
  }
  static void ConfigureTexture(string path,int size){var t=AssetImporter.GetAtPath(path) as TextureImporter;if(!t)return;t.maxTextureSize=size;t.mipmapEnabled=true;t.isReadable=false;t.textureCompression=TextureImporterCompression.Compressed;var android=t.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=size;android.format=TextureImporterFormat.ASTC_6x6;t.SetPlatformTextureSettings(android);t.SaveAndReimport();}
  static Material Mat(string name,Color color,float metallic,float smooth,int quadrant){var m=AssetDatabase.LoadAssetAtPath<Material>(F+name+".mat");if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,F+name+".mat");}m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;if(quadrant>=0){m.mainTexture=atlas;m.mainTextureScale=Vector2.one*.48f;m.mainTextureOffset=new Vector2(quadrant%2==0?.01f:.51f,quadrant<2?.51f:.01f);}EditorUtility.SetDirty(m);return m;}
  static GameObject ObjectMesh(string name,Transform parent,Mesh mesh,Material mat){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;}
  static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
  static Transform Anchor(string name,Transform parent,Vector3 pos){var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;return g.transform;}
  static void UV(Mesh mesh){mesh.RecalculateBounds();if(mesh.normals.Length!=mesh.vertexCount)mesh.RecalculateNormals();var v=mesh.vertices;var n=mesh.normals;var uv=new Vector2[v.Length];var b=mesh.bounds;for(int i=0;i<v.Length;i++){var p=v[i]-b.min;var normal=n[i];if(Mathf.Abs(normal.y)>.6f)uv[i]=new Vector2(p.x/Mathf.Max(.001f,b.size.x),p.z/Mathf.Max(.001f,b.size.z));else if(Mathf.Abs(normal.x)>.6f)uv[i]=new Vector2(p.z/Mathf.Max(.001f,b.size.z),p.y/Mathf.Max(.001f,b.size.y));else uv[i]=new Vector2(p.x/Mathf.Max(.001f,b.size.x),p.y/Mathf.Max(.001f,b.size.y));}mesh.uv=uv;}
  static Mesh Gem(string name,int sides,bool diamond)
  {
   var existing=AssetDatabase.LoadAssetAtPath<Mesh>(F+name+".asset");if(existing)return existing;var vertices=new List<Vector3>();var tris=new List<int>();
   for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices.Add(new Vector3(Mathf.Cos(a)*.12f,diamond?0:-.08f,Mathf.Sin(a)*.12f));vertices.Add(new Vector3(Mathf.Cos(a)*(diamond?.065f:.105f),diamond?.08f:.12f,Mathf.Sin(a)*(diamond?.065f:.105f)));}
   int top=vertices.Count;vertices.Add(new Vector3(0,diamond?.08f:.25f,0));int bottom=vertices.Count;vertices.Add(new Vector3(0,diamond?-.12f:-.08f,0));
   for(int i=0;i<sides;i++){int a=i*2,b=((i+1)%sides)*2;tris.AddRange(new[]{a,b,a+1,a+1,b,b+1,a+1,b+1,top,b,a,bottom});}
   for(int i=0;i<tris.Count;i+=3){int temp=tris[i];tris[i]=tris[i+2];tris[i+2]=temp;}
   var m=new Mesh{name=name};m.vertices=tris.Select(i=>vertices[i]).ToArray();m.triangles=Enumerable.Range(0,tris.Count).ToArray();m.RecalculateNormals();UV(m);AssetDatabase.CreateAsset(m,F+name+".asset");return m;
  }
  static Mesh Nugget(){string path=F+"GoldNugget.asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m)return m;m=UnityEngine.Object.Instantiate(raw);m.name="Irregular Gold Nugget";var v=m.vertices;for(int i=0;i<v.Length;i++){float f=.85f+Mathf.Sin(v[i].x*70+v[i].y*50+v[i].z*40)*.2f;v[i]*=f;}m.vertices=v;m.RecalculateNormals();UV(m);AssetDatabase.CreateAsset(m,path);return m;}
  static Mesh Ground(){string path=F+"TiledGround.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();for(int x=-15;x<15;x+=2)for(int z=-15;z<15;z+=2){int b=v.Count;v.AddRange(new[]{new Vector3(x,0,z),new Vector3(x,0,z+2),new Vector3(x+2,0,z+2),new Vector3(x+2,0,z)});uv.AddRange(new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right});t.AddRange(new[]{b,b+1,b+2,b,b+2,b+3});}mesh=new Mesh{name="Tiled Quarry Ground"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);return mesh;}
  static ParticleSystem Smoke(Transform parent,Vector3 pos,bool sparks)
  {
   var go=new GameObject(sparks?"Production Sparks":"Processing Smoke",typeof(ParticleSystem));go.transform.SetParent(parent,false);go.transform.localPosition=pos;var ps=go.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.playOnAwake=true;main.loop=!sparks;main.maxParticles=sparks?10:12;main.startLifetime=sparks?.38f:3f;main.startSpeed=sparks?1.1f:.35f;main.startSize=sparks?.055f:.55f;main.startColor=sparks?new Color(1,.62f,.15f):new Color(.85f,.87f,.9f,.8f);main.simulationSpace=ParticleSystemSimulationSpace.World;
   var emission=ps.emission;emission.rateOverTime=sparks?0:4;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=sparks?25:8;shape.radius=.07f;shape.rotation=new Vector3(-90,0,0);
   var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.y=new ParticleSystem.MinMaxCurve(sparks?.2f:.55f);velocity.x=new ParticleSystem.MinMaxCurve(.08f);
   var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.8f,.15f),new GradientAlphaKey(0,1)});color.color=gradient;
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.4f,1,1.8f));
   var renderer=ps.GetComponent<ParticleSystemRenderer>();var path=F+(sparks?"Sparks":"Smoke")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/1_Plugins/Kenney/kenney_particle-pack/PNG (Transparent)/"+(sparks?"star_01.png":"smoke_01.png")));AssetDatabase.CreateAsset(mat,path);}renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return ps;
  }
 }
}



