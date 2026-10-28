using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bulldozer.Player
{
    // Cosmetic only. Never reads/writes inventory or awards currency.
    // A fixed array and one canvas mesh replace per-coin GameObjects and tweens.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RewardCoinFeedback : MaskableGraphic
    {
        public TMP_Text balanceText;
        public Camera worldCamera;
        [Tooltip("Optional replacement for the placeholder banknote artwork; all notes share this texture.")]
        public Sprite coinSprite;
        public const int CoinLimit=8;
        struct Coin { public Vector2 start,position; public float delay,scale,alpha; }
        readonly Coin[] coins=new Coin[CoinLimit];
        int count;
        float age;
        Vector2 target;
        public int BurstsPlayed {get;private set;}
        public bool IsAnimating => count>0;
        public override Texture mainTexture => coinSprite?coinSprite.texture:Texture2D.whiteTexture;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        public void PlayReward(Vector3 worldPosition,int paidCoins)
        {
            if(paidCoins<=0||!isActiveAndEnabled||!balanceText)return;
            if(!worldCamera)worldCamera=Camera.main;
            balanceText.ForceMeshUpdate();target=BalancePosition();
            Vector3 screen=worldCamera?worldCamera.WorldToScreenPoint(worldPosition):new Vector3(Screen.width*.5f,Screen.height*.2f,1);
            // A truck behind the camera originates at the lower safe edge instead of mirrored coordinates.
            if(screen.z<=0)screen=new Vector3(Screen.width*.5f,Screen.height*.08f,1);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,null,out Vector2 origin);
            Rect area=rectTransform.rect;
            origin.x=Mathf.Clamp(origin.x,area.xMin+24,area.xMax-24);origin.y=Mathf.Clamp(origin.y,area.yMin+24,area.yMax-24);
            count=Mathf.Clamp(paidCoins,4,CoinLimit);age=0;BurstsPlayed++;
            for(int i=0;i<count;i++)coins[i]=new Coin{start=origin+new Vector2((i%4-1.5f)*14,(i/4)*14),position=origin,delay=i*.045f,scale=1,alpha=0};
            SetVerticesDirty();
        }
        Vector2 BalancePosition()
        {
            int first=balanceText.text.IndexOf("CASH $",System.StringComparison.Ordinal);
            if(first>=0&&balanceText.textInfo.characterCount>first+6)
            {
                first+=6;int last=first;
                while(last+1<balanceText.textInfo.characterCount&&char.IsDigit(balanceText.textInfo.characterInfo[last+1].character))last++;
                var a=balanceText.textInfo.characterInfo[first];var b=balanceText.textInfo.characterInfo[last];
                return rectTransform.InverseTransformPoint(balanceText.transform.TransformPoint((a.bottomLeft+b.topRight)*.5f));
            }
            return rectTransform.InverseTransformPoint(balanceText.transform.position);
        }
        void LateUpdate()
        {
            if(count==0)return;
            age+=Time.unscaledDeltaTime;target=BalancePosition();
            for(int i=0;i<count;i++)
            {
                var c=coins[i];float t=Mathf.Clamp01((age-c.delay)/.8f);float ease=t*t*(3-2*t);
                c.position=Vector2.Lerp(c.start,target,ease)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*85);
                c.alpha=age<c.delay||t>=1?0:Mathf.Min(1,t*12);
                c.scale=(1-.6f*t)*(.85f+.15f*Mathf.Cos(t*12));coins[i]=c;
            }
            if(age>.8f+(count-1)*.045f+.15f)count=0;
            SetVerticesDirty();
        }
        protected override void OnDisable(){count=0;age=0;base.OnDisable();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            for(int i=0;i<count;i++)
            {
                var c=coins[i];if(c.alpha<=0)continue;
                float radius=12*c.scale;
                if(coinSprite)
                {
                    var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(coinSprite);int n=vh.currentVertCount;
                    vh.AddVert(c.position+new Vector2(-radius,-radius),new Color(1,1,1,c.alpha),new Vector2(uv.x,uv.y));
                    vh.AddVert(c.position+new Vector2(-radius,radius),new Color(1,1,1,c.alpha),new Vector2(uv.x,uv.w));
                    vh.AddVert(c.position+new Vector2(radius,radius),new Color(1,1,1,c.alpha),new Vector2(uv.z,uv.w));
                    vh.AddVert(c.position+new Vector2(radius,-radius),new Color(1,1,1,c.alpha),new Vector2(uv.z,uv.y));
                    vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
                }
                else
                {
                    Note(vh,c.position,new Vector2(radius*1.4f,radius*.7f),new Color(.08f,.3f,.16f,c.alpha));
                    Note(vh,c.position,new Vector2(radius*1.17f,radius*.5f),new Color(.44f,.88f,.56f,c.alpha));
                    Disc(vh,c.position,radius*.31f,new Color(.12f,.45f,.24f,c.alpha));
                }
            }
        }
        static void Note(VertexHelper vh,Vector2 p,Vector2 size,Color tint)
        {
            int n=vh.currentVertCount;vh.AddVert(p+new Vector2(-size.x,-size.y),tint,Vector2.zero);vh.AddVert(p+new Vector2(-size.x,size.y),tint,Vector2.zero);vh.AddVert(p+size,tint,Vector2.zero);vh.AddVert(p+new Vector2(size.x,-size.y),tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
        static void Disc(VertexHelper vh,Vector2 p,float r,Color tint)
        {
            int start=vh.currentVertCount;vh.AddVert(p,tint,Vector2.zero);
            for(int j=0;j<12;j++){float a=j*Mathf.PI/6;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,tint,Vector2.zero);}
            for(int j=0;j<12;j++)vh.AddTriangle(start,start+1+j,start+1+(j+1)%12);
        }
    }
}

