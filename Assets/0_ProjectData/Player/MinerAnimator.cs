using UnityEngine;
using DitzelGames.FastIK;
namespace Bulldozer.Player
{
    // Authored animation drives arms; only the finger grip and tool socket are maintained here.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class MinerAnimator : MonoBehaviour
    {
        public MinerController controller;
        public MinerEquipment equipment;
        public FastIKFabric rightHandIK;
        public bool cartAnimations;
        public Transform gripTarget, elbowPole;
        public Vector3 carryPosition = new Vector3(.32f, .98f, .10f);
        public Quaternion carryRotation = Quaternion.identity;
        public Transform toolGrip;
        public Quaternion carryToolRotation=Quaternion.identity;
        public bool fixedPickaxeAngle;
        public Quaternion pickaxeFrontRotation=Quaternion.identity;
        Vector3 toolRestPosition;
        Quaternion toolRestRotation;
        Animator animator;
        Transform hand, elbow;
        Transform shoulder;
        float armLength;
        public float LastGripError {get;private set;}
        float carryWeight=1; int carryLayer=-1;
        float miningPoseUntil;
        bool wasMining,holdingMiningPose;
        Transform[] fingers;
        Quaternion[] fingerRotations;
        int rightFingerCount;
        static readonly int Speed = Animator.StringToHash("Speed");
        void OnEnable(){if(Application.isPlaying && !animator)Awake();}
        void Awake()
        {
            animator=GetComponent<Animator>();carryLayer=animator.GetLayerIndex("Pickaxe Carry");
            if(toolGrip){toolRestPosition=toolGrip.localPosition;toolRestRotation=toolGrip.localRotation;}
            hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            elbow=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            shoulder=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            armLength=Vector3.Distance(shoulder.position,elbow.position)+Vector3.Distance(elbow.position,hand.position);
            CacheGrip();
            if(rightHandIK)rightHandIK.enabled=false; // Legacy editor setup reference; never solved at runtime.
        }
        void CacheGrip()
        {
            // Convert the humanoid grip once. Reapplying HumanPose every frame can change the
            // body/root orientation while turning, and needlessly solves the whole avatar.
            var transforms=GetComponentsInChildren<Transform>(true);
            var positions=new Vector3[transforms.Length];var rotations=new Quaternion[transforms.Length];
            for(int i=0;i<transforms.Length;i++){positions[i]=transforms[i].localPosition;rotations[i]=transforms[i].localRotation;}
            var list=new System.Collections.Generic.List<Transform>();
            for(int i=(int)HumanBodyBones.RightThumbProximal;i<=(int)HumanBodyBones.RightLittleDistal;i++)
            {var bone=animator.GetBoneTransform((HumanBodyBones)i);if(bone)list.Add(bone);}
            rightFingerCount=list.Count;
            for(int i=(int)HumanBodyBones.LeftThumbProximal;i<=(int)HumanBodyBones.LeftLittleDistal;i++)
            {var bone=animator.GetBoneTransform((HumanBodyBones)i);if(bone)list.Add(bone);}
            fingers=list.ToArray();fingerRotations=new Quaternion[fingers.Length];
            try
            {
                using(var handler=new HumanPoseHandler(animator.avatar,transform))
                {
                    HumanPose pose=new HumanPose();handler.GetHumanPose(ref pose);
                    for(int i=0;i<HumanTrait.MuscleCount;i++)
                    {
                        string name=HumanTrait.MuscleName[i];
                        if((name.StartsWith("Right ")||name.StartsWith("Left "))&&name.Contains("Stretched"))pose.muscles[i]=name.Contains("Thumb")?-.35f:name.Contains("Index")?-.75f:-.82f;
                    }
                    handler.SetHumanPose(ref pose);
                    for(int i=0;i<fingers.Length;i++)fingerRotations[i]=fingers[i].localRotation;
                }
            }
            finally{for(int i=0;i<transforms.Length;i++){transforms[i].localPosition=positions[i];transforms[i].localRotation=rotations[i];}}
        }
        void Update()
        {
            if(!controller)return;
            animator.SetFloat(Speed,controller.Speed,.12f,Time.deltaTime);
            animator.SetBool("Grounded",controller.Grounded);
            animator.SetFloat("Vertical",controller.VerticalSpeed);
            bool mining=controller.IsMining;
            bool canHold=equipment&&equipment.PickaxeVisible&&!equipment.UsingWheelbarrow&&
                controller.Grounded&&!controller.InputBlocked&&controller.MoveInput.sqrMagnitude<.01f&&controller.Speed<.12f;
            if(mining)miningPoseUntil=Time.time+.35f;
            // The controller's .22-second repeat delay is recovery, not a return to
            // the one-handed carry pose. Non-looping clips hold their matching end
            // pose until the next swing; moving or leaving mining releases it.
            bool pose=mining||(canHold&&Time.time<miningPoseUntil);
            if(mining&&!wasMining&&holdingMiningPose){
                string state=controller.MineWall?"Mine Wall":"Mine Ground";
                if(animator.GetCurrentAnimatorStateInfo(0).IsName(state))animator.Play(state,0,0);
                else animator.CrossFadeInFixedTime(state,.08f,0,0);
            }
            animator.SetBool("Mining",pose);
            wasMining=mining;holdingMiningPose=pose;
            if(!canHold&&!mining)miningPoseUntil=0;
            animator.SetBool("MineWall",controller.MineWall);
            if(carryLayer<0)carryLayer=animator.GetLayerIndex("Pickaxe Carry");
            if(carryLayer>=0){var state=animator.GetCurrentAnimatorStateInfo(0);bool swing=controller.IsMining||state.IsName("Mine Ground")||state.IsName("Mine Wall");if(animator.IsInTransition(0)){var next=animator.GetNextAnimatorStateInfo(0);swing|=next.IsName("Mine Ground")||next.IsName("Mine Wall");}float target=equipment&&equipment.PickaxeVisible&&!equipment.UsingWheelbarrow&&!swing?1:0;carryWeight=Mathf.MoveTowards(carryWeight,target,Time.deltaTime*12);animator.SetLayerWeight(carryLayer,carryWeight);}
            if(cartAnimations){animator.SetBool("Cart",equipment&&equipment.UsingWheelbarrow);animator.SetFloat("CartSpeed",controller.CartSignedSpeed/2.5f,.1f,Time.deltaTime);}
        }
        void LateUpdate()
        {
            if(!controller || !hand || !equipment)return;
            if(equipment.UsingWheelbarrow){for(int i=0;i<fingers.Length;i++)fingers[i].localRotation=fingerRotations[i];return;}
            if(!equipment.PickaxeVisible)return;
            // Close the fingers around the shaft, retaining the authored wrist/arm animation.
            for(int i=0;i<rightFingerCount;i++)fingers[i].localRotation=fingerRotations[i];
            // Preserve the authored palm socket, including its position, through every clip.
            if(toolGrip){
                toolGrip.localPosition=toolRestPosition;
                var state=animator.GetCurrentAnimatorStateInfo(0);
                bool authoredStrike=state.IsName("Mine Ground")||state.IsName("Mine Wall");
                if(animator.IsInTransition(0)){
                    var next=animator.GetNextAnimatorStateInfo(0);
                    authoredStrike|=next.IsName("Mine Ground")||next.IsName("Mine Wall");
                }
                // The wrist must rotate the head through its overhead strike arc.
                // Locking its world angle here flattened every authored swing.
                if(fixedPickaxeAngle&&!authoredStrike)toolGrip.rotation=transform.rotation*pickaxeFrontRotation;
                else toolGrip.localRotation=toolRestRotation;
            }
        }
    }
}



