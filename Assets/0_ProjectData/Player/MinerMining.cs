using UnityEngine;
namespace Bulldozer.Player
{
    // Evaluated after MinerAnimator/Fast IK. Contact probes are anchored to the actual steel head.
    [DefaultExecutionOrder(200)]
    public sealed class MinerMining : MonoBehaviour
    {
        public MinerController controller;
        public MinerEquipment equipment;
        public Transform[] headProbes;
        public float reach=1.65f;
        public float contactRadius=.08f;
        // The compact boulder needs a closer stance than the tall quarry blocks.
        public float naturalStrikeDistance=1.1f;
        public TMPro.TMP_Text cargoLabel;
        public bool guidanceOwnsInstructions;
        public MineableRock Target { get; private set; }
        public int SuccessfulHits { get; private set; }
        public string Status { get; private set; }="Move close to a rock and stop to mine";
        readonly Collider[] nearby=new Collider[32];
        readonly RaycastHit[] sweep=new RaycastHit[16];
        Vector3[] previous;
        bool contactOpen,hitThisSwing,tracking,previousContactOpen;
        public void ShowStatus(string message) { Status=message; }
        public bool TryBeginSwing()
        {
            contactOpen=hitThisSwing=tracking=previousContactOpen=false;Target=null;
            if(equipment.UsingWheelbarrow){Status="Park the wheelbarrow before mining";return false;}
            if(equipment.IsFull){Status="Rack full - deliver raw ore to REFINING or ingots to SELL";return false;}
            Vector3 origin=controller.transform.position+Vector3.up*.65f;
            int count=Physics.OverlapSphereNonAlloc(origin,reach,nearby,~0,QueryTriggerInteraction.Ignore);
            float score=float.MaxValue;
            bool moveCloser=false;
            for(int i=0;i<count;i++)
            {
                var rock=nearby[i].GetComponentInParent<MineableRock>();
                if(!rock || rock.Depleted)continue;
                Vector3 delta=rock.HitCollider.bounds.center-controller.transform.position;delta.y=0;
                float angle=Vector3.Angle(transform.forward,delta);
                float distance=Vector3.Distance(origin,rock.HitCollider.ClosestPoint(origin));
                if(angle>65 || distance>reach || !ClearPath(rock))continue;
                Vector3 stance=rock.transform.position-controller.transform.position;stance.y=0;
                if(rock.naturalBoulder&&stance.sqrMagnitude>naturalStrikeDistance*naturalStrikeDistance)
                {moveCloser=true;continue;}
                float value=distance+angle*.01f;if(value<score){score=value;Target=rock;}
            }
            if(Target)
            {
                Vector3 direction=Target.HitCollider.bounds.center-controller.transform.position;direction.y=0;
                if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction);
                controller.MineWall=Target.wallMining;Status="Mining";
            }
            else if(moveCloser)Status="Move closer to the rock to strike";
            return Target!=null;
        }
        bool ClearPath(MineableRock rock)
        {
            Vector3 start=controller.transform.position+Vector3.up*.75f;
            Vector3 end=rock.HitCollider.ClosestPoint(start);
            Vector3 direction=end-start;float distance=direction.magnitude;
            int n=Physics.RaycastNonAlloc(start,direction.normalized,sweep,distance,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<n;i++)if(!sweep[i].transform.IsChildOf(controller.transform) && sweep[i].collider!=rock.HitCollider)return false;
            return true;
        }
        // Animation events open only the downward/forward strike portion of each clip.
        public void BeginMiningContact(){if(controller.IsMining)contactOpen=true;}
        public void EndMiningContact(){contactOpen=false;}
        void LateUpdate()
        {
            if(cargoLabel)cargoLabel.text=$"{(equipment.UsingWheelbarrow?"CART":"CARGO")} {equipment.CargoCount}/{equipment.Capacity}  |  CASH ${equipment.Cash}  |  {(!string.IsNullOrEmpty(equipment.SaveError)?"SAVE FAILED - progress unchanged":equipment.IsFull?"RACK FULL - REFINING / SELL":Status)}";
            if(cargoLabel&&guidanceOwnsInstructions)cargoLabel.text=$"CASH ${equipment.Cash}"+(!string.IsNullOrEmpty(equipment.SaveError)?"  |  SAVE FAILED - progress unchanged":"");
            if(headProbes==null || headProbes.Length==0)return;
            if(previous==null || previous.Length!=headProbes.Length)previous=new Vector3[headProbes.Length];
            bool active=controller.IsMining && equipment.PickaxeVisible && !controller.InputBlocked && controller.Grounded;
            if(!active){tracking=false;contactOpen=previousContactOpen=false;return;}
            for(int i=0;i<headProbes.Length;i++)
            {
                Vector3 point=headProbes[i].position;
                if(tracking && contactOpen && !hitThisSwing && Target && !Target.Depleted && !equipment.IsFull && ClearPath(Target))
                {
                    var collider=Target.HitCollider;
                    bool touched=Vector3.Distance(collider.ClosestPoint(point),point)<=contactRadius;
                    Vector3 delta=point-previous[i];
                    // Never sweep the wind-up arc when the contact window first opens.
                    if(!touched && previousContactOpen && delta.sqrMagnitude>.000001f)
                    {
                        int count=Physics.SphereCastNonAlloc(previous[i],contactRadius,delta.normalized,sweep,delta.magnitude,~0,QueryTriggerInteraction.Ignore);
                        for(int h=0;h<count;h++)if(sweep[h].collider==collider){touched=true;break;}
                    }
                    if(touched && Vector3.Distance(controller.transform.position,collider.ClosestPoint(controller.transform.position))<=reach)
                    {
                        hitThisSwing=true;
                        if(Target.TryHit(equipment,collider.ClosestPoint(point)))
                        {SuccessfulHits++;Bulldozer.UI.QuarryAudio.Play(Bulldozer.UI.QuarryAudio.Cue.Hit,point);if(Target.Depleted)Bulldozer.UI.QuarryAudio.Play(Bulldozer.UI.QuarryAudio.Cue.Break,point);Status=Target.Depleted?"Rock broken - walk over the stones to collect":$"Rock {Target.Health}/{Target.maxHealth}";}
                    }
                }
                previous[i]=point;
            }
            tracking=true;
            previousContactOpen=contactOpen;
        }
    }
}






