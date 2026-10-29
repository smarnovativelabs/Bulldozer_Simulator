using UnityEngine;
namespace Bulldozer.UI {
 [ExecuteAlways] public sealed class QuarryPanelFit : MonoBehaviour {
  public Vector2 design=new Vector2(900,560);Vector2 last;
  void OnEnable()=>Fit();void Update(){var p=transform.parent as RectTransform;if(p&&p.rect.size!=last)Fit();}
  public void Fit(){var p=transform.parent as RectTransform;if(!p)return;last=p.rect.size;float scale=Mathf.Clamp(Mathf.Min((last.x-40)/design.x,(last.y-40)/design.y),.1f,1);transform.localScale=Vector3.one*scale;}
 }
}
