using UnityEngine;
using MoreMountains.Feedbacks;
namespace Bulldozer.UI {
 public sealed class QuarryUIFeedback : MonoBehaviour {
  public AudioClip click,confirm,error; MMF_Player[] sounds;float nextSound,nextHaptic;
  public static string PreferencePrefix {
   get {
#if UNITY_EDITOR
    return UnityEditor.SessionState.GetString("Quarry.UI.TestPrefs","Quarry.UI.");
#else
    return "Quarry.UI.";
#endif
   }
  }
  public static bool Sound=>PlayerPrefs.GetInt(PreferencePrefix+"Sound",1)==1;
  public static bool Vibration=>PlayerPrefs.GetInt(PreferencePrefix+"Vibration",0)==1;
  void Awake(){sounds=new MMF_Player[3];var clips=new[]{click,confirm,error};for(int i=0;i<3;i++){var go=new GameObject("Cached UI feedback "+i);go.transform.SetParent(transform,false);var f=go.AddComponent<MMF_Player>();f.AddFeedback(new MMF_Sound{Sfx=clips[i],PlayMethod=MMF_Sound.PlayMethods.Cached,MinVolume=i==2?.10f:.18f,MaxVolume=i==2?.10f:.18f,MinPitch=i==2?.8f:1,MaxPitch=i==2?.8f:1,SpatialBlend=0});f.Initialization();sounds[i]=f;}ApplySettings();}
  public static void ApplySettings(){AudioListener.volume=Sound?PlayerPrefs.GetFloat(PreferencePrefix+"Volume",.8f):0;Application.targetFrameRate=PlayerPrefs.GetInt(PreferencePrefix+"Performance",0)==0?30:60;}
  public void Play(int kind=0){if(Time.unscaledTime<nextSound)return;nextSound=Time.unscaledTime+.1f;if(Sound&&sounds!=null)sounds[Mathf.Clamp(kind,0,2)].PlayFeedbacks();if(kind==1&&Vibration&&Application.isMobilePlatform&&Time.unscaledTime>=nextHaptic){nextHaptic=Time.unscaledTime+.5f;Handheld.Vibrate();}}
  void OnDisable(){if(sounds!=null)foreach(var f in sounds)if(f)f.StopFeedbacks();}
 }
}
