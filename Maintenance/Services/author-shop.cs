if(EditorApplication.isPlaying)throw new System.Exception("Stop Play first");
var config=AssetDatabase.LoadAssetAtPath<Bulldozer.Services.QuarryServiceSettings>("Assets/Resources/QuarryServiceSettings.asset");
if(!config){config=ScriptableObject.CreateInstance<Bulldozer.Services.QuarryServiceSettings>();AssetDatabase.CreateAsset(config,"Assets/Resources/QuarryServiceSettings.asset");}
string commonPath="Assets/0_ProjectData/UI/Common/CommonUI.prefab";
var common=PrefabUtility.LoadPrefabContents(commonPath);
var store=common.GetComponent<Bulldozer.Services.QuarryStore>();if(!store)store=common.AddComponent<Bulldozer.Services.QuarryStore>();store.settings=config;
var online=common.GetComponent<Bulldozer.Services.QuarryOnlineServices>();if(!online)online=common.AddComponent<Bulldozer.Services.QuarryOnlineServices>();online.settings=config;
if(!common.GetComponent<Bulldozer.Services.InternetConnectivity>())common.AddComponent<Bulldozer.Services.InternetConnectivity>();
PrefabUtility.SaveAsPrefabAsset(common,commonPath);PrefabUtility.UnloadPrefabContents(common);
string path="Assets/0_ProjectData/0_Scenes/1_Gameplay.unity";
var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
var ui=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Bulldozer.UI.QuarryGameUI>(true)).Single();
if(ui.shopPage)throw new System.Exception("Shop already authored; update incrementally");
var bold=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/0_ProjectData/UI/Fonts/Barlow_Condensed/BarlowCondensed-Bold SDF.asset");
var regular=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/0_ProjectData/UI/Fonts/Barlow/Barlow-Medium SDF.asset");
Sprite S(string file,string name)=>AssetDatabase.LoadAllAssetsAtPath("Assets/0_ProjectData/UI/SpriteSheets/"+file+".png").OfType<Sprite>().First(s=>s.name==name);
string sheet="03_Extra_Panels_Icons_Spritesheet_2048",equipmentSheet="Upgrade_Inventory_Spritesheet_2048";
Vector2 tl=new Vector2(0,1),center=new Vector2(.5f,.5f);
RectTransform R(string name,Transform parent,Vector2 size,Vector2 pos){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=tl;r.sizeDelta=size;r.anchoredPosition=pos;return r;}
UnityEngine.UI.Image I(string name,Transform parent,Vector2 size,Vector2 pos,Sprite sprite,bool icon=false){var r=R(name,parent,size,pos);var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.sprite=sprite;i.type=icon?UnityEngine.UI.Image.Type.Simple:UnityEngine.UI.Image.Type.Sliced;i.pixelsPerUnitMultiplier=3;i.preserveAspect=icon;i.raycastTarget=false;return i;}
TMPro.TextMeshProUGUI T(string name,Transform parent,string text,Vector2 size,Vector2 pos,int font=22,bool heading=false){var t=R(name,parent,size,pos).gameObject.AddComponent<TMPro.TextMeshProUGUI>();t.font=heading?bold:regular;t.text=text;t.fontSize=font;t.color=new Color(.93f,.94f,.92f);t.raycastTarget=false;t.textWrappingMode=TMPro.TextWrappingModes.Normal;return t;}
UnityEngine.UI.Button B(string name,Transform parent,string text,Vector2 size,Vector2 pos){var i=I(name,parent,size,pos,S(sheet,"DarkButton"));i.raycastTarget=true;var b=i.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=i;var t=T("Label",i.transform,text,size-new Vector2(16,8),new Vector2(8,-4),22,true);t.alignment=TMPro.TextAlignmentOptions.Center;return b;}
void Click(UnityEngine.UI.Button b,UnityEngine.Events.UnityAction action)=>UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick,action);
void Open(UnityEngine.UI.Button b,string page)=>UnityEditor.Events.UnityEventTools.AddStringPersistentListener(b.onClick,ui.Open,page);
ui.shopPage=R("Shop",ui.settingsPage.transform.parent,new Vector2(840,430),new Vector2(30,-100)).gameObject;
var shop=ui.shopPage.AddComponent<Bulldozer.Services.QuarryShopPanel>();
for(int n=0;n<2;n++){
 var card=I(n==0?"Remove Ads":"Cash Pack",ui.shopPage.transform,new Vector2(405,280),new Vector2(n*430,0),S(sheet,"Card"));
 I("Icon",card.transform,new Vector2(60,60),new Vector2(24,-20),n==0?S(sheet,"Store"):S(equipmentSheet,"Cash"),true);
 var title=T("Offer",card.transform,n==0?"REMOVE ADS":"QUARRY CASH",new Vector2(282,58),new Vector2(100,-28),28,true);
 if(n==1)shop.cashDescription=title;
 T("Description",card.transform,n==0?"One-time purchase. Removes automatic ads between excavation rounds.":"Add cash for equipment upgrades. Cash and upgrades carry into every round.",new Vector2(350,90),new Vector2(26,-100),21);
 var buy=B("Buy",card.transform,"SETUP PENDING",new Vector2(350,58),new Vector2(26,-202));
 if(n==0){shop.removeButton=buy;shop.removePrice=buy.GetComponentInChildren<TMPro.TMP_Text>();Click(buy,shop.BuyRemoveAds);}else{shop.cashButton=buy;shop.cashPrice=buy.GetComponentInChildren<TMPro.TMP_Text>();Click(buy,shop.BuyCash);}
}
shop.status=T("Store status",ui.shopPage.transform,"Store setup pending",new Vector2(840,56),new Vector2(0,-292),20);
shop.restoreButton=B("Restore",ui.shopPage.transform,"RESTORE PURCHASES",new Vector2(265,56),new Vector2(0,-365));Click(shop.restoreButton,shop.Restore);
var retry=B("Retry",ui.shopPage.transform,"RETRY STORE",new Vector2(215,56),new Vector2(280,-365));Click(retry,shop.Retry);
var privacy=B("Privacy",ui.shopPage.transform,"PRIVACY",new Vector2(215,56),new Vector2(625,-365));Open(privacy,"privacy");
var cashTarget=ui.cash.transform.parent.gameObject;var cashButton=cashTarget.GetComponent<UnityEngine.UI.Button>();if(!cashButton)cashButton=cashTarget.AddComponent<UnityEngine.UI.Button>();var cashImage=cashTarget.GetComponent<UnityEngine.UI.Image>();if(cashImage){cashImage.raycastTarget=true;cashButton.targetGraphic=cashImage;}Open(cashButton,"shop");
Open(B("Shop",ui.pausePage.transform,"QUARRY STORE",new Vector2(400,62),new Vector2(220,-315)),"shop");
Open(B("Privacy",ui.settingsPage.transform,"PRIVACY CHOICES",new Vector2(240,52),new Vector2(590,-382)),"privacy");
ui.privacyPage=R("Privacy",ui.settingsPage.transform.parent,new Vector2(840,430),new Vector2(30,-100)).gameObject;
var pp=ui.privacyPage.AddComponent<Bulldozer.Services.QuarryPrivacyPanel>();
UnityEngine.UI.Toggle Toggle(string title,string body,int row,UnityEngine.Events.UnityAction<bool> action){var root=R(title,ui.privacyPage.transform,new Vector2(840,80),new Vector2(0,-row*85));T("Title",root,title,new Vector2(650,32),Vector2.zero,24,true);T("Description",root,body,new Vector2(670,40),new Vector2(0,-34),18);var track=I("Toggle",root,new Vector2(100,56),new Vector2(725,-8),S(sheet,"DarkButton"));track.raycastTarget=true;var check=I("On",track.transform,new Vector2(32,32),new Vector2(34,-12),S(sheet,"Check"),true);var toggle=track.gameObject.AddComponent<UnityEngine.UI.Toggle>();toggle.targetGraphic=track;toggle.graphic=check;UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(toggle.onValueChanged,action,true);toggle.onValueChanged.RemoveAllListeners();return toggle;}
pp.analytics=Toggle("GAMEPLAY ANALYTICS","Share anonymous gameplay events to help improve the quarry.",0,pp.Analytics);
pp.crashReports=Toggle("CRASH REPORTS","Send technical crash reports when something goes wrong.",1,pp.CrashReports);
pp.attribution=Toggle("AD ATTRIBUTION","Help measure advertising. iOS also asks for tracking permission.",2,pp.Attribution);
// Dynamic bool listeners preserve the actual toggle value.
foreach(var t in new[]{pp.analytics,pp.crashReports,pp.attribution}){UnityEditor.Events.UnityEventTools.RemovePersistentListener(t.onValueChanged,0);}
UnityEditor.Events.UnityEventTools.AddPersistentListener(pp.analytics.onValueChanged,pp.Analytics);UnityEditor.Events.UnityEventTools.AddPersistentListener(pp.crashReports.onValueChanged,pp.CrashReports);UnityEditor.Events.UnityEventTools.AddPersistentListener(pp.attribution.onValueChanged,pp.Attribution);
pp.status=T("Status",ui.privacyPage.transform,"Services setup pending",new Vector2(840,70),new Vector2(0,-265),19);
Click(B("Ad choices",ui.privacyPage.transform,"AD PRIVACY",new Vector2(240,56),new Vector2(0,-365)),pp.AdPrivacy);
Click(B("Policy",ui.privacyPage.transform,"PRIVACY POLICY",new Vector2(240,56),new Vector2(285,-365)),pp.Policy);
Click(B("Retry",ui.privacyPage.transform,"RETRY SERVICES",new Vector2(240,56),new Vector2(570,-365)),pp.Retry);
ui.shopPage.SetActive(false);ui.privacyPage.SetActive(false);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);
PlayerSettings.Android.minSdkVersion=(AndroidSdkVersions)24;PlayerSettings.iOS.targetOSVersionString="15.0";
AssetDatabase.SaveAssets();Debug.Log("Quarry shop, privacy controls and persistent services authored.");
