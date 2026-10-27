using System;
using System.Linq;
using UnityEngine;
namespace Bulldozer.Player
{
    public sealed class MiningProduction : MonoBehaviour
    {
        public BoxCollider inputZone,outputZone,sellZone;
        public TMPro.TMP_Text inputLabel,outputLabel,truckLabel;
        public Transform inputStack,outputStack,truckStack,truckVisual;
        public GameObject stackTemplate;
        public Mesh rawMesh,ingotMesh;
        public Light furnaceLight;
        public ProductionEffects effects;
        public TruckDelivery delivery;
        public RewardCoinFeedback rewardFeedback;
        public Material[] stockMaterials;
        int[] departingItems=Array.Empty<int>();
        int[] shownInput,shownOutput,shownTruck;
        [Min(.1f)] public float secondsPerItem=1.5f;
        MinerEquipment equipment;MinerController player;
        float poll,processing,transfer;


        public int TruckCapacity=>2+equipment.TrucksCompleted%4;
        void Start(){player=FindFirstObjectByType<MinerController>();equipment=player.visual.GetComponent<MinerEquipment>();Prewarm(inputStack);Prewarm(outputStack);Prewarm(truckStack);Refresh();}
        void Prewarm(Transform root)
        {
            while(root.childCount<8){var go=Instantiate(stackTemplate,root);go.name="Stored Item";go.SetActive(false);}
        }
        public static bool Contains(BoxCollider zone,Vector3 position)=>zone&&(zone.ClosestPoint(position)-position).sqrMagnitude<.001f;
        void Update()
        {
            if(!equipment)return;
            if(equipment.RefineryInputCount>0&&equipment.RefineryOutputCount<100)
            {
                processing+=Time.deltaTime;
                if(processing>=secondsPerItem){if(ProcessOne())processing=0;else processing=secondsPerItem;}
            }
            else processing=0;
            if(furnaceLight)furnaceLight.intensity=equipment.RefineryInputCount>0?2.2f+Mathf.Sin(Time.time*14)*.3f:.3f;

            transfer-=Time.deltaTime;
            if(transfer<=0&&!player.InputBlocked&&!player.IsMining)
            {
                transfer=.35f;
                if(Contains(inputZone,player.transform.position))DepositRaw();
                else if(Contains(outputZone,player.transform.position))CollectOutput();
                else if(Contains(sellZone,player.transform.position)&&(!delivery||!delivery.IsBusy))LoadTruck();
            }
            poll-=Time.deltaTime;if(poll<=0){poll=.25f;Refresh();}
        }
        public bool DepositRaw()
        {
            var state=equipment.Snapshot;int i=Array.FindIndex(state.items,id=>!MiningResources.Refined(id));
            if(i<0||state.refineryInput.Length>=100)return false;
            int movedItem=state.items[i];state.refineryInput=state.refineryInput.Append(movedItem).ToArray();state.items=state.items.Where((_,index)=>index!=i).ToArray();
            if(!equipment.Commit(state))return false;if(effects)effects.Fly(movedItem,equipment.cargoRoot.position,effects.hopper.position);Status("Raw ore delivered to furnace");return true;
        }
        public bool ProcessOne()
        {
            var state=equipment.Snapshot;if(state.refineryInput.Length==0||state.refineryOutput.Length>=100)return false;
            int output=state.refineryInput[0]+6;state.refineryOutput=state.refineryOutput.Append(output).ToArray();state.refineryInput=state.refineryInput.Skip(1).ToArray();if(!equipment.Commit(state))return false;if(effects)effects.Finished(output);return true;
        }
        public bool CollectOutput()
        {
            var state=equipment.Snapshot;if(equipment.IsFull||state.refineryOutput.Length==0)return false;
            int movedItem=state.refineryOutput[0];state.items=state.items.Append(movedItem).ToArray();state.refineryOutput=state.refineryOutput.Skip(1).ToArray();
            if(!equipment.Commit(state))return false;if(effects)effects.Fly(movedItem,effects.tray.position,equipment.cargoRoot.position,equipment.cargoRoot);Status("Product collected - take it to SELL");return true;
        }
        public bool LoadTruck()
        {
            var state=equipment.Snapshot;int i=Array.FindIndex(state.items,MiningResources.Refined);if(i<0||(delivery&&delivery.IsBusy))return false;
            int movedItem=state.items[i];state.truckItems=state.truckItems.Append(movedItem).ToArray();state.items=state.items.Where((_,index)=>index!=i).ToArray();
            bool complete=state.truckItems.Length>=TruckCapacity;int payment=0;
            var loadedItems=state.truckItems;
            if(complete)
            {
                payment=state.truckItems.Sum(id=>MiningResources.Prices[id%6]);if(state.coins>int.MaxValue-payment||state.trucksCompleted==int.MaxValue)return false;
                state.coins+=payment;state.truckItems=new int[0];state.trucksCompleted++;
            }
            if(!equipment.Commit(state))return false;
            if(effects)effects.Fly(movedItem,equipment.cargoRoot.position,truckStack.position,truckStack);
            if(complete){departingItems=loadedItems;if(delivery)delivery.Dispatch();Refresh();Status("Truck dispatched: +"+payment+" coins");if(rewardFeedback)rewardFeedback.PlayReward(truckStack.position,payment);}else Status("Ingot loaded on truck");return true;
        }
        void Status(string text)=>player.visual.GetComponent<MinerMining>().ShowStatus(text);
        void Refresh()
        {
            var s=equipment.Snapshot;
            inputLabel.text="REFINING\nLOAD: "+s.refineryInput.Length;
            outputLabel.text="COLLECT INGOTS\nLOAD: "+s.refineryOutput.Length;
            truckLabel.text=delivery&&delivery.IsBusy?(delivery.Phase==TruckDelivery.DeliveryPhase.Arriving?"EMPTY TRUCK ARRIVING":"TRUCK DISPATCHED"):"SELL INGOTS\nTRUCK: "+s.truckItems.Length+"/"+TruckCapacity;
            if(!delivery||!delivery.IsBusy)departingItems=Array.Empty<int>();
            Stack(inputStack,s.refineryInput,ref shownInput);Stack(outputStack,s.refineryOutput,ref shownOutput);
            Stack(truckStack,delivery&&delivery.ShowDepartingCargo?departingItems:s.truckItems,ref shownTruck);
        }
        void Stack(Transform root,int[] items,ref int[] shown)
        {
            if(shown!=null&&shown.SequenceEqual(items))return;
            shown=items;
            int visible=Mathf.Min(items.Length,8);
            while(root.childCount<visible){var go=Instantiate(stackTemplate,root);go.name="Stored Item";}
            for(int i=0;i<root.childCount;i++)
            {
                var item=root.GetChild(i);item.gameObject.SetActive(i<visible);if(i>=visible)continue;
                item.localPosition=new Vector3((i%2)*.26f,(i/4)*.16f,(i/2%2)*.35f);item.localScale=Vector3.one;item.localRotation=Quaternion.identity;
                item.GetComponent<MeshFilter>().sharedMesh=equipment.MeshForItem(items[i]);
                var renderer=item.GetComponent<Renderer>();if(stockMaterials!=null&&stockMaterials.Length==12)renderer.sharedMaterial=stockMaterials[items[i]];
            }
        }
    }
}





