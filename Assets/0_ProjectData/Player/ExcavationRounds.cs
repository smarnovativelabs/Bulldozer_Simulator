using System.Linq;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player.Persistence;

namespace Bulldozer.Player
{
    public sealed class ExcavationRounds : MonoBehaviour
    {
        public MinerEquipment equipment;
        public MinerController player;
        public MiningProduction production;
        public CanvasGroup completionPanel,fade;
        public TMPro.TMP_Text heading,summary,roundLabel;
        public UnityEngine.UI.Button nextButton;
        string[] ids;
        float nextPoll;
        bool presented,transitioning;
        Tween panelTween,fadeTween;
        MMF_Player celebration;
        public bool Transitioning=>transitioning;
        public long RoundNumber=>(long)equipment.Snapshot.roundIndex+1;
        void Start()
        {
            ids=FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(r=>r.blockId).Where(id=>!string.IsNullOrEmpty(id)).Distinct().ToArray();
            completionPanel.alpha=0;completionPanel.gameObject.SetActive(false);fade.alpha=0;fade.blocksRaycasts=false;
            nextButton.onClick.AddListener(NextRound);
            celebration=gameObject.AddComponent<MMF_Player>();
            celebration.AddFeedback(new MMF_Scale{AnimateScaleTarget=heading.transform,UniformScaling=true,AnimateScaleDuration=.35f,RemapCurveZero=1,RemapCurveOne=1.025f});
            celebration.Initialization();Evaluate();
            if(equipment.Snapshot.roundIndex>0&&!presented)
            {
                fade.alpha=1;fade.blocksRaycasts=true;player.ExcavationInputBlocked=true;
                fadeTween=fade.DOFade(0,.4f).SetUpdate(true).SetLink(gameObject).OnComplete(()=>{fade.blocksRaycasts=false;if(!presented)player.ExcavationInputBlocked=false;});
            }
        }
        void Update(){if(Time.unscaledTime>=nextPoll){nextPoll=Time.unscaledTime+.4f;Evaluate();}}
        public bool TryComplete()
        {
            if(transitioning||!ExcavationRules.TryComplete(equipment.Snapshot,ids,out var next))return false;
            return equipment.Commit(next);
        }
        public void Evaluate()
        {
            if(transitioning)return;
            TryComplete();var s=equipment.Snapshot;
            int remaining=ids.Count(id=>System.Array.IndexOf(s.depletedBlocks,id)<0);
            roundLabel.text="EXCAVATION "+((long)s.roundIndex+1)+"  |  "+remaining+" ROCKS LEFT";
            if(!s.roundBonusClaimed||!ExcavationRules.Cleared(s,ids)||presented)return;
            Bulldozer.UI.QuarryAudio.Play(Bulldozer.UI.QuarryAudio.Cue.Round,player.transform.position);presented=true;player.ExcavationInputBlocked=true;player.CancelMining();if(player.touchControls)player.touchControls.Move=Vector2.zero;
            heading.text="EXCAVATION "+((long)s.roundIndex+1)+" COMPLETE";
            summary.text="All stone delivered\nCompletion bonus  +$"+ExcavationRules.Bonus(s.roundIndex)+"\nCash balance  $"+s.coins+"\n\nYour cash, equipment and upgrades carry forward.";
            completionPanel.gameObject.SetActive(true);completionPanel.blocksRaycasts=true;completionPanel.interactable=true;
            panelTween=completionPanel.DOFade(1,.3f).SetUpdate(true).SetLink(gameObject);celebration.PlayFeedbacks();
        }
        public void NextRound()
        {
            if(transitioning||!ExcavationRules.TryAdvance(equipment.Snapshot,ids,out var next))return;
            // Durable round state is written before any visual transition or scene reload.
            if(!equipment.Commit(next)){summary.text="Could not save the next excavation. Please retry.\nYour current progress is unchanged.";return;}
            Bulldozer.UI.QuarryGameUI.Instance?.feedback.Play();transitioning=true;nextButton.interactable=false;player.ExcavationInputBlocked=true;fade.blocksRaycasts=true;
            System.Action proceed=()=>{fadeTween=fade.DOFade(1,.45f).SetUpdate(true).SetLink(gameObject).OnComplete(()=>{var ui=Bulldozer.UI.CommonUI.Instance;if(ui!=null)ui.SceneLoading.TryLoadScene(gameObject.scene.path);else SceneManager.LoadSceneAsync(gameObject.scene.path);});};
            var services=Bulldozer.Services.QuarryOnlineServices.Instance;if(services){services.Event("quarry_round_complete",next.roundIndex);services.RoundBreak(proceed);}else proceed();
        }
        void OnDestroy(){panelTween?.Kill();fadeTween?.Kill();if(player)player.ExcavationInputBlocked=false;}
    }
}



