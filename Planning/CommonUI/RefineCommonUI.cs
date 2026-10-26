const string folder="Assets/0_ProjectData/UI/Common";
const string originalFont="Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset";
const string ownFont=folder+"/LoadingHeading SDF.asset";
if (!System.IO.File.Exists(ownFont)) UnityEditor.AssetDatabase.CopyAsset(originalFont,ownFont);
var font=UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(ownFont);
var root=UnityEditor.PrefabUtility.LoadPrefabContents(folder+"/CommonUI.prefab");
try{
 foreach(var text in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
  if(UnityEditor.AssetDatabase.GetAssetPath(text.font)==originalFont)text.font=font;
 foreach(var im in root.GetComponentsInChildren<UnityEngine.UI.Image>(true)){
  if(im.name=="BackdropTop" || im.name=="BackdropBottom" || im.name=="Emblem" || im.name=="InnerRing" || im.name=="EmblemShadow"){
   var go=im.gameObject;var color=im.color;UnityEngine.Object.DestroyImmediate(im);var disc=go.AddComponent<Bulldozer.UI.LoadingDiscGraphic>();disc.color=color;disc.raycastTarget=false;
  }else{im.sprite=null;im.type=UnityEngine.UI.Image.Type.Simple;}
 }
 var fill=root.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(i=>i.name=="ProgressFill").rectTransform;
 fill.anchorMin=UnityEngine.Vector2.zero;fill.anchorMax=UnityEngine.Vector2.one;fill.offsetMin=fill.offsetMax=UnityEngine.Vector2.zero;
 // One kicker is enough; leave the authored caption available but hidden.
 root.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t=>t.name=="EmblemCaption").gameObject.SetActive(false);
 var axe=root.GetComponentsInChildren<UnityEngine.RectTransform>(true).First(t=>t.name=="Pickaxe");axe.localScale=UnityEngine.Vector3.one*.85f;
 UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,folder+"/CommonUI.prefab");
}finally{UnityEditor.PrefabUtility.UnloadPrefabContents(root);}
UnityEditor.AssetDatabase.SaveAssetIfDirty(font);
return "Refined flat UI shapes and isolated heading font";
