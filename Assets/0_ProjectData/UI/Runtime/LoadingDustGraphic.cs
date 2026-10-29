using UnityEngine;
namespace Bulldozer.UI {
 public sealed class LoadingDustGraphic : UnityEngine.UI.MaskableGraphic {
  float tick;void Update(){if(Time.unscaledTime<tick)return;tick=Time.unscaledTime+.05f;SetVerticesDirty();}
  protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh){vh.Clear();var r=rectTransform.rect;for(int i=0;i<12;i++){float t=Time.unscaledTime*.012f;Vector2 p=new Vector2(r.xMin+Mathf.Repeat(i*.173f+t,1)*r.width,r.yMin+Mathf.Repeat(i*.317f+t*.35f,1)*r.height);float size=7+i%4*5;int n=vh.currentVertCount;vh.AddVert(p,new Color(1,.88f,.68f,.055f),Vector2.zero);for(int j=0;j<=8;j++){float a=j*Mathf.PI/4;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size,new Color(1,.88f,.68f,0),Vector2.zero);if(j>0)vh.AddTriangle(n,n+j,n+j+1);}}}
 }
}
