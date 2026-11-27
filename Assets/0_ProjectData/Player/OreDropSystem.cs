using UnityEngine;
using System.Linq;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player
{
    [DefaultExecutionOrder(250)]
    public sealed class OreDropSystem : MonoBehaviour
    {
        // 84 rocks x 3 ore: enough for the entire largest excavation without a pickup.
        // Shared materials and GPU instancing still cap ore submission at six draws.
        public const int PoolSize=256;
        public MinerEquipment equipment;
        public MinerController player;
        public Material[] materials;
        public ParticleSystem dust,chips,sparks;
        public int SparkBursts {get;private set;}
        public int DustBursts {get;private set;}
        public RockFractureEffects fracture;
        public float pickupRadius=.72f;
        public Transform carrierPickup;
        [Header("Magnet pickup")]
        [Tooltip("Settled ore within this distance flies to the player or cart. Zero turns the magnet off.")]
        public float magnetRadius=2f;
        [Tooltip("Top flight speed in metres per second; ore accelerates to it.")]
        public float magnetSpeed=9f;
        sealed class Slot {public string id;public int item;public Transform root,visual;public MeshFilter mesh;public MeshRenderer renderer;public SphereCollider trigger;public Vector3 start,end;public Quaternion rest;public float age,pull;public bool pulled;}
        float magnetTimer;
        public int PulledCount {get;private set;}
        public int MagnetPickups {get;private set;}
        Slot[] slots;
        public int PoolCapacity=>slots?.Length??0;
        readonly Matrix4x4[][] instances=new Matrix4x4[6][];
        readonly int[] instanceCounts=new int[6];
        bool instanced;
        readonly Collider[] overlaps=new Collider[PoolSize];
        float pickupTimer;
        Vector3 previousPickup;
        bool hasPickupSample;
        public void ResetPickupSweep(){hasPickupSample=false;pickupTimer=0;}
        public int ActiveCount {get;private set;}
        void Awake()
        {
            var state=equipment.Snapshot;
            int rocks=FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Count(r=>!string.IsNullOrEmpty(r.blockId)&&r.Available(state));
            // Prewarm only this excavation: 36 slots for the short first shift, up to 252 later.
            slots=new Slot[Mathf.Clamp(Mathf.Max(state.groundDrops.Length,rocks*3),1,PoolSize)];
            instanced=SystemInfo.supportsInstancing;for(int i=0;i<6;i++){instances[i]=new Matrix4x4[slots.Length];materials[i].enableInstancing=true;}
            for(int i=0;i<slots.Length;i++)
            {
                var go=new GameObject("Pooled Ore "+i,typeof(SphereCollider));go.transform.SetParent(transform,false);
                var trigger=go.GetComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=.16f;
                var v=new GameObject("Ore",typeof(MeshFilter),typeof(MeshRenderer));v.transform.SetParent(go.transform,false);
                var r=v.GetComponent<MeshRenderer>();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
                slots[i]=new Slot{root=go.transform,visual=v.transform,mesh=v.GetComponent<MeshFilter>(),renderer=r,trigger=trigger};go.SetActive(false);
            }
        }
        MoreMountains.Feedbacks.MMF_Player breakFeedback;
        void Start()
        {
            Sync(equipment.Snapshot.groundDrops,Vector3.zero,false);
            if(dust){var main=dust.main;main.startSize=new ParticleSystem.MinMaxCurve(.675f,1.275f);main.startLifetime=new ParticleSystem.MinMaxCurve(.55f,.9f);main.startSpeed=.65f;main.startColor=new Color(.84f,.78f,.65f,.8f);dust.transform.rotation=Quaternion.LookRotation(Vector3.up);var shape=dust.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=35;shape.radius=.18f;
                breakFeedback=gameObject.AddComponent<MoreMountains.Feedbacks.MMF_Player>();
                breakFeedback.AddFeedback(new MoreMountains.Feedbacks.MMF_Particles{BoundParticleSystem=dust,Mode=MoreMountains.Feedbacks.MMF_Particles.Modes.Emit,EmitCount=14,MoveToPosition=true,StopSystemOnInit=false,DeclaredDuration=.1f});breakFeedback.Initialization();}
        }
        public void Sync(GroundOreState[] state,Vector3 origin,bool scatter)
        {
            ActiveCount=state.Length;
            foreach(var slot in slots)
            {
                if(slot.id==null)continue;bool exists=false;foreach(var d in state)if(d.id==slot.id){exists=true;break;}
                if(!exists){slot.id=null;slot.pulled=false;slot.root.gameObject.SetActive(false);}
            }
            foreach(var d in state)
            {
                bool exists=false;foreach(var slot in slots)if(slot.id==d.id){exists=true;break;}if(exists)continue;
                foreach(var slot in slots)if(slot.id==null)
                {
                    slot.id=d.id;slot.item=d.item;slot.end=d.position;slot.start=scatter?origin:d.position;slot.age=scatter?0:1;slot.pulled=false;
                    // Stable per-pickup variation: reloading or reusing a pool slot keeps the same pose.
                    uint seed=2166136261;foreach(char c in d.id)seed=unchecked((seed^c)*16777619);
                    slot.rest=Quaternion.Euler(0,seed%360,0);
                    slot.root.position=d.position;slot.visual.position=slot.start;slot.visual.localScale=Vector3.one*(1.15f+(seed%21)*.01f);
                    slot.visual.localRotation=slot.rest;
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
            if(dust&&broken&&breakFeedback)breakFeedback.PlayFeedbacks(point);
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
                slot.visual.localRotation=slot.rest*Quaternion.Euler((1-slot.age)*100,(1-slot.age)*150,(1-slot.age)*45);
                if(slot.age>=1)slot.trigger.enabled=true;
            }
        }
        void CollectNearby()
        {
            if(!player)return;
            // Run after the cart has followed the player. Sweep between samples so turns and
            // low frame rates cannot skip a ground pickup. Attach/recall resets the sweep.
            Vector3 pickup=equipment.UsingWheelbarrow&&carrierPickup?carrierPickup.position:player.transform.position;
            pickup.y=player.transform.position.y+.18f;
            if(!hasPickupSample||player.InputBlocked||equipment.IsFull||(pickup-previousPickup).sqrMagnitude>4f){previousPickup=pickup;hasPickupSample=true;}
            Magnet(pickup);
            pickupTimer-=Time.deltaTime;if(player.InputBlocked||equipment.IsFull||pickupTimer>0)return;pickupTimer=.12f;
            int n=Physics.OverlapCapsuleNonAlloc(previousPickup,pickup,equipment.UsingWheelbarrow?.75f:pickupRadius,overlaps,~0,QueryTriggerInteraction.Collide);
            previousPickup=pickup;
            for(int i=0;i<n;i++)foreach(var slot in slots)
                if(slot.id!=null&&slot.age>=1&&!slot.pulled&&overlaps[i]==slot.trigger){if(equipment.TryCollectDrop(slot.id,slot.visual.position))Bulldozer.UI.QuarryHaptics.Play(Bulldozer.UI.QuarryHaptics.Kind.Pickup);return;}
        }
        // Settled ore near the collector flies in and is collected through the same saved
        // transaction as walking over it. Only as many pieces as there is free space are pulled,
        // and at most one is committed per frame so arrivals never batch several disk writes.
        void Magnet(Vector3 pickup)
        {
            if(PulledCount>0&&equipment.IsFull){foreach(var slot in slots)if(slot.pulled)Release(slot);PulledCount=0;}
            if(magnetRadius<=0||Time.deltaTime<=0)return;
            magnetTimer-=Time.deltaTime;
            if(magnetTimer<=0&&!player.InputBlocked&&!equipment.IsFull)
            {
                magnetTimer=.1f;int room=equipment.Capacity-equipment.CargoCount-PulledCount;float r2=magnetRadius*magnetRadius;
                for(int i=0;i<slots.Length&&room>0;i++)
                {
                    var slot=slots[i];if(slot.id==null||slot.pulled||slot.age<1)continue;
                    Vector3 d=slot.visual.position-pickup;d.y=0;if(d.sqrMagnitude>r2)continue;
                    slot.pulled=true;slot.pull=0;slot.trigger.enabled=false;PulledCount++;room--;
                }
            }
            if(PulledCount==0)return;
            Vector3 target=pickup+Vector3.up*.35f;bool committed=false;int pulled=0;
            foreach(var slot in slots)
            {
                if(!slot.pulled)continue;
                slot.pull+=Time.deltaTime;
                // Short hop up, then accelerate in: reads as "sucked in" rather than sliding.
                float speed=Mathf.Lerp(1.5f,magnetSpeed,Mathf.Clamp01(slot.pull/.3f));
                Vector3 p=Vector3.MoveTowards(slot.visual.position,target,speed*Time.deltaTime);
                p.y+=Mathf.Max(0,.25f-slot.pull)*2.2f*Time.deltaTime;
                slot.visual.position=p;slot.visual.Rotate(0,540*Time.deltaTime,0,Space.World);
                if(!committed&&(p-target).sqrMagnitude<.09f)
                {
                    committed=true;
                    if(equipment.TryCollectDrop(slot.id,p)){MagnetPickups++;Bulldozer.UI.QuarryHaptics.Play(Bulldozer.UI.QuarryHaptics.Kind.Pickup);continue;}
                    Release(slot);continue;
                }
                pulled++;
            }
            PulledCount=pulled;
        }
        // Return an uncollected piece to its saved ground position using the landing arc.
        void Release(Slot slot){slot.pulled=false;slot.start=slot.visual.position;slot.age=0;slot.trigger.enabled=false;}
        void LateUpdate()
        {
            CollectNearby();
            if(!instanced)return;
            System.Array.Clear(instanceCounts,0,6);
            foreach(var slot in slots)if(slot.id!=null)instances[slot.item][instanceCounts[slot.item]++]=slot.visual.localToWorldMatrix;
            for(int i=0;i<6;i++)if(instanceCounts[i]>0)
                Graphics.DrawMeshInstanced(equipment.MeshForItem(i),0,materials[i],instances[i],instanceCounts[i],null,UnityEngine.Rendering.ShadowCastingMode.Off,false);
        }
    }
}
