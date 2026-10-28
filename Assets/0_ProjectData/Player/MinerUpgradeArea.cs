using UnityEngine;
namespace Bulldozer.Player
{
    public sealed class MinerUpgradeArea : MonoBehaviour
    {
        public BoxCollider zone;
        public GameObject panel;
        public UnityEngine.UI.Button damageButton,capacityButton;
        public TMPro.TMP_Text damageText,capacityText,feedback;
        public UnityEngine.UI.Button speedButton;
        public TMPro.TMP_Text speedText;
        MinerController player;MinerEquipment equipment;
        void Start()
        {
            player=FindFirstObjectByType<MinerController>();equipment=player.visual.GetComponent<MinerEquipment>();
            damageButton.onClick.AddListener(()=>Purchase(false));capacityButton.onClick.AddListener(()=>Purchase(true));panel.SetActive(false);
            if(speedButton)speedButton.onClick.AddListener(()=>PurchaseSpeed());
        }
        void Update()
        {
            bool inside=player&&!player.InputBlocked&&MiningProduction.Contains(zone,player.transform.position);panel.SetActive(inside);if(!inside)return;
            damageText.text=equipment.DamageLevel>=10?"PICKAXE MAX":$"PICKAXE {equipment.Damage} > {equipment.Damage+1}\n${equipment.DamageCost}";
            capacityText.text=equipment.CapacityLevel>=15?"RACK MAX":$"RACK {equipment.Capacity} > {equipment.Capacity+1}\n${equipment.CapacityCost}";
            damageButton.interactable=equipment.DamageLevel<10&&equipment.Coins>=equipment.DamageCost;
            capacityButton.interactable=equipment.CapacityLevel<15&&equipment.Coins>=equipment.CapacityCost;
            if(speedText)speedText.text=equipment.SpeedLevel>=5?"SPEED MAX":$"SPEED {Mathf.RoundToInt(equipment.MovementMultiplier*100)}% > {Mathf.RoundToInt((equipment.MovementMultiplier+.08f)*100)}%\n${equipment.SpeedCost}";
            if(speedButton)speedButton.interactable=equipment.SpeedLevel<5&&equipment.Cash>=equipment.SpeedCost;
        }
        public bool PurchaseSpeed()
        {
            if(!player||player.InputBlocked||player.IsMining||!MiningProduction.Contains(zone,player.transform.position))return false;
            bool ok=equipment.TryUpgradeSpeed();feedback.text=ok?"SPEED UPGRADE SAVED":string.IsNullOrEmpty(equipment.SaveError)?"NOT ENOUGH CASH OR MAX LEVEL":"SAVE FAILED - PURCHASE CANCELLED";return ok;
        }
        public bool Purchase(bool capacity)
        {
            if(!player||player.InputBlocked||player.IsMining||!MiningProduction.Contains(zone,player.transform.position))return false;
            bool ok=equipment.TryUpgrade(capacity);feedback.text=ok?"UPGRADE SAVED":string.IsNullOrEmpty(equipment.SaveError)?"NOT ENOUGH CASH OR MAX LEVEL":"SAVE FAILED - PURCHASE CANCELLED";return ok;
        }
        void OnDisable(){if(panel)panel.SetActive(false);}
    }
}

