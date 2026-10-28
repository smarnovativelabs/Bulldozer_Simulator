using UnityEngine;
namespace Bulldozer.Player
{
    // Three overlapping eight-piece bursts, combined into one dynamic mesh/draw.
    // Analytic motion avoids rigidbodies, collisions and per-break allocations.
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class RockFractureEffects : MonoBehaviour
    {
        public const int Capacity=24;
        public Mesh sourceMesh;
        struct Piece {public Vector3 start,velocity,scale,spin;public Quaternion rotation;public float age,ground;public bool active;}
        readonly Piece[] pieces=new Piece[Capacity];
        Vector3[] source,sourceNormals,vertices,normals;Mesh mesh;MeshRenderer view;int cursor;
        public int ActiveCount {get;private set;}
        public float HighestLift {get;private set;}
        void Awake()
        {
            view=GetComponent<MeshRenderer>();view.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;view.receiveShadows=false;
            source=sourceMesh.vertices;sourceNormals=sourceMesh.normals;var uv=sourceMesh.uv;var indices=sourceMesh.triangles;
            vertices=new Vector3[source.Length*Capacity];normals=new Vector3[vertices.Length];var uvs=new Vector2[vertices.Length];var tris=new int[indices.Length*Capacity];
            for(int i=0;i<Capacity;i++){for(int v=0;v<source.Length;v++)uvs[i*source.Length+v]=uv[v];for(int t=0;t<indices.Length;t++)tris[i*indices.Length+t]=i*source.Length+indices[t];}
            mesh=new Mesh{name="Pooled rock fracture combined mesh"};mesh.MarkDynamic();mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uvs;mesh.triangles=tris;GetComponent<MeshFilter>().sharedMesh=mesh;view.enabled=false;
        }
        public void Burst(Bounds bounds,Vector3 outward)
        {
            for(int i=0;i<8;i++)
            {
                int index=cursor++%Capacity;Vector3 sign=new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1);
                Vector3 offset=Vector3.Scale(bounds.size,sign)*.24f;
                // Pieces begin at the block's volume, then separate vertically before falling.
                pieces[index]=new Piece{active=true,start=bounds.center+offset,scale=Vector3.Scale(bounds.size,Vector3.one*(.49f+(i%3)*.035f))/sourceMesh.bounds.size.x,
                    rotation=Quaternion.Euler(0,i*93,((i&2)==0?0:180)),spin=new Vector3(55+13*i,35*i-100,65-17*i),ground=bounds.min.y,
                    velocity=new Vector3(sign.x*.55f,2.4f+(i%3)*.3f,sign.z*.55f)+outward*.45f};
            }
            view.enabled=true;UpdateMesh(0);
        }
        void LateUpdate(){if(view.enabled)UpdateMesh(Time.deltaTime);}
        void UpdateMesh(float delta)
        {
            ActiveCount=0;var min=Vector3.one*float.MaxValue;var max=Vector3.one*float.MinValue;
            for(int i=0;i<Capacity;i++)
            {
                var p=pieces[i];p.age+=delta;if(p.age>1.15f)p.active=false;pieces[i]=p;
                float shrink=p.active?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1.15f,p.age)):0;
                Vector3 center=p.start+p.velocity*p.age+Vector3.down*(4.5f*p.age*p.age);
                if(p.active){HighestLift=Mathf.Max(HighestLift,center.y-p.start.y);ActiveCount++;}
                center.y=Mathf.Max(p.ground+p.scale.y*.32f,center.y);
                var rotation=p.rotation*Quaternion.Euler(p.spin*p.age);
                for(int v=0;v<source.Length;v++)
                {
                    int j=i*source.Length+v;
                    vertices[j]=p.active?transform.InverseTransformPoint(center+rotation*Vector3.Scale(source[v],p.scale*shrink)):Vector3.zero;
                    normals[j]=p.active?transform.InverseTransformDirection(rotation*sourceNormals[v]):Vector3.up;
                    if(p.active){min=Vector3.Min(min,vertices[j]);max=Vector3.Max(max,vertices[j]);}
                }
            }
            mesh.vertices=vertices;mesh.normals=normals;
            if(ActiveCount>0)mesh.bounds=new Bounds((min+max)*.5f,max-min);else view.enabled=false;
        }
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}
