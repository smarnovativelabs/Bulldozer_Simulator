using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace Bulldozer.Player.Editor
{
    // Editor-only mesh authoring. The finished environment has no Update scripts or prefab forest.
    public static class QuarryEnvironmentSetup
    {
        const string Folder="Assets/0_ProjectData/Player/Environment/";
        const string Art="Assets/0_ProjectData/Player/Production/";
        static Dictionary<Material,List<CombineInstance>> groups;
        static Mesh cube,cone,quad;
        static GameObject root;
        static Material road,stone,hill,dirt,white,yellow,metal,leaf,wood;

        [MenuItem("Tools/Bulldozer/Build Quarry Road and Environment")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first");
            if(GameObject.Find("QuarryEnvironment"))throw new InvalidOperationException("Environment already exists; edit the saved meshes instead of rebuilding over scene edits");
            if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/0_ProjectData/Player","Environment");
            root=new GameObject("QuarryEnvironment");groups=new Dictionary<Material,List<CombineInstance>>();
            var temp=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=temp.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(temp);
            temp=GameObject.CreatePrimitive(PrimitiveType.Quad);quad=temp.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(temp);cone=MakeCone();
            dirt=Load("QuarryDirt");stone=Load("PolishedStone");white=Load("White Marking");metal=Load("Dark Metal");wood=Load("Timber");
            road=Material("Quarry Asphalt",Load("WornSteel"),new Color(.31f,.34f,.35f));
            hill=Material("Cut Rock",stone,new Color(.49f,.52f,.45f));
            yellow=Material("Safety Ochre",white,new Color(1f,.66f,.16f));
            leaf=Material("Pine Green",white,new Color(.19f,.34f,.25f));
            // Extend support under the complete drive route; surfaces stay at ground level.
            for(int x=-50;x<54;x+=2)for(int z=-32;z<38;z+=2)Quad(dirt,x,z,x+2,z+2,-.015f);
            Collider("Quarry Ground",new Vector3(2,-.12f,3),new Vector3(104,.24f,70));
            var old=GameObject.Find("MiningProductionSite/Textured Quarry Ground");if(old)old.GetComponent<Renderer>().enabled=false;
            // Eastbound one-way road. Truck wheel centres follow z=-6.2 exactly.
            for(int x=-40;x<46;x+=2)Quad(road,x,-7.95f,x+2,-4.45f,.006f);
            Box(stone,new Vector3(4,-.045f,-5.3f),new Vector3(8,.1f,5.1f));
            // Re-cover the drive-through portion of the concrete bay with asphalt.
            for(int x=0;x<8;x+=2)Quad(road,x,-7.95f,x+2,-4.45f,.014f);
            for(int x=-38;x<44;x+=3)
            {
                Box(white,new Vector3(x,.021f,-7.7f),new Vector3(1.8f,.008f,.065f));
                if(x<0||x>8)Box(white,new Vector3(x,.021f,-4.7f),new Vector3(1.8f,.008f,.065f));
            }
            foreach(float x in new[]{-13f,-5f,13f,20f})Arrow(x,-6.2f);
            // Parking outline and pedestrian loading strip beside the existing SELL trigger.
            Box(yellow,new Vector3(4,.026f,-7.15f),new Vector3(5.2f,.012f,.08f));
            foreach(float x in new[]{1.4f,6.6f})Box(yellow,new Vector3(x,.026f,-6.25f),new Vector3(.08f,.012f,1.8f));
            Box(yellow,new Vector3(4,.026f,-5.35f),new Vector3(5.2f,.012f,.08f));
            for(float x=4.5f;x<7.7f;x+=.42f)Box(yellow,new Vector3(x,.025f,-4.05f),new Vector3(.12f,.009f,1.05f),Quaternion.Euler(0,-30,0));
            // Equipment pads and a short, unobstructed footpath to the loading strip.
            Box(stone,new Vector3(5,-.04f,-.9f),new Vector3(4.6f,.09f,4.5f));
            Box(stone,new Vector3(-4.5f,-.04f,-1.5f),new Vector3(3.5f,.09f,3.1f));
            Box(stone,new Vector3(2,-.04f,-2.45f),new Vector3(1.1f,.09f,2.1f));
            foreach(float x in new[]{.4f,7.7f})Bollard(x,-4.1f);
            // Railings protect the yard edges while leaving the refinery and loading routes open.
            Fence(-15,18,-10.2f);Fence(-12,11,10.3f);
            Collider("South Safety Rail",new Vector3(1.5f,.55f,-10.2f),new Vector3(33,1.1f,.22f));
            Collider("North Quarry Bank",new Vector3(0,1,11.7f),new Vector3(29,2,2.6f));
            foreach(float x in new[]{-15f,15f})
            {
                Box(hill,new Vector3(x,1.1f,4),new Vector3(2.1f,2.2f,13));
                Collider("Side Quarry Bank",new Vector3(x,1.1f,4),new Vector3(2.1f,2.2f,13));
            }
            // Layered quarry bench behind the ore field, with low-poly scree and treeline.
            for(int i=0;i<12;i++)
            {
                float x=-14+i*2.5f;
                Box(hill,new Vector3(x,.65f,11.8f),new Vector3(2.55f,1.3f,3));
                Box(hill,new Vector3(x,1.6f,13.1f),new Vector3(2.55f,1.8f,3));
            }
            // Covered rock cuttings hide the recycle points (-28 and +36) from the yard.
            Cutting(-28,-1);Cutting(36,1);
            var random=new System.Random(421);
            for(int i=0;i<42;i++)
            {
                float x=-32+(float)random.NextDouble()*68;
                float z=i<27?15+(float)random.NextDouble()*13:-16-(float)random.NextDouble()*12;
                float r=2+(float)random.NextDouble()*3;
                Add(hill,cone,new Vector3(x,-.1f,z),new Vector3(r*2,2+(float)random.NextDouble()*3,r*1.6f),Quaternion.Euler(0,i*39,0));
                if(i%2==0)Tree(x+.6f,z+.4f,2.3f+(float)random.NextDouble()*1.6f);
            }
            foreach(var p in new[]{new Vector3(-11,0,5),new Vector3(-10,0,8),new Vector3(10,0,7),new Vector3(12,0,4),new Vector3(-12,0,-8.8f),new Vector3(11,0,-9)})
            {
                Add(hill,cone,p,new Vector3(1.7f,.9f,1.4f),Quaternion.Euler(0,p.x*23,0));
            }
            // A compact shelter for the workbench, using existing shared wood/metal materials.
            foreach(float x in new[]{-5.8f,-3.2f})foreach(float z in new[]{-.5f,-2.8f})Box(wood,new Vector3(x,1.25f,z),new Vector3(.1f,2.5f,.1f));
            Box(metal,new Vector3(-4.5f,2.55f,-1.65f),new Vector3(3.05f,.12f,2.8f),Quaternion.Euler(6,0,0));
            // Save one mesh per shared material rather than hundreds of individual props.
            int triangles=0;
            foreach(var pair in groups)
            {
                var mesh=new Mesh{name=pair.Key.name+" Environment"};mesh.indexFormat=IndexFormat.UInt16;
                mesh.CombineMeshes(pair.Value.ToArray(),true,true);triangles+=mesh.triangles.Length/3;
                string path=Folder+pair.Key.name+".asset";AssetDatabase.CreateAsset(mesh,path);
                var go=new GameObject(pair.Key.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=pair.Key;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=true;
                renderer.motionVectorGenerationMode=MotionVectorGenerationMode.ForceNoMotion;
                GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);
            }
            UnityEngine.Object.DestroyImmediate(cone);
            var sky=new Material(Shader.Find("Skybox/Procedural"));sky.name="Quarry Sky";
            sky.SetColor("_SkyTint",new Color(.56f,.68f,.76f));sky.SetColor("_GroundColor",new Color(.42f,.43f,.37f));sky.SetFloat("_Exposure",1.1f);sky.SetFloat("_AtmosphereThickness",.8f);
            AssetDatabase.CreateAsset(sky,Folder+"Quarry Sky.mat");RenderSettings.skybox=sky;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.57f,.64f,.68f);RenderSettings.ambientEquatorColor=new Color(.47f,.48f,.43f);RenderSettings.ambientGroundColor=new Color(.28f,.25f,.21f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=50;RenderSettings.fogEndDistance=100;RenderSettings.fogColor=new Color(.63f,.68f,.68f);
            Camera.main.clearFlags=CameraClearFlags.Skybox;Camera.main.farClipPlane=120;
            PrefabUtility.SaveAsPrefabAsset(root,Folder+"QuarryEnvironment.prefab");
            EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);AssetDatabase.SaveAssets();
            string report="Environment triangles="+triangles+"; renderers/materials="+groups.Count+"; no new lights, realtime shadow casters, particles or runtime scripts. Road z=-6.2, supported x=-40..46; cuttings cover truck reset positions.";
            System.IO.File.WriteAllText("Maintenance/ArtPolish/environment-budget.txt",report);return report;
        }
        static Material Load(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Art+name+".mat");
        static Material Material(string name,Material source,Color color){var m=new Material(source){name=name,color=color,enableInstancing=true};m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,Folder+name+".mat");return m;}
        static void Add(Material m,Mesh mesh,Vector3 position,Vector3 scale,Quaternion rotation){if(!groups.ContainsKey(m))groups[m]=new List<CombineInstance>();groups[m].Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(position,rotation,scale)});}
        static void Box(Material m,Vector3 position,Vector3 scale,Quaternion rotation=default){Add(m,cube,position,scale,rotation.Equals(default(Quaternion))?Quaternion.identity:rotation);}
        static void Quad(Material m,float x0,float z0,float x1,float z1,float y){Add(m,quad,new Vector3((x0+x1)/2,y,(z0+z1)/2),new Vector3(x1-x0,z1-z0,1),Quaternion.Euler(90,0,0));}
        static void Collider(string name,Vector3 p,Vector3 size){var go=new GameObject(name,typeof(BoxCollider));go.transform.SetParent(root.transform,false);go.transform.position=p;go.GetComponent<BoxCollider>().size=size;}
        static void Arrow(float x,float z){Box(white,new Vector3(x,.025f,z),new Vector3(1.1f,.008f,.12f));foreach(float sign in new[]{-1f,1f})Box(white,new Vector3(x+.45f,.025f,z+sign*.17f),new Vector3(.5f,.008f,.12f),Quaternion.Euler(0,sign*45,0));}
        static void Bollard(float x,float z){Box(metal,new Vector3(x,.32f,z),new Vector3(.16f,.64f,.16f));Box(yellow,new Vector3(x,.5f,z),new Vector3(.17f,.15f,.17f));Collider("Loading Bollard",new Vector3(x,.32f,z),new Vector3(.18f,.64f,.18f));}
        static void Fence(float start,float end,float z){Box(metal,new Vector3((start+end)/2,.7f,z),new Vector3(end-start,.1f,.1f));Box(metal,new Vector3((start+end)/2,.32f,z),new Vector3(end-start,.07f,.08f));for(float x=start;x<=end;x+=2.5f)Box(wood,new Vector3(x,.5f,z),new Vector3(.13f,1,.13f));}
        static void Tree(float x,float z,float h){Box(wood,new Vector3(x,h*.35f,z),new Vector3(.22f,h*.7f,.22f));Add(leaf,cone,new Vector3(x,h*.35f,z),new Vector3(h*.8f,h*.55f,h*.8f),Quaternion.identity);Add(leaf,cone,new Vector3(x,h*.65f,z),new Vector3(h*.57f,h*.45f,h*.57f),Quaternion.Euler(0,23,0));}
        static void Cutting(float center,int side)
        {
            foreach(float z in new[]{-10.7f,-1.7f}){Box(hill,new Vector3(center,2.5f,z),new Vector3(22,5,5.2f));Collider("Cutting Wall",new Vector3(center,2.5f,z),new Vector3(22,5,5.2f));}
            Box(hill,new Vector3(center,4.4f,-6.2f),new Vector3(22,2.2f,4));
            Collider("Cutting Roof",new Vector3(center,4.4f,-6.2f),new Vector3(22,2.2f,4));
            float portal=center-side*11;
            foreach(float z in new[]{-8.18f,-4.22f})Box(stone,new Vector3(portal,1.6f,z),new Vector3(.6f,3.2f,.35f));
            Box(stone,new Vector3(portal,3.2f,-6.2f),new Vector3(.6f,.4f,4.3f));
            // Dark interior backing is beyond the hidden reset point, never in the visible drive path.
            Box(metal,new Vector3(center+side*9,1.6f,-6.2f),new Vector3(.2f,3.2f,3.8f));
            for(int i=0;i<4;i++)Add(hill,cone,new Vector3(center-8+i*5,4.8f,-6.2f),new Vector3(7,2.3f,9),Quaternion.Euler(0,i*31,0));
        }
        static Mesh MakeCone()
        {
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;
                var p=new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f);var q=new Vector3(Mathf.Cos(b)*.5f,0,Mathf.Sin(b)*.5f);
                int n=v.Count;v.Add(p);v.Add(Vector3.up);v.Add(q);uv.Add(Vector2.zero);uv.Add(Vector2.up);uv.Add(Vector2.right);t.AddRange(new[]{n,n+1,n+2});
            }
            var m=new Mesh{name="Faceted Crown"};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();return m;
        }
    }
}


