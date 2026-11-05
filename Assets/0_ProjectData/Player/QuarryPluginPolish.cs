using UnityEngine;
using Coffee.UIEffects;
using PixelPlay.OffScreenIndicator;
using TMPro;

namespace Bulldozer.Player
{
    // Presentation only: no save writes, inventory mutation or input ownership.
    [DefaultExecutionOrder(350)]
    public sealed class QuarryPluginPolish : MonoBehaviour
    {
        public QuarryGuidance guide;
        public RectTransform indicatorPanel,marker,arrow;
        public TMP_Text caption;
        public UIShiny rewardShine,cartShine;
        // Explicit build reference also keeps the dynamically selected plugin shader available.
        public Shader shinyShader;
        Camera view;
        Canvas canvas;
        int previousCash;
        bool previousCart,pendingReward,pendingCart;
        float nextPoll,nextReward,rewardEnd,cartEnd;
        QuarryGuidance.Goal shownGoal=(QuarryGuidance.Goal)(-1);
        int shownDistance=-1;
        public int RewardSweeps {get;private set;}
        public int CartSweeps {get;private set;}
        public bool IndicatorVisible=>marker&&marker.gameObject.activeSelf;
        void Start()
        {
            view=guide.player.followCamera.GetComponent<Camera>();canvas=indicatorPanel.GetComponentInParent<Canvas>();
            previousCash=guide.equipment.Cash;previousCart=guide.equipment.WheelbarrowUnlocked;
            StopShine(rewardShine);StopShine(cartShine);marker.gameObject.SetActive(false);
        }
        bool Hidden=>!guide.enabled||guide.player.InputBlocked||Time.timeScale<=0;
        void LateUpdate()
        {
            if(!view||!guide)return;
            if(Hidden){marker.gameObject.SetActive(false);StopShine(rewardShine);StopShine(cartShine);}
            else UpdateDirection();
            if(Time.unscaledTime<nextPoll)return;nextPoll=Time.unscaledTime+.15f;
            var e=guide.equipment;
            // Initial/restored values are baselined in Start: loading progress never celebrates again.
            pendingReward|=e.Cash>previousCash;pendingCart|=!previousCart&&e.WheelbarrowUnlocked;
            if(!Hidden&&pendingReward&&Time.unscaledTime>=nextReward)
            {if(PlayShine(rewardShine)){RewardSweeps++;rewardEnd=Time.unscaledTime+.75f;}pendingReward=false;nextReward=Time.unscaledTime+3;}
            if(!Hidden&&pendingCart)
            {if(PlayShine(cartShine)){CartSweeps++;cartEnd=Time.unscaledTime+.75f;}pendingCart=false;}
            previousCash=e.Cash;previousCart=e.WheelbarrowUnlocked;
            if(Time.unscaledTime>=rewardEnd)StopShine(rewardShine);
            if(Time.unscaledTime>=cartEnd)StopShine(cartShine);
        }
        void UpdateDirection()
        {
            Vector3 destination=guide.Destination;
            float distance=Vector3.Distance(guide.player.transform.position,destination);
            Vector3 projected=OffScreenIndicatorCore.GetScreenPosition(view,destination+Vector3.up*.25f);
            bool show=guide.HasRoute&&guide.CurrentGoal!=QuarryGuidance.Goal.Finished&&!guide.player.IsMining&&
                distance>2&&!OffScreenIndicatorCore.IsTargetVisible(projected)&&float.IsFinite(distance);
            marker.gameObject.SetActive(show);if(!show)return;
            // Keep clear of the joystick, action button, task panel, top HUD and screen notches.
            Rect safe=Screen.safeArea;
            Rect area=new Rect(safe.x+safe.width*.20f,safe.y+safe.height*.26f,safe.width*.50f,safe.height*.34f);
            float angle=0;Vector3 center=new Vector3(area.center.x,area.center.y,0);
            OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref projected,ref angle,center,new Vector3(area.width*.5f,area.height*.5f,0));
            if(!float.IsFinite(projected.x)||!float.IsFinite(projected.y)){marker.gameObject.SetActive(false);return;}
            RectTransformUtility.ScreenPointToLocalPointInRectangle(indicatorPanel,projected,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var local);
            marker.anchoredPosition=local;arrow.localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg);
            int metres=Mathf.CeilToInt(distance);
            if(shownGoal!=guide.CurrentGoal||shownDistance!=metres)
            {shownGoal=guide.CurrentGoal;shownDistance=metres;caption.text=GoalName(shownGoal)+"  "+metres+"m";}
        }
        static string GoalName(QuarryGuidance.Goal goal)
        {
            switch(goal){case QuarryGuidance.Goal.Mine:return "MINE";case QuarryGuidance.Goal.PickUp:return "COLLECT";
                case QuarryGuidance.Goal.Refine:return "REFINERY";case QuarryGuidance.Goal.Collect:return "READY ORE";
                case QuarryGuidance.Goal.Load:return "LOADING";case QuarryGuidance.Goal.Upgrade:return "WORKBENCH";case QuarryGuidance.Goal.Return:return "EXIT";default:return "";}
        }
        bool PlayShine(UIShiny effect)
        {
            if(!effect||!effect.gameObject.activeInHierarchy||!shinyShader||!shinyShader.isSupported)return false;
            effect.enabled=true;effect.Play();return true;
        }
        static void StopShine(UIShiny effect){if(effect&&effect.enabled){effect.Stop();effect.enabled=false;}}
        void OnDisable(){if(marker)marker.gameObject.SetActive(false);StopShine(rewardShine);StopShine(cartShine);}
    }
}
