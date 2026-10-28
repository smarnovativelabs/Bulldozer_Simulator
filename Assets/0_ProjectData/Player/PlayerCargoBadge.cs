using UnityEngine;
namespace Bulldozer.Player
{
    // Small world-space UI. The placeholder Image can be replaced with final artwork.
    public sealed class PlayerCargoBadge : MonoBehaviour
    {
        public MinerEquipment equipment;
        public TMPro.TMP_Text label;
        public UnityEngine.UI.Image icon,fill;
        Camera viewCamera;
        int lastCount=-1,lastCapacity=-1;bool lastCart;
        void LateUpdate()
        {
            if(!viewCamera)viewCamera=Camera.main;
            if(viewCamera)transform.rotation=viewCamera.transform.rotation;
            if(!equipment)return;
            int count=equipment.CargoCount,capacity=equipment.Capacity;bool cart=equipment.UsingWheelbarrow;
            if(count==lastCount&&capacity==lastCapacity&&cart==lastCart)return;
            lastCount=count;lastCapacity=capacity;lastCart=cart;
            label.text=(cart?"CART ":"CARGO ")+count+"/"+capacity;
            var color=count>=capacity?new Color(1,.65f,.2f):new Color(.65f,.9f,.8f);
            icon.color=fill.color=color;
            fill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(count/(float)Mathf.Max(1,capacity)),1);
            label.color=count>=capacity?color:Color.white;
        }
    }
}
