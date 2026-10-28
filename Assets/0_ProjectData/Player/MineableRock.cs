using UnityEngine;
namespace Bulldozer.Player
{
    [RequireComponent(typeof(Collider))]
    public sealed class MineableRock : MonoBehaviour
    {
        [Min(1)] public int maxHealth=3;
        [Range(0,5)] public int resourceType;
        public string blockId;
        public bool wallMining;
        public Renderer rockRenderer;
        public GameObject cracks;
        public int Health {get;private set;}
        public bool Depleted=>Health<=0;
        public Collider HitCollider {get;private set;}
        MaterialPropertyBlock block;Vector3 baseScale,basePosition,recoil;Color baseColor;float flash;
        void Awake(){Health=maxHealth;HitCollider=GetComponent<Collider>();block=new MaterialPropertyBlock();if(!rockRenderer)rockRenderer=GetComponentInChildren<Renderer>();baseScale=rockRenderer.transform.localScale;basePosition=rockRenderer.transform.localPosition;baseColor=rockRenderer.sharedMaterial.color;}
        void Start(){var e=FindFirstObjectByType<MinerEquipment>();if(e&&e.IsBlockDepleted(blockId)){Health=0;Hide();}}
        void Hide(){HitCollider.enabled=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;}
        public bool TryHit(MinerEquipment collector,Vector3 point)
        {
            if(Depleted||!collector||collector.IsFull)return false;
            if(Health<=collector.Damage&&!collector.TryBreakIntoDrops(resourceType,blockId,transform.position))return false;
            Health=Mathf.Max(0,Health-collector.Damage);flash=.24f;
            if(cracks)
            {
                cracks.SetActive(Health<maxHealth);
                int visible=Mathf.CeilToInt((1-Health/(float)maxHealth)*cracks.transform.childCount);
                for(int i=0;i<cracks.transform.childCount;i++)cracks.transform.GetChild(i).gameObject.SetActive(i<visible);
            }
            Vector3 normal=collector.transform.position-HitCollider.bounds.center;normal.y=.15f;normal.Normalize();
            recoil=rockRenderer.transform.parent.InverseTransformVector(-normal*.035f+Vector3.up*.07f);
            if(collector.drops)collector.drops.Impact(point,normal,Depleted,resourceType);
            if(Depleted&&collector.drops&&collector.drops.fracture)collector.drops.fracture.Burst(HitCollider.bounds,normal);
            if(Depleted)Hide();return true;
        }
        void Update()
        {
            if(Depleted||flash<=0)return;flash=Mathf.Max(0,flash-Time.deltaTime);
            block.SetColor("_BaseColor",Color.Lerp(baseColor,Color.Lerp(baseColor,Color.white,.3f),flash/.24f));rockRenderer.SetPropertyBlock(block);
            rockRenderer.transform.localScale=baseScale*(1+Mathf.Sin(flash*90)*flash*.12f);
            rockRenderer.transform.localPosition=basePosition+recoil*Mathf.Sin((1-flash/.24f)*Mathf.PI);
            // Restore SRP batching eligibility after the temporary hit flash.
            if(flash<=0){rockRenderer.SetPropertyBlock(null);rockRenderer.transform.localPosition=basePosition;rockRenderer.transform.localScale=baseScale;}
        }
    }
}
