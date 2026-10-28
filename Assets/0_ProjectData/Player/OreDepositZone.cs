using UnityEngine;
namespace Bulldozer.Player
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class OreDepositZone : MonoBehaviour
    {
        [Min(1)] public int coinsPerItem=10;
        BoxCollider zone;
        public int Deposits { get; private set; }
        void Awake(){zone=GetComponent<BoxCollider>();var sign=GetComponentInChildren<TMPro.TMP_Text>();if(sign)sign.text=$"DEPOSIT ORE\n${coinsPerItem} EACH";}
        void OnTriggerEnter(Collider other){Process(other);}
        void OnTriggerStay(Collider other){Process(other);}
        void Process(Collider other)
        {
            var player=other.GetComponentInParent<MinerController>();
            if(player)TryDeposit(player);
        }
        public bool TryDeposit(MinerController player)
        {
            if(!player || !zone || player.InputBlocked || player.IsMining)return false;
            // Validate presence even if called by another gameplay system.
            Vector3 position=player.transform.position;
            if((zone.ClosestPoint(position)-position).sqrMagnitude>.001f)return false;
            var equipment=player.visual.GetComponent<MinerEquipment>();
            if(!equipment || !equipment.TryDeposit(coinsPerItem,out int earned))return false;
            Deposits++;
            player.visual.GetComponent<MinerMining>()?.ShowStatus($"Deposited ore: +${earned} cash");
            return true;
        }
    }
}


