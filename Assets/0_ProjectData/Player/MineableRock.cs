using UnityEngine;
using DG.Tweening;
namespace Bulldozer.Player
{
    [RequireComponent(typeof(Collider))]
    public sealed class MineableRock : MonoBehaviour
    {
        [Min(1)] public int maxHealth=3;
        [Range(0,5)] public int resourceType;
        public string blockId;
        public bool wallMining;
        public bool naturalBoulder;
        public bool requiresCopperRidge;
        public bool Available(Persistence.MiningSnapshot state)=>!requiresCopperRidge||Persistence.QuarryProgressionRules.AreaAvailable(state);
        public Collider contactCollider;
        public Renderer rockRenderer;
        public GameObject cracks;
        public int Health {get;private set;}
        public bool Depleted=>Health<=0;
        public Collider HitCollider {get;private set;}
        MaterialPropertyBlock block;Vector3 baseScale,basePosition;Color baseColor;float flash;
        Tween impactTween;
        void Awake(){Health=maxHealth;HitCollider=contactCollider?contactCollider:GetComponent<Collider>();block=new MaterialPropertyBlock();if(!rockRenderer)rockRenderer=GetComponentInChildren<Renderer>();baseScale=rockRenderer.transform.localScale;basePosition=rockRenderer.transform.localPosition;baseColor=rockRenderer.sharedMaterial.color;}
        void Start()
        {
            var e=FindFirstObjectByType<MinerEquipment>();
            if(e){int round=e.Snapshot.roundIndex;maxHealth=Persistence.ExcavationRules.Health(maxHealth,round,blockId,transform.position.z<4.1f||Mathf.Abs(transform.position.x)>7.4f);Health=maxHealth;
                if(naturalBoulder&&round>0)rockRenderer.transform.Rotate(0,(round%4)*13,0,Space.Self);
                if(e.IsBlockDepleted(blockId)){Health=0;Hide();}
                else Health=Mathf.Max(1,maxHealth-e.SavedRockDamage(blockId));}
            RefreshCracks();
            impactTween=rockRenderer.transform.DOPunchPosition(Vector3.up*.025f,.18f,3,.15f).SetAutoKill(false).Pause().SetLink(gameObject);
        }
        void Hide(){HitCollider.enabled=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;}
        public bool TryHit(MinerEquipment collector,Vector3 point)
        {
            if(Depleted||!collector||collector.IsFull||(requiresCopperRidge&&!Available(collector.Snapshot)))return false;
            if(Health<=collector.Damage&&!collector.TryBreakIntoDrops(resourceType,blockId,transform.position,maxHealth>=6))return false;
            if(Health>collector.Damage&&!collector.SaveRockDamage(blockId,maxHealth-Health+collector.Damage))return false;
            Health=Mathf.Max(0,Health-collector.Damage);flash=.24f;
            if(!Depleted)impactTween?.Restart();
            RefreshCracks();
            Vector3 normal=collector.transform.position-HitCollider.bounds.center;normal.y=.15f;normal.Normalize();
            if(collector.drops)collector.drops.Impact(point,normal,Depleted,resourceType);
            if(Depleted&&collector.drops&&collector.drops.fracture)collector.drops.fracture.Burst(HitCollider.bounds,normal);
            if(Depleted){impactTween?.Rewind();Hide();}return true;
        }
        void RefreshCracks()
        {
            if(!cracks)return;
            cracks.SetActive(Health>0&&Health<maxHealth);
            int visible=Mathf.CeilToInt((1-Health/(float)maxHealth)*cracks.transform.childCount);
            for(int i=0;i<cracks.transform.childCount;i++)cracks.transform.GetChild(i).gameObject.SetActive(i<visible);
        }
        void Update()
        {
            if(Depleted||flash<=0)return;flash=Mathf.Max(0,flash-Time.deltaTime);
            block.SetColor("_BaseColor",Color.Lerp(baseColor,Color.Lerp(baseColor,Color.white,.3f),flash/.24f));rockRenderer.SetPropertyBlock(block);
            // Restore SRP batching eligibility after the temporary hit flash.
            if(flash<=0){rockRenderer.SetPropertyBlock(null);rockRenderer.transform.localPosition=basePosition;rockRenderer.transform.localScale=baseScale;}
        }
        void OnDestroy(){impactTween?.Kill();}
    }
}
