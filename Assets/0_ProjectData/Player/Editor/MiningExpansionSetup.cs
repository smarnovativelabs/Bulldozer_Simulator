using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
 public static class MiningExpansionSetup
 {
  const string Folder="Assets/0_ProjectData/Player/Production";
  static TMPro.TMP_FontAsset font;
  static Material dark,steel,orange,white,wood;
  [MenuItem("Tools/Bulldozer/Build Mining Production and Upgrades")]
  public static void Apply()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first");
   var player=UnityEngine.Object.FindFirstObjectByType<MinerController>();
   if(!player||!player.touchControls||!player.touchControls.safeArea)throw new InvalidOperationException("Player controls missing");
   if(GameObject.Find("MiningProductionSite"))throw new InvalidOperationException("Production site already exists; update it in place");
   if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/0_ProjectData/Player","Production");
   font=player.visual.GetComponent<MinerMining>().cargoLabel.font;
   dark=Mat("Dark Metal",new Color(.12f,.15f,.17f));steel=Mat("Steel",new Color(.46f,.52f,.55f));orange=Mat("Hot Furnace",new Color(1,.32f,.025f));white=Mat("White Marking",new Color(.9f,.91f,.83f));wood=Mat("Timber",new Color(.40f,.25f,.12f));
   var old=GameObject.Find("MineableRocks");if(old)old.SetActive(false);
   old=GameObject.Find("OreDepositArea");if(old)old.SetActive(false);
   var yard=GameObject.Find("PlayerTestYard");foreach(Transform child in yard.transform)if(child.name.IndexOf("Ground",StringComparison.OrdinalIgnoreCase)<0)child.gameObject.SetActive(false);
   var root=new GameObject("MiningProductionSite");
   var oreMesh=Bevel("OreBlock",new Vector3(1.12f,1.15f,1.12f),.07f);
   var raw=Bevel("RawChunk",new Vector3(.24f,.17f,.30f),.06f);
   var ingot=Bevel("Ingot",new Vector3(.24f,.14f,.34f),.025f);
   var ground=Mat("Quarry Stone",new Color(.49f,.50f,.44f));
   var palette=Enumerable.Range(0,6).Select(i=>Mat("Mineral "+i,MiningResources.Colors[i])).ToArray();
   var blocks=new GameObject("Resource Blocks");blocks.transform.SetParent(root.transform);
   for(int row=0;row<5;row++)for(int col=0;col<6;col++)
   {
    int type=(col+row*2)%6;
    var go=new GameObject(MiningResources.Names[type]+" "+row+"-"+col);go.transform.SetParent(blocks.transform);go.transform.position=new Vector3((col-2.5f)*1.18f,0,2.5f+row*1.18f);
    var view=MeshObject("Block",go.transform,oreMesh,ground);view.transform.localPosition=Vector3.up*.575f;
    var collider=go.AddComponent<BoxCollider>();collider.size=new Vector3(1.12f,1.15f,1.12f);collider.center=Vector3.up*.575f;
    var rock=go.AddComponent<MineableRock>();rock.resourceType=type;rock.blockId="quarry-v1-"+row+"-"+col;rock.maxHealth=3+type;rock.wallMining=true;rock.rockRenderer=view.GetComponent<Renderer>();
    if(type>0)for(int j=0;j<5;j++)
    {
     float x=((j*37%7)-3)*.13f,z=((j*13%7)-3)*.13f;
     var chip=MeshObject(type==1?"Orange Vein":"Crystal",view.transform,raw,palette[type]);chip.transform.localPosition=new Vector3(x,.57f,z);chip.transform.localScale=type==1?new Vector3(1.5f,.5f,1.4f):new Vector3(.8f,1.5f,.8f);chip.transform.localRotation=Quaternion.Euler(j*17,j*53,15);
     if(j<3){var face=MeshObject("Face Mineral",view.transform,raw,palette[type]);face.transform.localPosition=new Vector3(x,(j-1)*.28f,-.55f);face.transform.localRotation=Quaternion.Euler(80,j*28,20);}
    }
    var cracks=new GameObject("Damage Cracks");cracks.transform.SetParent(view.transform,false);
    for(int j=0;j<3;j++){var line=Box("Crack",cracks.transform,new Vector3((j-1)*.2f,0,-.563f),new Vector3(.018f,.7f,.008f),dark);line.transform.localRotation=Quaternion.Euler(0,0,25-j*30);}
    cracks.SetActive(false);rock.cracks=cracks;
   }
   var equipment=player.visual.GetComponent<MinerEquipment>();equipment.rawCargoMesh=raw;equipment.ingotCargoMesh=ingot;
   foreach(var slot in equipment.cargoSlots){slot.GetComponent<MeshFilter>().sharedMesh=raw;slot.transform.localScale=Vector3.one;}
   var machine=new GameObject("Refinery");machine.transform.SetParent(root.transform);machine.transform.position=new Vector3(5,0,0);
   Box("Foundation",machine.transform,new Vector3(0,.12f,0),new Vector3(2.1f,.24f,1.8f),dark);
   Box("Furnace Housing",machine.transform,new Vector3(0,.85f,0),new Vector3(1.5f,1.45f,1.25f),dark,true);
   Box("Glowing Mouth",machine.transform,new Vector3(0,.59f,-.64f),new Vector3(.65f,.58f,.04f),orange);
   Box("Mouth Lintel",machine.transform,new Vector3(0,.92f,-.72f),new Vector3(.91f,.16f,.18f),steel);
   Cylinder("Chimney A",machine.transform,new Vector3(-.85f,1.3f,.3f),new Vector3(.22f,1.15f,.22f),steel);
   Cylinder("Chimney B",machine.transform,new Vector3(-.52f,1.2f,.55f),new Vector3(.18f,1,.18f),steel);
   var hopper=MeshObject("Open Hopper",machine.transform,Funnel(),steel);hopper.transform.localPosition=new Vector3(.2f,1.5f,.1f);
   Box("Chute",machine.transform,new Vector3(0,.31f,-1.05f),new Vector3(.7f,.10f,1),steel).transform.localRotation=Quaternion.Euler(-15,0,0);
   foreach(float x in new[]{-.4f,.4f})Box("Chute Wall",machine.transform,new Vector3(x,.43f,-1.05f),new Vector3(.08f,.24f,1),steel);
   Box("Output Tray",machine.transform,new Vector3(0,.1f,-1.85f),new Vector3(1.2f,.15f,.9f),dark);
   var lamp=new GameObject("Furnace Glow",typeof(Light));lamp.transform.SetParent(machine.transform,false);lamp.transform.localPosition=new Vector3(0,.7f,-.9f);var light=lamp.GetComponent<Light>();light.color=new Color(1,.35f,.03f);light.range=3;light.intensity=2;
   var production=root.AddComponent<MiningProduction>();production.furnaceLight=light;production.rawMesh=raw;production.ingotMesh=ingot;
   production.inputZone=Pad("Refinery Input",root.transform,new Vector3(3.1f,0,0),"REFINING\nLOAD: 0",out production.inputLabel);
   production.outputZone=Pad("Refinery Output",root.transform,new Vector3(5,0,-2.7f),"COLLECT INGOTS\nLOAD: 0",out production.outputLabel);
   production.inputStack=Anchor("Input Stock",machine.transform,new Vector3(-1.75f,.1f,.5f));production.outputStack=Anchor("Output Stock",machine.transform,new Vector3(-.2f,.2f,-1.9f));
   var template=MeshObject("Stack Template",root.transform,raw,palette[0]);template.SetActive(false);production.stackTemplate=template;
   production.sellZone=Pad("Truck Loading",root.transform,new Vector3(2,0,-4.4f),"SELL INGOTS\nTRUCK: 0/2",out production.truckLabel);
   var truck=new GameObject("Delivery Truck");truck.transform.SetParent(root.transform);truck.transform.position=new Vector3(4,0,-6.2f);production.truckVisual=truck.transform;
   var blue=Mat("Truck Blue",new Color(.08f,.26f,.55f));var glass=Mat("Truck Glass",new Color(.18f,.39f,.47f));
   Box("Truck Chassis",truck.transform,new Vector3(0,.48f,0),new Vector3(3.6f,.25f,1.5f),dark,true);
   Box("Cab",truck.transform,new Vector3(1.25f,1.05f,0),new Vector3(1,1,1.45f),blue,true);
   Box("Windshield",truck.transform,new Vector3(1.76f,1.2f,0),new Vector3(.03f,.42f,1.12f),glass);
   Box("Load Bed",truck.transform,new Vector3(-.65f,.75f,0),new Vector3(2.4f,.15f,1.5f),steel);
   foreach(float z in new[]{-.72f,.72f})Box("Bed Rail",truck.transform,new Vector3(-.65f,.98f,z),new Vector3(2.4f,.4f,.08f),steel);
   foreach(float x in new[]{-1.1f,1.1f})foreach(float z in new[]{-.8f,.8f}){var wheel=Cylinder("Wheel",truck.transform,new Vector3(x,.4f,z),new Vector3(.75f,.16f,.75f),dark);wheel.transform.localRotation=Quaternion.Euler(90,0,0);}
   production.truckStack=Anchor("Truck Stock",truck.transform,new Vector3(-1,.9f,-.3f));
   var upgrade=new GameObject("Upgrade Area");upgrade.transform.SetParent(root.transform);var upgradeArea=upgrade.AddComponent<MinerUpgradeArea>();
   upgradeArea.zone=Pad("Upgrade Pad",upgrade.transform,new Vector3(-4.5f,0,-2),"UPGRADES\nPICKAXE + RACK",out var sign);
   Box("Upgrade Workbench",upgrade.transform,new Vector3(-4.5f,.7f,-.8f),new Vector3(1.7f,.17f,.7f),wood,true);
   foreach(float x in new[]{-5.2f,-3.8f})Box("Bench Leg",upgrade.transform,new Vector3(x,.35f,-.8f),new Vector3(.13f,.7f,.5f),dark);
   var panel=UI("UpgradePanel",player.touchControls.safeArea,new Vector2(0,80),new Vector2(480,210));panel.AddComponent<UnityEngine.UI.Image>().color=new Color(.06f,.09f,.12f,.96f);upgradeArea.panel=panel;
   Text("Heading",panel.transform,new Vector2(0,70),new Vector2(460,36),"UPGRADE EQUIPMENT",24);
   upgradeArea.damageButton=Button(panel.transform,"PickaxeUpgrade",new Vector2(-115,10),out upgradeArea.damageText);
   upgradeArea.capacityButton=Button(panel.transform,"RackUpgrade",new Vector2(115,10),out upgradeArea.capacityText);
   upgradeArea.feedback=Text("Feedback",panel.transform,new Vector2(0,-70),new Vector2(450,40),"Choose an upgrade. Walk away to close.",17);
   panel.SetActive(false);
   player.visual.GetComponent<MinerMining>().cargoLabel.text="CARGO 0/5 | COINS 0 | Mine > Refine > Collect ingots > Truck";
   PrefabUtility.SaveAsPrefabAsset(root,Folder+"/MiningProductionSite.prefab");
   PrefabUtility.SaveAsPrefabAsset(player.gameObject,"Assets/0_ProjectData/Player/MinerPlayer.prefab");
   PrefabUtility.SaveAsPrefabAsset(player.touchControls.gameObject,"Assets/0_ProjectData/Player/MinerControls.prefab");
   EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
  }
  static Material Mat(string name,Color color){string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,path);}return m;}
  static Transform Anchor(string name,Transform parent,Vector3 position){var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;return g.transform;}
  static GameObject MeshObject(string name,Transform parent,Mesh mesh,Material mat){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
  static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat,bool solid=false){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
  static GameObject Cylinder(string name,Transform parent,Vector3 pos,Vector3 size,Material mat){var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
  static BoxCollider Pad(string name,Transform parent,Vector3 position,string label,out TMPro.TMP_Text text)
  {
   var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=position;var col=go.AddComponent<BoxCollider>();col.isTrigger=true;col.center=Vector3.up*.6f;col.size=new Vector3(1.65f,1.5f,1.65f);
   foreach(float v in new[]{-.8f,.8f}){Box("Border",go.transform,new Vector3(v,.03f,0),new Vector3(.035f,.02f,1.6f),white);Box("Border",go.transform,new Vector3(0,.03f,v),new Vector3(1.6f,.02f,.035f),white);}
   var t=new GameObject("Label",typeof(TMPro.TextMeshPro));t.transform.SetParent(go.transform,false);t.transform.localPosition=new Vector3(0,.05f,0);t.transform.localRotation=Quaternion.Euler(90,0,0);text=t.GetComponent<TMPro.TextMeshPro>();text.font=font;text.fontSize=1.45f;text.text=label;text.alignment=TMPro.TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(1.6f,1.5f);return col;
  }
  static GameObject UI(string name,Transform parent,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return go;}
  static TMPro.TMP_Text Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize){var go=UI(name,parent,pos,size);var t=go.AddComponent<TMPro.TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=fontSize;t.alignment=TMPro.TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
  static UnityEngine.UI.Button Button(Transform parent,string name,Vector2 position,out TMPro.TMP_Text text){var go=UI(name,parent,position,new Vector2(215,80));go.AddComponent<UnityEngine.UI.Image>().color=new Color(.20f,.37f,.43f);var b=go.AddComponent<UnityEngine.UI.Button>();text=Text("Label",go.transform,Vector2.zero,new Vector2(205,76),name,19);return b;}
  static Mesh Bevel(string name,Vector3 size,float bevel)
  {
   string path=Folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
   var vertices=new List<Vector3>();var triangles=new List<int>();Vector3 h=size*.5f,inner=h-Vector3.one*bevel;
   Action<Vector3[]> face=points=>{int start=vertices.Count;Vector3 center=points.Aggregate(Vector3.zero,(a,b)=>a+b)/points.Length;if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),center)<0)Array.Reverse(points);vertices.AddRange(points);for(int i=1;i<points.Length-1;i++)triangles.AddRange(new[]{start,start+i,start+i+1});};
   for(int axis=0;axis<3;axis++)foreach(int sign in new[]{-1,1}){int b=(axis+1)%3,c=(axis+2)%3;var p=new Vector3[4];for(int i=0;i<4;i++){p[i][axis]=h[axis]*sign;p[i][b]=inner[b]*(i<2?-1:1);p[i][c]=inner[c]*(i==0||i==3?-1:1);}face(p);}
   for(int a=0;a<3;a++)for(int b=a+1;b<3;b++)foreach(int sa in new[]{-1,1})foreach(int sb in new[]{-1,1}){int c=3-a-b;var p=new Vector3[4];for(int i=0;i<4;i++){bool one=i<2;p[i][a]=(one?h[a]:inner[a])*sa;p[i][b]=(one?inner[b]:h[b])*sb;p[i][c]=inner[c]*(i==0||i==3?-1:1);}face(p);}
   foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})face(new[]{new Vector3(h.x*x,inner.y*y,inner.z*z),new Vector3(inner.x*x,h.y*y,inner.z*z),new Vector3(inner.x*x,inner.y*y,h.z*z)});
   var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
  }
  static Mesh Funnel()
  {
   string path=Folder+"/Hopper.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
   var v=new List<Vector3>();var t=new List<int>();int segments=16;
   for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments;var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));v.Add(d*.27f);v.Add(d*.58f+Vector3.up*.85f);v.Add(d*.53f+Vector3.up*.85f);v.Add(d*.23f+Vector3.up*.1f);}
   for(int i=0;i<segments;i++)for(int k=0;k<3;k++){int a=i*4+k,b=((i+1)%segments)*4+k;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
   var mesh=new Mesh{name="Open Hopper"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
  }
 }
}

