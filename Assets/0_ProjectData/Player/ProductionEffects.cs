using UnityEngine;
namespace Bulldozer.Player
{
    // Cosmetic pool only. Transactions are committed before any flight or burst starts.
    public sealed class ProductionEffects : MonoBehaviour
    {
        public Transform hopper,mouth,tray;
        public ParticleSystem[] smoke;
        public ParticleSystem completionSparks;
        public Material itemMaterial;
        public MinerEquipment equipment;
        const int PoolSize=12;
        sealed class Flight {public Transform transform;public MeshFilter mesh;public Renderer renderer;public Vector3 from,to;public Transform destination;public float age,duration;public bool active;public MaterialPropertyBlock properties=new MaterialPropertyBlock();}
        readonly Flight[] flights=new Flight[PoolSize];
        int cursor;
        public int ActiveFlights {get;private set;}
        void Awake()
        {
            if(!equipment)equipment=FindFirstObjectByType<MinerEquipment>();
            for(int i=0;i<PoolSize;i++)
            {
                var go=new GameObject("Pooled Transfer "+i,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=itemMaterial;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;go.SetActive(false);
                flights[i]=new Flight{transform=go.transform,mesh=go.GetComponent<MeshFilter>(),renderer=r};
            }
        }
        void OnEnable(){if(Application.isPlaying)foreach(var s in smoke)if(s)s.Play();}
        public void Fly(int id,Vector3 from,Vector3 to,Transform follow=null)
        {
            if(flights[0]==null)return;var f=flights[cursor++%PoolSize];f.from=from;f.to=to;f.destination=follow;f.age=0;f.duration=.65f;f.active=true;
            f.mesh.sharedMesh=equipment.MeshForItem(id);f.properties.SetColor("_BaseColor",MiningResources.Color(id));f.renderer.SetPropertyBlock(f.properties);f.transform.position=from;f.transform.localScale=Vector3.one*1.6f;f.transform.gameObject.SetActive(true);
        }
        public void Finished(int id){Fly(id,mouth.position,tray.position);if(completionSparks)completionSparks.Emit(6);}
        void Update()
        {
            bool processing=equipment&&equipment.RefineryInputCount>0&&equipment.RefineryOutputCount<100;
            foreach(var system in smoke)if(system){var emission=system.emission;emission.enabled=processing;}
            ActiveFlights=0;
            foreach(var f in flights)
            {
                if(f==null||!f.active)continue;f.age+=Time.deltaTime;float t=Mathf.Clamp01(f.age/f.duration);Vector3 end=f.destination?f.destination.position:f.to;
                f.transform.position=Vector3.Lerp(f.from,end,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.65f);f.transform.rotation=Quaternion.Euler(t*80,t*140,0);
                if(t>=1){f.active=false;f.transform.gameObject.SetActive(false);}else ActiveFlights++;
            }
        }
        void OnDisable(){foreach(var f in flights)if(f!=null){f.active=false;f.transform.gameObject.SetActive(false);}foreach(var s in smoke)if(s)s.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    }
}

