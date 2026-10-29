using UnityEngine;
using DG.Tweening;
using Bulldozer.Player.Persistence;

namespace Bulldozer.Player
{
    public sealed class QuarryOptionalTasks : MonoBehaviour
    {
        public MinerEquipment equipment;
        public TMPro.TMP_Text shiftLabel,title;
        public TMPro.TMP_Text[] rows;
        public RectTransform card;
        float poll,celebrateUntil;
        int previousMask,lastRound=-1,lastReward;
        Tween rewardPulse;
        void Start()
        {
            var s=equipment.Snapshot;previousMask=s.taskClaimedMask;lastRound=s.roundIndex;
            rewardPulse=card.DOPunchScale(Vector3.one*.015f,.3f,2,.15f).SetAutoKill(false).Pause().SetLink(gameObject);
            Refresh();
        }
        void Update(){if(Time.unscaledTime>=poll){poll=Time.unscaledTime+.25f;Refresh();}}
        public void Refresh()
        {
            var s=equipment.Snapshot;
            if(lastRound!=s.roundIndex){previousMask=s.taskClaimedMask;lastRound=s.roundIndex;celebrateUntil=0;}
            int earned=0;
            for(int i=0;i<3;i++)if((s.taskClaimedMask&(1<<i))!=0&&(previousMask&(1<<i))==0)earned+=OptionalTaskRules.Reward(s.roundIndex,i);
            if(earned>0){lastReward=earned;celebrateUntil=Time.unscaledTime+3;rewardPulse?.Restart();}
            previousMask=s.taskClaimedMask;
            bool complete=s.taskClaimedMask==7&&Time.unscaledTime>=celebrateUntil;
            card.sizeDelta=new Vector2(card.sizeDelta.x,complete?64:154);
            shiftLabel.text=s.roundIndex==0&&s.damageLevel>0?"FIRST SHIFT: upgrades carry into every round":QuarryBalance.Shift(s.roundIndex);
            title.text=Time.unscaledTime<celebrateUntil?"TASK BONUS  +$"+lastReward:complete?"ALL OPTIONAL TASKS COMPLETE":"OPTIONAL TASKS  /  AUTO REWARDS";
            for(int i=0;i<3;i++)
            {
                bool done=(s.taskClaimedMask&(1<<i))!=0;int target=OptionalTaskRules.Target(s.roundIndex,i);
                rows[i].gameObject.SetActive(!complete);
                rows[i].text=(done?"[DONE] ":"")+OptionalTaskRules.Label(s.roundIndex,i)+"  "+Mathf.Min(target,OptionalTaskRules.Progress(s,i))+"/"+target+"  +$"+OptionalTaskRules.Reward(s.roundIndex,i);
                rows[i].color=done?new Color(.55f,.95f,.7f):Color.white;
            }
        }
        void OnDestroy(){rewardPulse?.Kill();}
    }
}
