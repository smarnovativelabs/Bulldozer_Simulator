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
        Animator animator;
        Transform hand, elbow;
        HumanPoseHandler poseHandler;
        HumanPose pose;
        float miningWeight;
        static readonly int Speed = Animator.StringToHash("Speed");
        void OnEnable(){if(Application.isPlaying && poseHandler==null)Awake();}
        void Awake()
        {
            animator=GetComponent<Animator>();
            hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            elbow=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            poseHandler=new HumanPoseHandler(animator.avatar,transform);
            if(rightHandIK)rightHandIK.enabled=false; // Solve once, after targets and fingers are updated.
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
            if(!controller || !hand || !equipment || !equipment.PickaxeVisible)return;
            // Close the fingers around the shaft, retaining the authored wrist/arm animation.
            poseHandler.GetHumanPose(ref pose);
            for(int i=0;i<HumanTrait.MuscleCount;i++)
                if(HumanTrait.MuscleName[i].StartsWith("Right ") && HumanTrait.MuscleName[i].Contains("Stretched"))
                    pose.muscles[i]=HumanTrait.MuscleName[i].Contains("Thumb") ? -.15f : -.65f;
            poseHandler.SetHumanPose(ref pose);
            miningWeight=Mathf.MoveTowards(miningWeight,controller.IsMining?1:0,Time.deltaTime*10);
            if(!rightHandIK || !gripTarget)return;
            gripTarget.position=Vector3.Lerp(transform.TransformPoint(carryPosition),hand.position,miningWeight);
            gripTarget.rotation=Quaternion.Slerp(transform.rotation*carryRotation,hand.rotation,miningWeight);
            elbowPole.position=elbow.position+transform.right*.25f-transform.forward*.1f;
            rightHandIK.Solve();
            hand.rotation=gripTarget.rotation;
        }
        void OnDisable(){poseHandler?.Dispose();poseHandler=null;}
    }
}

