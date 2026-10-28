using System;
using System.Linq;
using UnityEngine;
namespace Bulldozer.Player
{
    public sealed class MiningProduction : MonoBehaviour
    {
        public BoxCollider inputZone,outputZone,sellZone;
        public TMPro.TMP_Text inputLabel,outputLabel,truckLabel,refineryTimer,orderLabel;
        public Transform inputStack,outputStack,truckStack,truckVisual,loadingStack;
        public GameObject stackTemplate;
        public Mesh rawMesh,ingotMesh;
        public Light furnaceLight;
        public ProductionEffects effects;
        public TruckDelivery delivery,secondDelivery;
        public RewardCoinFeedback rewardFeedback;
        public Material[] stockMaterials;
        [Min(.1f)] public float secondsPerItem=1.5f;
        MinerEquipment equipment; MinerController player;
        int[] shownInput,shownOutput,shownLoading;
        readonly int[][] shownTrucks=new int[2][];
        readonly int[][] departingItems={Array.Empty<int>(),Array.Empty<int>()};
        int activeTruck;
        float poll,processing,transfer,inputReady,loadingReady,truckTransfer;
        bool feeding;
        public int TruckCapacity=>2+(equipment?equipment.TrucksCompleted:0)%4;
        public float ProcessingRemaining=>Mathf.Clamp(secondsPerItem+.65f-processing,0,secondsPerItem);
        public TruckDelivery ActiveTruck=>activeTruck==0?delivery:secondDelivery;
        TruckDelivery Truck(int index)=>index==0?delivery:secondDelivery;
        void Start()
        {
            player=FindFirstObjectByType<MinerController>();equipment=player.visual.GetComponent<MinerEquipment>();
            Prewarm(inputStack);Prewarm(outputStack);Prewarm(truckStack);
            if(secondDelivery)Prewarm(secondDelivery.cargo);if(loadingStack)Prewarm(loadingStack);
            inputReady=loadingReady=Time.time+.85f;Refresh();
        }
        void Prewarm(Transform root)
        {
            if(!root)return;
            while(root.childCount<8){var go=Instantiate(stackTemplate,root);go.name="Stored Item";go.SetActive(false);}
        }
        public static bool Contains(BoxCollider zone,Vector3 position)=>zone&&(zone.ClosestPoint(position)-position).sqrMagnitude<.001f;
        void Update()
        {
            if(!equipment)return;
            if(equipment.RefineryInputCount>0&&equipment.RefineryOutputCount<100&&Time.time>=inputReady)
            {
                if(!feeding){feeding=true;if(effects)effects.Fly(equipment.Snapshot.refineryInput[0],inputStack.position,effects.hopper.position);Refresh();}
                processing+=Time.deltaTime;
                // Feed flight precedes the processing countdown.
                if(processing>=secondsPerItem+.65f&&ProcessOne()){processing=0;feeding=false;}
            }
            else {processing=0;feeding=false;}
            if(furnaceLight)furnaceLight.intensity=feeding?2.2f+Mathf.Sin(Time.time*14)*.3f:.3f;
            transfer-=Time.deltaTime;
            if(transfer<=0&&!player.InputBlocked&&!player.IsMining)
            {
                transfer=.35f;
                if(Contains(inputZone,player.transform.position))DepositRaw();
                else if(Contains(outputZone,player.transform.position))CollectOutput();
                else if(Contains(sellZone,player.transform.position))LoadTruck();
            }
            truckTransfer-=Time.deltaTime;
            if(truckTransfer<=0&&Time.time>=loadingReady){truckTransfer=.8f;TransferToTruck();}
            poll-=Time.deltaTime;if(poll<=0){poll=.1f;Refresh();}
        }
        public bool DepositRaw()
        {
            var s=equipment.Snapshot;int i=Array.FindIndex(s.items,id=>!MiningResources.Refined(id));
            if(i<0||s.refineryInput.Length>=100)return false;
            bool empty=s.refineryInput.Length==0;int id=s.items[i];s.refineryInput=s.refineryInput.Append(id).ToArray();s.items=s.items.Where((_,n)=>n!=i).ToArray();
            if(!equipment.Commit(s))return false;
            if(empty)inputReady=Time.time+.9f;
            if(effects)effects.Fly(id,equipment.cargoRoot.position,inputStack.position);
            Status("Stone placed on refinery input");return true;
        }
        public bool ProcessOne()
        {
            var s=equipment.Snapshot;if(s.refineryInput.Length==0||s.refineryOutput.Length>=100)return false;
            int id=s.refineryInput[0]+6;s.refineryOutput=s.refineryOutput.Append(id).ToArray();s.refineryInput=s.refineryInput.Skip(1).ToArray();
            if(!equipment.Commit(s))return false;if(effects)effects.Finished(id);return true;
        }
        public bool CollectOutput()
        {
            var s=equipment.Snapshot;if(equipment.IsFull||s.refineryOutput.Length==0)return false;
            int id=s.refineryOutput[0];s.items=s.items.Append(id).ToArray();s.refineryOutput=s.refineryOutput.Skip(1).ToArray();
            if(!equipment.Commit(s))return false;if(effects)effects.Fly(id,effects.tray.position,equipment.cargoRoot.position,equipment.cargoRoot);
            Status("Finished stone collected - take it to LOADING");return true;
        }
        // The ground buffer accepts goods even when both vehicles are travelling.
        public bool LoadTruck()
        {
            var s=equipment.Snapshot;int i=Array.FindIndex(s.items,MiningResources.Refined);
            if(i<0||s.loadingQueue.Length>=100)return false;
            bool empty=s.loadingQueue.Length==0;int id=s.items[i];s.loadingQueue=s.loadingQueue.Append(id).ToArray();s.items=s.items.Where((_,n)=>n!=i).ToArray();
            if(!equipment.Commit(s))return false;if(empty)loadingReady=Time.time+.9f;
            if(effects)effects.Fly(id,equipment.cargoRoot.position,loadingStack.position);
            Status("Stone placed on loading pad");return true;
        }
        public bool TransferToTruck()
        {
            var s=equipment.Snapshot;if(s.loadingQueue.Length==0)return false;
            if(ActiveTruck&&ActiveTruck.IsBusy)
            {
                int other=1-activeTruck;if(s.truckItems.Length>0||!Truck(other)||Truck(other).IsBusy)return false;activeTruck=other;
            }
            var truck=ActiveTruck;if(!truck||truck.IsBusy)return false;
            int id=s.loadingQueue[0];s.loadingQueue=s.loadingQueue.Skip(1).ToArray();s.truckItems=s.truckItems.Append(id).ToArray();
            int capacity=TruckCapacity;bool complete=s.truckItems.Length>=capacity;int payment=0;bool bonus=false;var loaded=s.truckItems;
            if(complete)
            {
                bonus=(s.trucksCompleted+1)%3==0;
                payment=s.truckItems.Sum(item=>MiningResources.Prices[item%6])+(bonus?30:0);
                if(s.coins>int.MaxValue-payment||s.trucksCompleted==int.MaxValue)return false;
                s.coins+=payment;s.trucksCompleted++;s.truckItems=Array.Empty<int>();
            }
            if(!equipment.Commit(s))return false;
            if(effects)effects.Fly(id,loadingStack.position,truck.cargo.position,truck.cargo);
            if(complete)
            {
                departingItems[activeTruck]=loaded;truck.loadedCount=loaded.Length;truck.orderCapacity=capacity;truck.Dispatch();
                Status("Delivery +$"+payment+(bonus?" | 3-delivery bonus +$30!":""));
                if(rewardFeedback)rewardFeedback.PlayReward(truck.cargo.position,payment);
                if(Truck(1-activeTruck))activeTruck=1-activeTruck;
            }
            Refresh();return true;
        }
        void Status(string text)=>player.visual.GetComponent<MinerMining>().ShowStatus(text);
        void Refresh()
        {
            var s=equipment.Snapshot;
            inputLabel.text="REFINERY INPUT\n"+s.refineryInput.Length+" STONES";
            outputLabel.text="COLLECT\n"+s.refineryOutput.Length+" READY";
            truckLabel.text="LOADING PAD\n"+s.loadingQueue.Length+" QUEUED";
            if(refineryTimer)refineryTimer.text=s.refineryOutput.Length>=100?"OUTPUT FULL":s.refineryInput.Length==0?"ADD STONE":!feeding||processing<.65f?"FEEDING":MiningResources.Names[s.refineryInput[0]].ToUpperInvariant()+"\n"+Mathf.Max(0,secondsPerItem+.65f-processing).ToString("0.0")+"s";
            if(orderLabel)orderLabel.text="DELIVERY BONUS "+(s.trucksCompleted%3)+"/3\n3 DELIVERIES = +$30";
            Stack(inputStack,feeding?s.refineryInput.Skip(1).ToArray():s.refineryInput,ref shownInput);
            Stack(outputStack,s.refineryOutput,ref shownOutput);Stack(loadingStack,s.loadingQueue,ref shownLoading);
            for(int i=0;i<2;i++)
            {
                var truck=Truck(i);if(!truck)continue;
                if(!truck.IsBusy)departingItems[i]=Array.Empty<int>();
                int[] items=truck.ShowDepartingCargo?departingItems[i]:i==activeTruck&&!truck.IsBusy?s.truckItems:Array.Empty<int>();
                Stack(truck.cargo,items,ref shownTrucks[i]);
                if(!truck.IsBusy){truck.loadedCount=items.Length;truck.orderCapacity=TruckCapacity;}
            }
        }
        void Stack(Transform root,int[] items,ref int[] shown)
        {
            if(!root||shown!=null&&shown.SequenceEqual(items))return;shown=items;
            int visible=Mathf.Min(items.Length,8);
            for(int i=0;i<root.childCount;i++)
            {
                var item=root.GetChild(i);item.gameObject.SetActive(i<visible);if(i>=visible)continue;
                item.localPosition=new Vector3((i%2)*.26f,(i/4)*.16f,(i/2%2)*.35f);item.localScale=Vector3.one;item.localRotation=Quaternion.identity;
                item.GetComponent<MeshFilter>().sharedMesh=equipment.MeshForItem(items[i]);
                if(stockMaterials!=null&&stockMaterials.Length==12)item.GetComponent<Renderer>().sharedMaterial=stockMaterials[items[i]];
            }
        }
    }
}

