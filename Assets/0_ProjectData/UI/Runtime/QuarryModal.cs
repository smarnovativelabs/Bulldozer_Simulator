using UnityEngine;
using DG.Tweening;
namespace Bulldozer.UI {
 public sealed class QuarryModal : MonoBehaviour {
  public CanvasGroup panelGroup;public RectTransform panel;public UnityEngine.UI.Image dim;
  Sequence motion;public bool IsOpen{get;private set;}public System.Action Closed;public bool Busy=>motion!=null&&motion.IsActive();
  public void Open(){motion?.Kill();gameObject.SetActive(true);IsOpen=true;panelGroup.blocksRaycasts=true;panelGroup.interactable=true;dim.raycastTarget=true;panel.localScale=Vector3.one*.97f;panelGroup.alpha=0;var c=dim.color;c.a=0;dim.color=c;motion=DOTween.Sequence().SetUpdate(true).Append(dim.DOFade(.55f,.18f)).Join(panelGroup.DOFade(1,.22f)).Join(panel.DOScale(1,.22f).SetEase(Ease.OutCubic));}
  public void Close(){if(!gameObject.activeSelf)return;motion?.Kill();IsOpen=false;panelGroup.interactable=false;motion=DOTween.Sequence().SetUpdate(true).Append(dim.DOFade(0,.15f)).Join(panelGroup.DOFade(0,.15f)).Join(panel.DOScale(.97f,.15f)).OnComplete(()=>{panelGroup.blocksRaycasts=false;dim.raycastTarget=false;gameObject.SetActive(false);Closed?.Invoke();});}
  void OnDisable(){motion?.Kill();motion=null;panel.localScale=Vector3.one;}
 }
}
