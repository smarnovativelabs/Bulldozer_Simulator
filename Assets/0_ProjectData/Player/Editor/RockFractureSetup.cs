using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class RockFractureSetup
    {
        const string Root="Assets/0_ProjectData/Player/Production/";
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit mode required");
            var rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None);
            var sample=rocks.First(r=>r.cracks);
            var material=sample.cracks.GetComponentInChildren<Renderer>(true).sharedMaterial;
            var meshes=new Mesh[3];for(int s=0;s<3;s++)meshes[s]=Cracks(s,sample.rockRenderer.GetComponent<MeshFilter>().sharedMesh.bounds);
            foreach(var rock in rocks)
            {
                if(rock.cracks)Undo.DestroyObjectImmediate(rock.cracks);
                var root=new GameObject("Damage Cracks");Undo.RegisterCreatedObjectUndo(root,"Readable rock cracks");root.transform.SetParent(rock.rockRenderer.transform,false);
                for(int s=0;s<3;s++)
                {
                    var stage=new GameObject("Fracture Stage "+s,typeof(MeshFilter),typeof(MeshRenderer));stage.transform.SetParent(root.transform,false);
                    stage.GetComponent<MeshFilter>().sharedMesh=meshes[s];var renderer=stage.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                }
                root.SetActive(false);rock.cracks=root;EditorUtility.SetDirty(rock);
            }
            var drops=UnityEngine.Object.FindFirstObjectByType<OreDropSystem>();
            var fx=UnityEngine.Object.FindFirstObjectByType<RockFractureEffects>();
            if(!fx){var go=new GameObject("Pooled Block Fracture",typeof(MeshFilter),typeof(MeshRenderer));Undo.RegisterCreatedObjectUndo(go,"Rock fracture effect");fx=go.AddComponent<RockFractureEffects>();}
            fx.sourceMesh=ShardMesh();fx.GetComponent<MeshRenderer>().sharedMaterial=sample.rockRenderer.sharedMaterial;drops.fracture=fx;
            var camera=UnityEngine.Object.FindFirstObjectByType<MinerCamera>();Undo.RecordObject(camera,"Closer mining camera");camera.distance=5.8f;camera.startPitch=46;camera.startYaw=-22;
            EditorUtility.SetDirty(camera);EditorUtility.SetDirty(fx);EditorUtility.SetDirty(drops);
            var p=camera.target;PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/0_ProjectData/Player/MinerPlayer.prefab");
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
            return "Five-face branching cracks; angular eight-piece breakup, 24-piece combined pool; camera 5.8m / 46 pitch / -22 yaw";
        }
        static Mesh ShardMesh()
        {
            string path=Root+"FractureShard.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;
            var points=new[]{new Vector3(-.5f,-.5f,-.4f),new Vector3(.5f,-.45f,-.5f),new Vector3(.4f,-.5f,.5f),new Vector3(-.45f,-.4f,.4f),new Vector3(-.2f,.5f,-.2f),new Vector3(.25f,.25f,.25f)};
            int[] faces={0,2,1,0,3,2,0,1,4,1,2,5,1,5,4,2,3,5,3,0,4,3,4,5};
            var vertices=new Vector3[faces.Length];var uv=new Vector2[faces.Length];
            for(int i=0;i<faces.Length;i+=3)
            {
                Vector3 a=points[faces[i]],b=points[faces[i+1]],c=points[faces[i+2]];
                if(Vector3.Dot(Vector3.Cross(b-a,c-a),(a+b+c)/3)<0){var swap=b;b=c;c=swap;}
                vertices[i]=a;vertices[i+1]=b;vertices[i+2]=c;
                var normal=Vector3.Cross(b-a,c-a).normalized;
                for(int j=0;j<3;j++){var v=vertices[i+j];uv[i+j]=Mathf.Abs(normal.y)>.6f?new Vector2(v.x+.5f,v.z+.5f):Mathf.Abs(normal.x)>.6f?new Vector2(v.z+.5f,v.y+.5f):new Vector2(v.x+.5f,v.y+.5f);}
            }
            mesh=new Mesh{name="Angular fracture shard"};mesh.vertices=vertices;mesh.triangles=Enumerable.Range(0,vertices.Length).ToArray();mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Mesh Cracks(int stage,Bounds bounds)
        {
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int face=0;face<5;face++)for(int branch=0;branch<2;branch++)
            {
                float angle=(stage*2+branch)*Mathf.PI/3+.14f*face;
                var start=new Vector2(.03f,-.02f);var end=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.47f;
                var previous=start;
                for(int j=1;j<=3;j++)
                {
                    var next=Vector2.Lerp(start,end,j/3f);if(j<3)next+=new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle))*(j%2==0?-.065f:.065f);
                    var direction=(next-previous).normalized;var width=new Vector2(-direction.y,direction.x)*(.018f+stage*.004f)*(1-j*.13f);
                    int n=verts.Count;verts.Add(Face(face,previous-width,bounds));verts.Add(Face(face,previous+width,bounds));verts.Add(Face(face,next+width*.65f,bounds));verts.Add(Face(face,next-width*.65f,bounds));
                    var normal=face==0?Vector3.up:face==1?Vector3.back:face==2?Vector3.right:face==3?Vector3.left:Vector3.forward;
                    bool forward=Vector3.Dot(Vector3.Cross(verts[n+1]-verts[n],verts[n+2]-verts[n]),normal)>0;
                    tris.AddRange(forward?new[]{n,n+1,n+2,n,n+2,n+3}:new[]{n+2,n+1,n,n+3,n+2,n});previous=next;
                }
            }
            string path=Root+"FractureCracks"+stage+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name="Branching Fracture "+stage};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
        static Vector3 Face(int face,Vector2 p,Bounds b)
        {
            Vector3 v=face==0?new Vector3(p.x,.501f,p.y):face==1?new Vector3(p.x,p.y,-.501f):face==2?new Vector3(.501f,p.y,p.x):face==3?new Vector3(-.501f,p.y,p.x):new Vector3(p.x,p.y,.501f);
            return b.center+Vector3.Scale(v,b.size);
        }
    }
}
