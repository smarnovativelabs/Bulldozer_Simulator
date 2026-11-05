using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
using Coffee.UIEffects;
using TMPro;
public static class SetupPluginPolish {
 const string ShaderPath="Assets/0_ProjectData/Player/Production/LaunchPolish/QuarryUIShiny.shader";
 static UIShiny Shine(UnityEngine.UI.Image image){var old=image.GetComponent<UIShiny>();if(old&&!(old is QuarryUIShiny))Undo.DestroyObjectImmediate(old);var s=image.GetComponent<QuarryUIShiny>();if(!s)s=Undo.AddComponent<QuarryUIShiny>(image.gameObject);s.compatibleShader=AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);s.width=.16f;s.softness=.8f;s.brightness=.3f;s.gloss=.15f;s.rotation=110;s.effectFactor=0;s.effectPlayer.duration=.65f;s.effectPlayer.loop=false;s.effectPlayer.play=false;s.enabled=false;EditorUtility.SetDirty(s);return s;}
 public static object FixAtlas(){if(EditorApplication.isPlaying)throw new Exception("Stop Play first");var fx=UnityEngine.Object.FindFirstObjectByType<QuarryPluginPolish>();var wallet=fx.rewardShine.GetComponent<UnityEngine.UI.Image>();var cart=fx.cartShine.GetComponent<UnityEngine.UI.Image>();fx.rewardShine=Shine(wallet);fx.cartShine=Shine(cart);fx.shinyShader=AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);EditorUtility.SetDirty(fx);EditorSceneManager.MarkSceneDirty(fx.gameObject.scene);EditorSceneManager.SaveScene(fx.gameObject.scene);return "Project-local atlas-safe UIShiny adapter installed";}
 public static object Main(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
 var g=UnityEngine.Object.FindFirstObjectByType<QuarryGuidance>();var canvas=g.dialog.GetComponentInParent<Canvas>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();
 var fx=g.GetComponent<QuarryPluginPolish>();if(fx)throw new Exception("Already authored; inspect before editing");
 fx=Undo.AddComponent<QuarryPluginPolish>(g.gameObject);fx.guide=g;
 var panel=new GameObject("Objective direction overlay",typeof(RectTransform),typeof(Canvas));Undo.RegisterCreatedObjectUndo(panel,"Objective direction overlay");var root=panel.GetComponent<RectTransform>();root.SetParent(canvas.transform,false);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
 var marker=new GameObject("Objective marker",typeof(RectTransform));marker.transform.SetParent(root,false);var rect=marker.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(140,70);
 var plate=new GameObject("Marker contrast plate",typeof(RectTransform),typeof(UnityEngine.UI.Image));plate.transform.SetParent(rect,false);plate.GetComponent<RectTransform>().sizeDelta=new Vector2(154,68);plate.GetComponent<UnityEngine.UI.Image>().color=new Color(.035f,.055f,.065f,.9f);plate.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
 var arrow=new GameObject("Direction arrow",typeof(RectTransform),typeof(UnityEngine.UI.Image));arrow.transform.SetParent(rect,false);var a=arrow.GetComponent<RectTransform>();a.sizeDelta=new Vector2(32,32);a.anchoredPosition=new Vector2(0,12);var image=arrow.GetComponent<UnityEngine.UI.Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Pixel Play/Sprites/WhiteArrow.png");image.color=new Color(1,.79f,.28f);image.raycastTarget=false;
 var caption=new GameObject("Destination and distance",typeof(RectTransform),typeof(TextMeshProUGUI));caption.transform.SetParent(rect,false);var text=caption.GetComponent<TextMeshProUGUI>();text.font=g.heading.font;text.fontSharedMaterial=g.heading.fontSharedMaterial;text.fontSize=17;text.fontStyle=FontStyles.Bold;text.alignment=TextAlignmentOptions.Center;text.color=Color.white;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.NoWrap;text.rectTransform.sizeDelta=new Vector2(170,28);text.rectTransform.anchoredPosition=new Vector2(0,-18);text.text="REFINERY  12m";
 fx.indicatorPanel=root;fx.marker=rect;fx.arrow=a;fx.caption=text;
 var wallet=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(i=>i.name=="Cash Header");
 fx.rewardShine=Shine(wallet);fx.cartShine=Shine(cart.actionButton.GetComponent<UnityEngine.UI.Image>());
 fx.shinyShader=AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
 marker.SetActive(false);EditorUtility.SetDirty(fx);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
 return new{arrowSprite=image.sprite!=null,shader=fx.shinyShader?fx.shinyShader.name:"MISSING",shineComponents=2,additionalGraphicRaycasters=0,additionalRenderingCameras=0};
 }
}
