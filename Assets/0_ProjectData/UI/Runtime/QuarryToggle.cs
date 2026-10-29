using UnityEngine;
using DG.Tweening;
namespace Bulldozer.UI {
 public sealed class QuarryToggle : MonoBehaviour {
  public UnityEngine.UI.Toggle toggle;public UnityEngine.UI.Image track;public RectTransform thumb;public TMPro.TMP_Text label;public string preference="Sound";public Sprite onSprite,offSprite;Tween move,tint;
  void Start(){toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(QuarryUIFeedback.PreferencePrefix+preference,preference=="Sound"?1:0)==1);Apply(toggle.isOn,false);toggle.onValueChanged.AddListener(Change);}
  void Change(bool on){PlayerPrefs.SetInt(QuarryUIFeedback.PreferencePrefix+preference,on?1:0);PlayerPrefs.Save();QuarryUIFeedback.ApplySettings();Apply(on,true);GetComponentInParent<QuarryGameUI>()?.feedback.Play();}
  void Apply(bool on,bool animated){move?.Kill();tint?.Kill();float x=on?26:-26;if(onSprite&&offSprite)track.sprite=on?onSprite:offSprite;Color c=on?new Color(.83f,.67f,.37f):new Color(.65f,.7f,.73f);label.text=on?"ON":"OFF";if(animated){move=thumb.DOAnchorPosX(x,.15f).SetUpdate(true);tint=track.DOColor(c,.15f).SetUpdate(true);}else{thumb.anchoredPosition=new Vector2(x,0);track.color=c;}}
  public void Refresh(){bool on=PlayerPrefs.GetInt(QuarryUIFeedback.PreferencePrefix+preference,preference=="Sound"?1:0)==1;toggle.SetIsOnWithoutNotify(on);Apply(on,false);}
  void OnDisable(){move?.Kill();tint?.Kill();}void OnDestroy(){toggle.onValueChanged.RemoveListener(Change);}
 }
}

