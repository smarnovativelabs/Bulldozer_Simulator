using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player
{
    public sealed class MinerEquipment : MonoBehaviour
    {
        public GameObject pickaxe;
        public Transform cargoRoot;
        public GameObject[] cargoSlots;
        public Mesh rawCargoMesh,ingotCargoMesh;
        public Mesh[] resourceMeshes;
        public OreDropSystem drops;
        public Mesh MeshForItem(int id)=>resourceMeshes!=null && id>=0 && id<resourceMeshes.Length && resourceMeshes[id]?resourceMeshes[id]:MiningResources.Refined(id)?ingotCargoMesh:rawCargoMesh;
        [SerializeField] int cargoCount;
        Vector3[] slotPositions;
        MiningProgressStore progress;
        public MiningSnapshot Snapshot=>progress.Current.Copy();
        public int CargoCount=>cargoCount;
        public int RefineryInputCount=>progress.Current.refineryInput.Length;
        public int RefineryOutputCount=>progress.Current.refineryOutput.Length;
        public int TrucksCompleted=>progress.Current.trucksCompleted;
        // Keep the serialized coins field for existing save compatibility.
        public int Cash=>progress?.Current.coins??0;
        public int Coins=>Cash;
        public int DamageLevel=>progress?.Current.damageLevel??0;
        public int CapacityLevel=>progress?.Current.capacityLevel??0;
        public int Damage=>1+DamageLevel;
        public int DamageCost=>20+DamageLevel*20;
        public int CapacityCost=>15+CapacityLevel*15;
        public string SaveError=>progress?.Error;
        public string SavePath=>progress?.Path;
        public bool UsingWheelbarrow=>progress?.Current.usingWheelbarrow??false;
        public int Capacity=>UsingWheelbarrow?12:5+CapacityLevel;
        public bool IsFull=>CargoCount>=Capacity;
        public bool PickaxeVisible=>pickaxe&&pickaxe.activeInHierarchy;
        void OnEnable(){if(Application.isPlaying && progress==null)Awake();}
        void Awake()
        {
            progress=new MiningProgressStore(MiningProgressStore.DefaultPath);
            EnsureSlots(Mathf.Max(20,progress.Current.cargo));
            ApplyCargoCount(progress.Current.cargo);
            if(!string.IsNullOrEmpty(SaveError))Debug.LogWarning(SaveError,this);
        }
        void EnsureSlots(int count)
        {
            if(cargoSlots==null||cargoSlots.Length==0)return;
            int old=cargoSlots.Length;
            if(old<count)
            {
                Array.Resize(ref cargoSlots,count);
                for(int i=old;i<count;i++)
                {
                    cargoSlots[i]=Instantiate(cargoSlots[0],cargoSlots[0].transform.parent);
                    cargoSlots[i].name="Cargo Slot "+(i+1);
                    cargoSlots[i].transform.localPosition=new Vector3((i%2-.5f)*.25f,(i/2)*.17f,0);
                }
            }
            slotPositions=new Vector3[cargoSlots.Length];
            for(int i=0;i<cargoSlots.Length;i++)if(cargoSlots[i]){cargoSlots[i].transform.localPosition=new Vector3((i%2-.5f)*.25f,(i/2)*.17f,0);slotPositions[i]=cargoSlots[i].transform.localPosition;}
        }
        public void SetPickaxeVisible(bool visible){if(pickaxe)pickaxe.SetActive(visible);if(!visible)GetComponentInParent<MinerController>()?.CancelMining();}
        public void SetCargoCount(int count)
        {
            count=Mathf.Clamp(count,0,Capacity);
            if(progress!=null&&!progress.TryCommit(count,Coins))return;
            ApplyCargoCount(count);
        }
        void ApplyCargoCount(int count)
        {
            StopAllCoroutines();cargoCount=count;
            if(cargoSlots==null)return;
            for(int i=0;i<cargoSlots.Length;i++)if(cargoSlots[i])
            {
                var slot=cargoSlots[i];slot.SetActive(i<count&&!UsingWheelbarrow);
                if(slotPositions!=null)slot.transform.localPosition=slotPositions[i];
                if(i<count&&progress!=null)
                {
                    int id=progress.Current.items[i];
                    var filter=slot.GetComponent<MeshFilter>();if(filter && rawCargoMesh && ingotCargoMesh)filter.sharedMesh=MeshForItem(id);
                    var renderer=slot.GetComponent<Renderer>();var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",MiningResources.Color(id));renderer.SetPropertyBlock(block);
                }
            }
        }
        public bool Commit(MiningSnapshot state)
        {
            state.cargo=state.items.Length;
            if(!progress.TryCommit(state))return false;
            ApplyCargoCount(state.cargo);return true;
        }
        public bool IsBlockDepleted(string id)=>!string.IsNullOrEmpty(id)&&progress!=null&&Array.IndexOf(progress.Current.depletedBlocks,id)>=0;
        public bool TryBreakIntoDrops(int item,string blockId,Vector3 center)
        {
            if(UsingWheelbarrow||IsFull||progress==null||string.IsNullOrEmpty(blockId)||IsBlockDepleted(blockId)||item<0||item>=6)return false;
            var next=Snapshot;if(next.groundDrops.Length+3>OreDropSystem.PoolSize)return false;
            int n=next.groundDrops.Length;Array.Resize(ref next.groundDrops,n+3);
            for(int i=0;i<3;i++)next.groundDrops[n+i]=new GroundOreState{id=blockId+"/"+i,item=item,position=new Vector3(center.x+(i-1)*.30f,.13f,center.z+(i==1?.20f:-.20f))};
            next.depletedBlocks=next.depletedBlocks.Append(blockId).ToArray();
            if(!Commit(next))return false;
            if(drops)drops.Sync(next.groundDrops,center+Vector3.up*.6f,true);return true;
        }
        public bool TryCollectDrop(string id,Vector3 origin)
        {
            if(IsFull||progress==null)return false;var next=Snapshot;int index=Array.FindIndex(next.groundDrops,d=>d.id==id);if(index<0)return false;
            next.items=next.items.Append(next.groundDrops[index].item).ToArray();next.groundDrops=next.groundDrops.Where((_,i)=>i!=index).ToArray();
            if(!Commit(next))return false;
            if(drops)drops.Sync(next.groundDrops,Vector3.zero,false);
            if(!UsingWheelbarrow&&cargoSlots[cargoCount-1])StartCoroutine(FlyToRack(cargoCount-1,origin));return true;
        }
        public bool TryCollect(Vector3 origin)=>TryCollect(0,origin,null);
        public bool TryCollect(int item,Vector3 origin,string blockId=null)
        {
            if(IsFull||progress==null||!MiningResources.Valid(item)||IsBlockDepleted(blockId))return false;
            var next=Snapshot;next.items=next.items.Append(item).ToArray();
            if(!string.IsNullOrEmpty(blockId))next.depletedBlocks=next.depletedBlocks.Append(blockId).ToArray();
            if(!Commit(next))return false;
            if(!UsingWheelbarrow&&cargoSlots[cargoCount-1])StartCoroutine(FlyToRack(cargoCount-1,origin));return true;
        }
        public bool TryUpgrade(bool capacity)
        {
            int level=capacity?CapacityLevel:DamageLevel,cost=capacity?CapacityCost:DamageCost;
            if(level>=(capacity?15:10)||Coins<cost)return false;
            var next=Snapshot;next.coins-=cost;if(capacity)next.capacityLevel++;else next.damageLevel++;
            return Commit(next);
        }
        public bool TryDeposit(int coinsPerItem,out int earned)
        {
            earned=0;if(cargoCount<=0||coinsPerItem<=0||progress==null)return false;
            long payment=(long)cargoCount*coinsPerItem;if(payment>int.MaxValue-Coins)return false;
            var next=Snapshot;next.items=new int[0];next.coins+=(int)payment;
            if(!Commit(next))return false;earned=(int)payment;return true;
        }
        IEnumerator FlyToRack(int slot,Vector3 origin)
        {
            var item=cargoSlots[slot].transform;
            for(float t=0;t<1;t+=Time.deltaTime/.4f)
            {
                Vector3 destination=item.parent.TransformPoint(slotPositions[slot]);
                item.position=Vector3.Lerp(origin,destination,Mathf.SmoothStep(0,1,t))+Vector3.up*Mathf.Sin(t*Mathf.PI)*.4f;yield return null;
            }
            if(item)item.localPosition=slotPositions[slot];
        }
    }
}






