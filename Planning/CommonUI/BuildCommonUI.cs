// Run once with Pipeline script eval_file. All UI is authored as an editable prefab.
const string folder = "Assets/0_ProjectData/UI/Common";
const string prefabPath = folder + "/CommonUI.prefab";
const string menuPath = "Assets/0_ProjectData/0_Scenes/0_MainMenu.unity";
const string gamePath = "Assets/0_ProjectData/0_Scenes/1_Gameplay.unity";
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode first.");
if (System.IO.File.Exists(prefabPath)) throw new System.Exception("Prefab exists; edit it in place rather than rebuilding.");
if (Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)) throw new System.Exception("Unsaved scene changes; refusing to replace scene state.");
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(menuPath);
var ink = new UnityEngine.Color32(26,55,57,255);
var paper = new UnityEngine.Color32(246,240,226,255);
var gold = new UnityEngine.Color32(233,174,64,255);
var muted = new UnityEngine.Color32(88,109,103,255);
var white = UnityEngine.Color.white;
var round = UnityEditor.AssetDatabase.GetBuiltinExtraResource<UnityEngine.Sprite>("UI/Skin/UISprite.psd");
var circle = UnityEditor.AssetDatabase.GetBuiltinExtraResource<UnityEngine.Sprite>("UI/Skin/Knob.psd");
var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
var bold = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
if (font == null || bold == null) throw new System.Exception("Required TMP font assets missing.");
UnityEngine.RectTransform Rect(string name, UnityEngine.Transform parent, float w, float h, float x=0, float y=0) {
 var r = new UnityEngine.GameObject(name,typeof(UnityEngine.RectTransform)).GetComponent<UnityEngine.RectTransform>();
 r.SetParent(parent,false);r.anchorMin=r.anchorMax=new UnityEngine.Vector2(.5f,.5f);r.sizeDelta=new UnityEngine.Vector2(w,h);r.anchoredPosition=new UnityEngine.Vector2(x,y);return r;
}
void Stretch(UnityEngine.RectTransform r) {r.anchorMin=UnityEngine.Vector2.zero;r.anchorMax=UnityEngine.Vector2.one;r.offsetMin=r.offsetMax=UnityEngine.Vector2.zero;}
UnityEngine.UI.Image Shape(string name,UnityEngine.Transform parent,float w,float h,UnityEngine.Color color,float x=0,float y=0,UnityEngine.Sprite sprite=null) {
 var r=Rect(name,parent,w,h,x,y);var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();im.color=color;im.raycastTarget=false;im.sprite=sprite;if(sprite!=null)im.type=UnityEngine.UI.Image.Type.Sliced;return im;
}
TMPro.TextMeshProUGUI Label(string name,UnityEngine.Transform parent,string value,float size,UnityEngine.Color color,float w,float h,float x,float y,bool heavy=false) {
 var r=Rect(name,parent,w,h,x,y);var t=r.gameObject.AddComponent<TMPro.TextMeshProUGUI>();t.font=heavy?bold:font;t.text=value;t.fontSize=size;t.color=color;t.alignment=TMPro.TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TMPro.TextWrappingModes.Normal;return t;
}
void Bind(UnityEngine.Object component,string field,UnityEngine.Object value) {var s=new UnityEditor.SerializedObject(component);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
var root=new UnityEngine.GameObject("CommonUI");
var common=root.AddComponent<Bulldozer.UI.CommonUI>();
var service=root.AddComponent<Bulldozer.UI.SceneLoadingService>();
var canvasRect=Rect("CommonUICanvas",root.transform,1080,1920);
var canvas=canvasRect.gameObject.AddComponent<UnityEngine.Canvas>();canvas.renderMode=UnityEngine.RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1000;
var scaler=canvasRect.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new UnityEngine.Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
canvasRect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
var panel=Rect("LoadingPanel",canvasRect,1080,1920);Stretch(panel);
var cover=panel.gameObject.AddComponent<UnityEngine.CanvasGroup>();
var background=Shape("Background",panel,1080,1920,paper);Stretch(background.rectTransform);background.raycastTarget=true;
var glow=Shape("BackdropTop",panel,1000,1000,new UnityEngine.Color32(231,226,207,255),-650,950,circle);
var glow2=Shape("BackdropBottom",panel,1100,1100,new UnityEngine.Color32(235,230,212,255),650,-950,circle);
var safe=Rect("SafeArea",panel,1080,1920);Stretch(safe);safe.gameObject.AddComponent<Crystal.SafeArea>();
var composition=Rect("Composition",safe,850,1160);
var contentGroup=composition.gameObject.AddComponent<UnityEngine.CanvasGroup>();
var hero=Rect("MiningEmblem",composition,500,500,0,310);
Shape("EmblemShadow",hero,414,414,new UnityEngine.Color32(211,205,184,255),0,-13,circle);
Shape("Emblem",hero,414,414,ink,0,0,circle);
Shape("InnerRing",hero,365,365,new UnityEngine.Color32(39,73,73,255),0,0,circle);
Shape("Ground",hero,236,12,new UnityEngine.Color32(15,38,39,255),0,-113,round);
var rock=Rect("Rock",hero,210,170,0,-43);
var stone=Shape("Stone",rock,142,142,new UnityEngine.Color32(139,160,150,255),0,0);stone.rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,22);
Shape("StoneFace",rock,92,110,new UnityEngine.Color32(177,193,171,255),-40,-3).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,-14);
Shape("Ore",rock,28,28,gold,32,15).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,28);
Shape("OreSmall",rock,16,16,gold,6,-34).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,28);
var axe=Rect("Pickaxe",hero,250,260,40,48);
Shape("HandleShadow",axe,22,193,new UnityEngine.Color32(15,38,39,255),10,0).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,-35);
Shape("Handle",axe,19,193,new UnityEngine.Color32(189,127,62,255),0,0).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,-35);
Shape("Grip",axe,24,56,gold,-40,-58).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,-35);
var head=Rect("Head",axe,166,40,54,81);head.localEulerAngles=new UnityEngine.Vector3(0,0,-28);
Shape("Steel",head,148,27,new UnityEngine.Color32(223,235,222,255));
Shape("SteelEdge",head,142,8,new UnityEngine.Color32(144,172,161,255),0,-13);
Shape("LeftTip",head,36,20,new UnityEngine.Color32(223,235,222,255),-78,-8).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,32);
Shape("RightTip",head,36,20,new UnityEngine.Color32(223,235,222,255),78,-8).rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,-32);
var sparks=Rect("Impact",hero,200,170,0,50);var impact=sparks.gameObject.AddComponent<UnityEngine.CanvasGroup>();impact.alpha=0;
for(int i=0;i<3;i++){var shard=Shape("Spark"+i,sparks,10,29,gold,-65+i*45,38+(i==1?25:0));shard.rectTransform.localEulerAngles=new UnityEngine.Vector3(0,0,(i-1)*-28);}
Label("EmblemCaption",hero,"ONE STRIKE AT A TIME",21,muted,500,40,0,-244,true).characterSpacing=5;
var copy=Rect("LoadingDetails",composition,680,600,0,-220);
Label("Eyebrow",copy,"A NEW DAY AT THE QUARRY",23,muted,680,45,0,260,true).characterSpacing=3;
Label("Heading",copy,"Small steps.\nBigger possibilities.",65,ink,680,174,0,149,true);
var status=Label("LoadingStatus",copy,"Preparing your worksite",29,ink,540,50,-55,-4);
status.alignment=TMPro.TextAlignmentOptions.Left;
var percent=Label("Percentage",copy,"0%",30,ink,100,50,280,-4,true);percent.alignment=TMPro.TextAlignmentOptions.Right;
var track=Shape("ProgressTrack",copy,660,20,new UnityEngine.Color32(215,216,197,255),0,-64,round);
var fill=Shape("ProgressFill",track.transform,660,20,ink,0,0,round);fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fill.fillAmount=0;
var tip=Label("Tip",copy,"Every great worksite starts with a single strike.",27,muted,640,100,0,-154);
var buttonImage=Shape("RetryButton",copy,260,68,ink,0,-270,round);buttonImage.raycastTarget=true;
var button=buttonImage.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=buttonImage;
Label("Label",button.transform,"Try again",28,white,240,60,0,0,true);button.gameObject.SetActive(false);
var presenter=panel.gameObject.AddComponent<Bulldozer.UI.LoadingPanel>();
Bind(presenter,"cover",cover);Bind(presenter,"contentGroup",contentGroup);Bind(presenter,"safeArea",safe);Bind(presenter,"composition",composition);Bind(presenter,"hero",hero);Bind(presenter,"copy",copy);Bind(presenter,"pickaxe",axe);Bind(presenter,"rock",rock);Bind(presenter,"impact",impact);Bind(presenter,"progressFill",fill);Bind(presenter,"status",status);Bind(presenter,"percentage",percent);Bind(presenter,"tip",tip);Bind(presenter,"retryButton",button);
Bind(service,"panel",presenter);
var events=new UnityEngine.GameObject("CommonUIEventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));events.transform.SetParent(root.transform,false);
var input=events.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();input.AssignDefaultActions();
Bind(common,"sceneLoading",service);Bind(common,"sharedEventSystem",events.GetComponent<UnityEngine.EventSystems.EventSystem>());
var prefab=UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(root,prefabPath,UnityEditor.InteractionMode.AutomatedAction);
var bootstrap=new UnityEngine.GameObject("MainMenuBootstrap").AddComponent<Bulldozer.UI.MainMenuBootstrap>();Bind(bootstrap,"commonUIPrefab",prefab.GetComponent<Bulldozer.UI.CommonUI>());
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
// Preserve other entries, but make bootstrap + gameplay the first enabled scenes.
var old=UnityEditor.EditorBuildSettings.scenes;
UnityEditor.EditorBuildSettings.scenes=new[]{new UnityEditor.EditorBuildSettingsScene(menuPath,true),new UnityEditor.EditorBuildSettingsScene(gamePath,true)}.Concat(old.Where(s=>s.path!=menuPath && s.path!=gamePath)).ToArray();
UnityEditor.AssetDatabase.SaveAssetIfDirty(prefab);
return new{prefabPath,mainMenu=scene.path,buildScenes=UnityEditor.EditorBuildSettings.scenes.Select(s=>new{s.path,s.enabled}).ToArray()};
