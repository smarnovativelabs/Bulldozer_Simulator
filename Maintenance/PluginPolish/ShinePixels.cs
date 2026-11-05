using UnityEngine;
using System.Collections;
using System.IO;
using Bulldozer.Player;
public static class ShinePixels {public static string Main(){new GameObject("Shine pixel check").AddComponent<ShinePixelRunner>();return "Comparing rendered reward pixels";}}
public class ShinePixelRunner:MonoBehaviour {
 IEnumerator Start(){var f=Object.FindFirstObjectByType<QuarryPluginPolish>();f.enabled=false;var effect=f.rewardShine;effect.enabled=true;effect.Stop();effect.effectFactor=0;yield return null;yield return new WaitForEndOfFrame();var a=ScreenCapture.CaptureScreenshotAsTexture();
 effect.effectFactor=.5f;yield return null;yield return new WaitForEndOfFrame();var b=ScreenCapture.CaptureScreenshotAsTexture();var rect=effect.GetComponent<RectTransform>();var corners=new Vector3[4];rect.GetWorldCorners(corners);var lo=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var hi=RectTransformUtility.WorldToScreenPoint(null,corners[2]);int changed=0,total=0;var ca=a.GetPixels();var cb=b.GetPixels();
 for(int y=Mathf.Max(0,(int)lo.y);y<Mathf.Min(a.height,(int)hi.y);y++)for(int x=Mathf.Max(0,(int)lo.x);x<Mathf.Min(a.width,(int)hi.x);x++){int n=y*a.width+x;var d=ca[n]-cb[n];if(Mathf.Abs(d.r)+Mathf.Abs(d.g)+Mathf.Abs(d.b)>.025f)changed++;total++;}
 f.cartShine.enabled=true;f.cartShine.Stop();Canvas.ForceUpdateCanvases();bool shared=effect.GetComponent<UnityEngine.UI.Image>().materialForRendering==f.cartShine.GetComponent<UnityEngine.UI.Image>().materialForRendering;
 File.WriteAllLines("Maintenance/PluginPolish/shine-pixels.txt",new[]{(changed>20&&changed<total*.65f?"PASS ":"FAIL ")+"Localized rendered shine changes "+changed+" of "+total+" reward-panel pixels",(shared?"PASS ":"FAIL ")+"Both effect targets reuse one cached plugin material"});Object.Destroy(a);Object.Destroy(b);effect.enabled=false;f.cartShine.enabled=false;f.enabled=true;Object.Destroy(gameObject);
 }
}
