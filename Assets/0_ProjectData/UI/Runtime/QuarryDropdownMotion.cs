using UnityEngine;
using DG.Tweening;
namespace Bulldozer.UI {
 public sealed class QuarryDropdownMotion : MonoBehaviour {
  CanvasGroup group;RectTransform rect;Vector2 home;Sequence motion;
  void Awake(){rect=(RectTransform)transform;home=rect.anchoredPosition;group=GetComponent<CanvasGroup>();if(!group)group=gameObject.AddComponent<CanvasGroup>();}
  void OnEnable(){if(!Application.isPlaying||!rect)return;motion?.Kill();group.alpha=0;rect.anchoredPosition=home+Vector2.up*8;motion=DOTween.Sequence().SetUpdate(true).Append(group.DOFade(1,.15f)).Join(rect.DOAnchorPos(home,.15f).SetEase(Ease.OutCubic));}
  void OnDisable(){motion?.Kill();if(rect)rect.anchoredPosition=home;}
 }
}
