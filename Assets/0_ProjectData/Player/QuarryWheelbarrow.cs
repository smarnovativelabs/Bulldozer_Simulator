using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DitzelGames.FastIK;
namespace Bulldozer.Player
{
    [DefaultExecutionOrder(150)]
    public sealed class QuarryWheelbarrow : MonoBehaviour
    {
        public MinerController player;public MinerEquipment equipment;public Transform wheel,loadRoot,leftGrip,rightGrip;public Transform[] loadSlots;
        public UnityEngine.UI.Button actionButton;public TMPro.TMP_Text actionText,counter;
        public Material[] stockMaterials;public int unlockCost=60;
        Transform chassis;Quaternion chassisRest,lastRotation;float engagement;float actionCooldown;
        Transform leftTarget,rightTarget,leftHand,rightHand;
        public float LeftGripError {get;private set;}
        public float RightGripError {get;private set;}
        public bool Pushing=>equipment&&equipment.UsingWheelbarrow;
        Transform backpackRoot;Vector3 dock,lastPlayer,lastWheel;float originalSpeed,saveAt,poll;FastIKFabric leftIK,rightIK;Transform oldRightTarget;BoxCollider bodyCollider;readonly Collider[] overlaps=new Collider[16];
        void Start()
        {
            backpackRoot=equipment.cargoRoot;dock=transform.position;originalSpeed=player.runSpeed;bodyCollider=GetComponent<BoxCollider>();
            equipment.drops.carrierPickup=loadRoot;chassis=wheel?wheel.parent:null;if(chassis)chassisRest=chassis.localRotation;
            var s=equipment.Snapshot;if(s.wheelbarrowUnlocked&&s.barrowPosition!=Vector3.zero)transform.position=s.barrowPosition;
            // Resume parked, retaining both inventories; never force a player into a moving carrier on load.
            if(s.usingWheelbarrow){s.barrowItems=s.items;s.items=s.backpackItems;s.backpackItems=Array.Empty<int>();s.usingWheelbarrow=false;equipment.Commit(s);}
            actionButton.onClick.AddListener(Interact);
            var animator=player.visual.GetComponent<Animator>();rightIK=player.visual.GetComponent<MinerAnimator>().rightHandIK;oldRightTarget=rightIK.Target;
            leftHand=animator.GetBoneTransform(HumanBodyBones.LeftHand);rightHand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftTarget=new GameObject("Cart left wrist target").transform;leftTarget.SetParent(transform,false);
            rightTarget=new GameObject("Cart right wrist target").transform;rightTarget.SetParent(transform,false);
            leftIK=leftHand.gameObject.AddComponent<FastIKFabric>();leftIK.enabled=false;leftIK.Iterations=8;var generatedTarget=leftIK.Target;leftIK.Target=leftTarget;if(generatedTarget)Destroy(generatedTarget.gameObject);
            lastPlayer=player.transform.position;lastWheel=transform.position;lastRotation=player.visual.rotation;Refresh();
        }
        public void Interact()
        {
            if(player.InputBlocked||player.IsMining||Time.time<actionCooldown)return;var s=equipment.Snapshot;float distance=Vector3.Distance(player.transform.position,transform.position);
            if(!s.wheelbarrowUnlocked)
            {
                if(distance>2.5f||s.coins<unlockCost)return;s.coins-=unlockCost;s.wheelbarrowUnlocked=true;s.barrowPosition=transform.position;if(!equipment.Commit(s))return;
            }
            else if(!Pushing&&distance>2.5f){s.barrowPosition=dock;if(equipment.Commit(s))transform.position=dock;Refresh();return;}
            s=equipment.Snapshot;
            if(!Pushing&&Blocked(CartPosition(),player.visual.rotation))
            {player.visual.GetComponent<MinerMining>().ShowStatus("Face clear ground before using the wheelbarrow");return;}
            if(!Pushing){s.backpackItems=s.items;s.items=s.barrowItems;s.barrowItems=Array.Empty<int>();s.usingWheelbarrow=true;}
            else{s.barrowItems=s.items;s.items=s.backpackItems;s.backpackItems=Array.Empty<int>();s.usingWheelbarrow=false;}
            s.barrowPosition=transform.position;if(!equipment.Commit(s))return;
            equipment.cargoRoot=Pushing?loadRoot:backpackRoot;equipment.SetPickaxeVisible(!Pushing);player.CancelMining();player.touchControls.Move=Vector2.zero;
            player.runSpeed=Pushing?2.5f:originalSpeed;bodyCollider.enabled=!Pushing;lastPlayer=player.transform.position;Refresh();
            lastRotation=player.visual.rotation;
            if(Pushing)transform.SetPositionAndRotation(CartPosition(),lastRotation);
            lastWheel=transform.position;equipment.drops.ResetPickupSweep();
            actionCooldown=Time.time+.35f;
            player.visual.GetComponent<MinerMining>().ShowStatus(Pushing?"Wheelbarrow ready - push over ore to collect":"Wheelbarrow parked - cargo kept inside");
            var dust=equipment.drops.dust;if(dust){var burst=new ParticleSystem.EmitParams();burst.position=transform.position+Vector3.up*.08f;burst.startSize=.3f;burst.startLifetime=.4f;burst.startColor=new Color(.65f,.59f,.48f,.35f);burst.velocity=Vector3.up*.25f;dust.Emit(burst,6);}
        }
        void Update()
        {
            if(Keyboard.current!=null&&Keyboard.current.fKey.wasPressedThisFrame)Interact();
            if(Time.time>=poll){poll=Time.time+.2f;Refresh();}
            if(Pushing&&Time.time>=saveAt){saveAt=Time.time+2;var s=equipment.Snapshot;s.barrowPosition=transform.position;equipment.Commit(s);}
        }
        void LateUpdate()
        {
            engagement=Mathf.MoveTowards(engagement,Pushing?1:0,Time.deltaTime*4);
            if(chassis)chassis.localRotation=Quaternion.AngleAxis(-4*engagement,Vector3.right)*chassisRest;
            if(Pushing)
            {
                Quaternion rotation=player.visual.rotation;Vector3 target=CartPosition();
                if(Blocked(target,rotation)){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=lastPlayer;cc.enabled=true;player.visual.rotation=lastRotation;}
                else{transform.SetPositionAndRotation(target,rotation);lastPlayer=player.transform.position;lastRotation=rotation;}
                float distance=Vector3.Dot(transform.position-lastWheel,transform.forward);if(wheel)wheel.Rotate(Vector3.forward,distance/.18f*Mathf.Rad2Deg,Space.Self);lastWheel=transform.position;
            }
            if(engagement<=0)return;
            float blend=Mathf.SmoothStep(0,1,engagement);
            leftTarget.SetPositionAndRotation(Vector3.Lerp(leftHand.position,leftGrip.position,blend),Quaternion.Slerp(leftHand.rotation,leftGrip.rotation,blend));
            rightTarget.SetPositionAndRotation(Vector3.Lerp(rightHand.position,rightGrip.position,blend),Quaternion.Slerp(rightHand.rotation,rightGrip.rotation,blend));
            rightIK.Target=rightTarget;rightIK.Solve();leftIK.Solve();rightIK.Target=oldRightTarget;
            leftHand.rotation=leftTarget.rotation;rightHand.rotation=rightTarget.rotation;
            LeftGripError=Vector3.Distance(leftHand.position,leftGrip.position);RightGripError=Vector3.Distance(rightHand.position,rightGrip.position);
        }
        Vector3 CartPosition(){var pos=player.transform.position+player.visual.forward*.95f;pos.y=.05f;return pos;}
        bool Blocked(Vector3 target,Quaternion rotation)
        {
            int n=Physics.OverlapBoxNonAlloc(target+Vector3.up*.55f,new Vector3(.38f,.32f,.55f),overlaps,rotation,~0,QueryTriggerInteraction.Ignore);
            if(n==overlaps.Length)return true;
            for(int i=0;i<n;i++)if(!overlaps[i].transform.IsChildOf(player.transform)&&!overlaps[i].transform.IsChildOf(transform))return true;
            return false;
        }
        void Refresh()
        {
            var s=equipment.Snapshot;bool near=Vector3.Distance(player.transform.position,transform.position)<2.5f;
            actionButton.gameObject.SetActive(near||s.wheelbarrowUnlocked);actionButton.interactable=!player.InputBlocked&&(s.wheelbarrowUnlocked||s.coins>=unlockCost);
            actionText.text=!s.wheelbarrowUnlocked?"UNLOCK CART  $"+unlockCost:Pushing?"PARK CART [F]":near?"USE CART [F]":"RECALL CART [F]";
            var items=Pushing?s.items:s.barrowItems;
            counter.text=!s.wheelbarrowUnlocked?"QUARRY CADDY\n$"+unlockCost:"CADDY "+items.Length+"/12";
            counter.gameObject.SetActive(near&&!Pushing);
            if(Camera.main)counter.transform.rotation=Camera.main.transform.rotation;
            for(int i=0;i<loadSlots.Length;i++){loadSlots[i].gameObject.SetActive(i<items.Length);if(i>=items.Length)continue;loadSlots[i].GetComponent<MeshFilter>().sharedMesh=equipment.MeshForItem(items[i]);loadSlots[i].GetComponent<MeshRenderer>().sharedMaterial=stockMaterials[items[i]];}
        }
        void OnDisable(){if(player)player.runSpeed=originalSpeed;if(equipment&&backpackRoot)equipment.cargoRoot=backpackRoot;if(rightIK&&oldRightTarget)rightIK.Target=oldRightTarget;}
    }
}
