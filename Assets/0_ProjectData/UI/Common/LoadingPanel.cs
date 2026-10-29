using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
namespace Bulldozer.UI {
 public sealed class LoadingPanel : MonoBehaviour {
  public CanvasGroup cover,artwork,contentGroup;public RectTransform artTransform,bottomPanel;public UnityEngine.UI.Image progressFill;public TMP_Text status,percentage,tip;public UnityEngine.UI.Button retryButton;public QuarryUIFeedback feedback;
  Tween artFade,push,panelFade,panelSlide,progressTween,exit,tipTween;float target,nextTip;int tipIndex;bool ready,showingError;Vector2 bottomHome;
  static readonly string[] tips={"Break two rocks to earn your first pickaxe upgrade.","Drop raw stone at REFINE. Collect finished material for the trucks.","Your cash and equipment upgrades carry into every excavation.","Park the wheelbarrow before using your pickaxe.","The final truck can leave with a partial load once the quarry is clear."};
  public event System.Action RetryRequested;public float DisplayedProgress=>progressFill.fillAmount;public bool IsVisible=>gameObject.activeSelf;public bool BlocksInput=>IsVisible&&cover.blocksRaycasts;
  void Awake(){bottomHome=bottomPanel.anchoredPosition;retryButton.onClick.AddListener(Retry);HideImmediately();}
  void Retry(){feedback?.Play();RetryRequested?.Invoke();}
  public void Show(){StopMotion();gameObject.SetActive(true);ready=showingError=false;target=0;progressFill.fillAmount=0;percentage.text="0%";cover.alpha=1;cover.blocksRaycasts=cover.interactable=true;retryButton.gameObject.SetActive(false);artwork.alpha=0;artTransform.localScale=Vector3.one;contentGroup.alpha=0;bottomPanel.anchoredPosition=bottomHome-Vector2.up*20;status.text="LOADING WORKSITE";tipIndex=0;tip.alpha=1;tip.text=tips[0];nextTip=Time.unscaledTime+5;artFade=artwork.DOFade(1,.5f).SetUpdate(true);push=artTransform.DOScale(1.025f,10).SetEase(Ease.Linear).SetUpdate(true);panelFade=contentGroup.DOFade(1,.3f).SetUpdate(true);panelSlide=bottomPanel.DOAnchorPos(bottomHome,.3f).SetEase(Ease.OutCubic).SetUpdate(true);}
  public void Report(float value,string message){if(float.IsNaN(value)||float.IsInfinity(value)||ready||showingError)return;float next=Mathf.Max(target,Mathf.Clamp(value,0,.99f));if(next>target+.0001f){target=next;progressTween?.Kill();progressTween=progressFill.DOFillAmount(target,.12f).SetEase(Ease.Linear).SetUpdate(true);}if(!string.IsNullOrEmpty(message))status.text=message.ToUpperInvariant();}
  void Update(){if(showingError)return;percentage.SetText("{0}%",ready?Mathf.FloorToInt(progressFill.fillAmount*100):Mathf.Min(99,Mathf.FloorToInt(progressFill.fillAmount*100)));if(!ready&&Time.unscaledTime>=nextTip){nextTip=Time.unscaledTime+5;tipTween?.Kill();tipTween=DOTween.Sequence().SetUpdate(true).Append(tip.DOFade(0,.2f)).AppendCallback(()=>{tipIndex=(tipIndex+1)%tips.Length;tip.text=tips[tipIndex];}).Append(tip.DOFade(1,.2f));}}
  public IEnumerator Reveal(){ready=true;tipTween?.Kill();tip.alpha=1;progressTween?.Kill();status.text="WORKSITE READY";progressTween=progressFill.DOFillAmount(1,.1f).SetEase(Ease.Linear).SetUpdate(true);yield return progressTween.WaitForCompletion();percentage.text="100%";feedback?.Play(1);exit=cover.DOFade(0,.3f).SetUpdate(true);yield return exit.WaitForCompletion();HideImmediately();}
  public void ShowError(string message,bool canRetry){StopMotion();gameObject.SetActive(true);showingError=true;cover.alpha=artwork.alpha=contentGroup.alpha=1;cover.blocksRaycasts=cover.interactable=true;artTransform.localScale=Vector3.one;bottomPanel.anchoredPosition=bottomHome;status.text="WORKSITE UNAVAILABLE";tip.text=message;tip.alpha=1;percentage.text="";retryButton.gameObject.SetActive(canRetry);}
  public void HideImmediately(){StopMotion();cover.alpha=0;cover.blocksRaycasts=cover.interactable=false;gameObject.SetActive(false);}
  void StopMotion(){artFade?.Kill();push?.Kill();panelFade?.Kill();panelSlide?.Kill();progressTween?.Kill();exit?.Kill();tipTween?.Kill();}
  void OnDisable()=>StopMotion();void OnDestroy(){if(retryButton)retryButton.onClick.RemoveListener(Retry);}
 }
}
