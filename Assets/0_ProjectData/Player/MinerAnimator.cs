using UnityEngine;
using DitzelGames.FastIK;
namespace Bulldozer.Player
{
    // Runs after Animator evaluation and explicitly before the IK solve.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class MinerAnimator : MonoBehaviour
    {
        public MinerController controller;
        public MinerEquipment equipment;
        public FastIKFabric rightHandIK;
        public Transform gripTarget, elbowPole;
        public Vector3 carryPosition = new Vector3(.32f, .98f, .10f);
        public Quaternion carryRotation = Quaternion.identity;
        public Transform toolGrip;
        public Quaternion carryToolRotation=Quaternion.identity;
        Animator animator;
        Transform hand, elbow;
        Transform shoulder;
        float armLength;
        public float LastGripError {get;private set;}
        float miningWeight;
        Transform[] fingers;
        Quaternion[] fingerRotations;
        int rightFingerCount;
        static readonly int Speed = Animator.StringToHash("Speed");
        void OnEnable(){if(Application.isPlaying && !animator)Awake();}
        void Awake()
        {
            animator=GetComponent<Animator>();
            hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            elbow=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            shoulder=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            armLength=Vector3.Distance(shoulder.position,elbow.position)+Vector3.Distance(elbow.position,hand.position);
            CacheGrip();
            if(rightHandIK)rightHandIK.enabled=false; // Solve once, after targets and fingers are updated.
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
            animator.SetBool("Mining",controller.IsMining);
            animator.SetBool("MineWall",controller.MineWall);
        }
        void LateUpdate()
        {
            if(!controller || !hand || !equipment)return;
            if(equipment.UsingWheelbarrow){for(int i=0;i<fingers.Length;i++)fingers[i].localRotation=fingerRotations[i];return;}
            if(!equipment.PickaxeVisible)return;
            // Close the fingers around the shaft, retaining the authored wrist/arm animation.
            for(int i=0;i<rightFingerCount;i++)fingers[i].localRotation=fingerRotations[i];
            // Gameplay's contact timer can finish before the animation recovery. Keep the
            // authored wrist through that recovery instead of twisting it toward carry mid-swing.
            var state=animator.GetCurrentAnimatorStateInfo(0);
            bool animatedMining=state.IsName("Mine Ground")||state.IsName("Mine Wall");
            if(animator.IsInTransition(0)){var next=animator.GetNextAnimatorStateInfo(0);animatedMining|=next.IsName("Mine Ground")||next.IsName("Mine Wall");}
            bool holdSwing=controller.IsMining||animatedMining;
            miningWeight=Mathf.MoveTowards(miningWeight,holdSwing?1:0,Time.deltaTime*(holdSwing?12:5));
            if(!rightHandIK || !gripTarget)return;
            float blend=miningWeight*miningWeight*(3-2*miningWeight);
            Vector3 animatedElbow=elbow.position;
            Vector3 carry=transform.TransformPoint(carryPosition);
            // Locomotion moves the shoulder; preserve elbow bend instead of stretching to an unreachable target.
            carry=shoulder.position+Vector3.ClampMagnitude(carry-shoulder.position,armLength*.92f);
            gripTarget.position=Vector3.Lerp(carry,hand.position,blend);
            // Rotate the wrist with the tool, never spin the shaft inside the closed palm.
            gripTarget.rotation=Quaternion.Slerp(transform.rotation*carryRotation*carryToolRotation,hand.rotation,blend);
            if(elbowPole)elbowPole.position=Vector3.Lerp(transform.TransformPoint(new Vector3(.65f,1.05f,-.18f)),animatedElbow,blend);
            rightHandIK.Solve();
            hand.rotation=gripTarget.rotation;
            LastGripError=Vector3.Distance(hand.position,gripTarget.position);
            if(toolGrip)toolGrip.localRotation=Quaternion.identity;
        }
    }
}

