using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class QuarryRevisionSetup
    {
        const string Folder="Assets/0_ProjectData/Player/Production/";
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
            var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
            p.secondDelivery.transform.localPosition=p.delivery.transform.localPosition-p.delivery.localTravelDirection.normalized*5;
            foreach(var t in new[]{p.delivery,p.secondDelivery}){t.singleLane=true;t.queuePeer=t==p.delivery?p.secondDelivery:p.delivery;t.queueBay=p.delivery.transform.localPosition;t.queueGap=5;EditorUtility.SetDirty(t);}
            FixGround();int added=ExpandRocks();
            if(!UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>())BuildCart(p);
            var guide=UnityEngine.Object.FindFirstObjectByType<Unity.AI.Navigation.NavMeshSurface>();guide.BuildNavMesh();
            var nav=AssetDatabase.LoadAssetAtPath<NavMeshData>(Folder+"QuarryGuideNavMesh.asset");if(nav!=guide.navMeshData){EditorUtility.CopySerialized(guide.navMeshData,nav);guide.RemoveData();guide.navMeshData=nav;guide.AddData();EditorUtility.SetDirty(nav);}
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
            return "Single-lane truck queue saved. Removed overlapping concrete top faces. Added "+added+" rocks; total="+UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Length+". Navmesh rebuilt.";
        }
        static void FixGround()
        {
            var f=GameObject.Find("QuarryEnvironment/PolishedStone").GetComponent<MeshFilter>();var mesh=f.sharedMesh;
            var v=mesh.vertices.ToList();var uv=mesh.uv.ToList();var triangles=mesh.triangles;var kept=new List<int>();
            for(int i=0;i<triangles.Length;i+=3){var a=v[triangles[i]];var b=v[triangles[i+1]];var c=v[triangles[i+2]];if(Mathf.Abs(a.y-.005f)<.0001f&&Mathf.Abs(b.y-.005f)<.0001f&&Mathf.Abs(c.y-.005f)<.0001f)continue;kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);}
            float[] xs={-6.25f,-2.75f,0,1.45f,2.55f,2.7f,7.3f,8},zs={-7.85f,-3.5f,-3.15f,-3.05f,-2.75f,-1.4f,.05f,1.35f};
            var rects=new[]{new Rect(0,-7.85f,8,5.1f),new Rect(2.7f,-3.15f,4.6f,4.5f),new Rect(-6.25f,-3.05f,3.5f,3.1f),new Rect(1.45f,-3.5f,1.1f,2.1f)};
            for(int x=0;x<xs.Length-1;x++)for(int z=0;z<zs.Length-1;z++)
            {
                Vector2 center=new Vector2((xs[x]+xs[x+1])*.5f,(zs[z]+zs[z+1])*.5f);if(!rects.Any(r=>r.Contains(center)))continue;
                int n=v.Count;var points=new[]{new Vector3(xs[x],.005f,zs[z]),new Vector3(xs[x],.005f,zs[z+1]),new Vector3(xs[x+1],.005f,zs[z+1]),new Vector3(xs[x+1],.005f,zs[z])};foreach(var a in points){v.Add(a);uv.Add(new Vector2(a.x,a.z)*.7f);}kept.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            mesh.Clear();mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(kept,0);mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);EditorUtility.SetDirty(mesh);
            var old=GameObject.Find("MiningProductionSite/Textured Quarry Ground");if(old)old.GetComponent<Renderer>().enabled=false;
            // The base support collider remains; its hidden surface must not compete with yard paving.
            var baseGround=GameObject.Find("PlayerTestYard/Ground");if(baseGround)baseGround.GetComponent<Renderer>().enabled=false;
        }
        static int ExpandRocks()
        {
            if(GameObject.Find("Expanded Ore Beds"))return 0;
            var rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None);var templates=new MineableRock[6];foreach(var rock in rocks){if(!templates[rock.resourceType])templates[rock.resourceType]=rock;rock.maxHealth=2+rock.resourceType/2;EditorUtility.SetDirty(rock);}
            var root=new GameObject("Expanded Ore Beds");int count=0;
            for(int bed=0;bed<2;bed++)for(int row=0;row<6;row++)for(int col=0;col<4;col++)
            {
                int type=(row+col*2+bed)%6;var copy=UnityEngine.Object.Instantiate(templates[type].gameObject,root.transform);copy.name=(bed==0?"Weathered":"Dense")+" "+MiningResources.Names[type]+" "+row+"-"+col;copy.transform.position=new Vector3((bed==0?-8.8f:5.1f)+col*1.18f,0,2.8f+row*1.18f);
                var rock=copy.GetComponent<MineableRock>();rock.blockId="quarry-expansion-"+bed+"-"+row+"-"+col;rock.maxHealth=bed==0?4+type/2:8+type;count++;
            }
            return count;
        }
        static Material Mat(string name,Color color){string path=Folder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(m,path);}return m;}
        static Transform Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;return go.transform;}
        static Mesh Tire()
        {
            var mesh=new Mesh{name="12 segment caddy wheel"};var v=new List<Vector3>();var t=new List<int>();for(int side=0;side<2;side++)for(int i=0;i<12;i++){float a=i*Mathf.PI/6;v.Add(new Vector3(Mathf.Cos(a)*.24f,Mathf.Sin(a)*.24f,(side==0?-.08f:.08f)));}v.Add(new Vector3(0,0,-.08f));v.Add(new Vector3(0,0,.08f));for(int i=0;i<12;i++){int j=(i+1)%12;t.AddRange(new[]{i,j,j+12,i,j+12,i+12,24,j,i,25,i+12,j+12});}mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"CaddyWheel.asset");return mesh;
        }
        static void BuildCart(MiningProduction p)
        {
            var player=UnityEngine.Object.FindFirstObjectByType<MinerController>();var root=new GameObject("Quarry Caddy");root.transform.position=new Vector3(-6.8f,.05f,-2.2f);var cart=root.AddComponent<QuarryWheelbarrow>();cart.player=player;cart.equipment=player.visual.GetComponent<MinerEquipment>();cart.stockMaterials=p.stockMaterials;
            var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/0_ProjectData/Player/Art/Wheelbarrow/Wheelbarrow.fbx"),root.transform);model.name="Supplied wheelbarrow - quarry variant";model.transform.localRotation=Quaternion.Euler(0,-90,0);model.transform.localScale=Vector3.one*.75f;model.transform.localPosition=new Vector3(0,.15f,.45f);
            var teal=Mat("Caddy Teal",new Color(.08f,.43f,.42f));var dark=Mat("Caddy Rubber",new Color(.09f,.12f,.14f));var yellow=Mat("Caddy Safety",new Color(1,.7f,.14f));
            foreach(var r in model.GetComponentsInChildren<MeshRenderer>()){r.sharedMaterial=r.name.Contains("Weel")?dark:teal;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=true;}
            var wf=model.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("Weel"));wf.sharedMesh=Tire();cart.wheel=wf.transform;
            Box("Raised side storage",root.transform,new Vector3(-.43f,.63f,.17f),new Vector3(.17f,.27f,.48f),teal);
            foreach(float x in new[]{-.3f,.3f}){Box("Extended ergonomic handle",root.transform,new Vector3(x,.85f,-.45f),new Vector3(.055f,.055f,.6f),dark);Box("Safety grip",root.transform,new Vector3(x,.85f,-.7f),new Vector3(.075f,.075f,.18f),yellow);}
            cart.leftGrip=new GameObject("Left grip target").transform;cart.leftGrip.SetParent(root.transform,false);cart.leftGrip.localPosition=new Vector3(-.3f,.85f,-.7f);
            cart.rightGrip=new GameObject("Right grip target").transform;cart.rightGrip.SetParent(root.transform,false);cart.rightGrip.localPosition=new Vector3(.3f,.85f,-.7f);
            cart.loadRoot=new GameObject("Caddy cargo").transform;cart.loadRoot.SetParent(root.transform,false);cart.loadRoot.localPosition=new Vector3(0,.68f,.2f);cart.loadSlots=new Transform[12];
            for(int i=0;i<12;i++){var go=UnityEngine.Object.Instantiate(p.stackTemplate,cart.loadRoot);go.name="Caddy cargo "+i;go.transform.localPosition=new Vector3((i%3-1)*.18f,(i/6)*.15f,(i/3%2-.5f)*.25f);go.transform.localScale=Vector3.one*.75f;cart.loadSlots[i]=go.transform;go.SetActive(false);}
            var collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,.45f,.2f);collider.size=new Vector3(.85f,.8f,1.2f);
            var nav=root.AddComponent<Unity.AI.Navigation.NavMeshModifier>();nav.ignoreFromBuild=true;
            var label=UnityEngine.Object.Instantiate(p.delivery.counter,root.transform);label.name="Caddy counter";label.transform.localPosition=new Vector3(0,1.5f,0);label.fontSize=2.2f;label.text="QUARRY CADDY\n$60";cart.counter=label;
            var safe=player.visual.GetComponent<MinerMining>().cargoLabel.transform.parent;var button=new GameObject("Wheelbarrow Action",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));var rect=button.GetComponent<RectTransform>();rect.SetParent(safe,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(1,0);rect.anchoredPosition=new Vector2(-28,155);rect.sizeDelta=new Vector2(250,56);button.GetComponent<UnityEngine.UI.Image>().color=new Color(.05f,.3f,.3f,.96f);cart.actionButton=button.GetComponent<UnityEngine.UI.Button>();
            var text=new GameObject("Label",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));text.transform.SetParent(rect,false);var tr=text.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;var tmp=text.GetComponent<TMPro.TextMeshProUGUI>();tmp.font=p.truckLabel.font;tmp.fontSize=18;tmp.alignment=TMPro.TextAlignmentOptions.Center;tmp.raycastTarget=false;tmp.text="UNLOCK CART  $60";cart.actionText=tmp;
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/0_ProjectData/Player/Art/Wheelbarrow/BaseColor.png");importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.SaveAndReimport();
        }
    }
}
