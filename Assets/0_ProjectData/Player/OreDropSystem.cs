using UnityEngine;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player
{
    public sealed class OreDropSystem : MonoBehaviour
    {
        public const int PoolSize=96;
        public MinerEquipment equipment;
        public MinerController player;
        public Material[] materials;
        public ParticleSystem dust,chips,sparks;
        public int SparkBursts {get;private set;}
        public int DustBursts {get;private set;}
        public RockFractureEffects fracture;
        public float pickupRadius=.72f;
        public Transform carrierPickup;
        sealed class Slot {public string id;public int item;public Transform root,visual;public MeshFilter mesh;public MeshRenderer renderer;public SphereCollider trigger;public Vector3 start,end;public float age;}
        readonly Slot[] slots=new Slot[PoolSize];
        readonly Matrix4x4[][] instances=new Matrix4x4[6][];
        readonly int[] instanceCounts=new int[6];
        bool instanced;
        readonly Collider[] overlaps=new Collider[PoolSize];
        float pickupTimer;
        public int ActiveCount {get;private set;}
        void Awake()
        {
            instanced=SystemInfo.supportsInstancing;for(int i=0;i<6;i++){instances[i]=new Matrix4x4[PoolSize];materials[i].enableInstancing=true;}
            for(int i=0;i<PoolSize;i++)
            {
                var go=new GameObject("Pooled Ore "+i,typeof(SphereCollider));go.transform.SetParent(transform,false);
                var trigger=go.GetComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=.16f;
                var v=new GameObject("Ore",typeof(MeshFilter),typeof(MeshRenderer));v.transform.SetParent(go.transform,false);
                var r=v.GetComponent<MeshRenderer>();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
                slots[i]=new Slot{root=go.transform,visual=v.transform,mesh=v.GetComponent<MeshFilter>(),renderer=r,trigger=trigger};go.SetActive(false);
            }
        }
        void Start(){Sync(equipment.Snapshot.groundDrops,Vector3.zero,false);}
        public void Sync(GroundOreState[] state,Vector3 origin,bool scatter)
        {
            ActiveCount=state.Length;
            foreach(var slot in slots)
            {
                if(slot.id==null)continue;bool exists=false;foreach(var d in state)if(d.id==slot.id){exists=true;break;}
                if(!exists){slot.id=null;slot.root.gameObject.SetActive(false);}
            }
            foreach(var d in state)
            {
                bool exists=false;foreach(var slot in slots)if(slot.id==d.id){exists=true;break;}if(exists)continue;
                foreach(var slot in slots)if(slot.id==null)
                {
                    slot.id=d.id;slot.item=d.item;slot.end=d.position;slot.start=scatter?origin:d.position;slot.age=scatter?0:1;
                    slot.root.position=d.position;slot.visual.position=slot.start;slot.visual.localScale=Vector3.one*1.25f;
                    slot.mesh.sharedMesh=equipment.MeshForItem(d.item);slot.renderer.sharedMaterial=materials[d.item];slot.renderer.enabled=!instanced;
                    slot.trigger.enabled=!scatter;slot.root.gameObject.SetActive(true);break;
                }
            }
        }
        public void Impact(Vector3 point,bool broken)
        {
            Impact(point,Vector3.up,broken,0);
        }
        public void Impact(Vector3 point,Vector3 normal,bool broken,int resource)
        {
            normal=normal.sqrMagnitude>.001f?normal.normalized:Vector3.up;
            Color stone=Color.Lerp(new Color(.49f,.46f,.40f),MiningResources.Color(resource),.35f);
            // Fixed world-space systems: impact sparks, completion dust and existing chips.
            if(sparks)
            {
                SparkBursts++;
                for(int i=0;i<(broken?14:8);i++)
                {
                    var spark=new ParticleSystem.EmitParams();spark.position=point+normal*.035f+Random.insideUnitSphere*.045f;
                    spark.velocity=normal*Random.Range(1.1f,2.5f)+Vector3.up*Random.Range(.7f,2.2f)+Random.insideUnitSphere*1.1f;
                    spark.startColor=Color.Lerp(new Color(1,.82f,.3f),new Color(1,1,.7f),Random.value);
                    spark.startSize=Random.Range(.028f,.06f);spark.startLifetime=Random.Range(.18f,.4f);sparks.Emit(spark,1);
                }
            }
            if(dust&&broken)DustBursts++;
            if(dust&&broken)for(int i=0;i<14;i++)
            {
                var emit=new ParticleSystem.EmitParams();
                emit.position=point+Random.insideUnitSphere*.25f;
                emit.velocity=normal*Random.Range(.25f,.65f)+Vector3.up*Random.Range(.3f,.7f)+Random.insideUnitSphere*.35f;
                emit.startColor=new Color(.84f,.78f,.65f,.8f);
                emit.startSize=Random.Range(.675f,1.275f);
                emit.startLifetime=Random.Range(.55f,.9f);dust.Emit(emit,1);
            }
            if(chips)for(int i=0;i<(broken?14:8);i++)
            {
                var emit=new ParticleSystem.EmitParams();
                emit.position=point+Random.insideUnitSphere*(broken?.16f:.04f);
                emit.velocity=normal*Random.Range(.8f,broken?2.6f:1.6f)+Vector3.up*Random.Range(.8f,2f)+Random.insideUnitSphere*.65f;
                emit.startColor=Color.Lerp(stone,Color.white,Random.Range(0,.15f));
                emit.startSize=Random.Range(broken?.12f:.035f,broken?.32f:.075f);
                emit.startLifetime=Random.Range(.25f,broken?.65f:.4f);
                emit.rotation3D=Random.insideUnitSphere*180;chips.Emit(emit,1);
            }
        }
        void Update()
        {
            foreach(var slot in slots)if(slot.id!=null&&slot.age<1)
            {
                slot.age=Mathf.Min(1,slot.age+Time.deltaTime/.5f);
                slot.visual.position=Vector3.Lerp(slot.start,slot.end,slot.age)+Vector3.up*Mathf.Sin(slot.age*Mathf.PI)*.5f;
                slot.visual.localRotation=Quaternion.Euler(slot.age*80,slot.age*135,slot.age*30);
                if(slot.age>=1)slot.trigger.enabled=true;
            }
            pickupTimer-=Time.deltaTime;if(pickupTimer>0||!player||player.InputBlocked||equipment.IsFull)return;pickupTimer=.12f;
            // Query the pickup trigger volume without dynamic rigidbodies or per-item Update callbacks.
            Vector3 pickup=equipment.UsingWheelbarrow&&carrierPickup?carrierPickup.position:player.transform.position;
            pickup.y=player.transform.position.y+.18f;
            int n=Physics.OverlapSphereNonAlloc(pickup,equipment.UsingWheelbarrow?.85f:pickupRadius,overlaps,~0,QueryTriggerInteraction.Collide);
            for(int i=0;i<n;i++)foreach(var slot in slots)
                if(slot.id!=null&&slot.age>=1&&overlaps[i]==slot.trigger){equipment.TryCollectDrop(slot.id,slot.visual.position);return;}
        }
        void LateUpdate()
        {
            if(!instanced)return;
            System.Array.Clear(instanceCounts,0,6);
            foreach(var slot in slots)if(slot.id!=null)instances[slot.item][instanceCounts[slot.item]++]=slot.visual.localToWorldMatrix;
            for(int i=0;i<6;i++)if(instanceCounts[i]>0)
                Graphics.DrawMeshInstanced(equipment.MeshForItem(i),0,materials[i],instances[i],instanceCounts[i],null,UnityEngine.Rendering.ShadowCastingMode.Off,false);
        }
    }
}


