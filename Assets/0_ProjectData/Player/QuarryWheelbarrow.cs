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
        Transform chassis;Quaternion chassisRest;float engagement;float actionCooldown;
        public bool Pushing=>equipment&&equipment.UsingWheelbarrow;
        Transform backpackRoot;Vector3 dock,lastPlayer,lastWheel;float originalSpeed,saveAt,poll;FastIKFabric leftIK,rightIK;Transform oldRightTarget;BoxCollider bodyCollider;readonly Collider[] overlaps=new Collider[16];
        void Start()
        {
            backpackRoot=equipment.cargoRoot;dock=transform.position;originalSpeed=player.runSpeed;bodyCollider=GetComponent<BoxCollider>();
            equipment.drops.carrierPickup=transform;chassis=wheel?wheel.parent:null;if(chassis)chassisRest=chassis.localRotation;
            var s=equipment.Snapshot;if(s.wheelbarrowUnlocked&&s.barrowPosition!=Vector3.zero)transform.position=s.barrowPosition;
            // Resume parked, retaining both inventories; never force a player into a moving carrier on load.
            if(s.usingWheelbarrow){s.barrowItems=s.items;s.items=s.backpackItems;s.backpackItems=Array.Empty<int>();s.usingWheelbarrow=false;equipment.Commit(s);}
            actionButton.onClick.AddListener(Interact);
            var animator=player.visual.GetComponent<Animator>();rightIK=player.visual.GetComponent<MinerAnimator>().rightHandIK;oldRightTarget=rightIK.Target;
            leftIK=animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject.AddComponent<FastIKFabric>();leftIK.enabled=false;leftIK.Iterations=8;var generatedTarget=leftIK.Target;leftIK.Target=leftGrip;if(generatedTarget)Destroy(generatedTarget.gameObject);
            lastPlayer=player.transform.position;lastWheel=transform.position;Refresh();
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
            if(!Pushing){s.backpackItems=s.items;s.items=s.barrowItems;s.barrowItems=Array.Empty<int>();s.usingWheelbarrow=true;}
            else{s.barrowItems=s.items;s.items=s.backpackItems;s.backpackItems=Array.Empty<int>();s.usingWheelbarrow=false;}
            s.barrowPosition=transform.position;if(!equipment.Commit(s))return;
            equipment.cargoRoot=Pushing?loadRoot:backpackRoot;equipment.SetPickaxeVisible(!Pushing);player.CancelMining();player.touchControls.Move=Vector2.zero;
            player.runSpeed=Pushing?2.5f:originalSpeed;bodyCollider.enabled=!Pushing;lastPlayer=player.transform.position;Refresh();
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
            if(!Pushing)return;
            Quaternion rotation=player.visual.rotation;Vector3 target=player.transform.position+rotation*Vector3.forward*1.05f;target.y=.05f;
            int n=Physics.OverlapBoxNonAlloc(target+Vector3.up*.55f,new Vector3(.38f,.32f,.55f),overlaps,rotation,~0,QueryTriggerInteraction.Ignore);bool blocked=false;
            for(int i=0;i<n;i++)if(!overlaps[i].transform.IsChildOf(player.transform)&&!overlaps[i].transform.IsChildOf(transform)){blocked=true;break;}
            if(blocked){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=lastPlayer;cc.enabled=true;}
            else{transform.SetPositionAndRotation(target,rotation);lastPlayer=player.transform.position;}
            float distance=Vector3.Distance(transform.position,lastWheel);if(wheel)wheel.Rotate(Vector3.forward,distance/.18f*Mathf.Rad2Deg,Space.Self);lastWheel=transform.position;
            rightIK.Target=rightGrip;rightIK.Solve();leftIK.Solve();rightIK.Target=oldRightTarget;
        }
        void Refresh()
        {
            var s=equipment.Snapshot;bool near=Vector3.Distance(player.transform.position,transform.position)<2.5f;
            actionButton.gameObject.SetActive(near||s.wheelbarrowUnlocked);actionButton.interactable=!player.InputBlocked&&(s.wheelbarrowUnlocked||s.coins>=unlockCost);
            actionText.text=!s.wheelbarrowUnlocked?"UNLOCK CART  $"+unlockCost:Pushing?"PARK CART [F]":near?"USE CART [F]":"RECALL CART [F]";
            var items=Pushing?s.items:s.barrowItems;
            counter.text=!s.wheelbarrowUnlocked?"QUARRY CADDY\n$"+unlockCost:"CADDY "+items.Length+"/12";
            if(Camera.main)counter.transform.rotation=Camera.main.transform.rotation;
            for(int i=0;i<loadSlots.Length;i++){loadSlots[i].gameObject.SetActive(i<items.Length);if(i>=items.Length)continue;loadSlots[i].GetComponent<MeshFilter>().sharedMesh=equipment.MeshForItem(items[i]);loadSlots[i].GetComponent<MeshRenderer>().sharedMaterial=stockMaterials[items[i]];}
        }
        void OnDisable(){if(player)player.runSpeed=originalSpeed;if(equipment&&backpackRoot)equipment.cargoRoot=backpackRoot;if(rightIK&&oldRightTarget)rightIK.Target=oldRightTarget;}
    }
}
