using UnityEngine;
namespace Bulldozer.UI {
 [ExecuteAlways] public sealed class QuarryHUDLayout : MonoBehaviour {
  public RectTransform safeArea,roundLabel;Vector2 previous;
  void OnEnable()=>Apply();void Update(){if(safeArea&&safeArea.rect.size!=previous)Apply();}
  public void Apply(){if(!safeArea||!roundLabel)return;previous=safeArea.rect.size;roundLabel.sizeDelta=new Vector2(Mathf.Clamp(previous.x-542,180,360),40);roundLabel.anchoredPosition=new Vector2(21,-28);}
 }
}
