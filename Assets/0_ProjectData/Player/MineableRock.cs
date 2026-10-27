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
        MaterialPropertyBlock block;Vector3 baseScale;Color baseColor;float flash;
        void Awake(){Health=maxHealth;HitCollider=GetComponent<Collider>();block=new MaterialPropertyBlock();if(!rockRenderer)rockRenderer=GetComponentInChildren<Renderer>();baseScale=rockRenderer.transform.localScale;baseColor=rockRenderer.sharedMaterial.color;}
        void Start(){var e=FindFirstObjectByType<MinerEquipment>();if(e&&e.IsBlockDepleted(blockId)){Health=0;Hide();}}
        void Hide(){HitCollider.enabled=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;}
        public bool TryHit(MinerEquipment collector,Vector3 point)
        {
            if(Depleted||!collector||collector.IsFull)return false;
            if(Health<=collector.Damage&&!collector.TryCollect(resourceType,point,blockId))return false;
            Health=Mathf.Max(0,Health-collector.Damage);flash=.16f;
            if(cracks)cracks.SetActive(Health<maxHealth);
            if(Depleted)Hide();return true;
        }
        void Update()
        {
            if(Depleted||flash<=0)return;flash=Mathf.Max(0,flash-Time.deltaTime);
            block.SetColor("_BaseColor",Color.Lerp(baseColor,new Color(1,.78f,.35f),flash/.16f));rockRenderer.SetPropertyBlock(block);
            rockRenderer.transform.localScale=baseScale*(1+Mathf.Sin(flash*90)*flash*.12f);
            // Restore SRP batching eligibility after the temporary hit flash.
            if(flash<=0)rockRenderer.SetPropertyBlock(null);
        }
    }
}
