using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Bulldozer.Player
{
    // Two reusable virtual cameras; no extra rendering cameras, art, lights or particles.
    [DefaultExecutionOrder(300)]
    public sealed class QuarryCameraGuidance : MonoBehaviour
    {
        public const int Refinery=1,Workbench=2,Wheelbarrow=4,PlayerIntro=8;
        public QuarryGuidance guide;
        public QuarryWheelbarrow cart;
        public float blendSeconds=.65f,holdSeconds=2f;
        [Min(1)] public float introRevealSeconds=3f;
        [Min(.5f)] public float introReturnSeconds=1.3f;
        public bool Playing {get;private set;}
        public int ActiveCue {get;private set;}
        public int ShotsStarted {get;private set;}
        MinerCamera follow;
        Camera output;
        CinemachineBrain brain;
        CinemachineCamera home,focus;
        Vector3 homePosition;
        Quaternion homeRotation;
        float began,nextPoll,readyAt,returnAt;
        bool returning,introPending,introPrepared;
        Vector3 introStart,introEnd,introAim,introArc;

        void Start()
        {
            if(!guide)guide=GetComponent<QuarryGuidance>();
            if(!cart)cart=FindFirstObjectByType<QuarryWheelbarrow>();
            follow=guide.player.followCamera;
            output=follow?follow.GetComponent<Camera>():null;
            introPending=output&&(guide.equipment.Snapshot.guidanceSeenMask&PlayerIntro)==0;
            guide.player.IntroInputBlocked=introPending;
            readyAt=Time.unscaledTime+2;
            if(introPending)PrepareIntro();
        }
        bool ModalOpen=>Time.timeScale<=0||Bulldozer.UI.QuarryGameUI.BlocksInput||
            Bulldozer.Services.QuarryServiceOverlay.BlocksInput||
            (Bulldozer.UI.CommonUI.Instance&&Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput);
        void Update()
        {
            if(introPending)
            {
                if(ModalOpen||guide.player.ExcavationInputBlocked)return;
                // The overview is already framed under the loading cover. Start its
                // clock only after loading/consent/connectivity UI releases control.
                var state=guide.equipment.Snapshot;
                state.guidanceSeenMask|=PlayerIntro;
                introPending=false;
                if(!guide.equipment.Commit(state)){Finish();return;}
                introPrepared=false;Playing=true;ActiveCue=PlayerIntro;ShotsStarted++;
                began=Time.unscaledTime;
                guide.ShowCameraHint("WELCOME TO YOUR QUARRY","Mine stone, refine ore, and load the trucks.  Tap to continue.");
                return;
            }
            if(Playing)
            {
                if(ModalOpen||guide.player.ExcavationInputBlocked)
                {
                    // A temporary overlay must not snap the camera back to gameplay.
                    began+=Time.unscaledDeltaTime;if(returning)returnAt+=Time.unscaledDeltaTime;
                    return;
                }
                bool skip=Time.unscaledTime-began>.35f&&
                    ((Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||
                    (Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame)||
                    (Keyboard.current!=null&&(Keyboard.current.spaceKey.wasPressedThisFrame||Keyboard.current.escapeKey.wasPressedThisFrame))||
                    (Gamepad.current!=null&&Gamepad.current.buttonSouth.wasPressedThisFrame));
                if(ActiveCue==PlayerIntro&&!returning)UpdateIntroPose();
                if(!returning&&(skip||Time.unscaledTime-began>=(ActiveCue==PlayerIntro?introRevealSeconds:blendSeconds+holdSeconds)))Return();
                if(returning&&Time.unscaledTime>=returnAt)Finish();
                return;
            }
            if(Time.unscaledTime<readyAt||Time.unscaledTime<nextPoll)return;
            nextPoll=Time.unscaledTime+.2f;
            if(!guide||!output||guide.player.InputBlocked||guide.player.IsMining||guide.player.Speed>.15f||guide.equipment.UsingWheelbarrow)return;
            var s=guide.equipment.Snapshot;
            if(!s.firstRefineryDeposit&&guide.equipment.IsFull)
                TryShow(Refinery,guide.production.inputZone.transform.position,"REFINERY INPUT","Unload here. After your first deposit, a full bag no longer stops mining.");
            else if(s.wheelbarrowUnlocked&&cart)
                TryShow(Wheelbarrow,cart.transform.position,"YOUR WHEELBARROW","Carry 12 ore per trip. Use the cart here; park it to mine.");
            // The player has experienced mining and at least one unloading trip before this optional suggestion.
            if(!Playing&&s.firstRefineryDeposit&&s.taskRocks>=2&&s.damageLevel==0&&s.coins>=guide.equipment.DamageCost&&guide.upgrades)
                TryShow(Workbench,guide.upgrades.zone.transform.position,"OPTIONAL PICKAXE UPGRADE","Upgrade here for fewer hits on tough rocks. Buying is optional.");
        }
        void PrepareIntro()
        {
            EnsureCameras();
            follow.GetFollowPose(out homePosition,out homeRotation);
            home.transform.SetPositionAndRotation(homePosition,homeRotation);
            home.Lens=focus.Lens=LensSettings.FromCamera(output);
            var bounds=new Bounds(guide.player.transform.position,Vector3.zero);
            var state=guide.equipment.Snapshot;
            foreach(var rock in FindObjectsByType<MineableRock>(FindObjectsSortMode.None))
                if(rock.Available(state))bounds.Encapsulate(rock.transform.position);
            if(guide.production)
            {
                if(guide.production.inputZone)bounds.Encapsulate(guide.production.inputZone.bounds.center);
                if(guide.production.sellZone)bounds.Encapsulate(guide.production.sellZone.bounds.center);
            }
            introAim=bounds.center;introAim.y=guide.player.transform.position.y+.6f;
            float span=Mathf.Clamp(Mathf.Max(bounds.size.x,bounds.size.z),10,24);
            var viewRotation=Quaternion.Euler(48,follow.startYaw-12,0);
            float distance=Mathf.Max(13,span*.72f/Mathf.Tan(output.fieldOfView*Mathf.Deg2Rad*.5f));
            introStart=introAim+viewRotation*Vector3.back*distance;
            introEnd=introAim+viewRotation*Vector3.back*(distance*.82f)+viewRotation*Vector3.right*2.2f;
            introArc=viewRotation*Vector3.right*1.4f+Vector3.up*.8f;
            focus.transform.SetPositionAndRotation(introStart,Quaternion.LookRotation(introAim-introStart));
            home.Priority=10;focus.Priority=20;
            home.gameObject.SetActive(false);focus.gameObject.SetActive(true);
            guide.player.GuidanceInputBlocked=true;guide.player.CancelMining();
            brain.enabled=true;brain.ResetState();brain.ManualUpdate();
            home.gameObject.SetActive(true);
            introPrepared=true;returning=false;
        }
        void UpdateIntroPose()
        {
            float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-began)/introRevealSeconds));
            var position=Vector3.Lerp(introStart,introEnd,t)+introArc*(4*t*(1-t));
            focus.transform.SetPositionAndRotation(position,Quaternion.LookRotation(introAim-position));
        }
        public bool TryShow(int cue,Vector3 destination,string title,string body)
        {
            if(!isActiveAndEnabled||Playing||introPrepared||!output||!guide||guide.player.InputBlocked||ModalOpen||
                (cue!=Refinery&&cue!=Workbench&&cue!=Wheelbarrow&&cue!=PlayerIntro))return false;
            var state=guide.equipment.Snapshot;
            if((state.guidanceSeenMask&cue)!=0)return false;
            if(cue==PlayerIntro)
            {
                introPending=true;guide.player.IntroInputBlocked=true;
                PrepareIntro();return true;
            }
            EnsureCameras();
            state.guidanceSeenMask|=cue;
            // Persist before taking control; reloads cannot replay a paid-for/seen hint.
            if(!guide.equipment.Commit(state))return false;
            homePosition=output.transform.position;homeRotation=output.transform.rotation;
            home.transform.SetPositionAndRotation(homePosition,homeRotation);
            home.Lens=focus.Lens=LensSettings.FromCamera(output);
            Vector3 aim=destination+Vector3.up*.65f;
            Vector3 direction=guide.player.transform.position-destination;direction.y=0;
            if(direction.sqrMagnitude<1)direction=Vector3.back;
            focus.transform.position=aim+direction.normalized*(cue==Refinery?5.5f:4.5f)+Vector3.up*(cue==Refinery?5.5f:1.8f);
            focus.transform.rotation=Quaternion.LookRotation(aim-focus.transform.position);
            home.gameObject.SetActive(true);focus.gameObject.SetActive(false);
            home.Priority=10;focus.Priority=20;

            guide.player.GuidanceInputBlocked=true;guide.player.CancelMining();
            if(guide.player.touchControls)guide.player.touchControls.Move=Vector2.zero;
            brain.enabled=true;brain.ResetState();brain.ManualUpdate();
            focus.gameObject.SetActive(true);
            Playing=true;returning=false;ActiveCue=cue;ShotsStarted++;began=Time.unscaledTime;
            guide.ShowCameraHint(title,body+"  Tap to continue.");
            return true;
        }
        void EnsureCameras()
        {
            if(brain)return;
            brain=output.gameObject.AddComponent<CinemachineBrain>();brain.enabled=false;
            brain.UpdateMethod=CinemachineBrain.UpdateMethods.ManualUpdate;
            brain.IgnoreTimeScale=true;
            brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut,blendSeconds);
            home=MakeCamera("Guidance return camera");focus=MakeCamera("Guidance destination camera");
        }
        CinemachineCamera MakeCamera(string label)
        {
            var go=new GameObject(label);go.SetActive(false);go.transform.SetParent(transform,false);
            return go.AddComponent<CinemachineCamera>();
        }
        public void Skip(){if(Playing&&!returning)Return();}
        void Return()
        {
            returning=true;
            // Ground settling can move the player slightly while the overview runs.
            follow.GetFollowPose(out homePosition,out homeRotation);
            home.transform.SetPositionAndRotation(homePosition,homeRotation);
            float duration=ActiveCue==PlayerIntro?introReturnSeconds:blendSeconds;
            brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut,duration);
            focus.Priority=0;returnAt=Time.unscaledTime+duration;
        }
        void LateUpdate(){if((Playing||introPrepared)&&brain&&(!Playing||!ModalOpen))brain.ManualUpdate();}
        public void Finish()
        {
            if(!Playing&&!introPrepared)return;
            Playing=false;introPrepared=false;introPending=false;ActiveCue=0;
            if(brain)brain.enabled=false;
            if(home)home.gameObject.SetActive(false);if(focus)focus.gameObject.SetActive(false);
            if(output)output.transform.SetPositionAndRotation(homePosition,homeRotation);

            if(brain)brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut,blendSeconds);
            if(guide&&guide.player){guide.player.IntroInputBlocked=false;guide.player.GuidanceInputBlocked=false;guide.player.CancelMining();if(guide.player.touchControls)guide.player.touchControls.Move=Vector2.zero;}
            if(guide)guide.EndCameraHint();
            readyAt=Time.unscaledTime+3;
        }
        void OnApplicationPause(bool paused){if(paused)Finish();}
        void OnDisable(){introPending=false;if(guide&&guide.player)guide.player.IntroInputBlocked=false;Finish();}
        void OnDestroy(){if(brain)Destroy(brain);}
    }
}
