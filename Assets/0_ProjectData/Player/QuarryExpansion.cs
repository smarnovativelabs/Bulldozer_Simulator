using UnityEngine;
using Bulldozer.Player.Persistence;

namespace Bulldozer.Player
{
    [DefaultExecutionOrder(-50)]
    public sealed class QuarryExpansion : MonoBehaviour
    {
        public MinerEquipment equipment;
        public GameObject gate;
        public MineableRock[] deposits;
        public TMPro.TMP_Text sign;
        void Start()
        {
            var state=equipment.Snapshot;
            bool open=QuarryProgressionRules.AreaAvailable(state);
            gate.SetActive(!open);
            foreach(var rock in deposits)
            {
                rock.enabled=open;
                foreach(var collider in rock.GetComponentsInChildren<Collider>())collider.enabled=open;
            }
            sign.text=open?"COPPER RIDGE\nOPEN - 6 COPPER DEPOSITS":"COPPER RIDGE\nFINISH 3 EXCAVATIONS TO OPEN";
        }
    }
}
